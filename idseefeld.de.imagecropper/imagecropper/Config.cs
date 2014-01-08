using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using Umbraco.Core.Media;
using System.Reflection;

namespace idseefeld.de.imagecropper.imagecropper {
	public class Config {
		public string UploadPropertyAlias { get; set; }
		public string BackgroundColor { get; set; }
        public int ResizeMax { get; set; }
        public int Version { get; set; }
		public bool GenerateImages { get; set; }
		public bool IgnoreICC { get; set; }
		public IImageResizeEngine ResizeEngine { get; set; }
		public bool CustomProvider { get; set; }
		public bool ShowIgnoreICC { get; set; }
		public bool AutoGenerateImages { get; set; }
		public bool CompatibilityModeJpeg { get; set; }
		public int Quality { get; set; }
		public bool ShowLabel { get; set; }
		public ArrayList presets { get; set; }
		public List<string> versionedCropFiles { get; set; }
		public List<string> newCropFiles { get; set; }
		public Dictionary<string, string> cropHashDict { get; set; }

		public Config()
		{ }
		public Config(string configuration)
		{
			presets = new ArrayList();
			versionedCropFiles = new List<string>();
			newCropFiles = new List<string>();
			cropHashDict = new Dictionary<string, string>();
			this.CustomProvider = false;

			bool useDefaultEngine = true;
			try
			{
				Assembly myDllAssembly =
				   Assembly.LoadFile(
				   String.Format("{0}\\ImageResizer.dll",
				   System.Web.HttpRuntime.BinDirectory));
				if (myDllAssembly != null)
				{
					ResizeEngine = new ImageEngineImageResizer();
					this.CustomProvider = true;
					useDefaultEngine = false;
				}
			}
			catch { }

			if (useDefaultEngine)
			{
				ResizeEngine = new ImageResizeEngineDefault();
			}

			string[] configData = configuration.Split('|');
			if (configData.Length < 2) return;
			string[] generalSettings = configData[0].Split(',');
			UploadPropertyAlias = generalSettings[0];
			GenerateImages = generalSettings[1] == "1";
			ShowLabel = generalSettings[2] == "1";

			int _quality;
			if (generalSettings.Length > 3 && Int32.TryParse(generalSettings[3], out _quality))
			{
				Quality = _quality;
			}
			if (Quality == 0)
			{
				Quality = 90;
			}
			if (generalSettings.Length > 4)
				CompatibilityModeJpeg = generalSettings[4] == "1";
			else
				CompatibilityModeJpeg = false;
			if (generalSettings.Length > 5)
			{
				BackgroundColor = generalSettings[5];
			}
			else
			{
				BackgroundColor = "transparent";
			}
			int _ResizeMax = 0;
			if (generalSettings.Length > 6)
				Int32.TryParse(generalSettings[6], out _ResizeMax);
			ResizeMax = _ResizeMax;
			if (generalSettings.Length > 7)
				AutoGenerateImages = generalSettings[7] == "1";
			if (generalSettings.Length > 8)
				IgnoreICC = generalSettings[8] == "1";
			else
				IgnoreICC = true;//legacy behaviour
			if (generalSettings.Length > 9)
				ShowIgnoreICC = generalSettings[9] == "1";
			else
				ShowIgnoreICC = false;//legacy behaviour

			string[] presetData = configData[1].Split(';');

			for (int i = 0; i < presetData.Length; i++)
			{
				Preset _p = new Preset(presetData[i]);
				if (!String.IsNullOrEmpty(_p.Name) && (_p.TargetHeight > 0 || _p.TargetWidth > 0))
				{
					presets.Add(_p);
				}
			}
		}
	}

	public class ImageResizeEngineConfigSection : ConfigurationSection {
		[ConfigurationProperty("provider", IsRequired = false)]
		public ProviderCollection ResizerProvider
		{
			get { return (ProviderCollection)this["provider"]; }
		}
	}
	public class ProviderElement : ConfigurationElement {
		[ConfigurationProperty("name", IsRequired = true)]
		public string Name
		{
			get { return (string)this["name"]; }
			set { this["name"] = value; }
		}

		[ConfigurationProperty("type", IsRequired = true)]
		public string Type
		{
			get { return (string)this["type"]; }
			set { this["type"] = value; }
		}
	}
	public class ProviderCollection : ConfigurationElementCollection {
		public ProviderElement this[int index]
		{
			get { return BaseGet(index) as ProviderElement; }
			set
			{
				if (BaseGet(index) != null)
				{
					BaseRemoveAt(index);
				}
				BaseAdd(index, value);
			}
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new ProviderElement();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((ProviderElement)element).Name;
		}
	}
}