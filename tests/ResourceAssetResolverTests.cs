using NUnit.Framework;
using System;
using System.Reflection;

namespace AssetManagementBase.Tests
{
	[TestFixture]
	public class ResourceAssetResolverTests
	{
		private static readonly Assembly _assembly = typeof(ResourceAssetResolverTests).Assembly;

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
		public void TestWithoutEndDot()
		{
			var assetManager = AssetManager.CreateResourceAssetManager(_assembly, "Resources");
			var text = assetManager.LoadString("test.txt");
			Assert.AreEqual(text, "Test");
		}

		[Test]
		public void TestWithEndDot()
		{
			var assetManager = AssetManager.CreateResourceAssetManager(_assembly, "Resources.");
			var text = assetManager.LoadString("test.txt");
			Assert.AreEqual(text, "Test");
		}

		[Test]
		public void TestWithoutPrependAssemblyName()
		{
			var assetManager = AssetManager.CreateResourceAssetManager(_assembly, "AssetManagementBase.Tests.Resources", false);
			var text = assetManager.LoadString("test.txt");
			Assert.AreEqual(text, "Test");
		}
	}
}
