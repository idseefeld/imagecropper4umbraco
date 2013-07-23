using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;
using System.Text;
using System.Xml;
using System.Configuration;
using umbraco.businesslogic;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic.member;
using umbraco.editorControls.uploadfield;
using umbraco.cms.businesslogic.property;
using umbraco.presentation;
using Umbraco.Core.IO;

namespace idseefeld.de.imagecropper.imagecropper
{
	public class ImageEventsForCropper : ApplicationStartupHandler
	{
		//ToDo: make contentType with croppers configurable

		public ImageEventsForCropper()
		{
			Media.AfterSave += new Media.SaveEventHandler(Media_AfterSave);
			//Document.AfterSave += new Document.SaveEventHandler(Document_AfterSave);
			Member.AfterSave += new Member.SaveEventHandler(Member_AfterSave);

			Document.BeforePublish += new Document.PublishEventHandler(Document_BeforePublish);
		}

		void Member_AfterSave(Member sender, umbraco.cms.businesslogic.SaveEventArgs e)
		{
			HandleImageCropper(sender.GenericProperties, false);
		}


		void Media_AfterSave(Media sender, umbraco.cms.businesslogic.SaveEventArgs e)
		{
			HandleImageCropper(sender.GenericProperties, false);
		}


		void Document_AfterSave(Document sender, umbraco.cms.businesslogic.SaveEventArgs e)
		{
			//ToDo: setup a list of doctype with cropper props set to auto generate crops
			// save the list in application and use the list here instead of the appsetting in web.config

			//At least auto crops for croppers on document types are not that important, 
			//because the regular workflow involves a second save throug publishing 
			//and the crops are more coupled with the content. 

			//HandleImageCropper(sender.GenericProperties, false);
		}
		void Document_BeforePublish(Document sender, umbraco.cms.businesslogic.PublishEventArgs e)
		{
			DataType imageCropperDt = new DataType();

			List<string> activeFiles = new List<string>();
			List<string> mediaItemDirectories = new List<string>();

			var cropperProps = sender.GenericProperties
				.Where(p => p.PropertyType.DataTypeDefinition.DataType.Id.Equals(imageCropperDt.Id));
			foreach (var prop in cropperProps)
			{
				if (prop.Value == null
					|| String.IsNullOrEmpty(prop.Value.ToString()))
					continue;

				XmlDocument xml = new XmlDocument();
				xml.LoadXml(prop.Value.ToString());
				string url = String.Empty;
				foreach (XmlNode node in xml.DocumentElement.ChildNodes)
				{
					url = node.Attributes["newurl"] != null
						? node.Attributes["newurl"].Value : String.Empty;
					if (!String.IsNullOrEmpty(url))
						activeFiles.Add(url.Substring(url.LastIndexOf('/') + 1));
				}

				if (!String.IsNullOrEmpty(url))
				{
					string path = url.Substring(0, url.LastIndexOf('/'));
					if (!String.IsNullOrEmpty(path))
					{
						mediaItemDirectories.Add(path);
					}
				}
			}
			if (mediaItemDirectories.Count == 0)
				return;

			try
			{
				foreach (var dir in mediaItemDirectories)
				{
					string mappedDirPath = IOHelper.MapPath(dir);
					MediaFileSystem _fileSystem = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();
					foreach (var file in _fileSystem.GetFiles(mappedDirPath))
					{
						if (file.Substring(0, file.LastIndexOf('.')).EndsWith(DataType.CROP_POSTFIX))
						{
							string delFilePath = mappedDirPath + file.Substring(file.LastIndexOf('\\'));
							bool deleteFile = true;
							foreach (var activeFile in activeFiles)
							{
								if (delFilePath.EndsWith(activeFile))
								{
									deleteFile = false;
									break;
								}
							}
							if (deleteFile)
								_fileSystem.DeleteFile(delFilePath);
						}

					}
				}
			}
			catch (Exception ex)
			{
				string msg = ex.Message;
			}
		}



