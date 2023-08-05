using NUnit.Framework;
using System;
using System.Reflection;

namespace AssetManagementBase.Tests
{
	[TestFixture]
	public class ResourceAssetResolverTests
	{
		private static readonly Assembly _assembly = typeof(ResourceAssetResolverTests).Assembly;

		private static void TestResourceAccess(string prefix, bool prependAssemblyName, string assetName)
		{
			var assetManager = AssetManager.CreateResourceAssetManager(_assembly, prefix, prependAssemblyName);
			var text = assetManager.LoadString(assetName);
			Assert.AreEqual(text, "Test");
			Assert.AreEqual(assetManager.Cache.Count, 1);
			Assert.IsTrue(assetManager.HasAsset(assetName));

			// Test second access of the same resource
			text = assetManager.LoadString(assetName);
			Assert.AreEqual(text, "Test");
			Assert.AreEqual(assetManager.Cache.Count, 1);
			Assert.IsTrue(assetManager.HasAsset(assetName));
		}

		[Test]
		public void TestWrongPath()
		{
			var assetManager = AssetManager.CreateResourceAssetManager(_assembly, "WrongPath.Resources");

			Assert.Throws<Exception>(() =>
			{
				var text = assetManager.LoadString("test.txt");
			});
		}

		[Test]
		public void TestPathRooted()
		{
			TestResourceAccess("Resources", true, "/test.txt");
		}

		[Test]
		public void TestWithoutEndDot()
		{
			TestResourceAccess("Resources", true, "test.txt");
		}

		[Test]
		public void TestWithEndDot()
		{
			TestResourceAccess("Resources.", true, "test.txt");
		}

		[Test]
		public void TestWithoutPrependAssemblyName()
		{
			TestResourceAccess("AssetManagementBase.Tests.Resources", false, "test.txt");
		}
	}
}
