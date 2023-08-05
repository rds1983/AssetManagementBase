using System.IO;
using System;
using System.Reflection;
using AssetManagementBase.Utility;

namespace AssetManagementBase
{
	internal static class DefaultOpeners
	{
		public static AssetOpener CreateFileOpener(string baseFolder)
		{
			AssetOpener result = assetName =>
			{
				if (AssetManager.SeparatorSymbol != Path.DirectorySeparatorChar)
				{
					assetName = assetName.Replace(AssetManager.SeparatorSymbol, Path.DirectorySeparatorChar);
				}

				if (!Path.IsPathRooted(assetName) && !string.IsNullOrEmpty(baseFolder))
				{
					assetName = Path.Combine(baseFolder, assetName);
				}

				if (!File.Exists(assetName))
				{
					throw new Exception($"Could not find file '{assetName}'");
				}

				return File.OpenRead(assetName);
			};

			return result;
		}

		public static AssetOpener CreateResourceOpener(Assembly assembly, string prefix, bool prependAssemblyName)
		{

			if (prependAssemblyName)
			{
				prefix = assembly.GetName().Name + "." + prefix;
			}
			else
			{
			}

			if (!prefix.EndsWith("."))
			{
				prefix += ".";
			}

			AssetOpener result = assetName =>
			{
				assetName = assetName.Replace(AssetManager.SeparatorSymbol, '.');

				return Res.OpenResourceStream(assembly, prefix + assetName);
			};

			return result;
		}
	}
}
