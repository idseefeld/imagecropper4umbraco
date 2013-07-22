using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using ImageResizer;
using umbraco.BusinessLogic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using Umbraco.Core.Media;

namespace idseefeld.de.imagecropper.imagecropper {
	public class ImageEngineImageResizer : PersitenceFactory, IImageResizeEngine {
		Control _featureControls;

		string[] _prevalues;

		TextBox tbxRotation = new TextBox { ID = "RotationInput" };
		Label lblRotation = new Label { ID = "RotationLabel" };
		CheckBox chkFlip = new CheckBox { ID = "FlipInput" };
		Label lblFlip = new Label { ID = "FlipLabel" };
		TextBox tbxSharpenRadius = new TextBox { ID = "sharpenRadiusInput" };
		Label lblSharpenRadius = new Label { ID = "sharpenRadiusLabel" };

		CheckBox chkShowPluginSettings = new CheckBox { ID = "ShowPluginSettingsInput" };
		Label lblShowPluginSettings = new Label { ID = "ShowPluginSettingsLabel", Text = "show ImageResizer plugin settings" };

		Panel pluginPrevalueSettings = new Panel { ID = "pluginPrevalueSettings" };

		public Control PluginPrevalueSettings()
		{
			lblShowPluginSettings.AssociatedControlID = chkShowPluginSettings.ID;
			pluginPrevalueSettings.Controls.Add(chkShowPluginSettings);
			pluginPrevalueSettings.Controls.Add(lblShowPluginSettings);
			return pluginPrevalueSettings;
		}
		public void RenderPrevalueSettings(HtmlTextWriter writer)
		{
			writer.Write("<p><strong>ImageResizer plugin settings</strong></p>");

			writer.Write("<table>");
			writer.Write("  <tr><td valign=\"top\">");

			pluginPrevalueSettings.RenderControl(writer);

			writer.Write("  </td></tr>");
			writer.Write("</table>");

		}
		public void SetPrevalues(string values)
		{
			if (_prevalues != null)
				return;

			_prevalues = values.Split(',');
			if (_prevalues.Length > 0)
				chkShowPluginSettings.Checked = _prevalues[0] == "1";
		}
		public string GetPrevalues(Control placeholder)
		{
			Control ctl = placeholder.FindControl(chkShowPluginSettings.ID);
			bool showPluginSettings = false;
			if (ctl != null)
			{
				showPluginSettings = ((CheckBox)ctl).Checked;
			}

			return String.Format("{0},{1},{2}",
				showPluginSettings ? "1" : "0",
				"0",
				"0");
		}


		public string GetExtraHash()
		{
			string extraHash = String.Format("r{0}fH{1}sr{2}",
				Features.Rotation,
				Features.Flip == RotateFlipType.RotateNoneFlipX ? "1" : "0",
				Features.SharpenRadius);
			return extraHash;
		}
		public void Load(XmlDocument xml)
		{
			if (xml.DocumentElement.Attributes["rotation"] != null)
			{
				double rotation = 0.0;
				if (double.TryParse(xml.DocumentElement.Attributes["rotation"].Value, out rotation))
				{
					tbxRotation.Text = xml.DocumentElement.Attributes["rotation"].Value;
					Features.Rotation = rotation;
				}
			}
			if (xml.DocumentElement.Attributes["flipH"] != null)
			{
				bool flipH = false;
				if (bool.TryParse(xml.DocumentElement.Attributes["flipH"].Value, out flipH))
				{
					Features.Flip = flipH ? RotateFlipType.RotateNoneFlipX : RotateFlipType.RotateNoneFlipNone;
					chkFlip.Checked = flipH;
				}
			}
			if (xml.DocumentElement.Attributes["sharpen"] != null)
			{
				int sharpen = 0;
				if (int.TryParse(xml.DocumentElement.Attributes["sharpen"].Value, out sharpen))
				{
					tbxSharpenRadius.Text = xml.DocumentElement.Attributes["sharpen"].Value;
					Features.SharpenRadius = sharpen;
				}
			}
		}
		public void GetExtraXml(ref XmlDocument doc, ref XmlNode root)
		{
			if (!String.IsNullOrEmpty(tbxRotation.Text))
			{
				XmlNode rotationNode = doc.CreateNode(XmlNodeType.Attribute, "rotation", null);
				rotationNode.Value = tbxRotation.Text;
				root.Attributes.SetNamedItem(rotationNode);
			}
			if (chkFlip.Checked)
			{
				XmlNode flipHNode = doc.CreateNode(XmlNodeType.Attribute, "flipH", null);
				flipHNode.Value = chkFlip.Checked ? "true" : "false";
				root.Attributes.SetNamedItem(flipHNode);
			}
			if (!String.IsNullOrEmpty(tbxSharpenRadius.Text))
			{
				XmlNode sharpenNode = doc.CreateNode(XmlNodeType.Attribute, "sharpen", null);
				sharpenNode.Value = tbxSharpenRadius.Text;
				root.Attributes.SetNamedItem(sharpenNode);
			}
		}


