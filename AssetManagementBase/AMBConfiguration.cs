using System;

namespace AssetManagementBase
{
	/// <summary>
	/// Global configuration for the AssetManagementBase library
	/// </summary>
	public static class AMBConfiguration
	{
		/// <summary>
		/// Logger callback for the library
		/// </summary>
		public static Action<string> Logger;
	}
}
