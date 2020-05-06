using System.IO;

namespace G.Util
{
	public class FileInfoEx
	{
		public FileInfo Info { get; private set; }
		public string Directory { get { return Info.Directory.ToString(); } }
		public string Name { get { return Info.Name; } }
		public string AbsolutePath { get { return Info.FullName; } }
		public string RelativePath { get { return Info.ToString(); } }

		public FileInfoEx(string path)
		{
			Info = new FileInfo(path);
		}
	}
}