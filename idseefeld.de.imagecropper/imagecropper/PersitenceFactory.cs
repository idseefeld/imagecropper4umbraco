using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Umbraco.Core.IO;

namespace idseefeld.de.imagecropper.imagecropper
{
	public class PersitenceFactory
	{
		internal readonly MediaFileSystem _fileSystem;

		protected PersitenceFactory()
		{
			_fileSystem = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();
		}

		public string CopyToHashFile(string sourceFile, string name)
		{
			string ext = sourceFile.Substring(sourceFile.LastIndexOf('.') + 1);
			return CopyToHashFile(sourceFile, name, ext);
		}
		public string CopyToHashFile(string sourceFile, string name, string extension)
		{
			string newPath = String.Empty;
			if (!_fileSystem.FileExists(sourceFile))
				return newPath;

			string path = String.Empty;
			if (sourceFile.StartsWith("http"))
			{
				path = sourceFile.Substring(0, sourceFile.LastIndexOf('/'));
				newPath = String.Format("{0}/{1}.{2}", path, name, extension);
			}
			else
			{
				path = sourceFile.Substring(0, sourceFile.LastIndexOf('\\'));
				newPath = String.Format("{0}\\{1}.{2}", path, name, extension);
			}

			if (!_fileSystem.FileExists(newPath)){
				using (System.IO.Stream sourceStream = _fileSystem.OpenFile(sourceFile))
				{
					if (sourceStream.CanSeek)
					{
						sourceStream.Seek(0, SeekOrigin.Begin);
					}
					_fileSystem.AddFile(newPath, sourceStream);
				}
			}
			return newPath;
		}
	}
}
