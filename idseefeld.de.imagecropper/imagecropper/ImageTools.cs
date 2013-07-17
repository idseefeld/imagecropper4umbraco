using System.Drawing;
using System.IO;
using System.Web;

using umbraco.Utils;
using Umbraco.Core.Media;
using umbraco.cms.businesslogic.media;


namespace idseefeld.de.imagecropper.imagecropper
{
	public class ImageTools
	{
		private Media img;

		public ImageTools(Media img)
		{
			this.img = img;
		}
		public ImageTools()
		{
			this.img = null;
		}
		public void GenerateImageByWidth(int newWidth, string imgRelativePath, string cropRelativePath, bool ignoreICC, ImageResizeEngineDefault ResizeEngine)
		{
			if (imgRelativePath == cropRelativePath)
				return;

			string imgPath = HttpContext.Current.Server.MapPath(imgRelativePath);
			string cropPath = HttpContext.Current.Server.MapPath(cropRelativePath);
			if (File.Exists(cropPath))
			{
				File.Delete(cropPath);
			}
			if (File.Exists(imgPath))
			{
				Bitmap img = new Bitmap(imgPath);
				string imgExtension = imgPath.Substring(imgPath.LastIndexOf('.') + 1);
				int newHeight = (int)((double)newWidth / (double)img.Width * (double)img.Height);

				ResizeEngine.saveNewImageSize(
					imgPath,
					imgExtension,
					cropPath,
					newWidth, false, img.Width,
					ignoreICC);
			}
		}
		public string GenerateImageByWidth(int newWidth, UmbracoImage umbImage, bool ignoreICC, ImageResizeEngineDefault ResizeEngine)
		{
			string result = umbImage.Src;
			string newSrc = umbImage.Src.Substring(0, umbImage.Src.LastIndexOf('.')) + "_autoWidth" + newWidth + "." + umbImage.Extension;
			string newPath = HttpContext.Current.Server.MapPath(newSrc);
			if (File.Exists(newPath))
			{
				return newSrc;
			}
			int newHeight = (int)((double)newWidth / (double)umbImage.Width * (double)umbImage.Height);
			if (ResizeEngine.saveNewImageSize(
				HttpContext.Current.Server.MapPath(umbImage.Src),
					umbImage.Extension,
					newPath,
					newWidth, false, umbImage.Width,
					ignoreICC))
				result = newSrc;

			return result;
		}
	}
}