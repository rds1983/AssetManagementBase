using AssetManagementBase.Utility;
using NUnit.Framework;
using System;
using System.IO;

namespace AssetManagementBase.Tests
{
	[TestFixture]
	public class FileAssetResolverTests
	{
		private static AssetManager CreateExecutingDirectoryAssetManager() => AssetManager.CreateFileAssetManager(Utility.ExecutingAssemblyDirectory);

		private static void TestUserProfile(UserProfile userProfile)
		{
			Assert.AreEqual(userProfile.Name, "AssetManagementBase");
			Assert.AreEqual(userProfile.Score, 10000);
		}

		private static void TestUserProfile(AssetManager assetManager, string assetName)
		{
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
		public void TestPaths()
		{
			// Make sure separator symbol is removed from the folder end
			var path = Utility.ExecutingAssemblyDirectory;
			if (!path.EndsWith(Path.DirectorySeparatorChar))
			{
				path += Path.DirectorySeparatorChar;
			}
			var assetManager = AssetManager.CreateFileAssetManager(path);
			Assert.IsFalse(assetManager.CurrentFolder.EndsWith(PathUtils.SeparatorSymbol));

			var basePath = assetManager.CurrentFolder;

			// Test path combines
			var path1 = basePath + PathUtils.SeparatorSymbol + "userProfile.xml";
			var assetPath = assetManager.BuildFullPath("userProfile.xml");
			Assert.AreEqual(path1, assetPath);

			assetPath = assetManager.BuildFullPath("/userProfile.xml");
			Assert.AreEqual(path1, assetPath);

			assetPath = assetManager.BuildFullPath("files/userProfile.xml");

			var amr = assetManager.GetSubManagerForAsset(assetPath);
			var subManager = amr.AssetManager;
			Assert.AreEqual(basePath + PathUtils.SeparatorSymbol + "files", subManager.CurrentFolder);

			assetPath = subManager.BuildFullPath("userProfile.xml");
			Assert.AreEqual(basePath + PathUtils.SeparatorSymbol + "files/userProfile.xml", assetPath);

			// If path is rooted, then base folder should be reset
			assetPath = subManager.BuildFullPath("/userProfile.xml");
			Assert.AreEqual(path1, assetPath);

			assetPath = assetManager.BuildFullPath("files/../userProfile.xml");
			Assert.AreEqual(path1, assetPath);

			assetPath = assetManager.BuildFullPath("/files/../userProfile.xml");
			Assert.AreEqual(path1, assetPath);

			assetPath = assetManager.BuildFullPath("files/../files/../userProfile.xml");
			Assert.AreEqual(path1, assetPath);

			assetPath = assetManager.BuildFullPath("/files/../files/../userProfile.xml");
			Assert.AreEqual(path1, assetPath);

			assetPath = assetManager.BuildFullPath("files/files/../../userProfile.xml");
			Assert.AreEqual(path1, assetPath);

			assetPath = assetManager.BuildFullPath("/files/files/../../userProfile.xml");
			Assert.AreEqual(path1, assetPath);
		}

		[Test]
		public void LoadUserProfile()
		{
			TestUserProfile(CreateExecutingDirectoryAssetManager(), "userProfile.xml");
		}

		[Test]
		public void LoadUserProfilePathRooted()
		{
			TestUserProfile(CreateExecutingDirectoryAssetManager(), "/userProfile.xml");
		}


		[Test]
		public void WrongPath()
		{
			var assetManager = CreateExecutingDirectoryAssetManager();

			Assert.Throws<Exception>(() =>
			{
				var userProfile = assetManager.LoadUserProfile("userProfile2.xml");
			});

			Assert.Throws<Exception>(() =>
			{
				// Rooted paths should fail on asset managers with base folder
				var userProfile = assetManager.LoadUserProfile(Path.Combine(Utility.ExecutingAssemblyDirectory, "userProfile.xml"));
			});
		}

		[Test]
		public void FileExistance()
		{
			var assetManager = CreateExecutingDirectoryAssetManager();

			Assert.IsTrue(assetManager.Exists("userProfile.xml"));
			Assert.IsTrue(assetManager.Exists("/userProfile.xml"));

			// Rooted paths should fail on asset managers with base folder
			Assert.IsFalse(assetManager.Exists(Path.Combine(Utility.ExecutingAssemblyDirectory, "userProfile.xml")));
			Assert.IsFalse(assetManager.Exists("userProfile2.xml"));
		}

		[Test]
		public void RootedManager()
		{
			var assetManager = AssetManager.CreateRootedFileAssetManager();
			var fullPath = Path.Combine(Utility.ExecutingAssemblyDirectory, "userProfile.xml");
			TestUserProfile(assetManager, fullPath);
		}
	}
}
