using System.Collections.Generic;
using System.IO;
using System;

namespace AssetManagementBase
{
	internal class AssetManagerCore
	{
		private readonly AssetOpener _assetOpener;

		public Dictionary<string, object> Cache { get; } = new Dictionary<string, object>();

		public AssetManagerCore(AssetOpener assetOpener)
		{
			_assetOpener = assetOpener ?? throw new ArgumentNullException(nameof(assetOpener));
		}

		public Stream OpenAssetStream(string assetName) => _assetOpener(assetName);

		/// <summary>
		/// Reads asset stream as string
		/// </summary>
		/// <returns></returns>
		public string ReadAssetAsString(string assetName)
		{
			string result;

			using (var stream = OpenAssetStream(assetName))
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
		public byte[] ReadAssetAsByteArray(string assetName)
		{
			using (var stream = OpenAssetStream(assetName))
			using (var ms = new MemoryStream())
			{
				stream.CopyTo(ms);
				return ms.ToArray();
			}
		}
	}
}
