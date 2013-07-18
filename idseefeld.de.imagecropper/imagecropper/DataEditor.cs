using System;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.property;
using umbraco.cms.businesslogic.web;
using Content = umbraco.cms.businesslogic.Content;
using System.IO;
using System.Web.Hosting;
using Umbraco.Core.IO;
using umbraco.editorControls;

[assembly: WebResource("idseefeld.de.imagecropper.imagecropper.Resources.imageCropperScript.js", "text/javascript")]
[assembly: WebResource("idseefeld.de.imagecropper.imagecropper.Resources.json2.js", "text/javascript")]
[assembly: WebResource("idseefeld.de.imagecropper.imagecropper.Resources.jCropScript.js", "text/javascript")]
[assembly: WebResource("idseefeld.de.imagecropper.imagecropper.Resources.jCropCSS.css", "text/css")]
[assembly: WebResource("idseefeld.de.imagecropper.imagecropper.Resources.Jcrop.gif", "image/gif")]
namespace idseefeld.de.imagecropper.imagecropper
{
	public class DataEditor : PlaceHolder, umbraco.interfaces.IDataEditor
	{
		private readonly bool autoUpdateChangedWidth = false;

		private umbraco.interfaces.IData data;
		private Config config;
		private XmlDocument _xml;
		private bool ParentIsDocument;

		protected HiddenField cropImagePathHidden = new HiddenField();
		protected HiddenField imagePathHidden = new HiddenField();
		protected HiddenField reziseWidthHidden = new HiddenField();
		protected Panel cropperUpdatePanel = new Panel();
		protected Button cropperUpdateButton = new Button();
		protected Label cropperUpdateLabel = new Label();

		protected CheckBox chkIgnoreICC  = new CheckBox ();
		protected Label labelIgnoreICC = new Label ();

		public Image imgImage = new Image();
		public HiddenField hdnJson = new HiddenField();
		public HiddenField hdnRaw = new HiddenField();
		public HiddenField hdnSer = new HiddenField();

		public DataEditor(umbraco.interfaces.IData Data, string Configuration)
		{
			data = Data;
			config = new Config(Configuration);
		}

		public virtual bool TreatAsRichTextEditor { get { return false; } }

		public bool ShowLabel { get { return config.ShowLabel; } }

		public Control Editor { get { return this; } }

