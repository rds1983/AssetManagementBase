using System.IO;

namespace AssetManagementBase
{
	/// <summary>
	/// Defines how assets are accessed from a particular source (e.g., file system or embedded resources).
	/// </summary>
	/// <remarks>
	/// Implement this interface to support loading assets from custom sources beyond file system and embedded resources.
	/// Common implementations include file system paths, network streams, ZIP archives, or database blobs.
	/// </remarks>
	public interface IAssetAccessor
	{
		/// <summary>
		/// Gets a friendly name identifying this asset source (e.g., "FileSystem", "Resources", "Database").
		/// Used for logging and diagnostics.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Checks whether an asset exists at the specified path.
		/// </summary>
		/// <param name="path">The asset path to check</param>
		/// <returns>True if the asset exists; false otherwise</returns>
		bool Exists(string path);

		/// <summary>
		/// Opens a readable stream for the asset at the specified path.
		/// </summary>
		/// <param name="path">The asset path to open</param>
		/// <returns>A stream positioned at the start of the asset content</returns>
		/// <exception cref="System.IO.FileNotFoundException">Thrown if the asset does not exist</exception>
		Stream Open(string path);
	}
}