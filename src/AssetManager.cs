using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace AssetManagementBase
{
	public delegate T AssetLoader<T>(AssetManager manager, string assetName, IAssetSettings settings, object tag);

	public class AssetManager
	{
		internal const char SeparatorSymbol = '/';
		internal const string SeparatorString = "/";

		private readonly AssetManagerCore _core;
		private readonly string _currentFolder = SeparatorString;

		public string CurrentFolder => _currentFolder;
		public Dictionary<string, object> Cache => _core.Cache;

		public AssetManager(IAssetAccessor assetAccesssor)
		{
			_core = new AssetManagerCore(assetAccesssor);
		}

		internal AssetManager(AssetManagerCore core, string currentFolder)
		{
			_core = core;
			_currentFolder = currentFolder;
		}

		public bool Exists(string assetName) => _core.Exists(BuildFullPath(assetName));

		public Stream Open(string assetName) => _core.Open(BuildFullPath(assetName));

		/// <summary>
		/// Reads asset stream as string
		/// </summary>
		/// <returns></returns>
		public string ReadAsString(string assetName) => _core.ReadAsString(BuildFullPath(assetName));

		/// <summary>
		/// Reads asset stream as byte array
		/// </summary>
		/// <returns></returns>
		public byte[] ReadAsByteArray(string assetName) => _core.ReadAsByteArray(BuildFullPath(assetName));


		public bool IsCached(string assetName, IAssetSettings settings = null)
		{
			assetName = BuildFullPath(assetName);
			var cacheKey = BuildCacheKey(assetName, settings);

			return _core.Cache.ContainsKey(cacheKey);
		}

		public T UseLoader<T>(AssetLoader<T> loader, string assetName, IAssetSettings settings = null, object tag = null, bool storeInCache = true)
		{
			assetName = BuildFullPath(assetName);
			var cacheKey = BuildCacheKey(assetName, settings);

			object cached;
			if (_core.Cache.TryGetValue(cacheKey, out cached))
			{
				// Found in cache
				return (T)cached;
			}

			var assetManager = this;
			var separatorIndex = assetName.LastIndexOf(SeparatorSymbol);
			if (separatorIndex != -1)
			{
				var assetFolder = assetName.Substring(0, separatorIndex);
				if (!string.IsNullOrEmpty(assetFolder))
				{
					assetManager = new AssetManager(_core, assetFolder);
				}
			}

			var result = loader(assetManager, assetName, settings, tag);

			if (storeInCache)
			{
				// Store in cache
				_core.Cache[cacheKey] = result;
			}

			return result;
		}

		private string BuildFullPath(string assetName)
		{
			assetName = assetName.Replace('\\', SeparatorSymbol);
			assetName = CombinePath(_currentFolder, assetName);

			return assetName;
		}

		private static string BuildCacheKey(string assetName, IAssetSettings settings)
		{
			var cacheKey = assetName;
			if (settings != null)
			{
				cacheKey += "|" + settings.BuildKey();

			}

			return cacheKey;
		}

		private static string CombinePath(string _base, string url)
		{
			if (string.IsNullOrEmpty(_base))
			{
				return url;
			}

			if (string.IsNullOrEmpty(url))
			{
				return _base;
			}

			if (url[0] == SeparatorSymbol)
			{
				// Path is rooted
				return url;
			}

			if (_base[_base.Length - 1] == SeparatorSymbol)
			{
				return _base + url;
			}

			return _base + SeparatorSymbol + url;
		}

		public static AssetManager CreateFileAssetManager(string baseFolder) => new AssetManager(new FileAssetAccessor(baseFolder));
		public static AssetManager CreateResourceAssetManager(Assembly assembly, string prefix, bool prependAssemblyName = true) => new AssetManager(new ResourceAssetAccessor(assembly, prefix, prependAssemblyName));
	}
}