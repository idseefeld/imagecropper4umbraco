using System;
using System.Web.UI;
using System.Xml;

namespace idseefeld.de.imagecropper.imagecropper
{
	public interface IImageResizeEngine
	{
		bool saveCroppedNewImageSize(string imgPath, string fileExtension, string newPath, int newWidth, bool forceResize, int oldWidth, int sizeHeight, int cropX, int cropY, int cropWidth, int cropHeight, int quality, bool ignoreICC, bool onlyIfNew);
		bool saveNewImageSize(string imgPath, string fileExtension, string newPath, bool onlyIfNew);
		bool saveNewImageSize(string imgPath, string fileExtension, string newPath, int newWidth, bool forceResize, int oldWidth, bool ignoreICC, bool onlyIfNew);

		Control GetControls();

		string GetExtraHash();

		void GetExtraXml(ref XmlDocument doc, ref XmlNode root);

		void Load(XmlDocument xml);

		Control PluginPrevalueSettings();

		void RenderPrevalueSettings(HtmlTextWriter writer);

		string GetPrevalues(Control placeholder);

		void SetPrevalues(string values);
	}
}
