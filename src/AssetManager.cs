using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace AssetManagementBase
{
	public delegate Stream AssetOpener(string assetName);
	public delegate T AssetLoader<T>(AssetLoaderContext context, string assetName);

	public class AssetManager
	{
		public const char SeparatorSymbol = '/';
		private readonly Dictionary<string, object> _cache = new Dictionary<string, object>();
		private readonly AssetOpener _assetOpener;

		public AssetOpener AssetOpener => _assetOpener;

		public AssetManager(AssetOpener assetOpener)
		{
			_assetOpener = assetOpener ?? throw new ArgumentNullException(nameof(assetOpener));
		}

		public void ClearCache()
		{
			// TODO: resources disposal
			_cache.Clear();
		}

		internal Stream Open(string path)
		{
			var stream = _assetOpener(path);
			if (stream == null)
			{
				throw new Exception(string.Format("Can't open asset {0}", path));
			}

			return stream;
		}

		public T UseLoader<T>(AssetLoader<T> loader, string assetName, bool storeInCache = true)
		{
			assetName = assetName.Replace('\\', SeparatorSymbol);
			object cached;
			if (_cache.TryGetValue(assetName, out cached))
			{
				// Found in cache
				return (T)cached;
			}

			var baseFolder = string.Empty;
			var assetFileName = assetName;

			var separatorIndex = assetName.LastIndexOf(SeparatorSymbol);
			if (separatorIndex != -1)
			{
				baseFolder = assetName.Substring(0, separatorIndex);
				assetFileName = assetName.Substring(separatorIndex + 1);
			}

			var context = new AssetLoaderContext(this, baseFolder);
			var result = loader(context, assetFileName);
			if (storeInCache)
			{
				// Store in cache
				_cache[assetName] = result;
			}

			return result;
		}

		public static AssetManager CreateFileAssetManager(string baseFolder) => new AssetManager(DefaultOpeners.CreateFileOpener(baseFolder));
		public static AssetManager CreateResourceAssetManager(Assembly assembly, string prefix, bool prependAssemblyName = true) => new AssetManager(DefaultOpeners.CreateResourceOpener(assembly, prefix, prependAssemblyName));
	}
}