using AssetManagementBase.Utility;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace AssetManagementBase
{
	public delegate T AssetLoader<T>(AssetManager manager, string assetName, IAssetSettings settings, object tag);

	public class AssetManager
	{
		private readonly AssetManagerCore _core;
		private readonly string _currentFolder = string.Empty;

		public string CurrentFolder => _currentFolder;
		public Dictionary<string, object> Cache => _core.Cache;

		public AssetManager(IAssetAccessor assetAccesssor, string currentFolder)
		{
			currentFolder = currentFolder.FixFolderPath();
			_core = new AssetManagerCore(assetAccesssor, currentFolder);
			_currentFolder = currentFolder;
		}

		internal AssetManager(AssetManagerCore core, string currentFolder)
		{
			_core = core;
			_currentFolder = currentFolder;
		}

		public void Unload()
		{
			Cache.Clear();
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

		private string BuildFullPath(string assetName)
		{
			assetName = assetName.FixFilePath();

			if (assetName.IsWindowsPathRooted())
			{
				// Do nothing, since path is rooted
			}
			else if (assetName.StartsWith(PathUtils.RootedPathSymbol))
			{
				// This symbol at the beginnig also specifies the path is rooted
				assetName = assetName.Substring(1);
			}
			else if (assetName.StartsWith(PathUtils.SeparatorString))
			{
				// Path is rooted from the base asset manager
				assetName = PathUtils.CombinePath(_core.BaseFolder, assetName);
			}
			else
			{
				// Not rooted
				assetName = PathUtils.CombinePath(_currentFolder, assetName);
			}

			// Fix '..'
			return PathUtils.ProcessPath(assetName);
		}

		public bool IsCached(string assetName, IAssetSettings settings = null)
		{
			assetName = BuildFullPath(assetName);
			var cacheKey = BuildCacheKey(assetName, settings);

			return _core.Cache.ContainsKey(cacheKey);
		}

		public T UseLoader<T>(AssetLoader<T> loader, string assetName, IAssetSettings settings = null, object tag = null, bool storeInCache = true)
		{
			var fullPath = BuildFullPath(assetName);
			var cacheKey = BuildCacheKey(fullPath, settings);

			object cached;
			if (_core.Cache.TryGetValue(cacheKey, out cached))
			{
				// Found in cache
				return (T)cached;
			}

			var assetManager = this;
			var separatorIndex = fullPath.LastIndexOf(PathUtils.SeparatorSymbol);
			if (separatorIndex != -1)
			{
				assetName = fullPath.Substring(separatorIndex + 1);
				var assetFolder = fullPath.Substring(0, separatorIndex);
				if (!string.IsNullOrEmpty(assetFolder) && assetFolder != _currentFolder)
				{
					assetManager = new AssetManager(_core, assetFolder);
				}
			}

			if (AMBConfiguration.Logger != null)
			{
				AMBConfiguration.Logger($"AMB: Loading asset '{fullPath}' of type '{typeof(T).Name}' from '{_core.Name}'");
			}

			var result = loader(assetManager, assetName, settings, tag);
			if (storeInCache)
			{
				// Store in cache
				_core.Cache[cacheKey] = result;
			}

			return result;
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

		public static AssetManager CreateFileAssetManager(string baseFolder) => new AssetManager(new FileAssetAccessor(), baseFolder);

		public static AssetManager CreateResourceAssetManager(Assembly assembly, string prefix, bool prependAssemblyName = true) =>
			new AssetManager(new ResourceAssetAccessor(assembly), ResourceAssetAccessor.BuildPrefix(assembly, prefix, prependAssemblyName));
	}
}