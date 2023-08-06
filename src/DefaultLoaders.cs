namespace AssetManagementBase
{
	public static class DefaultLoaders
	{
		private static AssetLoader<string> StringLoader = context => context.ReadDataAsString();
		private static AssetLoader<byte[]> ByteArrayLoader = context => context.ReadAssetAsByteArray();

		public static string LoadString(this AssetManager assetManager, string assetName, bool storeInCache = true) => assetManager.UseLoader(StringLoader, assetName, storeInCache: storeInCache);
		public static byte[] LoadByteArray(this AssetManager assetManager, string assetName, bool storeInCache = true) => assetManager.UseLoader(ByteArrayLoader, assetName, storeInCache: storeInCache);
	}
}
