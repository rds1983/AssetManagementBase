﻿using System.IO;

namespace AssetManagementBase
{
	public class FileAssetResolver : IAssetResolver
	{
		public string BaseFolder { get; set; }

		public FileAssetResolver(string baseFolder)
		{
			BaseFolder = baseFolder;
		}

		public Stream Open(string assetName)
		{
			if (AssetManager.SeparatorSymbol != Path.DirectorySeparatorChar)
			{
				assetName = assetName.Replace(AssetManager.SeparatorSymbol, Path.DirectorySeparatorChar);
			}

			if (!Path.IsPathRooted(assetName) && !string.IsNullOrEmpty(BaseFolder))
			{
				assetName = Path.Combine(BaseFolder, assetName);
			}

			return File.OpenRead(assetName);
		}
	}
}
