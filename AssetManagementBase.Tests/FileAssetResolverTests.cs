using NUnit.Framework;
using System;
using System.IO;

namespace AssetManagementBase.Tests
{
	[TestFixture]
	public class FileAssetResolverTests
	{
		private static string AssetPath = Path.Combine(Utility.ExecutingAssemblyDirectory, "FileAssets");
		private static AssetManager CreateExecutingDirectoryAssetManager() => AssetManager.CreateFileAssetManager(AssetPath);

		private static void TestUserProfile(UserProfile userProfile)
		{
			Assert.AreEqual(userProfile.Name, "AssetManagementBase");
			Assert.AreEqual(userProfile.Score, 10000);
		}

		private static void TestUserProfile(string[] assetNames)
		{
			var assetManager = CreateExecutingDirectoryAssetManager();

			// All provided asset names should point to same asset
			for (var i = 0; i < assetNames.Length; ++i)
			{
				var assetName = assetNames[i];
				Assert.IsTrue(assetManager.Exists(assetName));
				var userProfile = assetManager.LoadUserProfile(assetName);

				TestUserProfile(userProfile);
				Assert.AreEqual(assetManager.Cache.Count, 1);
				Assert.IsTrue(assetManager.IsCached(assetName));

				// Test second access of the same resource
				userProfile = assetManager.LoadUserProfile(assetName);
				TestUserProfile(userProfile);
				Assert.AreEqual(assetManager.Cache.Count, 1);
				Assert.IsTrue(assetManager.IsCached(assetName));
			}
		}

		[Test]
		public void LoadUserProfile()
		{
			// All these expression point to one file
			TestUserProfile
			([
				"userProfile.xml",
				"/userProfile.xml",
				"files/../userProfile.xml",
				"/files/../userProfile.xml",
				"files/../files/../userProfile.xml",
				"/files/../files/../userProfile.xml",
				"files/files/../../userProfile.xml",
				"/files/files/../../userProfile.xml",
				"@" + Path.Combine(AssetPath, "userProfile.xml"),
				"@" + Path.Combine(AssetPath, "files/../userProfile.xml")
			]);

			TestUserProfile
			([
				"files/userProfile.xml",
				"/files/userProfile.xml",
				"files/files/../userProfile.xml",
				"/files/files/../userProfile.xml",
				"files/files/../files/../userProfile.xml",
				"/files/files/../files/../userProfile.xml",
				"files/files/files/../../userProfile.xml",
				"/files/files/files/../../userProfile.xml",
				"@" + Path.Combine(AssetPath, "files/userProfile.xml"),
				"@" + Path.Combine(AssetPath, "files/files/../userProfile.xml")
			]);
		}


		[Test]
		public void WrongPath()
		{
			var assetManager = CreateExecutingDirectoryAssetManager();

			Assert.Throws<Exception>(() =>
			{
				var userProfile = assetManager.LoadUserProfile("userProfile2.xml");
			});
		}
	}
}
