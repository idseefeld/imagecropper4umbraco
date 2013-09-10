using System.Xml;

namespace idseefeld.de.imagecropper.imagecropper
{
	public class DataTypeData : umbraco.cms.businesslogic.datatype.FileHandlerData
    {
        public DataTypeData(umbraco.cms.businesslogic.datatype.BaseDataType DataType) : base(DataType, "") { }

		public override XmlNode ToXMl(XmlDocument data)
		{
			if (Value.ToString() != "")
			{
				XmlDocument xd = new XmlDocument();
				xd.LoadXml(Value.ToString());
				return data.ImportNode(xd.DocumentElement, true);
			}
			else
			{
				return base.ToXMl(data);
			}

		}
    }
}