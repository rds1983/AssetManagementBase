using Xunit;
using System;
using System.Reflection;

namespace AssetManagementBase.Tests
{
	public class ResourceAssetResolverTests
	{
		private static readonly Assembly _assembly = typeof(ResourceAssetResolverTests).Assembly;

		private static void TestResourceAccess(string prefix, bool prependAssemblyName, string assetName)
		{
			var assetManager = AssetManager.CreateResourceAssetManager(_assembly, prefix, prependAssemblyName);
			var text = assetManager.ReadAsString(assetName);
			Assert.Equal("Test", text);

			// Test second access of the same resource
			text = assetManager.ReadAsString(assetName);
			Assert.Equal("Test", text);
		}

		[Fact]
		public void WrongPath()
		{
			var assetManager = AssetManager.CreateResourceAssetManager(_assembly, "WrongPath.Resources");

			_ = Assert.Throws<Exception>(() =>
			{
				var text = assetManager.ReadAsString("test.txt");
			});
		}

		[Fact]
		public void PathRooted()
		{
			TestResourceAccess("Resources", true, "/test.txt");
		}

		[Fact]
		public void WithoutEndDot()
		{
			TestResourceAccess("Resources", true, "test.txt");
		}

		[Fact]
		public void WithEndDot()
		{
			TestResourceAccess("Resources.", true, "test.txt");
		}

		[Fact]
		public void SubFolder()
		{
			TestResourceAccess("Resources.", true, "sub/test.txt");
		}

		[Fact]
		public void WithoutPrependAssemblyName()
		{
			TestResourceAccess("AssetManagementBase.Tests.Resources", false, "test.txt");
		}

		[Fact]
		public void ResourceExistance()
		{
			var assetManager = AssetManager.CreateResourceAssetManager(_assembly, "Resources.", true);

			Assert.True(assetManager.Exists("test.txt"));
			Assert.True(assetManager.Exists("/test.txt"));
			Assert.True(assetManager.Exists("sub/test.txt"));
			Assert.True(assetManager.Exists("/sub/test.txt"));
			Assert.False(assetManager.Exists("test2.txt"));
		}
	}
}
