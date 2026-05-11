using System;

namespace AssetManagementBase
{
	/// <summary>
	/// Thrown when an asset cannot be found or accessed.
	/// </summary>
	public class AssetNotFoundException : Exception
	{
		/// <summary>
		/// The asset path that could not be found.
		/// </summary>
		public string AssetPath { get; }

		/// <summary>
		/// Creates a new AssetNotFoundException.
		/// </summary>
		/// <param name="assetPath">The path of the asset that was not found</param>
		public AssetNotFoundException(string assetPath)
			: base($"Asset not found: '{assetPath}'")
		{
			AssetPath = assetPath;
		}

		/// <summary>
		/// Creates a new AssetNotFoundException with a custom message.
		/// </summary>
		/// <param name="assetPath">The path of the asset that was not found</param>
		/// <param name="message">Custom error message</param>
		public AssetNotFoundException(string assetPath, string message)
			: base(message)
		{
			AssetPath = assetPath;
		}

		/// <summary>
		/// Creates a new AssetNotFoundException with an inner exception.
		/// </summary>
		/// <param name="assetPath">The path of the asset that was not found</param>
		/// <param name="innerException">The underlying exception that caused this error</param>
		public AssetNotFoundException(string assetPath, Exception innerException)
			: base($"Asset not found: '{assetPath}'", innerException)
		{
			AssetPath = assetPath;
		}

		/// <summary>
		/// Creates a new AssetNotFoundException with a custom message and inner exception.
		/// </summary>
		/// <param name="assetPath">The path of the asset that was not found</param>
		/// <param name="message">Custom error message</param>
		/// <param name="innerException">The underlying exception that caused this error</param>
		public AssetNotFoundException(string assetPath, string message, Exception innerException)
			: base(message, innerException)
		{
			AssetPath = assetPath;
		}
	}
}
