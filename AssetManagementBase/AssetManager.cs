using AssetManagementBase.Utility;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace AssetManagementBase
{
	/// <summary>
	/// Delegate for custom asset loading logic.
	/// </summary>
	/// <typeparam name="T">The type of asset to load</typeparam>
	/// <param name="manager">AssetManager instance with context-aware path resolution for the current asset's folder.
	/// Use this to load dependent assets relative to the current asset's location.</param>
	/// <param name="assetName">Name of the asset file (already resolved to filename only)</param>
	/// <param name="settings">Optional settings that affect loading behavior and cache key generation</param>
	/// <param name="tag">Optional user-defined tag for custom loader-specific purposes</param>
	/// <returns>The loaded asset of type T</returns>
	public delegate T AssetLoader<T>(AssetManager manager, string assetName, IAssetSettings settings, object tag);

	/// <summary>
	/// Manages loading of assets with automatic caching and support for recursive asset loading.
	/// </summary>
	/// <remarks>
	/// AssetManager supports flexible path resolution:
	/// - Relative paths (e.g., "config/settings.xml"): resolved relative to current folder context
	/// - Rooted paths (e.g., "/config/settings.xml"): resolved from base folder
	/// - Absolute paths (e.g., "@C:\Assets\config.xml"): explicit file system paths using @ prefix
	///
	/// All paths are normalized: backslashes become forward slashes, and ".." sequences are resolved.
	/// For recursive loading, the manager parameter passed to loaders provides context-aware resolution
	/// so relative references work correctly at any nesting level.
	/// </remarks>
	public class AssetManager
	{
		private readonly AssetManagerCore _core;
		private readonly string _currentFolder = string.Empty;

		/// <summary>
		/// Gets the current folder context for relative path resolution.
		/// </summary>
		public string CurrentFolder => _currentFolder;

		/// <summary>
		/// Gets the shared asset cache. Cache keys are generated from the full asset path and optional settings.
		/// </summary>
		public Dictionary<string, object> Cache => _core.Cache;

		/// <summary>
		/// Creates a new AssetManager with the specified accessor and base folder.
		/// </summary>
		/// <param name="assetAccesssor">The accessor implementation for reading assets</param>
		/// <param name="currentFolder">The base folder for all asset resolution</param>
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

		/// <summary>
		/// Clears all cached assets.
		/// </summary>
		public void Unload()
		{
			Cache.Clear();
		}

		/// <summary>
		/// Checks if an asset exists at the specified path.
		/// </summary>
		/// <param name="assetName">Asset path (supports relative, rooted, or absolute formats)</param>
		/// <returns>True if the asset exists, false otherwise</returns>
		public bool Exists(string assetName) => _core.Exists(BuildFullPath(assetName));

		/// <summary>
		/// Opens an asset stream for reading.
		/// </summary>
		/// <param name="assetName">Asset path (supports relative, rooted, or absolute formats)</param>
		/// <returns>Stream for reading the asset content</returns>
		public Stream Open(string assetName) => _core.Open(BuildFullPath(assetName));

		/// <summary>
		/// Reads an asset as a string.
		/// </summary>
		/// <param name="assetName">Asset path (supports relative, rooted, or absolute formats)</param>
		/// <returns>Asset content as string</returns>
		public string ReadAsString(string assetName) => _core.ReadAsString(BuildFullPath(assetName));

		/// <summary>
		/// Reads an asset as a byte array.
		/// </summary>
		/// <param name="assetName">Asset path (supports relative, rooted, or absolute formats)</param>
		/// <returns>Asset content as byte array</returns>
		public byte[] ReadAsByteArray(string assetName) => _core.ReadAsByteArray(BuildFullPath(assetName));

		/// <summary>
		/// Resolves an asset path to its full canonical path using flexible path resolution.
		/// </summary>
		/// <remarks>
		/// Path resolution rules (in order of precedence):
		/// 1. Windows absolute paths (e.g., "C:\path") - used as-is
		/// 2. Explicit rooted paths starting with @ (e.g., "@C:\path") - @ is stripped, path used as-is
		/// 3. Base-relative rooted paths starting with / (e.g., "/config.xml") - resolved from base folder
		/// 4. Relative paths (e.g., "config.xml" or "sub/config.xml") - resolved from current folder context
		///
		/// All paths are normalized: backslashes → forward slashes, ".." sequences are resolved.
		/// </remarks>
		private string BuildFullPath(string assetName)
		{
			assetName = assetName.FixFilePath();

			if (assetName.IsWindowsPathRooted())
			{
				// Windows absolute path - use as-is
			}
			else if (assetName.StartsWith(PathUtils.RootedPathSymbol))
			{
				// Explicit rooted path with @ prefix - strip it
				assetName = assetName.Substring(1);
			}
			else if (assetName.StartsWith(PathUtils.SeparatorString))
			{
				// Base-relative path starting with / - resolve from base folder
				assetName = PathUtils.CombinePath(_core.BaseFolder, assetName);
			}
			else
			{
				// Relative path - resolve from current folder context
				assetName = PathUtils.CombinePath(_currentFolder, assetName);
			}

			return PathUtils.ProcessPath(assetName);
		}

		/// <summary>
		/// Checks if an asset is cached with optional settings-based cache differentiation.
		/// </summary>
		/// <param name="assetName">Asset path (supports relative, rooted, or absolute formats)</param>
		/// <param name="settings">Optional settings that affect the cache key. Different settings = different cache entries.</param>
		/// <returns>True if the asset is cached with the given settings, false otherwise</returns>
		public bool IsCached(string assetName, IAssetSettings settings = null)
		{
			assetName = BuildFullPath(assetName);
			var cacheKey = BuildCacheKey(assetName, settings);

			return _core.Cache.ContainsKey(cacheKey);
		}

		/// <summary>
		/// Loads an asset using a custom loader delegate with automatic caching.
		/// </summary>
		/// <remarks>
		/// The loader receives a context-aware AssetManager instance where the current folder is set to the asset's parent directory.
		/// This enables recursive loading where referenced assets use relative paths that resolve correctly.
		/// Assets are cached by their full path and optional settings. The same asset with different settings creates separate cache entries.
		/// </remarks>
		/// <typeparam name="T">The type of asset to load</typeparam>
		/// <param name="loader">Custom loader delegate that reads and parses the asset</param>
		/// <param name="assetName">Asset path (supports relative, rooted, or absolute formats)</param>
		/// <param name="settings">Optional settings affecting load behavior and cache key. Pass different settings to create separate cache entries.</param>
		/// <param name="tag">Optional user-defined object passed to the loader for custom purposes</param>
		/// <param name="storeInCache">Whether to cache the loaded asset (default: true)</param>
		/// <returns>The loaded asset, from cache if available</returns>
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

		/// <summary>
		/// Creates an AssetManager that loads files from the file system.
		/// </summary>
		/// <param name="baseFolder">The root folder where assets are located</param>
		/// <returns>A new AssetManager for file-based asset loading</returns>
		public static AssetManager CreateFileAssetManager(string baseFolder) => new AssetManager(new FileAssetAccessor(), baseFolder);

		/// <summary>
		/// Creates an AssetManager that loads resources from an assembly.
		/// </summary>
		/// <param name="assembly">The assembly containing embedded resources</param>
		/// <param name="prefix">The namespace prefix for resources (e.g., "MyApp.Resources")</param>
		/// <param name="prependAssemblyName">If true, the assembly name is prepended to the prefix.
		/// For assembly "MyApp.dll" with prefix "Resources", the final prefix becomes "MyApp.Resources".
		/// If false, the prefix is used as-is.</param>
		/// <returns>A new AssetManager for resource-based asset loading</returns>
		public static AssetManager CreateResourceAssetManager(Assembly assembly, string prefix, bool prependAssemblyName = true) =>
			new AssetManager(new ResourceAssetAccessor(assembly), ResourceAssetAccessor.BuildPrefix(assembly, prefix, prependAssemblyName));
	}
}