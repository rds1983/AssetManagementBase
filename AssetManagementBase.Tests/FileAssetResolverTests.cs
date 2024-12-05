using NUnit.Framework;
using System;
using System.IO;

namespace AssetManagementBase.Tests
{
	[TestFixture]
	public class FileAssetResolverTests
	{
		private static AssetManager CreateAssetManager() => AssetManager.CreateFileAssetManager(Utility.ExecutingAssemblyDirectory);

		private static void TestUserProfile(UserProfile userProfile)
		{
			Assert.AreEqual(userProfile.Name, "AssetManagementBase");
			Assert.AreEqual(userProfile.Score, 10000);
		}

		private static void TestUserProfile(string assetName)
		{
			var assetManager = CreateAssetManager();
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

		[Test]
		public void LoadUserProfile()
		{
			TestUserProfile("userProfile.xml");
		}

		[Test]
		public void LoadUserProfilePathRooted()
		{
			TestUserProfile("@userProfile.xml");
		}


		[Test]
		public void WrongPath()
		{
			var assetManager = CreateAssetManager();

			Assert.Throws<Exception>(() =>
			{
				var userProfile = assetManager.LoadUserProfile("userProfile2.xml");
			});
		}

		[Test]
		public void FileExistance()
		{
			var assetManager = CreateAssetManager();

			Assert.IsTrue(assetManager.Exists("userProfile.xml"));
			Assert.IsTrue(assetManager.Exists("@userProfile.xml"));
			Assert.IsTrue(assetManager.Exists(Path.Combine(Utility.ExecutingAssemblyDirectory, "userProfile.xml")));
			Assert.IsFalse(assetManager.Exists("userProfile2.xml"));
		}
	}
}
