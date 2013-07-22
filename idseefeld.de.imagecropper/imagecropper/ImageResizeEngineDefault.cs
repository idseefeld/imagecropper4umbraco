using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using umbraco.BusinessLogic;
using System.Web;
using Umbraco.Core.IO;
using System.Web.UI;
using System.Xml;
using Umbraco.Core.Media;

namespace idseefeld.de.imagecropper.imagecropper
{
    public class ImageResizeEngineDefault : PersitenceFactory, IImageResizeEngine
    {
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
            if (fileExtension.StartsWith("."))
                fileExtension = fileExtension.Substring(1);
            bool newImgSaved = false;
            InterpolationMode iMode = GetInterpolationMode(oldWidth, newWidth);
            try
            {
                bool useIcm = !ignoreICC;//this is the Microsoft name for ICC profiles
                using (Bitmap orig = new Bitmap(imgPath, useIcm))
                {
                    int newHeight;

                    #region encoder settings
                    ImageFormat format = ImageFormat.Jpeg;
                    ImageCodecInfo imgEncoder = GetEncoder(ImageFormat.Jpeg);
                    EncoderParameter imgEncoderParameter;
                    EncoderParameters imgEncoderParameters;
                    System.Drawing.Imaging.Encoder qualtiyEncoder =
                        System.Drawing.Imaging.Encoder.Quality;
                    switch (fileExtension)
                    {
                        case "png":
                            format = ImageFormat.Png;
                            imgEncoder = GetEncoder(ImageFormat.Png);
                            break;
                        case "gif":
                            format = ImageFormat.Gif;
                            imgEncoder = GetEncoder(ImageFormat.Gif);
                            break;
                        default:
                            format = ImageFormat.Jpeg;
                            imgEncoder = GetEncoder(ImageFormat.Jpeg);
                            break;
                    }
                    imgEncoderParameters = new EncoderParameters(1);

                    imgEncoderParameter = new EncoderParameter(qualtiyEncoder, quality);

                    imgEncoderParameters.Param[0] = imgEncoderParameter;
                    #endregion

                    if (forceResize || orig.Width > newWidth)
                    {
                        newHeight = (int)Math.Round(((double)newWidth) / orig.Width * orig.Height);

                        int cW = newWidth;
                        int cH = newHeight;
                        if (cropWidth > 0)
                        {
                            cW = cropWidth;
                            cH = cropHeight;
                            newHeight = sizeHeight;
                        }
                        using (Bitmap crop = new Bitmap(cW, cH))
                        {
                            if (cropWidth > 0)
                            {
                                Graphics graphCrop = Graphics.FromImage(crop);

                                graphCrop.SmoothingMode = SmoothingMode.HighQuality;
                                //graphCrop.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                graphCrop.PixelOffsetMode = PixelOffsetMode.HighQuality;

                                graphCrop.InterpolationMode = iMode;
                                GraphicsUnit units = GraphicsUnit.Pixel;
                                graphCrop.DrawImage(orig, new Rectangle(0, 0, cW, cH), cropX, cropY, cW, cH, units);
                            }

                            using (Bitmap bmp = new Bitmap(newWidth, newHeight))
                            {
                                Graphics graph = Graphics.FromImage(bmp);
                                graph.SmoothingMode = SmoothingMode.HighQuality;
                                graph.PixelOffsetMode = PixelOffsetMode.HighQuality;
                                graph.InterpolationMode = iMode;
                                if (cropWidth > 0)
                                    graph.DrawImage(crop, new Rectangle(0, 0, newWidth, newHeight));
                                else
                                    graph.DrawImage(orig, new Rectangle(0, 0, newWidth, newHeight));
                                if (_fileSystem.FileExists(newPath))
                                    _fileSystem.DeleteFile(newPath);

                                //bmp.Save(newPath, imgEncoder, imgEncoderParameters);
                                var ms = new MemoryStream();
                                bmp.Save(ms, imgEncoder, imgEncoderParameters);
                                ms.Seek(0, 0);

                                _fileSystem.AddFile(newPath, ms);
                                //for backward compatibilty save also JPEG for PNGs
                                //render image on white background
                                if (fileExtension.Equals("png", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    string newPathJpg = String.Format("{0}.jpg", newPath.Remove(newPath.LastIndexOf('.')));
                                    imgEncoder = GetEncoder(ImageFormat.Jpeg);
                                    var msCopy = new MemoryStream();
                                    graph.FillRectangle(new SolidBrush(Color.White), 0, 0, newWidth, newHeight);
                                    ms.Seek(0, 0);
                                    graph.DrawImage(Image.FromStream(ms), new Rectangle(0, 0, newWidth, newHeight));
                                    bmp.Save(msCopy, imgEncoder, imgEncoderParameters);
                                    msCopy.Seek(0, 0);
                                    _fileSystem.AddFile(newPathJpg, msCopy);
                                }

                                ms.Close();

                                newImgSaved = true;
                            }
                        }

                    }
                    else
                    {
                        _fileSystem.AddFile(newPath, _fileSystem.OpenFile(imgPath), true);
						
                        newImgSaved = true;
                    }
                }
            }
            catch { }
            return newImgSaved;
        }

        #region private
        private InterpolationMode GetInterpolationMode(int oldWidth, int newWidth)
        {
            InterpolationMode iMode = InterpolationMode.Default;
            if (oldWidth == 0)
                return InterpolationMode.HighQualityBicubic;

            double resizeFactor = (double)newWidth / (double)oldWidth;
            if (resizeFactor > 0.25)
                iMode = InterpolationMode.HighQualityBicubic;
            if (resizeFactor > 0.5)
                iMode = InterpolationMode.HighQualityBilinear;

            return iMode;
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
        #endregion



		#region IImageResizeEngine Member


		public Control PluginPrevalueSettings()
		{
			return null;
		}
		public void RenderPrevalueSettings(HtmlTextWriter writer) { }
		public void SetPrevalues(string values) { }
		public string GetPrevalues(Control placeholder)
		{
			return String.Empty;
		}

		public void Load(XmlDocument xml)
		{
			return;
		}
		public string GetExtraHash()
		{
			return String.Empty;
		}
		public void GetExtraXml(ref XmlDocument doc, ref XmlNode root)
		{
			return;
		}
		public Control GetControls()
		{
			return null;
		}

		#endregion
	}

}
