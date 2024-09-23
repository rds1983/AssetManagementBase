using System.Collections.Generic;
using System.IO;
using System;

namespace AssetManagementBase
{
	internal class AssetManagerCore
	{
		private readonly IAssetAccessor _assetAccessor;

		public Dictionary<string, object> Cache { get; } = new Dictionary<string, object>();

		public AssetManagerCore(IAssetAccessor assetOpener)
		{
			_assetAccessor = assetOpener ?? throw new ArgumentNullException(nameof(assetOpener));
		}

		public bool Exists(string assetName) => _assetAccessor.Exists(assetName);

		public Stream Open(string assetName) => _assetAccessor.Open(assetName);

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
