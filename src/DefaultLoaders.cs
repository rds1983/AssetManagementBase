namespace AssetManagementBase
{
	public static class DefaultLoaders
	{
		private static AssetLoader<string> StringLoader = (ctx, assetName) => ctx.ReadAsText(assetName);
		private static AssetLoader<byte[]> ByteArrayLoader = (ctx, assetName) => ctx.ReadAsByteArray(assetName);

		public static string LoadString(this AssetManager assetManager, string assetName) => assetManager.UseLoader(StringLoader, assetName);
		public static byte[] LoadByteArray(this AssetManager assetManager, string assetName) => assetManager.UseLoader(ByteArrayLoader, assetName);
	}
}
