namespace AssetManagementBase
{
	public static class DefaultLoaders
	{
		private static AssetLoader<string> StringLoader = (manager, assetName, settings, tag) => manager.ReadAsString(assetName);
		private static AssetLoader<byte[]> ByteArrayLoader = (manager, assetName, settings, tag) => manager.ReadAsByteArray(assetName);

		public static string LoadString(this AssetManager assetManager, string assetName, bool storeInCache = true) => assetManager.UseLoader(StringLoader, assetName, storeInCache: storeInCache);
		public static byte[] LoadByteArray(this AssetManager assetManager, string assetName, bool storeInCache = true) => assetManager.UseLoader(ByteArrayLoader, assetName, storeInCache: storeInCache);
	}
}