		protected override void OnInit(EventArgs e)
		{
			InitCropper();
			base.OnInit(e);
		}
		private string GetDataTypeName()
		{
			int dtdId = ((umbraco.cms.businesslogic.datatype.DefaultData)this.data).DataTypeDefinitionId;
			umbraco.cms.businesslogic.datatype.DataTypeDefinition dtd = new umbraco.cms.businesslogic.datatype.DataTypeDefinition(dtdId);
			return dtd.Text;
		}
		protected void InitCropper()
		{
			var uploadControl = PropertyHelper.FindControlRecursive<uploadField>(Page, String.Format("prop_{0}", this.config.UploadPropertyAlias));

			if (uploadControl == null)
			{	
				//ToDo: get this message from /umbraco/config/lang *.xml
				cropperUpdateLabel.Text = String.Format("Please check the UploadPropertyAlias for data type \"{0}\". No upload with alias \"{1}\" is available.<br />Make also sure the related upload field is sorted before this cropper.", GetDataTypeName(), this.config.UploadPropertyAlias);
				cropperUpdateLabel.ForeColor = System.Drawing.Color.Red;
				cropperUpdatePanel.Controls.Add(cropperUpdateLabel);
				Controls.Add(cropperUpdatePanel);
				return;
			}
			if (config.presets.Count == 0)
			{
				//ToDo: get this message from /umbraco/config/lang *.xml	
				cropperUpdateLabel.Text = String.Format("There are no crops defined for this cropper.<br />Please check settings for data type \"{0}\".", GetDataTypeName());
				cropperUpdateLabel.ForeColor = System.Drawing.Color.Red;
				cropperUpdatePanel.Controls.Add(cropperUpdateLabel);
				Controls.Add(cropperUpdatePanel);
				return;
			}

			string relativeImagePath = ImageTransform.FixBrowserUnsupportedFormats(
				uploadControl.Text,
				config.ResizeEngine);

			string uploadControlClientID = uploadControl.ClientID;
			string itemId = Page.Request.QueryString["id"];

			ParentIsDocument = Helper.isParentDocumentType();

			int propertyId = ((umbraco.cms.businesslogic.datatype.DefaultData)data).PropertyId;

			int currentDocumentId = ((umbraco.cms.businesslogic.datatype.DefaultData)data).NodeId;

			ImageInfo imageInfo = new ImageInfo(relativeImagePath, config, ParentIsDocument);

			cropperUpdatePanel.Visible = false;
			if (!autoUpdateChangedWidth
				&& config.ResizeMax != imageInfo.Width
				&& relativeImagePath != imageInfo.RelativePath)
			{
				cropperUpdateLabel.Text = "The width setting of this cropper has changed.";
				cropperUpdateLabel.ForeColor = System.Drawing.Color.Red;
				cropperUpdateButton.ForeColor = System.Drawing.Color.Red;
				cropperUpdateButton.Text = "update";
				cropperUpdateButton.Click += new EventHandler(updateButton_Click);
				cropperUpdatePanel.BackColor = System.Drawing.Color.FromArgb(255, 200, 200);
				cropperUpdatePanel.Visible = true;
			}
			cropperUpdatePanel.Controls.Add(cropperUpdateLabel);
			cropperUpdatePanel.Controls.Add(cropperUpdateButton);
			Controls.Add(cropperUpdatePanel);
			cropImagePathHidden.Value = relativeImagePath;
			Controls.Add(cropImagePathHidden);
			imagePathHidden.Value = imageInfo.RelativePath;
			Controls.Add(imagePathHidden);
			reziseWidthHidden.Value = config.ResizeMax.ToString();
			Controls.Add(reziseWidthHidden);

			Controls.Add(imgImage);
			Panel featurePanel = new Panel();
			featurePanel.CssClass = "cropperFeatures";

			labelIgnoreICC.Text = "ignore ICC";
			chkIgnoreICC.Visible = config.ShowIgnoreICC;
			chkIgnoreICC.ID = ClientID + "_chkIgnoreICC";
			chkIgnoreICC.ClientIDMode = System.Web.UI.ClientIDMode.Static;
			labelIgnoreICC.AssociatedControlID = ClientID + "_chkIgnoreICC"; 
			labelIgnoreICC.Visible = config.ShowIgnoreICC;
			featurePanel.Controls.Add(chkIgnoreICC);
			featurePanel.Controls.Add(labelIgnoreICC);


			Panel cropSelectorPanel = new Panel();
			cropSelectorPanel.CssClass = "cropSelector";
			cropSelectorPanel.Controls.Add(hdnJson);
			cropSelectorPanel.Controls.Add(hdnRaw);
			featurePanel.Controls.Add(cropSelectorPanel);

			Controls.Add(featurePanel);

			UpdateCropper(relativeImagePath);

			Page.ClientScript.RegisterClientScriptBlock(GetType(), "emptyImageCss", "<style type='text/css'>img[src='']{display:none;}</style>", false);

		}

