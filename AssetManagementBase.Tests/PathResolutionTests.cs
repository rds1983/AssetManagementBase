using Xunit;

namespace AssetManagementBase.Tests
{
	/// <summary>
	/// Tests for flexible path resolution in various scenarios.
	/// </summary>
	public class PathResolutionTests : AssetResolverTestBase
	{
		[Fact]
		public void RelativePath_InSubdirectory_ResolvesCorrectly()
		{
			var assetManager = CreateAssetManager();

			// Load employee from subfolder using relative path
			var employee = assetManager.LoadEmployee("subfolder/employee_sibling_ref.xml");

			Assert.Equal("Bob Wilson", employee.Name);
			Assert.Equal("job.xml", employee.JobPath);

			// Job should be resolved relative to subfolder
			Assert.NotNull(employee.Job);
			Assert.Equal("Manager", employee.Job.Title);
			Assert.Equal(95000m, employee.Job.Salary);
		}

		[Fact]
		public void RelativePath_WithParentNavigation_ResolvesCorrectly()
		{
			var assetManager = CreateAssetManager();

			// Load employee from subfolder using ../ to reference parent
			var employee = assetManager.LoadEmployee("subfolder/employee_parent_ref.xml");

			Assert.Equal("Alice Johnson", employee.Name);
			Assert.Equal("../job.xml", employee.JobPath);

			// Job should be resolved from parent directory
			Assert.NotNull(employee.Job);
			Assert.Equal("Senior Developer", employee.Job.Title);
			Assert.Equal(120000m, employee.Job.Salary);
		}

		[Fact]
		public void AbsolutePath_IgnoresAssetDirectory_ResolvesFromBase()
		{
			var assetManager = CreateAssetManager();

			// Load employee from subfolder but reference job with absolute path
			// Absolute path should be resolved from base, not from subfolder
			var employee = assetManager.LoadEmployee("employee_absolute_path.xml");

			// Verify job was resolved from base directory (120000 salary, not 95000)
			Assert.NotNull(employee.Job);
			Assert.Equal(120000m, employee.Job.Salary);
		}

		[Fact]
		public void MultipleParentNavigation_ResolvesCorrectly()
		{
			var assetManager = CreateAssetManager();

			// Load from nested path with multiple .. navigations
			var employee = assetManager.LoadEmployee("subfolder/employee_parent_ref.xml");

			// The job.xml at root should be loaded (120000 salary)
			Assert.NotNull(employee.Job);
			Assert.Equal(120000m, employee.Job.Salary);
		}

		[Fact]
		public void NestedRelativePaths_CacheBothAssets()
		{
			var assetManager = CreateAssetManager();

			var employee = assetManager.LoadEmployee("subfolder/employee_sibling_ref.xml");

			// Both the employee and job should be cached
			Assert.True(assetManager.IsCached("subfolder/employee_sibling_ref.xml"));
			Assert.True(assetManager.IsCached("subfolder/job.xml"));
			Assert.Equal(2, assetManager.Cache.Count);
		}

		[Fact]
		public void AbsoluteAndRelativeReferences_ResolveToDifferentAssets()
		{
			var assetManager = CreateAssetManager();

			// Load both employees to test different path types
			var employeeAbsolute = assetManager.LoadEmployee("employee_absolute_path.xml");
			var employeeRelative = assetManager.LoadEmployee("subfolder/employee_sibling_ref.xml");

			// They should reference different job files
			Assert.Equal(120000m, employeeAbsolute.Job.Salary);  // From root
			Assert.Equal(95000m, employeeRelative.Job.Salary);   // From subfolder

			// Both assets should be cached
			Assert.Equal(4, assetManager.Cache.Count);
		}

		[Fact]
		public void RootedPath_WithParentNavigation_ResolvesFromBase()
		{
			var assetManager = CreateAssetManager();

			// Create a test employee that uses /subfolder/job.xml (rooted path)
			var employee = assetManager.LoadEmployee("employee.xml");

			// Job should be loaded from root as normal
			Assert.NotNull(employee.Job);
			Assert.Equal(120000m, employee.Job.Salary);
		}

		[Fact]
		public void SameAsset_DifferentPaths_UseCache()
		{
			var assetManager = CreateAssetManager();

			// Load same job through different path formats
			var job1 = assetManager.LoadJob("job.xml");
			var job2 = assetManager.LoadJob("/job.xml");

			// Should use the same cache entry (resolved to same path)
			Assert.Same(job1, job2);
			Assert.Equal(1, assetManager.Cache.Count);
		}

		[Fact]
		public void LoadFromSubdirectory_ThenLoadFromRoot_BothCached()
		{
			var assetManager = CreateAssetManager();

			// Load from subdirectory context
			var employee1 = assetManager.LoadEmployee("subfolder/employee_sibling_ref.xml");
			var job1 = employee1.Job; // This loads subfolder/job.xml

			// Load job from root context
			var job2 = assetManager.LoadJob("job.xml");

			// Should be different cache entries (different paths)
			Assert.NotSame(job1, job2);
			Assert.Equal(95000m, job1.Salary);
			Assert.Equal(120000m, job2.Salary);

			// Both should be cached
			Assert.Equal(3, assetManager.Cache.Count);
		}
	}
}
