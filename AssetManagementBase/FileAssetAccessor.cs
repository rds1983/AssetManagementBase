using AssetManagementBase.Utility;
using System;
using System.IO;

namespace AssetManagementBase
{
	internal class FileAssetAccessor : IAssetAccessor
	{
		public string Name => $"File";

		public FileAssetAccessor()
		{
		}

		private static string ToPlatformPath(string path)
		{
			// Update separators
			if (PathUtils.SeparatorSymbol != Path.DirectorySeparatorChar)
			{
				path = path.Replace(PathUtils.SeparatorSymbol, Path.DirectorySeparatorChar);
			}

			return path;
		}

		public bool Exists(string path)
		{
			return File.Exists(ToPlatformPath(path));
		}

		public Stream Open(string path)
		{
			path = ToPlatformPath(path);
			if (!File.Exists(path))
			{
				throw new Exception($"Could not find file '{path}'");
			}

			return File.OpenRead(path);
		}
	}
}
