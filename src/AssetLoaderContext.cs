using System;
using System.IO;

namespace AssetManagementBase
{
	public class AssetLoaderContext
	{
		private readonly string _baseFolder;

		public AssetManager AssetManager { get; }

		internal AssetLoaderContext(AssetManager assetManager, string baseFolder)
		{
			AssetManager = assetManager ?? throw new Exception("assetManager");
			_baseFolder = baseFolder;
		}

		/// <summary>
		/// Opens a stream specified by asset path
		/// Throws an exception on failure
		/// </summary>
		/// <param name="assetName"></param>
		/// <returns></returns>
		public Stream Open(string assetName)
		{
			return AssetManager.Open(CombinePath(_baseFolder, assetName));
		}

		/// <summary>
		/// Reads specified asset to string
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public string ReadAsText(string path)
		{
			string result;
			using (var input = Open(path))
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
		public byte[] ReadAsByteArray(string path)
		{
			using (var input = Open(path))
			using (var ms = new MemoryStream())
			{
				input.CopyTo(ms);
				return ms.ToArray();
			}
		}

		/// <summary>
		/// Builds path relative to the asset that is being loaded
		/// </summary>
		/// <param name="relativePath"></param>
		/// <returns></returns>
		public string BuildAbsolutePath(string relativePath) => CombinePath(_baseFolder, relativePath);

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

			if (_base[_base.Length - 1] == AssetManager.SeparatorSymbol)
			{
				return _base + url;
			}

			return _base + AssetManager.SeparatorSymbol + url;
		}
	}
}