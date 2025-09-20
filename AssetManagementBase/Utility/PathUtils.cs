using System.Collections.Generic;

namespace AssetManagementBase.Utility
{
	internal static class PathUtils
	{
		public const char SeparatorSymbol = '/';
		public const string SeparatorString = "/";

		public static string FixFilePath(this string path)
		{
			if (!string.IsNullOrEmpty(path))
			{
				path = path.Replace('\\', SeparatorSymbol);
			}

			return path;
		}

		public static string FixFolderPath(this string path)
		{
			path = path.FixFilePath();

			// Remove separator from the end
			if (!string.IsNullOrEmpty(path) && path.EndsWith(SeparatorString))
			{
				path = path.Substring(0, path.Length - 1);
			}

			return path;
		}

		public static string CombinePath(string _base, string url)
		{
			if (string.IsNullOrEmpty(_base))
			{
				return url;
			}

			if (string.IsNullOrEmpty(url))
			{
				return _base;
			}

			if (url[0] == SeparatorSymbol)
			{
				return _base + url;
			}

			return _base + SeparatorSymbol + url;
		}

		public static string ProcessPath(string path)
		{
			if (!path.Contains(".."))
			{
				return path;
			}

			// Remove ".."
			var parts = path.Split(SeparatorSymbol);
			var partsStack = new List<string>();
			for (var i = 0; i < parts.Length; i++)
			{
				if (parts[i] == ".." && partsStack.Count > 0 && partsStack[partsStack.Count - 1] != ".." && partsStack[partsStack.Count - 1] != ".")
				{
					partsStack.RemoveAt(partsStack.Count - 1);
				}
				else if (!string.IsNullOrEmpty(parts[i]))
				{
					partsStack.Add(parts[i]);
				}
			}

			if (path.StartsWith(SeparatorString))
			{
				path = SeparatorSymbol + string.Join(SeparatorString, partsStack);
			}
			else
			{
				path = string.Join(SeparatorString, partsStack);
			}

			return path;
		}
	}
}
