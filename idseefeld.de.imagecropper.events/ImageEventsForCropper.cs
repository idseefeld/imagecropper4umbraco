using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Text;
using System.Xml;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.property;
using umbraco.editorControls.uploadfield;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Services;
using Umbraco.Core.Models;

namespace idseefeld.de.imagecropper.imagecropper {
	public class ImageEventsForCropper : Umbraco.Core.ApplicationEventHandler
	{
		//ToDo: make contentType with croppers configurable

		public ImageEventsForCropper()
		{
			Member.AfterSave += new Member.SaveEventHandler(Member_AfterSave);
			//Document.BeforePublish += new Document.PublishEventHandler(Document_BeforePublish);

			MediaService.Saving += MediaService_Saving;
			ContentService.Publishing += ContentService_Publishing;
		}

		

		void MediaService_Saving(IMediaService sender, Umbraco.Core.Events.SaveEventArgs<IMedia> e)
		{
			foreach (var entity in e.SavedEntities)
			{
				bool isDirty = HandleImageCropper(entity.PropertyTypes, entity.Properties, false);

			}
		}

		void Member_AfterSave(Member sender, umbraco.cms.businesslogic.SaveEventArgs e)
		{
			HandleImageCropper(sender.GenericProperties, false);
		}

		void ContentService_Publishing(Umbraco.Core.Publishing.IPublishingStrategy sender, Umbraco.Core.Events.PublishEventArgs<IContent> e)
		{
			DataType imageCropperDt = new DataType();

			List<string> activeFiles = new List<string>();
			List<string> mediaItemDirectories = new List<string>();

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
					MediaFileSystem _fileSystem = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();
					//string mappedDirPath = IOHelper.MapPath(dir);
					foreach (var file in _fileSystem.GetFiles(dir))
					{
						if (file.Substring(0, file.LastIndexOf('.')).EndsWith(DataType.CROP_POSTFIX))
						{
							bool deleteFile = true;
							foreach (var activeFile in activeFiles)
							{
								if (file.EndsWith(activeFile))
								{
									deleteFile = false;
									break;
								}
							}
							if (deleteFile)
							{
								_fileSystem.DeleteFile(file);
							}
						}

					}
				}
			}
			catch (Exception ex)
			{
				string msg = ex.Message;
			}
		}

		private static bool HandleImageCropper(
			IEnumerable<PropertyType> propertyTypes, 
			PropertyCollection properties,
			bool parentIsDocument)
		{
			bool isDirty = false;
			DataType imageCropperDt = new DataType();
			DataTypeUploadField uploadFieldDt = new DataTypeUploadField();
			var _fileSystem = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();
			IDataTypeService ds = ApplicationContext.Current.Services.DataTypeService;
			var uploadProp = propertyTypes
				.Where(p => p.DataTypeId.Equals(uploadFieldDt.Id))
				.FirstOrDefault();
			var cropperProp = propertyTypes
				.Where(p => p.DataTypeId.Equals(imageCropperDt.Id))
				.FirstOrDefault();
			if (cropperProp == null || uploadProp == null)
				return isDirty;

			var prop = properties
				.Where(p => p.Alias.Equals(cropperProp.Alias))
				.FirstOrDefault();

			var prevalues = ds.GetPreValuesByDataTypeId(cropperProp.DataTypeDefinitionId);
			Config config = new Config(prevalues.FirstOrDefault());

			if (!uploadProp.Alias.Equals(config.UploadPropertyAlias))
				return isDirty;//I assume that only one cropper data type is defined for a media image

			var uploadFieldProp = properties
				.Where(p => p.Alias.Equals(uploadProp.Alias))
				.FirstOrDefault();

			string imgUrl = uploadFieldProp.Value != null ? uploadFieldProp.Value.ToString() : String.Empty;
			if (String.IsNullOrEmpty(imgUrl))
				return isDirty;

			string imgUrlWithoutExtension = imgUrl.Substring(0, imgUrl.LastIndexOf('.'));

			if (prop.Value == null
				|| String.IsNullOrEmpty(prop.Value.ToString())
				|| !prop.Value.ToString().Contains(imgUrlWithoutExtension + "_"))
			{
				int imgWidth = 0;
				int imgHeight = 0;
				var imgPath = _fileSystem.GetRelativePath(imgUrl);
				using (System.IO.Stream imgStream = _fileSystem.OpenFile(imgPath))
				{
					using (Bitmap img = new Bitmap(imgStream)) //using (Bitmap img = new Bitmap(HttpContext.Current.Server.MapPath(imgUrl)))
					{
						imgWidth = img.Width;
						imgHeight = img.Height;
					}
				}
				if (imgWidth == 0)
					return isDirty;

				var presets = config.presets;
				//see: idseefeld.de.imagecropper.imagecropper.DataEditor.Save() for how to setup cropper default property.Value
				StringBuilder sbRaw = new StringBuilder();
				ImageInfo imageInfo = new ImageInfo(imgUrl, config, parentIsDocument);
				int cropIndex = 1;
				foreach (var presetConfig in presets)
				{
					Preset preset = (Preset)presetConfig;
					Crop crop;
					if (imageInfo.Exists)
					{
						crop = preset.Fit(imageInfo);
					}
					else
					{
						crop.X = 0;
						crop.Y = 0;
						crop.X2 = preset.TargetWidth;
						crop.Y2 = preset.TargetHeight;
					}
					sbRaw.Append(String.Format("{0},{1},{2},{3}", crop.X, crop.Y, crop.X2, crop.Y2));
					if (cropIndex < presets.Count)
					{
						sbRaw.Append(";");
					}
					if (config.GenerateImages)
					{
						GenerateCrop(
							preset,
							imgUrl,
							config,
							parentIsDocument,
							prop.Value == null ? String.Empty : prop.Value.ToString(),
							cropIndex - 1
						);
					}
					cropIndex++;
				}
				SaveData saveData = new SaveData(sbRaw.ToString());
				//save cropper default property.Value
				prop.Value = saveData.Xml(config, imageInfo, parentIsDocument);
				isDirty = true;
			}
			return isDirty;
		}


		private static void HandleImageCropper(Properties properties, bool parentIsDocument)
		{
			DataType imageCropperDt = new DataType();
			DataTypeUploadField uploadFieldDt = new DataTypeUploadField();
			var _fileSystem = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();

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
						var imgPath = _fileSystem.GetRelativePath(imgUrl);
						using (System.IO.Stream imgStream = _fileSystem.OpenFile(imgPath))
						{
							using (Bitmap img = new Bitmap(imgStream)) //using (Bitmap img = new Bitmap(HttpContext.Current.Server.MapPath(imgUrl)))
							{
								imgWidth = img.Width;
								imgHeight = img.Height;
							}
						}
						if (imgWidth == 0)
							continue;

						var presets = config.presets;
						//see: idseefeld.de.imagecropper.imagecropper.DataEditor.Save() for how to setup cropper default property.Value
						StringBuilder sbRaw = new StringBuilder();
						ImageInfo imageInfo = new ImageInfo(imgUrl, config, parentIsDocument);
						int cropIndex = 1;
						foreach (var presetConfig in presets)
						{
							Preset preset = (Preset)presetConfig;
							Crop crop;
							if (imageInfo.Exists)
							{
								crop = preset.Fit(imageInfo);
							}
							else
							{
								crop.X = 0;
								crop.Y = 0;
								crop.X2 = preset.TargetWidth;
								crop.Y2 = preset.TargetHeight;
							}
							sbRaw.Append(String.Format("{0},{1},{2},{3}", crop.X, crop.Y, crop.X2, crop.Y2));
							if (cropIndex < presets.Count)
							{
								sbRaw.Append(";");
							}
							if (config.GenerateImages)
							{
								GenerateCrop(
									preset,
									imgUrl,
									config,
									parentIsDocument,
									prop.Value == null ? String.Empty : prop.Value.ToString(),
									cropIndex - 1
								);
							}
							cropIndex++;
						}
						SaveData saveData = new SaveData(sbRaw.ToString());
						//save cropper default property.Value
						prop.Value = saveData.Xml(config, imageInfo, parentIsDocument);
					}
				}
			}
		}

		private static void GenerateCrop(Preset preset, string imgUrl, Config config, bool parentIsDocument, string savedValues, int cropIndex)
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
			imgInfo.GenerateThumbnails(data, config, cropIndex);
		}
	}
}