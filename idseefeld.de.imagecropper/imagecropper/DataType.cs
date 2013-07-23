using System;
using System.Reflection;

namespace idseefeld.de.imagecropper.imagecropper
{
	public class DataType : umbraco.cms.businesslogic.datatype.BaseDataType, umbraco.interfaces.IDataType
    {
		public const string CROP_POSTFIX = "_HASHEDCROP";

        private umbraco.interfaces.IDataEditor _editor;
        private umbraco.interfaces.IData _baseData;
		private umbraco.interfaces.IDataPrevalue _prevalueEditor;

        public override umbraco.interfaces.IDataEditor DataEditor
        {
            get
            {
                if (_editor == null)
                    _editor = new DataEditor(Data, ((PrevalueEditor)PrevalueEditor).Configuration);
                return _editor;
            }
        }

        public override umbraco.interfaces.IData Data
        {
            get
            {
                if (_baseData == null)
                    _baseData = new DataTypeData(this);
                return _baseData;
            }
        }
        public override Guid Id
        {
            get { return new Guid("5E8E0761-22B0-42DB-8A99-DC7DD4CE797B"); }
        }

        public override string DataTypeName
        {
            get { return "Image Cropper Extended"; }
        }        

        public override umbraco.interfaces.IDataPrevalue PrevalueEditor
        {
            get
            {
                if (_prevalueEditor == null)
                    _prevalueEditor = new PrevalueEditor(this);
                return _prevalueEditor;                
            }
        }

        public static int Version
        {
            get
            {
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                return version.Major*1000 + version.Minor*100;
            }
        }
	}
}