		void updateButton_Click(object sender, EventArgs e)
		{
			ImageInfo imageInfo = new ImageInfo(cropImagePathHidden.Value, config, ParentIsDocument);
			int rWidth = 0;
			int.TryParse(reziseWidthHidden.Value, out rWidth);
			ImageTools iTools = new ImageTools();
			iTools.GenerateImageByWidth(rWidth, cropImagePathHidden.Value, imagePathHidden.Value, config.IgnoreICC, config.ResizeEngine);
			cropperUpdatePanel.Visible = false;
			data.Value = "";
			UpdateCropper(cropImagePathHidden.Value);
		}
		public void UpdateCropper()
		{
			UpdateCropper(false, "");
		}
		public void UpdateCropper(bool isDelete, string uploadImagePath)
		{
			int currentDocumentId = ((umbraco.cms.businesslogic.datatype.DefaultData)data).NodeId;

			var uploadControl = PropertyHelper.FindControlRecursive<uploadField>(Page, String.Format("prop_{0}", this.config.UploadPropertyAlias));
			if (uploadControl == null)
			{
				return;
			}
			string relativeImagePath = ImageTransform.FixBrowserUnsupportedFormats(uploadControl.Text, config.ResizeEngine);


			if (isDelete && uploadImagePath.Equals(relativeImagePath))
			{
				UpdateCropper("");
			}
			else
			{
				UpdateCropper(relativeImagePath);
			}
		}
		public void UpdateCropper(string relativeImagePath)
		{
			int propertyId = ((umbraco.cms.businesslogic.datatype.DefaultData)data).PropertyId;
			int currentDocumentId = ((umbraco.cms.businesslogic.datatype.DefaultData)data).NodeId;

			UpdateCropper(relativeImagePath, propertyId, currentDocumentId);
		}
		public void UpdateCropper(string relativeImagePath, int propertyId, int currentDocumentId)
		{
			if (String.IsNullOrEmpty(relativeImagePath))
			{
				imgImage.ImageUrl = "";
				this.Visible = false;
				return;
			}
			this.Visible = true;
			ImageInfo imageInfo = new ImageInfo(relativeImagePath, config, ParentIsDocument);

			if (autoUpdateChangedWidth
				&& config.ResizeMax != imageInfo.Width)
			{
				ImageTools iTools = new ImageTools();
				iTools.GenerateImageByWidth(config.ResizeMax,
					relativeImagePath,
					imageInfo.RelativePath,
					config.IgnoreICC,
					config.ResizeEngine);
				imageInfo = new ImageInfo(relativeImagePath, config, ParentIsDocument);
			}

			imgImage.ImageUrl = imageInfo.RelativePath;// relativeImagePath;

			imgImage.ID = String.Format("cropBox_{0}", propertyId);

			StringBuilder sbJson = new StringBuilder();
			StringBuilder sbRaw = new StringBuilder();

			try
			{
				_xml = new XmlDocument();
				string oldValue = data.Value.ToString();
				if (oldValue.Contains(String.Format("{0}_", imageInfo.Name)))
				{
					_xml.LoadXml(oldValue);
				}
				else
				{
					_xml = SaveData.createBaseXmlDocument();
				}
			}
			catch
			{
				_xml = SaveData.createBaseXmlDocument();
			}

			sbJson.Append("{ \"current\": 0, \"crops\": [");

			ConfigurateCrops(config, _xml, imageInfo, sbJson, sbRaw);

			chkIgnoreICC.Checked = config.IgnoreICC;

			sbJson.Append("]}");

			hdnJson.Value = sbJson.ToString();
			hdnRaw.Value = sbRaw.ToString();

			string imageCropperInitScript =
				"initImageCropper('" +
				imgImage.ClientID + "', '" +
				hdnJson.ClientID + "', '" +
				hdnRaw.ClientID +
				"');";

			Controls.Add(new LiteralControl(string.Format(
@"<style type='text/css'>
    @import url('{0}');
    .jcrop-vline, .jcrop-hline {{ background-image: url('{1}'); }}
</style>",
				Page.ClientScript.GetWebResourceUrl(typeof(DataEditor), "idseefeld.de.imagecropper.imagecropper.Resources.jCropCSS.css"),
				Page.ClientScript.GetWebResourceUrl(typeof(DataEditor), "idseefeld.de.imagecropper.imagecropper.Resources.Jcrop.gif")
				)));


			Page.ClientScript.RegisterClientScriptResource(typeof(DataEditor), "idseefeld.de.imagecropper.imagecropper.Resources.json2.js");
			Page.ClientScript.RegisterClientScriptResource(typeof(DataEditor), "idseefeld.de.imagecropper.imagecropper.Resources.jCropScript.js");
			Page.ClientScript.RegisterClientScriptResource(typeof(DataEditor), "idseefeld.de.imagecropper.imagecropper.Resources.imageCropperScript.js");

			Page.ClientScript.RegisterStartupScript(GetType(), ClientID + "_imageCropper", imageCropperInitScript, true);

		}

