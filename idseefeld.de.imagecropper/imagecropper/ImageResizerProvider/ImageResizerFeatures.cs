using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace idseefeld.de.imagecropper.imagecropper
{
	public class ImageResizerFeatures
	{
		public RotateFlipType SourceFlip { get; set; }
		public RotateFlipType SourceRotation { get; set; }
		public double Rotation { get; set; }
		public RotateFlipType Flip { get; set; }
		public bool AdvancedFiltersInstalled { get; set; }
		//advanced filters
		//only available if e.g. ImageResizer.Plugins.AdvancedFilters package is installed and licensed
		public int SharpenRadius { get; set; }
		public int BlurRadius { get; set; }
		public int Contrast { get; set; }
		public int Brightness { get; set; }
		public int Saturation { get; set; }
		public bool Sepia { get; set; }
	}
}
