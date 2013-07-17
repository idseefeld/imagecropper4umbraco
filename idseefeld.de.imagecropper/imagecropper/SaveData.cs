using System;
using System.Collections;
using System.Xml;

namespace idseefeld.de.imagecropper.imagecropper
{
	public class SaveData
	{
		public ArrayList data { get; set; }

		public string Xml(Config config, ImageInfo imageInfo, bool ParentIsDocument)
		{
			XmlDocument doc = createBaseXmlDocument();
			XmlNode root = doc.DocumentElement;

			if (root == null) return null;

			XmlNode dateStampNode = doc.CreateNode(XmlNodeType.Attribute, "date", null);
			dateStampNode.Value = imageInfo.DateStamp.ToString("s");
			root.Attributes.SetNamedItem(dateStampNode);

			XmlNode ignoreICCNode = doc.CreateNode(XmlNodeType.Attribute, "ignoreICC", null);
			ignoreICCNode.Value = config.IgnoreICC ? "true":"false";
			root.Attributes.SetNamedItem(ignoreICCNode);

			for (int i = 0; i < data.Count; i++)
			{
				Crop crop = (Crop)data[i];
				Preset preset = (Preset)config.presets[i];

				int cWidth = crop.X2 - crop.X;
				int cHeight = crop.Y2 - crop.Y;
				int tWidth = preset.TargetWidth;
				int tHeight = preset.TargetHeight;
				if (tWidth == 0)
				{
					tWidth = tHeight * cWidth / cHeight;
				}
				else if (tHeight == 0)
				{
					tHeight = tWidth * cHeight / cWidth;
				}

				XmlNode newNode = doc.CreateElement("crop");

				XmlNode nameNode = doc.CreateNode(XmlNodeType.Attribute, "name", null);
				nameNode.Value = preset.Name;
				newNode.Attributes.SetNamedItem(nameNode);

				XmlNode xNode = doc.CreateNode(XmlNodeType.Attribute, "x", null);
				xNode.Value = crop.X.ToString();
				newNode.Attributes.SetNamedItem(xNode);

				XmlNode yNode = doc.CreateNode(XmlNodeType.Attribute, "y", null);
				yNode.Value = crop.Y.ToString();
				newNode.Attributes.SetNamedItem(yNode);

				XmlNode x2Node = doc.CreateNode(XmlNodeType.Attribute, "x2", null);
				x2Node.Value = crop.X2.ToString();
				newNode.Attributes.SetNamedItem(x2Node);

				XmlNode y2Node = doc.CreateNode(XmlNodeType.Attribute, "y2", null);
				y2Node.Value = crop.Y2.ToString();
				newNode.Attributes.SetNamedItem(y2Node);

				XmlNode widthNode = doc.CreateNode(XmlNodeType.Attribute, "width", null);
				widthNode.Value = tWidth.ToString();
				newNode.Attributes.SetNamedItem(widthNode);

				XmlNode heightNode = doc.CreateNode(XmlNodeType.Attribute, "height", null);
				heightNode.Value = tHeight.ToString();
				newNode.Attributes.SetNamedItem(heightNode);

				if (config.GenerateImages)
				{
					string extension = ImageTransform.GetAdjustedFileExtension(imageInfo.RelativePath); 
					if (extension.StartsWith("tif", StringComparison.InvariantCultureIgnoreCase))
						extension = "jpg";
					XmlNode urlNode = doc.CreateNode(XmlNodeType.Attribute, "url", null);
					string urlStr = String.Format("{0}/{1}_{2}.{3}",
							imageInfo.RelativePath.Substring(0,	imageInfo.RelativePath.LastIndexOf('/')),
							imageInfo.Name,
							preset.Name,
							extension
							);
					urlNode.Value = urlStr;
					newNode.Attributes.SetNamedItem(urlNode);

					//support preview / publish
					XmlNode newUrlNode = doc.CreateNode(XmlNodeType.Attribute, "newurl", null);
					if (ParentIsDocument)
					{
						string cropHash = String.Format("{0}{1}",
											umbraco.library.md5(
												String.Format("{0}{1}{2}{3}{4}{5}{6}",
													imageInfo.Name,
													preset.Name,
													crop.X.ToString(),
													crop.Y.ToString(),
													crop.X2.ToString(),
													crop.Y2.ToString(),
													config.IgnoreICC ? "T" : "F"
												)
											), DataType.CROP_POSTFIX);

						newUrlNode.Value = String.Format("{0}/{1}.{2}",
								imageInfo.RelativePath.Substring(0, imageInfo.RelativePath.LastIndexOf('/')),
								cropHash,
								extension
							);
						config.cropHashDict.Add(preset.Name, cropHash);
					}
					else
					{
						newUrlNode.Value = urlStr;
					}
					newNode.Attributes.SetNamedItem(newUrlNode);
				}

				root.AppendChild(newNode);
			}
			return doc.InnerXml;
		}

		public SaveData()
		{
			data = new ArrayList();
		}

		public SaveData(string raw)
		{
			data = new ArrayList();

			string[] crops = raw.Split(';');

			foreach (string crop in crops)
			{
				var val = crop.Split(',');

				data.Add(
					new Crop(
						Convert.ToInt32(val[0]),
						Convert.ToInt32(val[1]),
						Convert.ToInt32(val[2]),
						Convert.ToInt32(val[3])
						)
					);
			}

		}

		public static XmlDocument createBaseXmlDocument()
		{
			XmlDocument doc = new XmlDocument();
			XmlNode root = doc.CreateElement("crops");
			doc.AppendChild(root);
			return doc;
		}


	}
}