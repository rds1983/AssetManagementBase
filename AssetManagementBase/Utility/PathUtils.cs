using System.Collections.Generic;
using System.IO;

namespace AssetManagementBase.Utility
{
	/// <summary>
	/// Utility methods for path normalization and resolution.
	/// </summary>
	internal static class PathUtils
	{
		/// <summary>
		/// Symbol prefix for explicit absolute file system paths (e.g., "@C:\path").
		/// </summary>
		public const string RootedPathSymbol = "@";

		/// <summary>
		/// Standard path separator character (forward slash) used internally.
		/// </summary>
		public const char SeparatorSymbol = '/';

		/// <summary>
		/// Standard path separator string (forward slash) used internally.
		/// </summary>
		public const string SeparatorString = "/";

		/// <summary>
		/// Normalizes a file path by converting backslashes to forward slashes.
		/// </summary>
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

		/// <summary>
		/// Resolves ".." path segments to produce a canonical path.
		/// </summary>
		/// <remarks>
		/// Converts paths like "a/b/../c" to "a/c" and "a/./b" to "a/b".
		/// Handles edge cases: ".." at path start is ignored, and multiple consecutive ".." are processed in order.
		/// Preserves the rooted status of the path (leading "/" is maintained if present).
		/// </remarks>
		/// <example>
		/// "a/b/../c" → "a/c"
		/// "/a/b/../c" → "/a/c"
		/// "a/./b" → "a/b"
		/// "a/b/../../c" → "c"
		/// </example>
		public static string ProcessPath(string path)
		{
			if (!path.Contains(".."))
			{
				return path;
			}

			var parts = path.Split(SeparatorSymbol);
			var partsStack = new List<string>();
			for (var i = 0; i < parts.Length; i++)
			{
				if (parts[i] == ".." && partsStack.Count > 0 && partsStack[partsStack.Count - 1] != ".." && partsStack[partsStack.Count - 1] != ".")
				{
					// Go up one level by removing the last path component
					partsStack.RemoveAt(partsStack.Count - 1);
				}
				else if (!string.IsNullOrEmpty(parts[i]))
				{
					// Add non-empty, non-current-dir components
					partsStack.Add(parts[i]);
				}
			}

			// Preserve rooted status
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

		public static bool IsWindowsPathRooted(this string path)
		{
			var drive = Path.GetPathRoot(path);

			return !string.IsNullOrEmpty(drive) && drive[0] != '/' && drive[0] != '\\';
		}
	}
}
