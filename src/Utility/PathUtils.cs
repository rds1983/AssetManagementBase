using System.IO;

namespace AssetManagementBase.Utility
{
	internal static class PathUtils
	{
		public static bool IsPathRooted2(this string path)
		{
			var drive = Path.GetPathRoot(path);

			return !string.IsNullOrEmpty(drive) && drive[0] != '/' && drive[0] != '\\';
		}
	}
}
