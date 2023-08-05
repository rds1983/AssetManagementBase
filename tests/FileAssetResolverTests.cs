using AssetManagementBase.Utility;
using NUnit.Framework;
using System;

namespace AssetManagementBase.Tests
{
	[TestFixture]
	public class FileAssetResolverTests
	{
		[Test]
		public void LoadUserProfile()
		{
			var assetManager = AssetManager.CreateFileAssetManager(Utility.ExecutingAssemblyDirectory);
			var userProfile = assetManager.LoadUserProfile("userProfile.xml");

			Assert.AreEqual(userProfile.Name, "AssetManagementBase");
			Assert.AreEqual(userProfile.Score, 10000);
		}

		[Test]
		public void WrongPath()
		{
			var assetManager = AssetManager.CreateFileAssetManager(Utility.ExecutingAssemblyDirectory);

			Assert.Throws<Exception>(() =>
			{
				var userProfile = assetManager.LoadUserProfile("userProfile2.xml");
			});
		}
	}
}
