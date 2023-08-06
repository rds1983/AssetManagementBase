using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace AssetManagementBase
{
	public struct AssetLoaderContext
	{
		public AssetManager Manager;
		public string Name;
		public Func<Stream> DataStreamOpener;
		public object Settings;

		/// <summary>
		/// Reads asset stream as string
		/// </summary>
		/// <returns></returns>
		public string ReadDataAsString()
		{
			string result;

			using (var stream = DataStreamOpener())
			using (var textReader = new StreamReader(stream))
			{
				result = textReader.ReadToEnd();
			}

			return result;
		}

		/// <summary>
		/// Reads asset stream as byte array
		/// </summary>
		/// <returns></returns>
		public byte[] ReadAssetAsByteArray()
		{
			using (var stream = DataStreamOpener())
			using (var ms = new MemoryStream())
			{
				stream.CopyTo(ms);
				return ms.ToArray();
			}
		}
	}

	public delegate Stream AssetOpener(string assetName);
	public delegate T AssetLoader<T>(AssetLoaderContext context);

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

		public bool HasAsset(string assetName, object settings = null)
		{
			assetName = BuildFullPath(assetName);
			var cacheKey = BuildCacheKey(assetName, settings);

			return Cache.ContainsKey(cacheKey);
		}

		private Func<Stream> CreateStreamOpener(string assetName)
		{
			return () =>
			{
				var stream = _assetOpener(assetName);

				if (stream == null)
				{
					throw new Exception(string.Format("Can't open asset {0}", assetName));
				}

				return stream;
			};
		}

		public T UseLoader<T>(AssetLoader<T> loader, string assetName, object settings = null, bool storeInCache = true)
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
				var separatorIndex = assetName.LastIndexOf(SeparatorSymbol);
				if (separatorIndex != -1)
				{
					_currentFolder = assetName.Substring(0, separatorIndex);
					if (string.IsNullOrEmpty(_currentFolder))
					{
						_currentFolder = SeparatorString;
					}
				}

				var context = new AssetLoaderContext
				{
					Manager = this,
					Name = assetName,
					DataStreamOpener = CreateStreamOpener(assetName),
					Settings = settings,
				};

				result = loader(context);
			}
			finally
			{
				_currentFolder = oldFolder;
			}

			if (storeInCache)
			{
				// Store in cache
				Cache[cacheKey] = result;
			}

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