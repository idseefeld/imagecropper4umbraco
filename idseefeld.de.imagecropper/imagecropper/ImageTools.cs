using System.Drawing;
using System.Web;

using umbraco.Utils;
using Umbraco.Core.Media;
using umbraco.cms.businesslogic.media;


namespace idseefeld.de.imagecropper.imagecropper
{
	public class ImageTools: PersitenceFactory
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
		public void GenerateImageByWidth(int newWidth, string imgRelativePath, string cropRelativePath, bool ignoreICC, IImageResizeEngine ResizeEngine)
		{
			if (imgRelativePath == cropRelativePath)
				return;

			string imgPath = HttpContext.Current.Server.MapPath(imgRelativePath);
			string cropPath = HttpContext.Current.Server.MapPath(cropRelativePath);
			if (_fileSystem.FileExists(cropPath))
			{
				_fileSystem.DeleteFile(cropPath);
			}
			if (_fileSystem.FileExists(imgPath))
			{
				using (System.IO.Stream imgStream = _fileSystem.OpenFile(imgPath))
				{
					using (Bitmap img = new Bitmap(imgStream))
					{ //Bitmap img = new Bitmap(imgPath);
						string imgExtension = imgPath.Substring(imgPath.LastIndexOf('.') + 1);
						int newHeight = (int)((double)newWidth / (double)img.Width * (double)img.Height);

						ResizeEngine.saveNewImageSize(
							imgPath,
							imgExtension,
							cropPath,
							newWidth, false, img.Width,
							ignoreICC,
							false);
					}
				}
			}
		}
		public string GenerateImageByWidth(int newWidth, UmbracoImage umbImage, bool ignoreICC, IImageResizeEngine ResizeEngine)
		{
			string result = umbImage.Src;
			string newSrc = umbImage.Src.Substring(0, umbImage.Src.LastIndexOf('.')) + "_autoWidth" + newWidth + "." + umbImage.Extension;
			string newPath = HttpContext.Current.Server.MapPath(newSrc);
			if (_fileSystem.FileExists(newPath))
			{
				return newSrc;
			}
			int newHeight = (int)((double)newWidth / (double)umbImage.Width * (double)umbImage.Height);
			if (ResizeEngine.saveNewImageSize(
				HttpContext.Current.Server.MapPath(umbImage.Src),
					umbImage.Extension,
					newPath,
					newWidth, false, umbImage.Width,
					ignoreICC,
					false))
				result = newSrc;

			return result;
		}
	}
}