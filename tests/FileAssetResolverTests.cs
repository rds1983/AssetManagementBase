using NUnit.Framework;
using System;

namespace AssetManagementBase.Tests
{
	[TestFixture]
	public class FileAssetResolverTests
	{
		private static void TestUserProfile(UserProfile userProfile)
		{
			Assert.AreEqual(userProfile.Name, "AssetManagementBase");
			Assert.AreEqual(userProfile.Score, 10000);
		}

		private static void TestUserProfile(string assetName)
		{
			var assetManager = AssetManager.CreateFileAssetManager(Utility.ExecutingAssemblyDirectory);
			var userProfile = assetManager.LoadUserProfile(assetName);

			TestUserProfile(userProfile);
			Assert.AreEqual(assetManager.Cache.Count, 1);
			Assert.IsTrue(assetManager.HasAsset(assetName));

			// Test second access of the same resource
			userProfile = assetManager.LoadUserProfile(assetName);
			TestUserProfile(userProfile);
			Assert.AreEqual(assetManager.Cache.Count, 1);
			Assert.IsTrue(assetManager.HasAsset(assetName));
		}

		[Test]
		public void LoadUserProfile()
		{
			TestUserProfile("userProfile.xml");
		}

		[Test]
		public void LoadUserProfilePathRooted()
		{
			TestUserProfile("/userProfile.xml");
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
