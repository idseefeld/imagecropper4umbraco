using System;
using System.Drawing;
using System.IO;
using idseefeld.de.imagecropper.Model;
using umbraco.BusinessLogic;
using Umbraco.Core;
using Umbraco.Core.IO;


namespace idseefeld.de.imagecropper.imagecropper {
	public class ImageInfo : PersitenceFactory {
		public Image Image { get; set; }
		public string Name { get; set; }
		public string Extension { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public float Aspect { get; set; }
		public DateTimeOffset DateStamp { get; set; }
		/// <summary>
		/// the image url, UNC path or maped physical file path
		/// </summary>
		public string PathOrUrl { get; set; }
		/// <summary>
		/// for url based storage or UNC paths this returns the same as PathOrUrl
		/// for local files this returns the absolute url
		/// </summary>
		public string ImageUrl { get; set; }

		private Config _config;
		private bool ParentIsDocument = false;
		private bool isCropBase = false;

		private bool deleteCropsOnRemoveImage = false;//new delete on upload image remove disabled

		private string GetRelativeFilePathOrUrl(string fullPathOrUrl)
		{
			string RootPath = _fileSystem.GetFullPath("");
			string _rootUrl = _fileSystem.GetUrl("");

			var relativePath = String.Empty;
			if (!fullPathOrUrl.StartsWith("http"))
			{
				relativePath = fullPathOrUrl
					.TrimStart(_rootUrl)
					.Replace('/', Path.DirectorySeparatorChar)
					.TrimStart(RootPath)
					.TrimStart(Path.DirectorySeparatorChar);
			}
			else
			{
				relativePath = fullPathOrUrl.Substring(0, fullPathOrUrl.LastIndexOf('/') + 1);
			}
			return relativePath;
		}
		private string GetRelativeDirectoryPathOrUrl(string pathOrUrl)
		{
			string rVal = String.Empty;
			if (!pathOrUrl.StartsWith("http") && pathOrUrl.LastIndexOf('/') <= 0)
			{
				rVal = pathOrUrl.Substring(0, pathOrUrl.LastIndexOf('\\') + 1);
			}
			else
			{
				rVal = pathOrUrl.Substring(0, pathOrUrl.LastIndexOf('/') + 1);
			}
			return rVal;
		}
		private string GetFileName(string pathOrUrl)
		{
			string rVal = pathOrUrl;

			int posLastSeparator = pathOrUrl.LastIndexOf('\\');
			if (posLastSeparator < 0)
				posLastSeparator = pathOrUrl.LastIndexOf('/');
			if (posLastSeparator > 0)
				rVal = pathOrUrl.Substring(posLastSeparator + 1);

			return rVal;
		}
		public ImageInfo(string relativeOrFullImageUrl, Config config, bool ParentIsDocument, umbraco.cms.businesslogic.datatype.FileHandlerData data = null)
		{
			this._config = config;
			this.ParentIsDocument = ParentIsDocument;
			ImageUrl = relativeOrFullImageUrl;
			if (String.IsNullOrWhiteSpace(PathOrUrl))
				PathOrUrl = _fileSystem.GetFullPath(_fileSystem.GetRelativePath(relativeOrFullImageUrl));
			string relPath = GetRelativeDirectoryPathOrUrl(ImageUrl);

			if (_fileSystem.FileExists(PathOrUrl))
			{
				#region original image exists

				string fileName = GetFileName(PathOrUrl);
				Name = fileName.Substring(0, fileName.LastIndexOf('.'));
				using (System.IO.Stream _stream = _fileSystem.OpenFile(PathOrUrl))
				{
					try
					{
						using (Image = Image.FromStream(_stream))
						{

							Width = Image.Width;
							Height = Image.Height;
							Aspect = (float)Width / Height;
							DateStamp = _fileSystem.GetLastModified(PathOrUrl);

							if (config.ResizeMax > 0 && !isCropBase)
							{
								string newPath = CreateCropBaseImage(config.ResizeMax, Width, Height, Aspect, DateStamp, config.ResizeEngine);
								if (!String.IsNullOrEmpty(newPath))
								{
									PathOrUrl = newPath;
									fileName = PathOrUrl.Substring(PathOrUrl.LastIndexOf('\\') + 1);
									ImageUrl = relPath + fileName;
									Name = fileName.Substring(0, fileName.LastIndexOf('.'));
									using (System.IO.Stream _stream2 = _fileSystem.OpenFile(PathOrUrl))
									{
										try
										{
											using (Image = Image.FromStream(_stream2))
											{

												Width = Image.Width;
												Height = Image.Height;
												Aspect = (float)Width / Height;
												DateStamp = _fileSystem.GetLastModified(PathOrUrl);
												isCropBase = true;
											}
										}
										catch { }
									}
								}
							}
						}
					}
					catch (Exception)
					{
						Width = 0;
						Height = 0;
						Aspect = 0;
					}
				}
				#endregion
			}
			else
			{
				if (deleteCropsOnRemoveImage)
				{
					#region deleteCropsOnRemoveImage
					if (data != null && data.Value != null && !String.IsNullOrEmpty(data.Value.ToString()))
					{
						//if crops exist then delete them now
						ImageCropperModel cropperModel = new ImageCropperModel(data.Value.ToString());
						string mediaDirPath = null;
						foreach (var crop in cropperModel.Crops)
						{
							if (mediaDirPath == null)
							{
								mediaDirPath = _fileSystem.GetRelativePath(crop.Url);
								mediaDirPath = mediaDirPath.Contains("\\")
									? mediaDirPath.Substring(0, mediaDirPath.LastIndexOf('\\'))
									: mediaDirPath;
							}
							var files = _fileSystem.GetFiles(mediaDirPath);
							foreach (var f in files)
							{
								if (f.Contains(String.Format("_{0}.", crop.Name)))
								{
									_fileSystem.DeleteFile(f);
								}
								else if (f.Contains(DataType.CROP_POSTFIX))
								{
									_fileSystem.DeleteFile(f);
								}
							}
						}
						if (!String.IsNullOrEmpty(mediaDirPath) && _fileSystem.DirectoryExists(mediaDirPath))
						{
							bool dirIsEmpty = true;
							var dirs = _fileSystem.GetDirectories(mediaDirPath);
							foreach (var d in dirs)
							{
								if (!String.IsNullOrEmpty(d))
								{
									dirIsEmpty = false;
									break;
								}
							}
							if (dirIsEmpty)
							{
								var files = _fileSystem.GetFiles(mediaDirPath);
								foreach (var f in files)
								{
									if (!String.IsNullOrEmpty(f))
									{
										dirIsEmpty = false;
										break;
									}
								}
							}
							if (dirIsEmpty)
							{
								//if dir is empty then delete dir too
								_fileSystem.DeleteDirectory(mediaDirPath);
							}
						}
					}
					#endregion
				}
				Width = 0;
				Height = 0;
				Aspect = 0;
			}
		}
		private string CreateCropBaseImage(int maxWidth, int width, int height, float aspect, DateTimeOffset dateStamp, IImageResizeEngine ResizeEngine)
		{
			if (maxWidth >= width || Name.EndsWith("_cb"))
				return "";

			string cropBaseName = String.Format("{0}_{1}", Name, "cb");
			string path = PathOrUrl.Substring(0, PathOrUrl.LastIndexOf('\\'));
			string ext = ImageTransform.GetAdjustedFileExtension(PathOrUrl);// Path.Substring(Path.LastIndexOf('.') + 1);
			string newPath = String.Format("{0}\\{1}.{2}", path, cropBaseName, ext);
			if (this._fileSystem.FileExists(newPath))
			{
				DateTimeOffset cropDateStamp = _fileSystem.GetLastModified(newPath);
				if (cropDateStamp.CompareTo(dateStamp) > 0)
					return newPath;
			}

			bool forceResize = true;//must
			ResizeEngine.saveNewImageSize(PathOrUrl, ext, newPath, maxWidth, forceResize, width, _config.IgnoreICC, false);

			return newPath;
		}
		public bool Exists
		{
			get { return Width > 0 && Height > 0; }
		}

		public string Directory
		{
			get { return PathOrUrl.Substring(0, PathOrUrl.LastIndexOf('\\')); }
		}

		public void GenerateThumbnails(SaveData saveData, Config config)
		{
			GenerateThumbnails(saveData, config, -1);
		}
		public void GenerateThumbnails(SaveData saveData, Config config, int cropIndex)
		{
			if (config.GenerateImages)
			{
				int startIndex = 0;
				int maxIndex = config.presets.Count;
				if (cropIndex >= 0)
				{
					startIndex = cropIndex;
					maxIndex = startIndex + 1;
				}
				for (int i = startIndex; i < maxIndex; i++)
				{
					Crop crop = (Crop)saveData.data[i];
					Preset preset = (Preset)config.presets[i];

					int cX = crop.X;
					int cY = crop.Y;
					int cWidth = crop.X2 - crop.X;
					int cHeight = crop.Y2 - crop.Y;
					int tWidth = preset.TargetWidth;
					int tHeight = preset.TargetHeight;
					if (tWidth == 0)
					{
						tWidth = tHeight * cWidth / cHeight;
					}
					else if (tHeight == 0)
					{
						tHeight = tWidth * cHeight / cWidth;
					}

					// Crop rectangle bigger than actual image
					if (cWidth > Width)
					{
						cWidth = Width;
						cX = 0;
					}
					if (cHeight > Height)
					{
						cHeight = Height;
						cY = 0;
					}

					string newPath = ImageTransform.Execute(
						PathOrUrl,
						String.Format("{0}_{1}", Name, preset.Name),
						cX,
						cY,
						cWidth,
						cHeight,
						tWidth,
						tHeight,
						config,
						_fileSystem
					);
					if (ParentIsDocument)
					{
						//support preview / publish
						string cropHash = config.cropHashDict[preset.Name];
						string newFile = CopyToHashFile(
												newPath,
												String.Format("{0}",
													cropHash)
											);
						config.newCropFiles.Add(newFile);
					}
				}
			}
		}
	}

