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
			var text = assetManager.ReadAsString(assetName);
			Assert.AreEqual(text, "Test");

			// Test second access of the same resource
			text = assetManager.ReadAsString(assetName);
			Assert.AreEqual(text, "Test");
		}

		[Test]
		public void TestWrongPath()
		{
			var assetManager = AssetManager.CreateResourceAssetManager(_assembly, "WrongPath.Resources");

			Assert.Throws<Exception>(() =>
			{
				var text = assetManager.ReadAsString("test.txt");
			});
		}

		[Test]
		public void TestPathRooted()
		{
			TestResourceAccess("Resources", true, "@test.txt");
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
		public void TestSubFolder()
		{
			TestResourceAccess("Resources.", true, "sub/test.txt");
		}

		[Test]
		public void TestWithoutPrependAssemblyName()
		{
			TestResourceAccess("AssetManagementBase.Tests.Resources", false, "test.txt");
		}

		[Test]
		public void TestResourceExistance()
		{
			var assetManager = AssetManager.CreateResourceAssetManager(_assembly, "Resources.", true);

			Assert.IsTrue(assetManager.Exists("test.txt"));
			Assert.IsTrue(assetManager.Exists("@test.txt"));
			Assert.IsTrue(assetManager.Exists("sub/test.txt"));
			Assert.IsTrue(assetManager.Exists("@sub/test.txt"));
			Assert.IsFalse(assetManager.Exists("test2.txt"));
		}
	}
}