		public static void ConfigurateCrops(Config config, XmlDocument xml, ImageInfo imageInfo, StringBuilder sbJson, StringBuilder sbRaw)
		{
			if (config.ShowIgnoreICC && xml.DocumentElement != null)
			{
				bool ignoreICC = true;
				if (xml.DocumentElement.Attributes["ignoreICC"] != null && bool.TryParse(xml.DocumentElement.Attributes["ignoreICC"].Value, out ignoreICC))
				{
					config.IgnoreICC = ignoreICC;
				}
			}
			for (int i = 0; i < config.presets.Count; i++)
			{
				Preset preset = (Preset)config.presets[i];
				Crop crop;

				sbJson.Append("{\"name\":'" + preset.Name + "'");

				bool keepAspect = preset.KeepAspect && preset.TargetWidth > 0 && preset.TargetHeight > 0;
				sbJson.Append(",\"config\":{" +
							  String.Format("\"targetWidth\":{0},\"targetHeight\":{1},\"keepAspect\":{2}",
											preset.TargetWidth, preset.TargetHeight,
											(keepAspect ? "true" : "false") + "}"));

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

				// stored
				if (xml.DocumentElement != null && xml.DocumentElement.ChildNodes.Count == config.presets.Count)
				{
					XmlNode xmlNode = xml.DocumentElement.ChildNodes[i];

					string newUrl = xmlNode.Attributes["newUrl"] != null
						? xmlNode.Attributes["newUrl"].Value : String.Empty;
					if (!String.IsNullOrEmpty(newUrl))
					{
						config.versionedCropFiles.Add(newUrl);
					}
					int xml_x = Convert.ToInt32(xmlNode.Attributes["x"].Value);
					int xml_y = Convert.ToInt32(xmlNode.Attributes["y"].Value);
					int xml_x2 = Convert.ToInt32(xmlNode.Attributes["x2"].Value);
					int xml_y2 = Convert.ToInt32(xmlNode.Attributes["y2"].Value);

					DateTime fileDate = Convert.ToDateTime(xml.DocumentElement.Attributes["date"].Value);

					if (crop.X != xml_x || crop.X2 != xml_x2 || crop.Y != xml_y || crop.Y2 != xml_y2)
					{
						crop.X = xml_x;
						crop.Y = xml_y;
						crop.X2 = xml_x2;
						crop.Y2 = xml_y2;
					}
				}

				sbJson.Append(",\"value\":{" + String.Format("\"x\":{0},\"y\":{1},\"x2\":{2},\"y2\":{3}", crop.X, crop.Y, crop.X2, crop.Y2) + "}}");
				sbRaw.Append(String.Format("{0},{1},{2},{3}", crop.X, crop.Y, crop.X2, crop.Y2));

				if (i < config.presets.Count - 1)
				{
					sbJson.Append(",");
					sbRaw.Append(";");
				}
			}
		}

		public void Save()
		{
			var uploadControl = PropertyHelper.FindControlRecursive<uploadField>(
				Page,
				String.Format("prop_{0}", this.config.UploadPropertyAlias));

			//reset existing crops definitions after changed image upload
			string fixedUrl = ImageTransform.FixBrowserUnsupportedFormats(uploadControl.Text, config.ResizeEngine);
			if (String.IsNullOrEmpty(imgImage.ImageUrl) ||
				!imgImage.ImageUrl.Equals(fixedUrl, StringComparison.InvariantCultureIgnoreCase))
			{
				imgImage.ImageUrl = fixedUrl;
				UpdateCropper(imgImage.ImageUrl);
			}
			ImageInfo imageInfo = new ImageInfo(imgImage.ImageUrl, config, ParentIsDocument);
			if (!imageInfo.Exists)
			{
				data.Value = "";
			}
			else
			{
				if (config.ShowIgnoreICC)
				{
					config.IgnoreICC = chkIgnoreICC.Checked;
				}

				SaveData saveData = new SaveData(hdnRaw.Value);
				data.Value = saveData.Xml(config, imageInfo, ParentIsDocument);
				imageInfo.GenerateThumbnails(saveData, config);
			}
		}
	}
}
