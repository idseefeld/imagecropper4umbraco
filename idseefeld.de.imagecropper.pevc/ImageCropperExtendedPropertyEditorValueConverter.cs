using System;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web;
using idseefeld.de.imagecropper.Model;
using idseefeld.de.imagecropper.imagecropper;
using System.Text;
using System.Xml;

namespace idseefeld.de.imagecropper.PropertyEditorValueConverter {
	public class ImageCropperExtendedPropertyEditorValueConverter : IPropertyEditorValueConverter {
		/// <summary>
		/// PEVC for Image Cropper Extended
		/// </summary>
		/// <param name="propertyEditorId">Image Cropper Extended Guid</param>
		/// <param name="docTypeAlias">not used</param>
		/// <param name="propertyTypeAlias">not used</param>
		/// <returns></returns>
		public bool IsConverterFor(Guid propertyEditorId, string docTypeAlias, string propertyTypeAlias)
		{
			return ImageCropperBase.PropertyEditorId.Equals(propertyEditorId);
		}
		/// <summary>
		/// Converts the property value into strongly typed CropList (List&lt;CropModel&gt;).
		/// </summary>
		/// <param name="value">Property value</param>
		/// <returns>CropList</returns>
		public Attempt<object> ConvertPropertyValue(object value)
		{
			if (UmbracoContext.Current != null)
			{
				try
				{
					ImageCropperModel imageCropperExtendedContent = new ImageCropperModel(value.ToString());
					CropList result = new CropList(imageCropperExtendedContent, value.ToString());
					return new Attempt<object>(true, result);
				}
				catch { }
			}
			return Attempt<object>.False;
		}
	}
	/// <summary>
	/// Image Cropper Model
	/// </summary>
	public class CropList : List<CropModel> {
		private string PropertyValue { get; set; }
 		/// <summary>
 		/// List of all crops as definded in the data type.
 		/// </summary>
 		/// <param name="model">ImageCropperModel (strongly typed model fro Image Cropper Extended data types aka property editor)</param>
 		/// <param name="propertyValue">original property value for legacy support</param>
		public CropList(ImageCropperModel model, string propertyValue)
		{
			if (model == null)
				return;

			this.PropertyValue = propertyValue;

			foreach (var crop in model.Crops)
			{
				this.Add(crop);
			}
		}
		/// <summary>
		/// Selects a crop by its name
		/// </summary>
		/// <param name="cropName">Name of crop as definde in the data type.</param>
		/// <returns>CropModel (strongly typed crop)</returns>
		public CropModel Find(string cropName)
		{
			return this.Find(c => c.Name.Equals(cropName));
		}
		/// <summary>
		/// Return the original property value for backward compatibility
		/// </summary>
		/// <returns>original property value as string</returns>
		public override string ToString()
		{
			return this.PropertyValue;
		}

	}
}
