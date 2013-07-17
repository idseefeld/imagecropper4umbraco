using System;
namespace idseefeld.de.imagecropper.imagecropper {
	public struct Crop {
		public int X;
		public int Y;
		public int X2;
		public int Y2;

		public Crop(int x, int y, int x2, int y2)
		{
			X = x;
			Y = y;
			X2 = x2;
			Y2 = y2;
		}
	}

	enum DefaultCropPosition {
		CenterCenter = 0,
		CenterTop,
		CenterBottom,
		LeftCenter,
		LeftTop,
		LeftBottom,
		RightCenter,
		RightTop,
		RightBottom
	}

	public struct Preset {
		public string Name;
		public int TargetWidth;
		public int TargetHeight;
		public bool KeepAspect;
		public string PositionH;
		public string PositionV;

		public float Aspect
		{
			get { return (float)TargetWidth / TargetHeight; }
		}
		public Crop Fit(ImageInfo imageInfo)
		{
			Crop crop;

			if (TargetWidth == 0 || TargetHeight == 0)
			{
				crop.X = 0;
				crop.X2 = imageInfo.Width;
				crop.Y = 0;
				crop.Y2 = imageInfo.Height;
			}
			else if (Aspect >= imageInfo.Aspect)
			{
				// crop widest            hor    ver
				// relevant positioning: center top, center center, center bottom

				float h = ((float)imageInfo.Width / TargetWidth) * TargetHeight;

				crop.X = 0;
				crop.X2 = imageInfo.Width;

				switch (PositionV)
				{
					case "T":
						crop.Y = 0;
						crop.Y2 = (int)h;
						break;
					case "B":
						crop.Y = imageInfo.Height - (int)h;
						crop.Y2 = imageInfo.Height;
						break;
					default: // CC
						crop.Y = (int)(imageInfo.Height - h) / 2;
						crop.Y2 = (int)(crop.Y + h);
						break;
				}
			}
			else
			{
				// image widest
				// relevant positioning: left/right center, left/right top, left/right bottom

				float w = ((float)imageInfo.Height / TargetHeight) * TargetWidth;

				crop.Y = 0;
				crop.Y2 = imageInfo.Height;

				switch (PositionH)
				{
					case "L":
						crop.X = 0;
						crop.X2 = (int)w;
						break;
					case "R":
						crop.X = imageInfo.Width - (int)w;
						crop.X2 = imageInfo.Width;
						break;
					default: // CC
						crop.X = (int)(imageInfo.Width - w) / 2;
						crop.X2 = (int)(crop.X + w);
						break;
				}

			}

			return crop;
		}

		public Preset(string name, int targetWidth, int targetHeight, bool keepAspect, string positionH, string positionV)
		{
			Name = name;
			TargetWidth = targetWidth;
			TargetHeight = targetHeight;
			KeepAspect = keepAspect;
			PositionH = positionH;
			PositionV = positionV;
		}

		public Preset(string serializedPreset)
		{
			string name = String.Empty;
			int targetWidth = 0;
			int targetHeight = 0;
			bool keepAspect = true;
			string positionH = "";
			string positionV = "";

			string[] p = serializedPreset.Split(',');


			if (p.Length >= 4 && Int32.TryParse(p[1], out targetWidth) && Int32.TryParse(p[2], out targetHeight))
			{
				char[] cropPosition = { 'C', 'M' };

				if (p.Length >= 5)
				{
					cropPosition = p[4].ToCharArray();
				}

				name = p[0];
				keepAspect = p[3] == "1";
				positionH = cropPosition[0].ToString();
				positionV = cropPosition[1].ToString();
			}

			Name = name;
			TargetWidth = targetWidth;
			TargetHeight = targetHeight;
			KeepAspect = keepAspect;
			PositionH = positionH;
			PositionV = positionV;
		}

	}
}