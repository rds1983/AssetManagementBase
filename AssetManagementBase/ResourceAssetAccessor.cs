using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using AssetManagementBase.Utility;

namespace AssetManagementBase
{
	internal class ResourceAssetAccessor: IAssetAccessor
	{
		private readonly Assembly _assembly;
		private readonly string _prefix;
		private readonly HashSet<string> _resourceNames;

		public string Name => $"Res:{_assembly.GetName().Name}/{_prefix}";

		public ResourceAssetAccessor(Assembly assembly, string prefix, bool prependAssemblyName)
		{
			_assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
			_resourceNames = new HashSet<string>(assembly.GetManifestResourceNames());

			if (!string.IsNullOrEmpty(prefix))
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
			}

			_prefix = prefix;
		}

		public bool Exists(string assetName) => _resourceNames.Contains(BuildResourcePath(assetName));

		public Stream Open(string assetName) => _assembly.OpenResourceStream(BuildResourcePath(assetName));

		private string BuildResourcePath(string assetName)
		{
			assetName =	assetName.Replace(AssetManager.SeparatorSymbol, '.');

			// Asset name should always have dot at the end
			// While base folder shouldnt
			// Hence such combine should work
			assetName = _prefix + assetName;

			return assetName;
		}
	}
}
