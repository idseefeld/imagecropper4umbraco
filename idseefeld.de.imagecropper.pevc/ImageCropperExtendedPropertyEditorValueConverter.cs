using System;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web;
using idseefeld.de.imagecropper.Model;
using idseefeld.de.imagecropper.imagecropper;

namespace idseefeld.de.imagecropper.PropertyEditorValueConverter {
	public class ImageCropperExtendedPropertyEditorValueConverter : IPropertyEditorValueConverter {
		string _propertyTypeAlias = string.Empty;
		string _docTypeAlias = string.Empty;

		public bool IsConverterFor(Guid propertyEditorId, string docTypeAlias, string propertyTypeAlias)
		{
			_propertyTypeAlias = propertyTypeAlias;
			_docTypeAlias = docTypeAlias;
			return ImageCropperBase.PropertyEditorId.Equals(propertyEditorId);
		}
		public Attempt<object> ConvertPropertyValue(object value)
		{
			if (UmbracoContext.Current != null)
			{
				try
				{
					ImageCropperModel imageCropperExtendedContent = new ImageCropperModel(value.ToString());
					CropList result = new CropList(imageCropperExtendedContent);
					return new Attempt<object>(true, result);
				}
				catch { }
			}
			return Attempt<object>.False;
		}
	}
	public class CropList : List<CropModel> {
		public CropList(ImageCropperModel model)
		{
			if (model == null)
				return;

			foreach (var crop in model.Crops)
			{
				this.Add(crop);
			}
		}
	}
}
