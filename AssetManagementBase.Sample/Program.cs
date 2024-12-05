namespace AssetManagementBase.Sample
{
	internal class Program
	{
		static void Main(string[] args)
		{
			var assetManager = AssetManager.CreateFileAssetManager(Utility.ExecutingAssemblyDirectory);

			var userProfile = assetManager.LoadUserProfile("userProfile.xml");
		}
	}
}
