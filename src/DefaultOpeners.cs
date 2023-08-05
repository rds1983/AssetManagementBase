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
			if (string.IsNullOrEmpty(baseFolder))
			{
				throw new ArgumentNullException(nameof(baseFolder));
			}

			// Base folder shouldn't have separator char at the end
			if (baseFolder.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				baseFolder = baseFolder.Substring(0, baseFolder.Length - 1);
			}

			AssetOpener result = assetName =>
			{
				if (AssetManager.SeparatorSymbol != Path.DirectorySeparatorChar)
				{
					assetName = assetName.Replace(AssetManager.SeparatorSymbol, Path.DirectorySeparatorChar);
				}

				// Asset name should always have directory separator at the end
				// While base folder shouldnt
				// Hence such combine should work
				assetName = baseFolder + assetName;

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

			// Prefix shouldn't have dot at the end
			if (prefix.EndsWith("."))
			{
				prefix = prefix.Substring(0, prefix.Length - 1);
			}

			AssetOpener result = assetName =>
			{
				assetName = assetName.Replace(AssetManager.SeparatorSymbol, '.');

				// Asset name should always have dot at the end
				// While base folder shouldnt
				// Hence such combine should work
				assetName = prefix + assetName;

				return Res.OpenResourceStream(assembly, assetName);
			};

			return result;
		}
	}
}
