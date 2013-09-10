using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.XPath;
using idseefeld.de.imagecropper.imagecropper;
using umbraco.BusinessLogic;

namespace idseefeld.de.imagecropper.Model {
	/// <summary>
	/// Model for a certain Image Cropper Extended defined crop.
	/// </summary>
	public class CropModel {
		/// <summary>
		/// Width of crop image.
		/// </summary>
		public int Width { get; private set; }
		/// <summary>
		/// Height of crop image.
		/// </summary>
		public int Height { get; private set; }
		/// <summary>
		/// Coordinates of crop area base on the original image.
		/// </summary>
		public Crop Crop { get; private set; }
		/// <summary>
		/// Crop Image url with hashed name for preview function on content docuemnt types.
		/// </summary>
		public string NewUrl { get; private set; }
		/// <summary>
		/// Url of crop image.
		/// </summary>
		public string Url { get; private set; }
		/// <summary>
		/// Name of the crop definition.
		/// </summary>
		public string Name { get; private set; }
		/// <summary>
		/// Model for a certain Image Cropper Extended defined crop.
		/// </summary>
		/// <param name="crop">crop xml</param>
		public CropModel(XmlNode crop)
		{
			if (crop.Attributes["url"] == null || crop.Attributes["newurl"] == null)
			{
				NewUrl = Url = String.Empty;
			}
			else
			{
				NewUrl = crop.Attributes["newurl"].Value;
				Url = crop.Attributes["url"].Value;
			}
			Name = crop.Attributes["name"].Value;
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
	/// <summary>
	/// This is model for an Image Cropper Extended property editor value.
	/// The Crops property of this model is of type a List&lt;CropModel&gt;.
	/// The Find(cropName) method selects a certain crop by name.
	/// </summary>
	public class ImageCropperModel {
		XmlDocument _data = new XmlDocument();
		/// <summary>
		/// List of CropModel types (all defined crops of certain property editor).
		/// </summary>
		public List<CropModel> Crops { get; private set; }

		/// <summary>
		/// Model for Image Cropper Extended property editor value. If you specify a cropperName the model will chached for the current request. 
		/// </summary>
		/// <param name="propertyValue">The property value (Xml).</param>
		/// <param name="cropperName">[optional] if provided, this will be used as cache key for request scope.</param>
		public ImageCropperModel(string propertyValue, string cropperName = "")
		{
			if (String.IsNullOrEmpty(cropperName))
			{
				Initialise(propertyValue);
				return;
			}

			object cache = HttpContext.Current.Items[cropperName];
			if (cache != null)
			{
				Crops = (List<CropModel>)HttpContext.Current.Items[cropperName];
			}
			else
			{
				Initialise(propertyValue);
				HttpContext.Current.Items[cropperName] = Crops;
			}
		}
		/// <summary>
		/// Finds a certain crop by its name.
		/// </summary>
		/// <param name="name">Name of a certain crop</param>
		/// <returns>CropModel</returns>
		public CropModel Find(string name)
		{
			if (this.Crops != null)
				return this.Crops.Where(n => n.Name.Equals(name)).FirstOrDefault();
			else
				return null;
		}

		private void Initialise(string propertyValue)
		{
			try
			{
				_data.LoadXml(propertyValue);

				XmlNodeList nodes = _data.DocumentElement.SelectNodes("crop");
				if (nodes.Count > 0)
				{
					Crops = new List<CropModel>();
					foreach (XmlNode node in nodes)
					{
						CropModel crop = new CropModel(node);
						Crops.Add(crop);
					}
				}
			}
			catch (Exception ex)
			{
				//for version 6.x only
				//Umbraco.Core.Logging.LogHelper.Error<ImageCropperModel>(
				//    String.Format("ImageCropperModel could be initialisied from property value. Exception message: {0}",
				//        ex.Message), 
				//        ex);

				//for version 4.11.x and higher
				Log.Add(LogTypes.Error, -1,
					String.Format("ImageCropperModel could not be initialisied from property value. Exception message: {0}",
						ex.Message));
			}
		}
	}
}
