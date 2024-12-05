using System.Collections.Generic;
using System.IO;
using System;

namespace AssetManagementBase
{
	internal class AssetManagerCore
	{
		private readonly IAssetAccessor _assetAccessor;

		public string Name => _assetAccessor.Name;
		public string BaseFolder { get; }

		public Dictionary<string, object> Cache { get; } = new Dictionary<string, object>();

		public AssetManagerCore(IAssetAccessor assetOpener, string baseFolder)
		{
			_assetAccessor = assetOpener ?? throw new ArgumentNullException(nameof(assetOpener));
			BaseFolder = baseFolder;
		}

		public bool Exists(string path) => _assetAccessor.Exists(path);

		public Stream Open(string path) => _assetAccessor.Open(path);

		/// <summary>
		/// Reads asset stream as string
		/// </summary>
		/// <returns></returns>
		public string ReadAsString(string assetName)
		{
			string result;

			using (var stream = Open(assetName))
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
		public byte[] ReadAsByteArray(string assetName)
		{
			using (var stream = Open(assetName))
			using (var ms = new MemoryStream())
			{
				stream.CopyTo(ms);
				return ms.ToArray();
			}
		}
	}
}
