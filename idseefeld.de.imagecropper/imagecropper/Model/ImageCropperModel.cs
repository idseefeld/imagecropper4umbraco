using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using idseefeld.de.imagecropper.imagecropper;

namespace idseefeld.de.imagecropper.Model {
	//<crop name="fixWidth" x="0" y="441" x2="2815" y2="1475" width="300" height="110" url="/media/1001/p1010800_fixWidth.jpg" newurl="/media/1001/cda045406bf121386045cc780c9a6ac3_HASHEDCROP.jpg" />
	public class CropModel {
		public int Width { get; private set; }
		public int Height { get; private set; }
		public Crop Crop { get; private set; }
		public string NewUrl { get; private set; }
		public string Url { get; private set; }
		public string Name { get; private set; }

		public CropModel(XmlNode crop)
		{
			Name = crop.Attributes["name"].Value;
			NewUrl = crop.Attributes["newurl"].Value;
			Url = crop.Attributes["url"].Value;
			Width = NumericAttribute(crop, "width");
			Height = NumericAttribute(crop, "height");
			Crop = new Crop(
				NumericAttribute(crop, "x"),
				NumericAttribute(crop, "y"),
				NumericAttribute(crop, "x2"),
				NumericAttribute(crop, "y2")
			);
		}
		private int NumericAttribute(XmlNode node, string attributeName)
		{
			int rVal = 0;
			int.TryParse(node.Attributes[attributeName].Value, out rVal);
			return rVal;
		}
	}

	public class ImageCropperModel {
		XmlDocument _data = new XmlDocument();

		public List<CropModel> Crops { get; private set; }

		public ImageCropperModel(string propertyValue)
		{
			_data.LoadXml(propertyValue);
			XmlNodeList nodes = _data.DocumentElement.SelectNodes("crop");
			if (nodes.Count > 0)
			{
				Crops = new List<CropModel>();
				foreach (XmlNode node in nodes)
				{
					Crops.Add(new CropModel(node));
				}
			}
		}
	}
	public static class ImageCropperModelExtentions {
		public static CropModel Find(this ImageCropperModel model, string name)
		{
			return model.Crops.Where(n => n.Name.Equals(name)).FirstOrDefault();
		}
	}
}
