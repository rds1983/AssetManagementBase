using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace AssetManagementBase
{
	public delegate Stream AssetOpener(string assetName);
	public delegate T AssetLoader<T>(AssetManager assetManager, string assetName, object settings);

	public class AssetManager
	{
		public const char SeparatorSymbol = '/';
		public const string SeparatorString = "/";
		private readonly AssetOpener _assetOpener;
		private string _currentFolder = SeparatorString;

		public Dictionary<string, object> Cache { get; } = new Dictionary<string, object>();

		public AssetManager(AssetOpener assetOpener)
		{
			_assetOpener = assetOpener ?? throw new ArgumentNullException(nameof(assetOpener));
		}

		public void ClearCache()
		{
			// TODO: resources disposal
			Cache.Clear();
		}

		/// <summary>
		/// Opens a stream specified by asset path
		/// Throws an exception on failure
		/// </summary>
		/// <param name="assetName"></param>
		/// <returns></returns>
		public Stream OpenAsset(string assetName)
		{
			var path = CombinePath(_currentFolder, assetName);

			var stream = _assetOpener(path);
			if (stream == null)
			{
				throw new Exception(string.Format("Can't open asset {0}", path));
			}

			return stream;
		}

		/// <summary>
		/// Reads specified asset to string
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public string ReadAssetAsText(string path)
		{
			string result;
			using (var input = OpenAsset(path))
			{
				using (var textReader = new StreamReader(input))
				{
					result = textReader.ReadToEnd();
				}
			}

			return result;
		}

		/// <summary>
		/// Reads specified asset to string
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public byte[] ReadAssetAsByteArray(string path)
		{
			using (var input = OpenAsset(path))
			using (var ms = new MemoryStream())
			{
				input.CopyTo(ms);
				return ms.ToArray();
			}
		}

		public bool HasAsset(string assetName, object settings = null)
		{
			assetName = BuildFullPath(assetName);
			var cacheKey = BuildCacheKey(assetName, settings);

			return Cache.ContainsKey(cacheKey);
		}

		public T UseLoader<T>(AssetLoader<T> loader, string assetName, object settings = null)
		{
			assetName = BuildFullPath(assetName);
			var cacheKey = BuildCacheKey(assetName, settings);

			object cached;
			if (Cache.TryGetValue(cacheKey, out cached))
			{
				// Found in cache
				return (T)cached;
			}

			var oldFolder = _currentFolder;

			T result;

			try
			{
				var assetFileName = assetName;

				var separatorIndex = assetName.LastIndexOf(SeparatorSymbol);
				if (separatorIndex != -1)
				{
					_currentFolder = assetName.Substring(0, separatorIndex);
					if (string.IsNullOrEmpty(_currentFolder))
					{
						_currentFolder = SeparatorString;
					}
				}

				result = loader(this, assetFileName, settings);
			}
			finally
			{
				_currentFolder = oldFolder;
			}

			// Store in cache
			Cache[cacheKey] = result;

			return result;
		}

		private string BuildFullPath(string assetName)
		{
			assetName = assetName.Replace('\\', SeparatorSymbol);
			assetName = CombinePath(_currentFolder, assetName);

			return assetName;
		}

		private static string BuildCacheKey(string assetName, object settings)
		{
			var cacheKey = assetName;
			if (settings != null)
			{
				cacheKey += "|" + settings.ToString();

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

			if (_base[_base.Length - 1] == AssetManager.SeparatorSymbol)
			{
				return _base + url;
			}

			return _base + AssetManager.SeparatorSymbol + url;
		}

		public static AssetManager CreateFileAssetManager(string baseFolder) => new AssetManager(DefaultOpeners.CreateFileOpener(baseFolder));
		public static AssetManager CreateResourceAssetManager(Assembly assembly, string prefix, bool prependAssemblyName = true) => new AssetManager(DefaultOpeners.CreateResourceOpener(assembly, prefix, prependAssemblyName));
	}
}