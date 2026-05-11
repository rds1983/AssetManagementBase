using Xunit;
using System;
using System.IO;

namespace AssetManagementBase.Tests
{
	public class FileAssetResolverTests : AssetResolverTestBase
	{

		private static void TestUserProfile(UserProfile userProfile)
		{
			Assert.Equal("AssetManagementBase", userProfile.Name);
			Assert.Equal(10000, userProfile.Score);
		}

		private static void TestUserProfile(string[] assetNames)
		{
			var assetManager = CreateAssetManager();

			// All provided asset names should point to same asset
			for (var i = 0; i < assetNames.Length; ++i)
			{
				var assetName = assetNames[i];
				Assert.True(assetManager.Exists(assetName));
				var userProfile = assetManager.LoadUserProfile(assetName);

				TestUserProfile(userProfile);
				Assert.Equal(1, assetManager.Cache.Count);
				Assert.True(assetManager.IsCached(assetName));

				// Test second access of the same resource
				userProfile = assetManager.LoadUserProfile(assetName);
				TestUserProfile(userProfile);
				Assert.Equal(1, assetManager.Cache.Count);
				Assert.True(assetManager.IsCached(assetName));
			}
		}

		[Fact]
		public void LoadUserProfile_Various_PathFormats_ResolveToSameAsset()
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


		[Fact]
		public void WrongPath_ThrowsAssetNotFoundException()
		{
			var assetManager = CreateAssetManager();

			_ = Assert.Throws<AssetNotFoundException>(() =>
			{
				var userProfile = assetManager.LoadUserProfile("userProfile2.xml");
			});
		}

		[Fact]
		public void LoadJobAsset_ReturnsValidJob()
		{
			var assetManager = CreateAssetManager();

			Assert.True(assetManager.Exists("job.xml"));
			var job = assetManager.LoadJob("job.xml");

			Assert.Equal("Senior Developer", job.Title);
			Assert.Equal(120000m, job.Salary);
			Assert.Equal(1, assetManager.Cache.Count);
			Assert.True(assetManager.IsCached("job.xml"));
		}

		[Fact]
		public void LoadEmployee_WithRelativeJobPath_LoadsRecursively()
		{
			var assetManager = CreateAssetManager();

			Assert.True(assetManager.Exists("employee.xml"));
			var employee = assetManager.LoadEmployee("employee.xml");

			// Verify employee data
			Assert.Equal("John Smith", employee.Name);
			Assert.Equal("job.xml", employee.JobPath);

			// Verify recursively loaded job
			Assert.NotNull(employee.Job);
			Assert.Equal("Senior Developer", employee.Job.Title);
			Assert.Equal(120000m, employee.Job.Salary);

			// Both employee and job should be cached
			Assert.Equal(2, assetManager.Cache.Count);
			Assert.True(assetManager.IsCached("employee.xml"));
			Assert.True(assetManager.IsCached("job.xml"));

			// Second access should use cache
			var cachedEmployee = assetManager.LoadEmployee("employee.xml");
			Assert.Equal(2, assetManager.Cache.Count);
			Assert.Equal(cachedEmployee.Name, employee.Name);
			Assert.Equal(cachedEmployee.Job.Title, employee.Job.Title);
		}

		[Fact]
		public void LoadEmployee_WithAbsoluteJobPath_ResolvesFromBase()
		{
			var assetManager = CreateAssetManager();

			Assert.True(assetManager.Exists("employee_absolute_path.xml"));
			var employee = assetManager.LoadEmployee("employee_absolute_path.xml");

			// Verify employee data
			Assert.Equal("Jane Doe", employee.Name);
			Assert.Equal("/job.xml", employee.JobPath);

			// Verify job loaded via absolute path
			Assert.NotNull(employee.Job);
			Assert.Equal("Senior Developer", employee.Job.Title);
			Assert.Equal(120000m, employee.Job.Salary);

			// Both assets should be cached
			Assert.Equal(2, assetManager.Cache.Count);
			Assert.True(assetManager.IsCached("employee_absolute_path.xml"));
			Assert.True(assetManager.IsCached("/job.xml"));
		}

		[Fact]
		public void LoadEmployee_WithWrongJobPath_ThrowsAssetNotFoundException()
		{
			var assetManager = CreateAssetManager();

			Assert.True(assetManager.Exists("employee_wrong_path.xml"));

			// Loading employee should throw because the referenced job doesn't exist
			_ = Assert.Throws<AssetNotFoundException>(() =>
			{
				var employee = assetManager.LoadEmployee("employee_wrong_path.xml");
			});
		}
	}
}
