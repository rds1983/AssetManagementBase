namespace AssetManagementBase
{
	public static class DefaultLoaders
	{
		private static AssetLoader<string> StringLoader = (manager, assetName, settings) => manager.ReadAssetAsString(assetName);
		private static AssetLoader<byte[]> ByteArrayLoader = (manager, assetName, settings) => manager.ReadAssetAsByteArray(assetName);

		public static string LoadString(this AssetManager assetManager, string assetName, bool storeInCache = true) => assetManager.UseLoader(StringLoader, assetName, storeInCache: storeInCache);
		public static byte[] LoadByteArray(this AssetManager assetManager, string assetName, bool storeInCache = true) => assetManager.UseLoader(ByteArrayLoader, assetName, storeInCache: storeInCache);
	}
}
