using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using AssetManagementBase.Utility;

namespace AssetManagementBase
{
	internal class ResourceAssetAccessor : IAssetAccessor
	{
		private readonly Assembly _assembly;
		private readonly HashSet<string> _resourceNames;

		public string Name => $"Res:{_assembly.GetName().Name}";

		public ResourceAssetAccessor(Assembly assembly)
		{
			_assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
			_resourceNames = new HashSet<string>(assembly.GetManifestResourceNames());
		}

		private string ToPlatformPath(string fullPath) => fullPath.Replace(PathUtils.SeparatorSymbol, '.');

		public bool Exists(string path) => _resourceNames.Contains(ToPlatformPath(path));

		public Stream Open(string path) => _assembly.OpenResourceStream(ToPlatformPath(path));

		public static string BuildPrefix(Assembly assembly, string prefix, bool prependAssemblyName)
		{
			if (!string.IsNullOrEmpty(prefix))
			{
				if (prependAssemblyName)
				{
					prefix = assembly.GetName().Name + "." + prefix;
				}
			}

			return prefix.Replace('.', PathUtils.SeparatorSymbol);
		}
	}
}
