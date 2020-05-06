using System.IO;

namespace G.Util
{
	public class FileEx
	{
		public static string SearchParentDirectory(string filePath, int retryParentDirectory = 2)
		{
			if (File.Exists(filePath))
				return filePath;

			string dir = Path.GetDirectoryName(filePath);
			string fileName = Path.GetFileName(filePath);

			for (int i = 1; i <= retryParentDirectory; i++)
			{
				dir = Path.Combine(dir, "..");
				filePath = Path.Combine(dir, fileName);

				if (File.Exists(filePath))
					return filePath;
			}

			return null;
		}

		public static string GetDirectory(string filePath)
		{
			if (string.IsNullOrEmpty(filePath)) return null;

			int index = filePath.LastIndexOfAny(new char[] { '/', '\\' });
			if (index < 0) return string.Empty;
			
			return filePath.Substring(0, index);
		}

		public static string GetRelativePath(string baseDir, string filePath)
		{
			string relativeDir = DirectoryEx.GetRelativePath(baseDir, FileEx.GetDirectory(filePath));
			return Path.Combine(relativeDir, Path.GetFileName(filePath));
		}

		public static void MoveTo(string targetDir, params string[] files)
		{
			DirectoryEx.Create(targetDir, false);

			foreach (var fromPath in files)
			{
				if (string.IsNullOrWhiteSpace(fromPath)) continue;

				var fileName = Path.GetFileName(fromPath);
				var toPath = Path.Combine(targetDir, fileName);

				File.Delete(toPath);

				try
				{
					File.Move(fromPath, toPath);
				}
				catch (IOException)
				{
					continue;
				}
			}
		}
	}
}