		public Control GetControls()
		{
			if (!chkShowPluginSettings.Checked)
				return null;

			if (_featureControls == null)
			{
				Panel featurePanel = new Panel();

				//if (config.ImgEngFeatures != null)
				//{
				lblFlip.AssociatedControlID = chkFlip.ID;
				lblFlip.Text = "flip horizontal";
				featurePanel.Controls.Add(chkFlip);
				featurePanel.Controls.Add(lblFlip);

				lblRotation.AssociatedControlID = tbxRotation.ID;
				lblRotation.Text = "rotation";
				featurePanel.Controls.Add(tbxRotation);
				featurePanel.Controls.Add(lblRotation);

				Panel pluginPanel = new Panel();
				pluginPanel.CssClass = "cropperPlugins";
				//check for ImageResizer advanced filters ?
				//if (config.ImgEngFeatures.AdvancedFiltersInstalled)
				//{
				lblSharpenRadius.AssociatedControlID = tbxSharpenRadius.ID;
				lblSharpenRadius.Text = "sharpen radius:";
				pluginPanel.Controls.Add(lblSharpenRadius);
				pluginPanel.Controls.Add(tbxSharpenRadius);

				featurePanel.Controls.Add(pluginPanel);
				//}
				//}
				_featureControls = featurePanel;
			}

			return _featureControls;
		}
		ImageResizerFeatures _features;
		ImageResizerFeatures Features
		{
			get
			{
				return ReadFeatureSettings();
			}
			set { }
		}
		ImageResizerFeatures ReadFeatureSettings()
		{
			if (_features == null)
			{
				_features = new ImageResizerFeatures();

				double rotation = 0;
				if (double.TryParse(tbxRotation.Text, out rotation))
				{
					_features.Rotation = rotation;
				}
				int sharpenRadius = 0;
				if (int.TryParse(tbxSharpenRadius.Text, out sharpenRadius))
				{
					_features.SharpenRadius = sharpenRadius;
				}
				if (chkFlip.Checked)
				{
					_features.Flip = RotateFlipType.RotateNoneFlipX;
				}
				//TODO: check
				_features.AdvancedFiltersInstalled = CheckAdvancedFiltersInstalled();
			}
			return _features;
		}
		private bool CheckAdvancedFiltersInstalled()
		{
			//TODO: check advanced filters are installed
			return true;
		}
		public bool saveNewImageSize(string imgPath, string fileExtension, string newPath)
		{
			int newWidth = 0;
			int oldWidth = 0;
			bool forceResize = true;
			bool ignoreICC = true;
			using (Bitmap bm = new Bitmap(imgPath))
			{
				newWidth = oldWidth = bm.Width;
			}
			return saveNewImageSize(imgPath, fileExtension, newPath, newWidth, forceResize, oldWidth, ignoreICC);
		}
		public bool saveNewImageSize(string imgPath, string fileExtension, string newPath, int newWidth, bool forceResize, int oldWidth, bool ignoreICC)
		{
			return saveCroppedNewImageSize(imgPath, fileExtension, newPath, newWidth, forceResize, oldWidth,
				0, 0, 0, 0, 0, 100, ignoreICC);
		}

		public bool saveCroppedNewImageSize(
			string imgPath,
			string fileExtension, string newPath,
			int newWidth, bool forceResize, int oldWidth,
			int sizeHeight,
			int cropX, int cropY, int cropWidth, int cropHeight,
			int quality, bool ignoreICC)
		{
			//ToDo: implement new feature setup
			ImageResizerFeatures irFeatures = ReadFeatureSettings();

			if (irFeatures == null)
			{
				return false;
			}
			else
			{
				try
				{
					using (Bitmap orig = new Bitmap(imgPath))
					{
						int newHeight;
						if (forceResize || orig.Width > newWidth)
						{
							newHeight = (int)Math.Round(((double)newWidth) / orig.Width * orig.Height);
							string settings = String.Empty;
							ResizeSettings resizeSettings = new ResizeSettings();
							int cW = newWidth;
							int cH = newHeight;
							int oW = orig.Width;
							int oH = orig.Height;
							if (cropWidth > 0)
							{
								cW = cropWidth;
								cH = cropHeight;
								newHeight = sizeHeight;

								PointF p = new PointF((float)(cropX + cW), (float)(cropY + cH));
								resizeSettings.CropBottomRight = p;
								p = new PointF((float)cropX, (float)cropY);
								resizeSettings.CropTopLeft = p;
								resizeSettings.CropXUnits = oW;
								resizeSettings.CropYUnits = oH;
								resizeSettings.MaxWidth = newWidth;
								resizeSettings.MaxHeight = newHeight;
							}
							else
							{
								resizeSettings.Width = newWidth;
							}
							resizeSettings.Mode = FitMode.Stretch;
							resizeSettings.Scale = ScaleMode.Both;
							resizeSettings.Quality = quality;
							resizeSettings.Add("ignoreicc", ignoreICC ? "true" : "false");
							//ImageResizer only features
							if (irFeatures != null)
							{
								if (irFeatures.SourceFlip != null)
									resizeSettings.SourceFlip = irFeatures.SourceFlip;
								if (irFeatures.Flip != null)
									resizeSettings.Flip = irFeatures.Flip;
								if (irFeatures.Rotation != 0)
									resizeSettings.Rotate = irFeatures.Rotation;
							}
							Stream sourceStream = _fileSystem.OpenFile(imgPath);
							Stream destinationStream = new MemoryStream();
							ImageJob irJob = new ImageJob(sourceStream, destinationStream, resizeSettings);
							ImageBuilder.Current.Build(irJob);
							_fileSystem.AddFile(newPath, destinationStream, true);
						}
						else
						{
							_fileSystem.AddFile(newPath, _fileSystem.OpenFile(imgPath), true);
						}
					}
					return true;
				}
				catch (Exception ex)
				{
					Log.Add(LogTypes.Error, -1,
						String.Format("ImageHelper could not resize the image {0}. Details: {1}",
							imgPath, ex.Message));
					return false;
				}

			}
		}
	}

}
