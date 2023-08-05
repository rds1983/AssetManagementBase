using System;
using System.IO;
using System.Reflection;

namespace AssetManagementBase.Utility
{
	internal static class PathUtils
	{
		public static string TryToMakePathRelativeTo(string path, string pathRelativeTo)
		{
			try
			{
				var fullPathUri = new Uri(path, UriKind.Absolute);

				if (!pathRelativeTo.EndsWith(Path.DirectorySeparatorChar.ToString()))
				{
					pathRelativeTo += Path.DirectorySeparatorChar;
				}
				var folderPathUri = new Uri(pathRelativeTo, UriKind.Absolute);

				path = folderPathUri.MakeRelativeUri(fullPathUri).ToString();
			}
			catch (Exception)
			{
			}

			return path;
		}
	}
}
