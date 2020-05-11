using System.IO;
using System.Text;
using System.Collections.Generic;

namespace G.Util
{
	public class DirectoryEx
	{
		public static DirectoryInfo Create(string path, bool deleteIfExists)
		{
			if (deleteIfExists && Directory.Exists(path))
				Directory.Delete(path, true);

			return Directory.CreateDirectory(path);
		}

		public static bool Delete(string path)
		{
			try
			{
				Directory.Delete(path, true);
				return true;
			}
			catch (DirectoryNotFoundException)
			{
				return false;
			}
		}

		public static IEnumerable<FileInfoEx> GetAllFiles(string dir)
		{
			var files = Directory.GetFiles(dir, "*", SearchOption.AllDirectories);

			foreach (var f in files)
			{
				yield return new FileInfoEx(f);
			}
		}

		public static IEnumerable<FileInfoEx> GetAllFiles(string dir, string pattern)
		{
			var files = Directory.GetFiles(dir, pattern, SearchOption.AllDirectories);

			foreach (var f in files)
			{
				yield return new FileInfoEx(f);
			}
		}

		public static IEnumerable<FileInfoEx> GetFiles(string dir)
		{
			var files = Directory.GetFiles(dir, "*", SearchOption.TopDirectoryOnly);

			foreach (var f in files)
			{
				yield return new FileInfoEx(f);
			}
		}

		public static IEnumerable<FileInfoEx> GetFiles(string dir, string pattern)
		{
			var files = Directory.GetFiles(dir, pattern, SearchOption.TopDirectoryOnly);

			foreach (var f in files)
			{
				yield return new FileInfoEx(f);
			}
		}

		public static string GetRelativePath(string baseDir, string dir)
		{
			string baseFullPath = Path.GetFullPath(baseDir);
			string fullPath = Path.GetFullPath(dir);

			string[] baseTokens = baseFullPath.Split(Path.DirectorySeparatorChar);
			string[] pathTokens = fullPath.Split(Path.DirectorySeparatorChar);

			StringBuilder sb = new StringBuilder();

			int count = baseTokens.Length;
			for (int i = 0; i < count; i++)
			{
				if (pathTokens.Length <= i || baseTokens[i] != pathTokens[i])
				{
					for (int j = i; j < baseTokens.Length; j++)
						sb.Append("../");

					for (int k = i; k < pathTokens.Length; k++)
						sb.Append(pathTokens[k] + "/");

					break;
				}
			}

			return sb.ToString();
		}
	}
}
