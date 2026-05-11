namespace AssetManagementBase
{
	/// <summary>
	/// Defines custom settings that influence how an asset is loaded and cached.
	/// </summary>
	/// <remarks>
	/// IAssetSettings allows you to load the same asset file in different ways—with different processing rules,
	/// configurations, or parameters—and cache each variant separately. Each distinct settings object creates a
	/// separate cache entry for the same asset path.
	///
	/// For example, you might load an image with different scale factors, or a config file with different environments.
	/// Pass settings to AssetManager.UseLoader() to enable settings-aware caching.
	/// </remarks>
	public interface IAssetSettings
	{
		/// <summary>
		/// Generates a cache key suffix that uniquely identifies this settings object.
		/// </summary>
		/// <remarks>
		/// AssetManager appends this key to the asset path to create the full cache key: "assetPath|settingsKey".
		/// Two IAssetSettings objects with the same BuildKey() will share a cache entry.
		/// Different BuildKey() values create separate cache entries for the same asset.
		/// </remarks>
		/// <returns>A non-null string that uniquely represents this settings configuration</returns>
		string BuildKey();
	}
}
