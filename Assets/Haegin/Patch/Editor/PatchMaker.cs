#define PATCH_HASH128
using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using G.Util;
using UnityEngine;
using Ionic.Zlib;
#if PATCH_HASH128
using UnityEngine.AssetGraph;
#endif

namespace G.Network
{
    class PatchMakerItem
    {
        public string FileName { get; set; }
        public string ZipName { get; set; }
        public string Source { get; set; }
        public string Target { get; set; }
        public string Relative { get; set; }
        public long FileSize { get; set; }
        public long ZipSize { get; set; }
        public DateTime LastWriteTime { get; set; }
        public bool HasCRC { get; set; }
        public uint CRC { get; set; }
#if PATCH_HASH128
        public string Hash128String { get; set; }
#endif
    }

    public class PatchMaker
    {
        private static bool useZipCompress = false;

        public static readonly string patchFileName = "Patch.bin";

        private uint[] key;
        private int dstFolderLength = -1;
        private List<PatchMakerItem> patchMakerItems;

        public bool PatchBinMerge(List<string> srcSubFolders, string rootFolder, string mergedPatchFileName, uint[] key)
        {
            this.key = key;
            patchMakerItems = new List<PatchMakerItem>();

            try { File.Delete(Path.Combine(rootFolder, mergedPatchFileName)); } catch { }

            foreach (string srcFolder in srcSubFolders)
            {

                byte[] result = null;
                try
                {
                    string srcFile = Path.Combine(rootFolder, srcFolder);
                    srcFile = Path.Combine(srcFile, patchFileName);
                    result = File.ReadAllBytes(srcFile);

                    if (result != null)
                    {
                        XXTea xxtea = new XXTea(key);
                        byte[] decrypted = xxtea.Decrypt(result);

                        using (var stream = new MemoryStream(decrypted))
                        using (var zipStream = new ZlibStream(stream, CompressionMode.Decompress))
                        using (var reader = new StreamReader(zipStream))
                        {
                            string line = null;
                            for (; ; )
                            {
                                line = reader.ReadLine();
                                if (line == null) break;

                                string[] tokens = line.Split(new char[] { ',' });
#if PATCH_HASH128
                                if (tokens.Length != 7 && tokens.Length != 8) break;
#else
                                if (tokens.Length != 6 && tokens.Length != 7) break;
#endif
                                PatchMakerItem item = new PatchMakerItem();
                                item.Relative = Path.Combine(srcFolder, tokens[0]);
                                item.FileName = tokens[1];
                                item.ZipName = tokens[2];
                                item.FileSize = long.Parse(tokens[3]);
                                item.ZipSize = long.Parse(tokens[4]);
                                item.LastWriteTime = new DateTime(long.Parse(tokens[5]));

#if PATCH_HASH128
                                if (tokens.Length == 8)
                                {
                                    item.HasCRC = true;
                                    item.CRC = uint.Parse(tokens[6]);
                                    item.Hash128String = tokens[7];
                                }
                                else
                                {
                                    item.Hash128String = tokens[6];
                                }
#else
                                if (tokens.Length == 7)
                                {
                                    item.HasCRC = true;
                                    item.CRC = uint.Parse(tokens[6]);
                                }
#endif
                                patchMakerItems.Add(item);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return false;
                }
            }

            SavePatchBin(rootFolder, mergedPatchFileName);

            return true;
        }

        public void Create(string srcFolder, string dstFolder, uint[] key, string obbPath)
        {
            if(!string.IsNullOrEmpty(obbPath))
                obbPath = Path.GetFullPath(obbPath);
            srcFolder = Path.GetFullPath(srcFolder);
            dstFolder = Path.GetFullPath(dstFolder);
            dstFolderLength = dstFolder.Length;

            this.key = key;

            patchMakerItems = new List<PatchMakerItem>();

            CreateInfos(srcFolder, dstFolder, true);

            foreach (PatchMakerItem item in patchMakerItems)
            {
                Console.WriteLine("FileName : " + item.FileName);
                Console.WriteLine("Source : " + item.Source);
                Console.WriteLine("Target : " + item.Target);
                Console.WriteLine("Relative : " + item.Relative);

                if (item.HasCRC)
                    Console.WriteLine(String.Format("CRC : {0:X08}", item.CRC));

                Console.WriteLine();
            }

            if (useZipCompress)
                ConvertToZip();
            else
                CopyFile();

            Debug.Log("---------------------------------------------");
            Debug.Log(obbPath);
            Debug.Log("---------------------------------------------");

            if (!string.IsNullOrEmpty(obbPath))
            {
                // obb file 생성
                AddToObbFile(obbPath);
            }

            SavePatchBin(dstFolder);

            Console.WriteLine("Done.");
        }

        private void CreateInfos(string srcPath, string dstFolder, bool hasCRC)
        {
            if (Directory.Exists(srcPath))
            {
                if (!Directory.Exists(dstFolder))
                {
                    try
                    {
                        Directory.CreateDirectory(dstFolder);
                    }
                    catch
                    {
                        Console.WriteLine("Error: Destination folder was not created");
                        Environment.Exit(3);
                    }
                }

                string[] files = Directory.GetFiles(srcPath);
                Array.Sort(files);
                foreach (var f in files)
                {
                    string fileName = Path.GetFileName(f);
                    if (fileName.StartsWith(".")) continue;

                    CreateInfos(f, dstFolder, hasCRC);
                }

                string[] directories = Directory.GetDirectories(srcPath);
                Array.Sort(directories);
                foreach (var d in directories)
                {
                    string dirName = Path.GetFileName(d);
                    if (dirName.StartsWith(".")) continue;

                    string src = Path.Combine(srcPath, dirName);
                    string dst = Path.Combine(dstFolder, dirName);
                    CreateInfos(src, dst, hasCRC);
                }
            }
            else if (File.Exists(srcPath))
            {
                PatchMakerItem item = new PatchMakerItem();
                item.FileName = Path.GetFileName(srcPath);
                if (useZipCompress)
                    item.ZipName = item.FileName + ".zip";
                else
                    item.ZipName = item.FileName;
                item.Source = srcPath;
                item.Target = Path.Combine(dstFolder, item.ZipName);
                item.Relative = dstFolder.Substring(dstFolderLength, dstFolder.Length - dstFolderLength).TrimStart('/', '\\');

                FileInfo fileInfo = new FileInfo(srcPath);
                item.FileSize = fileInfo.Length;
                item.LastWriteTime = fileInfo.LastWriteTime;
                item.HasCRC = hasCRC;
                if (hasCRC) item.CRC = Crc.GetCRC(srcPath);

#if PATCH_HASH128
                item.Hash128String = GetHash128String(srcPath, item.FileName);
                Debug.Log(item.FileName + " hash = " + item.Hash128String);
#endif
                patchMakerItems.Add(item);
            }
        }

#if PATCH_HASH128
        private string GetHash128String(string srcPath, String filename)
        {
            Debug.Log("Source Path : " + srcPath);
            Debug.Log("Filename : " + filename);
            if (filename.EndsWith(".manifest"))
            {
                filename = filename.Remove(filename.Length - 9);
            }
            Debug.Log("new Filename : " + filename);

            foreach (AssetBundleBuildReport report in AssetBundleBuildReport.BuildReports)
            {
                Debug.Log(report.ManifestFileName);
                try
                {
                    Hash128 hash = report.Manifest.GetAssetBundleHash(filename);
                    if(hash.isValid)
                        return hash.ToString();
                }
                catch(Exception e)
                {
                    continue;
                }
            }
            return "";
//            AssetBundle assetBundle = AssetBundle.LoadFromFile(srcPath);
//            AssetBundleManifest assetManifest = (AssetBundleManifest)(assetBundle.LoadAsset<AssetBundleManifest>("textdata.manifest"));
//            Hash128 hash128 = assetManifest.GetAssetBundleHash("");
//            return hash128.ToString();
        }
#endif

        private void ConvertToZip()
        {
            foreach (PatchMakerItem item in patchMakerItems)
            {
                Console.Write(item.Source);

                using (var inStream = File.Open(item.Source, FileMode.Open))
                using (var outStream = File.Create(item.Target))
                using (var zipStream = new ZipOutputStream(outStream))
                {
                    var zipEntry = new ZipEntry(item.FileName);
                    zipEntry.Size = item.FileSize;
                    zipEntry.DateTime = item.LastWriteTime;
                    zipStream.PutNextEntry(zipEntry);

                    if (zipEntry.Size > 0)
                    {
                        byte[] buf = new byte[81920];
                        int count;
                        while ((count = inStream.Read(buf, 0, 81920)) != 0)
                        {
                            zipStream.Write(buf, 0, count);
                        }
                    }
                    //      			inStream.CopyTo(zipStream);

                    zipStream.CloseEntry();
                }

                item.ZipSize = new FileInfo(item.Target).Length;

                Console.WriteLine(".....Zipped");
            }
        }

        private void CopyFile()
        {
            foreach (PatchMakerItem item in patchMakerItems)
            {
                Console.Write(item.Source);
                File.Copy(item.Source, item.Target, true);
                item.ZipSize = item.FileSize;
                Console.WriteLine(".....Copied");
            }
        }

        private void AddToObbFile(string obbFilePath)
        {
            int count = 0;
            try
            {
                using (var inStream = File.Open(obbFilePath, FileMode.Open))
                {
                    byte[] buf = new byte[14];
                    inStream.Read(buf, 0, 14);
                    count = (int)(((int)(buf[10]) << 24) & 0xFF000000) | (int)(((int)(buf[11]) << 16) & 0xFF0000) | (int)(((int)(buf[12]) << 8) & 0xFF00) | (int)((int)buf[13] & 0xFF);
                }
            }
            catch
            {
                using (var outStream = File.Open(obbFilePath, FileMode.Create))
                {
                    byte[] buf = new byte[14];
                    outStream.Seek(0, SeekOrigin.Begin);
                    buf[0] = 0;
                    buf[1] = 2;
                    buf[2] = 6;
                    buf[3] = 9;
                    buf[4] = 5;
                    buf[5] = 1;
                    buf[6] = 5;
                    buf[7] = 0;
                    buf[8] = 0;
                    buf[9] = 1;
                    buf[10] = (byte)((count >> 24) & 0xFF);
                    buf[11] = (byte)((count >> 16) & 0xFF);
                    buf[12] = (byte)((count >> 8) & 0xFF);
                    buf[13] = (byte)(count & 0xFF);
                    outStream.Write(buf, 0, 14);
                    outStream.Flush();
                    outStream.Close();
                }
            }

            foreach (PatchMakerItem item in patchMakerItems)
            {
                count++;
                try
                {
                    using (var inStream = File.Open(item.Source, FileMode.Open))
                    using (var outStream = File.Open(obbFilePath, FileMode.Open))
                    {
                        outStream.Seek(0, SeekOrigin.End);

                        byte[] filename = System.Text.Encoding.UTF8.GetBytes(item.FileName);
                        byte[] filenamelength = new byte[4];
                        byte[] filesize = new byte[4];
                        filenamelength[0] = (byte)((filename.Length >> 24) & 0xFF);
                        filenamelength[1] = (byte)((filename.Length >> 16) & 0xFF);
                        filenamelength[2] = (byte)((filename.Length >> 8) & 0xFF);
                        filenamelength[3] = (byte)(filename.Length & 0xFF);
                        outStream.Write(filenamelength, 0, 4);
                        outStream.Write(filename, 0, filename.Length);
                        filesize[0] = (byte)((item.FileSize >> 24) & 0xFF);
                        filesize[1] = (byte)((item.FileSize >> 16) & 0xFF);
                        filesize[2] = (byte)((item.FileSize >> 8) & 0xFF);
                        filesize[3] = (byte)(item.FileSize & 0xFF);
                        outStream.Write(filesize, 0, 4);
                        byte[] buf = new byte[81920];
                        int read;
                        while((read = inStream.Read(buf, 0, 81920)) != 0)
                        {
                            outStream.Write(buf, 0, read);
                        }
                        outStream.Seek(0, SeekOrigin.Begin);
                        buf[0] = 0;
                        buf[1] = 2;
                        buf[2] = 6;
                        buf[3] = 9;
                        buf[4] = 5;
                        buf[5] = 1;
                        buf[6] = 5;
                        buf[7] = 0;
                        buf[8] = 0;
                        buf[9] = 1;
                        buf[10] = (byte)((count >> 24) & 0xFF);
                        buf[11] = (byte)((count >> 16) & 0xFF);
                        buf[12] = (byte)((count >> 8) & 0xFF);
                        buf[13] = (byte)(count & 0xFF);
                        outStream.Write(buf, 0, 14);
                        outStream.Flush();
                        outStream.Close();
                        inStream.Close();
                    }
                }
                catch(Exception e) {
                    Debug.Log(e.ToString());
                }
            }
        }

        private void SavePatchBin(string dstFolder, string patchFileName = "Patch.bin")
        {
            string path = Path.Combine(dstFolder, patchFileName);

            using (var stream = new MemoryStream())
            using (var zipStream = new DeflaterOutputStream(stream))
            using (var writer = new StreamWriter(zipStream))
            {
                foreach (PatchMakerItem item in patchMakerItems)
                {
                    string line;
#if PATCH_HASH128
                    if (item.HasCRC)
                        line = String.Format("{0},{1},{2},{3},{4},{5},{6},{7}", item.Relative, item.FileName, item.ZipName, item.FileSize, item.ZipSize, item.LastWriteTime.Ticks, item.CRC, item.Hash128String);
                    else
                        line = String.Format("{0},{1},{2},{3},{4},{5},{6}", item.Relative, item.FileName, item.ZipName, item.FileSize, item.ZipSize, item.LastWriteTime.Ticks, item.Hash128String);
#else
                    if (item.HasCRC)
                        line = String.Format("{0},{1},{2},{3},{4},{5},{6}", item.Relative, item.FileName, item.ZipName, item.FileSize, item.ZipSize, item.LastWriteTime.Ticks, item.CRC);
                    else
                        line = String.Format("{0},{1},{2},{3},{4},{5}", item.Relative, item.FileName, item.ZipName, item.FileSize, item.ZipSize, item.LastWriteTime.Ticks);
#endif
                    writer.WriteLine(line);
				}
				writer.Flush();

				zipStream.Flush();
				zipStream.Finish();

				XXTea xxtea = new XXTea(key);
				xxtea.EncryptToFile(path, stream.GetBuffer(), 0, (int)stream.Position);
			}
		}
	}
}
