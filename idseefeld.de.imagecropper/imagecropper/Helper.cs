using System.IO;
using System.Xml.Serialization;

namespace idseefeld.de.imagecropper.imagecropper
{
    class Helper
    {
        public static bool isParentDocumentType()
        {
            bool result = false;
            string pageIdStr = System.Web.HttpContext.Current.Request.QueryString["id"];
            int pageId = 0;
            int.TryParse(pageIdStr, out pageId);
            if (pageId > 0)
            {
                try
                {
                    umbraco.cms.businesslogic.web.Document doc = new umbraco.cms.businesslogic.web.Document(pageId);
                    if (doc != null && doc.Id>0)
                    {
                        result = true;
                    }
                }
                catch { }
            }
            return result;
        }

        public static string SerializeToString(object obj)
        {
            XmlSerializer serializer = new XmlSerializer(obj.GetType());

            using (StringWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, obj);

                return writer.ToString();
            }
        }
    }
}