		private static void HandleImageCropper(Properties properties, bool parentIsDocument)
		{
			DataType imageCropperDt = new DataType();
			DataTypeUploadField uploadFieldDt = new DataTypeUploadField();

			foreach (var prop in properties)
			{
				if (prop.PropertyType.DataTypeDefinition.DataType.Id.Equals(imageCropperDt.Id))
				{
					var prevalues = prop.PropertyType.DataTypeDefinition.DataType.PrevalueEditor;

					Config config = new Config(((PrevalueEditor)prevalues).Configuration);					

					var uploadFieldProp = properties.Where(p => p.PropertyType.DataTypeDefinition.DataType.Id.Equals(uploadFieldDt.Id)
						&& p.PropertyType.Alias.Equals(config.UploadPropertyAlias)).FirstOrDefault();
					if (uploadFieldProp == null)
						continue;

					string imgUrl = uploadFieldProp.Value != null ? uploadFieldProp.Value.ToString() : String.Empty;
					if (String.IsNullOrEmpty(imgUrl))
						continue;
					string imgUrlWithoutExtension = imgUrl.Substring(0, imgUrl.LastIndexOf('.'));

					if (prop.Value == null
						|| String.IsNullOrEmpty(prop.Value.ToString())
						|| !prop.Value.ToString().Contains(imgUrlWithoutExtension + "_"))
					{
						int imgWidth = 0;
						int imgHeight = 0;
						using (Bitmap img = new Bitmap(HttpContext.Current.Server.MapPath(imgUrl)))
						{
							imgWidth = img.Width;
							imgHeight = img.Height;
						}
						if (imgWidth == 0)
							continue;

						var presets = config.presets;
						//see: idseefeld.de.imagecropper.imagecropper.DataEditor.Save() for how to setup cropper default property.Value
						StringBuilder sbRaw = new StringBuilder();
						ImageInfo imageInfo = new ImageInfo(imgUrl, config, parentIsDocument);
						int i = 1;
						foreach (var presetConfig in presets)
						{
							Preset preset = (Preset)presetConfig;
							Crop crop;
							if (imageInfo.Exists) {
								crop = preset.Fit(imageInfo);
							}
							else {
								crop.X = 0;
								crop.Y = 0;
								crop.X2 = preset.TargetWidth;
								crop.Y2 = preset.TargetHeight;
							}
							sbRaw.Append(String.Format("{0},{1},{2},{3}", crop.X, crop.Y, crop.X2, crop.Y2));
							if (i < presets.Count) {
								sbRaw.Append(";");
							}
							i++;
							if (config.GenerateImages) {
								GenerateCrop(
									preset,
									imgUrl,
									config,
									parentIsDocument,
									prop.Value == null ? String.Empty : prop.Value.ToString()
								);
							}
						}
						SaveData saveData = new SaveData(sbRaw.ToString());
						//save cropper default property.Value
						prop.Value = saveData.Xml(config, imageInfo, parentIsDocument);
					}
				}
			}
		}

		private static void GenerateCrop(Preset preset, string imgUrl, Config config, bool parentIsDocument, string savedValues)
		{
			string result = String.Empty;
			if (String.IsNullOrEmpty(preset.Name))
				return;

			ImageInfo imgInfo = new ImageInfo(imgUrl, config, parentIsDocument);
			StringBuilder sbJson = new StringBuilder();
			StringBuilder sbRaw = new StringBuilder();
			XmlDocument xml;
			if (String.IsNullOrEmpty(savedValues)
				|| !savedValues.Contains(String.Format("{0}_", imgInfo.Name)))
			{
				xml = SaveData.createBaseXmlDocument();
			}
			else
			{
				xml = new XmlDocument();
				xml.LoadXml(savedValues);
			}
			DataEditor.ConfigurateCrops(config, xml, imgInfo, sbJson, sbRaw);
			SaveData data = new SaveData(sbRaw.ToString());
			imgInfo.GenerateThumbnails(data, config);
		}
	}
}