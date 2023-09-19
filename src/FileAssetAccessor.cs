using System;
using System.IO;

namespace AssetManagementBase
{
	internal class FileAssetAccessor : IAssetAccessor
	{
		private readonly string _baseFolder;

		public FileAssetAccessor(string baseFolder)
		{
			// Base folder shouldn't have separator char at the end
			if (!string.IsNullOrEmpty(baseFolder) && baseFolder.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				baseFolder = baseFolder.Substring(0, baseFolder.Length - 1);
			}

			_baseFolder = baseFolder;
		}

		public bool Exists(string assetName)
		{
			return File.Exists(BuildFullPath(assetName));
		}

		public Stream Open(string assetName)
		{
			assetName = BuildFullPath(assetName);
			if (!File.Exists(assetName))
			{
				throw new Exception($"Could not find file '{assetName}'");
			}

			return File.OpenRead(assetName);
		}

		private string BuildFullPath(string assetName)
		{
			if (AssetManager.SeparatorSymbol != Path.DirectorySeparatorChar)
			{
				assetName = assetName.Replace(AssetManager.SeparatorSymbol, Path.DirectorySeparatorChar);
			}

			// Asset name should always have directory separator at the start
			// While base folder shouldnt
			// Hence such combine should work
			return _baseFolder + assetName;
		}
	}
}
