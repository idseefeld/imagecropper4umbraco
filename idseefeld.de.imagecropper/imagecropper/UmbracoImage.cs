using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using umbraco.cms.businesslogic.media;
using System.Text;
using umbraco.BusinessLogic;


namespace idseefeld.de.imagecropper.imagecropper
{
    public class UmbracoImage
    {
        public int Id { get; set; }
        public string Src { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Extension { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Bytes { get; set; }

        public Media MediaObject { get; set; }

        //public UmbracoImage() { }
        public UmbracoImage(int id)
        {
            Media file = null;
            try
            {
                file = new Media(id);
            }
            catch { }

            init(file);
        }
        public UmbracoImage(Media image)
        {
            init(image);
        }

        private void init(Media image)
        {
            this.MediaObject = image;
            try
            {
                this.Src = image.getProperty("umbracoFile").Value.ToString();
                int tmp = 0;
                int.TryParse(image.getProperty("umbracoWidth").Value.ToString(), out tmp);
                this.Width = tmp;
                tmp = 0;
                int.TryParse(image.getProperty("umbracoHeight").Value.ToString(), out tmp);
                this.Height = tmp;
                tmp = 0;
                int.TryParse(image.getProperty("umbracoBytes").Value.ToString(), out tmp);
                this.Bytes = tmp;
                this.Extension = image.getProperty("umbracoExtension").Value.ToString();

            }
            catch (Exception)
            {
                Log.Add(LogTypes.Error, image.Id, "getProperty error in UmbracoImage");
            }
        }
        public static string getThumbType(string imgSrc, string thumbType)
        {
            return getThumbType(imgSrc, thumbType, false);
        }
        public static string getThumbType(string imgSrc, string thumbType, bool forcePngType)
        {
            if (String.IsNullOrEmpty(imgSrc))
                return "";

            string fileExtension = imgSrc.Substring(imgSrc.LastIndexOf('.'));
            //PNGs will be used only as original upload
            string newFileExtension = ((fileExtension.ToLower() != ".jpg" || (!forcePngType && fileExtension.ToLower() == ".png"))
                    && !thumbType.Contains("_thumb")) ? ".jpg" : fileExtension;

            return (forcePngType && fileExtension.ToLower() == ".png")
                    ? imgSrc : imgSrc.Replace(fileExtension, thumbType + newFileExtension);
        }
    }
}
