using System.IO;

namespace AssetManagementBase.Tests
{
	/// <summary>
	/// Base class for asset resolver tests that provides common setup and helpers.
	/// </summary>
	public class AssetResolverTestBase
	{
		protected static string AssetPath => Path.Combine(Utility.ExecutingAssemblyDirectory, "FileAssets");

		protected static AssetManager CreateAssetManager() => AssetManager.CreateFileAssetManager(AssetPath);
	}
}
