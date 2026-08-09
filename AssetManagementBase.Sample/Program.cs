using System;

namespace AssetManagementBase.Sample
{
	internal class Program
	{
		static void Main(string[] args)
		{
			var assetManager = AssetManager.CreateFileAssetManager(AppDomain.CurrentDomain.BaseDirectory);

			var userProfile = assetManager.LoadUserProfile("@userProfile.xml");
		}
	}
}
