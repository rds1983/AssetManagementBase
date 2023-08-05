namespace AssetManagementBase
{
	public static class DefaultLoaders
	{
		private static AssetLoader<string> StringLoader = (context, assetName, settings) => context.ReadAssetAsText(assetName);
		private static AssetLoader<byte[]> ByteArrayLoader = (context, assetName, settings) => context.ReadAssetAsByteArray(assetName);

		public static string LoadString(this AssetManager assetManager, string assetName) => assetManager.UseLoader(StringLoader, assetName);
		public static byte[] LoadByteArray(this AssetManager assetManager, string assetName) => assetManager.UseLoader(ByteArrayLoader, assetName);
	}
}