	public class ImageTransform {
		public static string GetAdjustedFileExtension(string filename)
		{
			string extension = filename.Substring(filename.LastIndexOf('.') + 1);
			if (extension.StartsWith("tif", StringComparison.InvariantCultureIgnoreCase))
				extension = "jpg";//because the image engine supports tiff images, but web browsers do not!

			return extension;
		}
		public static string FixBrowserUnsupportedFormats(string relativeImagePath, IImageResizeEngine ResizeEngine)
		{
			var _fileSystem = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();

			string adjustedPath = relativeImagePath;
			string extension = relativeImagePath.Substring(relativeImagePath.LastIndexOf('.') + 1);
			if (extension.StartsWith("tif", StringComparison.InvariantCultureIgnoreCase))
			{
				extension = "jpg";
				adjustedPath = String.Format("{0}.{1}",
					adjustedPath.Substring(0, adjustedPath.LastIndexOf('.')),
					extension);
				string sourceFile = _fileSystem.GetFullPath(relativeImagePath);//IOHelper.MapPath(relativeImagePath);
				string newFile = _fileSystem.GetFullPath(adjustedPath);// IOHelper.MapPath(adjustedPath);

				ResizeEngine.saveNewImageSize(sourceFile, extension, newFile, true);
			}
			return adjustedPath;
		}
		public static string Execute(string sourceFile, string name, int cropX, int cropY, int cropWidth, int cropHeight, int sizeWidth, int sizeHeight, Config config, MediaFileSystem _fileSystem)
		{
			string result = "";
			if (String.IsNullOrEmpty(sourceFile) || !_fileSystem.FileExists(sourceFile)) return result;

			string relPath = _fileSystem.GetRelativePath(sourceFile);
			string path = relPath.Contains("\\") ? relPath.Substring(0, relPath.LastIndexOf('\\')) : relPath;

			string ext = ImageTransform.GetAdjustedFileExtension(sourceFile);
			string newPath = String.Format("{0}\\{1}.{2}", path, name, ext);
			bool forceResize = true;//must, but can not remeber why - TODO: check this
			bool onlyIfNew = false;
			int tryCounter = 1;
			int maxTry = 2;
			bool tryAgain = true;
			do
			{
				if (tryCounter > 1)
				{
					//TODO: check this!
					//This might be a work around, because I can not figure out why 
					//the newPath image is in use by another process

					//in case the crop image is in use by another process lets 
					//force Garbage Collection,
					GC.Collect();
					//wait ...
					GC.WaitForPendingFinalizers();
					//and try again
					Log.Add(LogTypes.Error, -1, "ImageHelper waiWaitForPendingFinalizers");
				}
				if (config.ResizeEngine.saveCroppedNewImageSize(sourceFile, ext, newPath, sizeWidth, forceResize, 0, sizeHeight, cropX, cropY, cropWidth, cropHeight, config.Quality, config.IgnoreICC, onlyIfNew))
				{
					tryAgain = false;
					result = newPath;
				}

				tryCounter++;
				if (tryCounter > maxTry)
				{
					tryAgain = false;
				}
			} while (tryAgain);

			return result;
		}


	}
}
