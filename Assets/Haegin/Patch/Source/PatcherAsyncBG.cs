#define PATCH_HASH128
#define USE_NBG_WHEN_ERROR
#define USE_OBB_PRE_PATCH    // obb 파일로부터 patch 받을 파일들을 미리 복사해 오기
//#define CREATE_RELATIVE_PATH   // 사용하지 않음
//#define PATCH_ALL_COMPLETED_WITH_PATCHERITEMS   // 4팀 패치용 : AllCompleted 호출 시 patcher items 리스트를 보냄
#define CALLBACK_COMPLETED_FOR_NON_CACHED_FILES   // 4팀 패치용 : 캐쉬 루틴을 불러야하는 경우
#define DONT_UNLOAD_BGWEBCLIENT
using System;
using System.Collections.Generic;
using System.IO;
using Ionic.Zip;
using Ionic.Zlib;
using G.Util;
using Haegin;
using UnityEngine;
using System.Net;
using System.ComponentModel;
#if USE_OBB_PRE_PATCH
using System.Threading;
#endif

namespace G.Network
{
    public class PatcherAsyncBG
    {
        bool IsCached(string name, Hash128 hash128)
        {
            if (name.EndsWith(".manifest"))
            {
                //name = name.Remove(name.Length - 9);
                return true;
            }
            bool isCached = hash128.isValid && Caching.IsVersionCached(name, hash128);
#if MDEBUG
            Debug.Log("Filename " + name + " Hash " + hash128.ToString() + " Cached=" + isCached);
#endif
            return isCached;
        }
#if CALLBACK_COMPLETED_FOR_NON_CACHED_FILES
#if USE_OBB_PRE_PATCH 
        public bool OBBPrepatched = false;
#endif
#endif
        public delegate void OnProgressed(string fileName, long fileDownloadedByte, long fileLength, int currentFileCount, int totalFileCount);
        public delegate void OnFileCompleted(string fileName, int currentFileCount, int totalFileCount);
#if PATCH_ALL_COMPLETED_WITH_PATCHERITEMS
        public delegate void OnAllCompleted(List<PatcherItem> patcherItems);
#else
        public delegate void OnAllCompleted(int totalFileCount);
#endif
        public delegate void OnErrorOccurred(string fileName, string message);
        public delegate void OnTotalProgressed(long totalDownloadedByte, long totalLength, int count, int totalCount);
#if USE_OBB_PRE_PATCH
        public delegate void OnCopyProgressed(long totalDownloadedByte, long totalLength, int count, int totalCount);
        public delegate void OnFinishCopy();
#endif
        public delegate void OnConfirm();
        public delegate void OnOpenConfirmDialog(long fileSize, OnConfirm callback);
        public delegate void OnReachabilityChanged(bool reachable);

        public event OnProgressed Progressed = delegate { };
        public event OnFileCompleted FileCompleted = delegate { };
        public event OnAllCompleted AllCompleted = delegate { };
        public event OnErrorOccurred ErrorOccurred = delegate { };
        public event OnTotalProgressed TotalProgressed = delegate { };
        public event OnReachabilityChanged ReachabilityChanged = delegate { };
#if USE_OBB_PRE_PATCH
        public event OnCopyProgressed CopyProgressed = delegate { };
#endif

        public OnOpenConfirmDialog OpenConfirmDialog;

        public static string patchFileName = "Patch.bin";

        private List<PatcherItem> patcherItems;
        private int currentIndex;

        private string baseUrl;
        private Uri uri;
        private string dstFolder;
        private uint[] key;

        public void ClearEvent()
        {
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
            client.ClearEvent();
#endif
        }

#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
        private BGWebClient client = null;

#if USE_NBG_WHEN_ERROR
        private bool hasErrorBGWebClient = false;
        private System.Net.WebClient clientNBG = null;
#endif
        private enum PatchStep
        {
            None,
            WaitPatchInfo,
            WaitDataFiles
        }
        private PatchStep patchStep = PatchStep.None;

        public PatcherAsyncBG(string url)
        {
#if MDEBUG
            Debug.Log("HAEGINHAEGIN new PatchAsyncBG " + this.GetHashCode());
#endif
            patchStep = PatchStep.None;
            baseUrl = url;
            client = BGWebClient.GetInstance();
#if DONT_UNLOAD_BGWEBCLIENT
            client.RefCount++;
#if MDEBUG
            Debug.Log("HAEGINHAEGIN RefCount = " + client.RefCount);
#endif
            client.ClearEvent();
#endif
            client.DownloadFileCompleted += OnAsyncCompleted;
            client.DownloadProgressChanged += OnDownloadProgressChanged;
            client.InternaetReachabilityChanged += OnInternetReachabilityChanged;
        }

        ~PatcherAsyncBG()
        {
#if MDEBUG
            Debug.Log("HAEGINHAEGIN ~PatchAsyncBG " + this.GetHashCode());
#endif
#if DONT_UNLOAD_BGWEBCLIENT
            if(client != null)
            {
                client.RefCount--;
#if MDEBUG
                Debug.Log("HAEGINHAEGIN RefCount = " + client.RefCount);
#endif
                if (!client.CheckRef())
                {
#if MDEBUG
                    Debug.Log("HAEGINHAEGIN Destroy BGWebClient");
#endif
                    client.gameObject.name = "ToBeDestroyed";
                    UnityEngine.Object.Destroy(client.gameObject);
                    client = null;
                }
            }
#endif
        }

#if (UNITY_ANDROID || UNITY_IOS) && USE_OBB_PRE_PATCH
        public void Patch(string patchName, string dstFolder, uint[] key, OnOpenConfirmDialog callback, string patchbin = "Patch.bin", int versionCode = -1, bool deleteObb = false, bool isOnlyUseBaseUrl = false)
        {
#if CALLBACK_COMPLETED_FOR_NON_CACHED_FILES
            OBBPrepatched = false;
#endif
#if MDEBUG
            Debug.Log("OBB File : " + client.GetOBBFilePath(versionCode));
#endif
            CopyFromObb(dstFolder, client.GetOBBFilePath(versionCode), deleteObb, () => {
                PatchSub(patchName, dstFolder, key, callback, patchbin, versionCode, deleteObb, isOnlyUseBaseUrl);
            });
        }

        public void PatchSub(string patchName, string dstFolder, uint[] key, OnOpenConfirmDialog callback, string patchbin = "Patch.bin", int versionCode = -1, bool deleteObb = false, bool isOnlyUseBaseUrl = false)
        {
#else
        public void Patch(string patchName, string dstFolder, uint[] key, OnOpenConfirmDialog callback, string patchbin = "Patch.bin", int versionCode = -1, bool deleteObb = false, bool isOnlyUseBaseUrl = false)
        {
#endif
            patchFileName = patchbin;
            OpenConfirmDialog = callback;
            currentIndex = -1;
            if(isOnlyUseBaseUrl)
            {
                uri = new Uri(baseUrl);
            }
            else if(string.IsNullOrEmpty(patchName))
            {
#if MDEBUG
                Debug.Log(baseUrl + AssetBundleUtility.GetPlatformName() + "/");
#endif
                uri = new Uri(baseUrl + AssetBundleUtility.GetPlatformName() + "/");
            }
            else 
            {
#if MDEBUG
                Debug.Log(baseUrl + AssetBundleUtility.GetPlatformName() + "/" + patchName + "/");
#endif
                uri = new Uri(baseUrl + AssetBundleUtility.GetPlatformName() + "/" + patchName + "/");
            }
            this.dstFolder = string.IsNullOrEmpty(dstFolder) ? string.Empty : Path.GetFullPath(dstFolder);
            this.key = key;

            patcherItems = new List<PatcherItem>();

            if (!Directory.Exists(dstFolder))
            {
                Directory.CreateDirectory(dstFolder);
            }
#if USE_NBG_WHEN_ERROR
            if (hasErrorBGWebClient)
            {
#if MDEBUG
                Debug.Log("--------------------------------------");
                Debug.Log("Disable BGDownload");
                Debug.Log("--------------------------------------");
#endif
                if(clientNBG == null) {
                    clientNBG = new System.Net.WebClient();

                    clientNBG.DownloadDataCompleted += new DownloadDataCompletedEventHandler(OnDownloadDataCompletedNBG);
                    clientNBG.DownloadFileCompleted += new AsyncCompletedEventHandler(OnAsyncCompletedNBG);
                    clientNBG.DownloadProgressChanged += new DownloadProgressChangedEventHandler(OnDownloadProgressChangedNBG);

                    client.DownloadFileCompleted -= OnAsyncCompleted;
                    client.DownloadProgressChanged -= OnDownloadProgressChanged;
                    client.InternaetReachabilityChanged -= OnInternetReachabilityChanged;
                }
                clientNBG.DownloadDataAsync(new Uri(uri, patchFileName), null);
            }
            else
            {
#endif
#if MDEBUG
                Debug.Log("--------------------------------------");
                Debug.Log("Enable BGDownload");
                Debug.Log("--------------------------------------");
#endif
                patchStep = PatchStep.WaitPatchInfo;
                client.DownloadFileAsync(new Uri(uri, patchFileName), Path.Combine(dstFolder, patchFileName), null);
#if USE_NBG_WHEN_ERROR
            }
#endif
        }

#if USE_NBG_WHEN_ERROR
#if USE_ASYNC_AWAIT
        private async void OnDownloadDataCompletedNBG(object sender, DownloadDataCompletedEventArgs e)
#else
        private void OnDownloadDataCompletedNBG(object sender, DownloadDataCompletedEventArgs e)
#endif
        {
            if (e.Cancelled)
            {
                ThreadSafeDispatcher.Instance.Invoke(() => {
                    ErrorOccurred(patchFileName, "Cancelled");
                });
                return;
            }

            if (e.Error != null)
            {
                ThreadSafeDispatcher.Instance.Invoke(() => {
                    ErrorOccurred(patchFileName, e.Error.Message);
                });
                return;
            }

#if !USE_ASYNC_AWAIT
#if PATCH_HASH128
            ThreadSafeDispatcher.Instance.Invoke(() =>
            {
#endif
#endif

                XXTea xxtea = new XXTea(key);
                byte[] decrypted = xxtea.Decrypt(e.Result);
                long totalSize = 0;
                using (var stream = new MemoryStream(decrypted))
                using (var zipStream = new ZlibStream(stream, CompressionMode.Decompress))
                using (var reader = new StreamReader(zipStream))
                {
                    for (; ; )
                    {
                        string line = reader.ReadLine();
                        if (line == null) break;

                        string[] tokens = line.Split(new char[] { ',' });
#if PATCH_HASH128
                        if (tokens.Length != 7 && tokens.Length != 8) break;
#else
                        if (tokens.Length != 6 && tokens.Length != 7) break;
#endif

                        PatcherItem item = new PatcherItem();
                        item.URL = null;
                        item.Relative = tokens[0];
                        item.FileName = tokens[1];
                        item.ZipName = tokens[2];
                        item.FileSize = long.Parse(tokens[3]);
                        item.ZipSize = long.Parse(tokens[4]);
                        item.LastWriteTime = new DateTime(long.Parse(tokens[5]));
#if !CREATE_RELATIVE_PATH
                        item.TargetFolder = dstFolder;
#else
                        item.TargetFolder = Path.Combine(dstFolder, item.Relative);
#endif
                        item.Target = Path.Combine(item.TargetFolder, item.FileName);

                        item.IsCompleted = false;
                        item.ReceivedSize = 0;

#if PATCH_HASH128
                        if (tokens.Length == 8)
                        {
                            item.HasCRC = true;
                            item.CRC = uint.Parse(tokens[6]);
                            item.Hash128 = Hash128.Parse(tokens[7]);
                        }
                        else
                        {
                            item.Hash128 = Hash128.Parse(tokens[6]);
                        }
                        if (IsCached(item.FileName, item.Hash128))
                        {
                            continue;
                        }
                        else if (File.Exists(item.Target))
#else
                        if (tokens.Length == 7)
                        {
                            item.HasCRC = true;
                            item.CRC = uint.Parse(tokens[6]);
                        }

                        if (File.Exists(item.Target))
#endif
                        {
                            FileInfo info = new FileInfo(item.Target);
                            if (info.Length == item.FileSize)
                            {
#if USE_ASYNC_AWAIT
                                if (item.HasCRC == false || item.CRC == await Crc.GetCRCAsync(item.Target)) 
#else
                                if (item.HasCRC == false || item.CRC == Crc.GetCRC(item.Target)) 
#endif

#if CALLBACK_COMPLETED_FOR_NON_CACHED_FILES
                                {
#if USE_OBB_PRE_PATCH
                                    if(!OBBPrepatched || OpenConfirmDialog == null) continue;                                                
#endif
                                    item.ReceivedSize = item.FileSize = item.ZipSize = 0;
                                    item.IsCompleted = true;

                                    if (FileCompleted != null)
                                    {
                                        long received;
                                        long totalSize2;
                                        int completedCount;
                                        GetTotalProgressed(out received, out totalSize2, out completedCount);

                                        ThreadSafeDispatcher.Instance.Invoke(() =>
                                        {
                                            FileCompleted(item.FileName, completedCount, patcherItems.Count);
                                        });
                                    }
                                    currentIndex++;
                                    totalSize += item.FileSize;
                                    patcherItems.Insert(0, item);
                                    continue;
                                }
#else
                                    continue;
#endif
                            }
                        }
                        totalSize += item.ZipSize;
                        patcherItems.Add(item);
                    }
                }
#if CALLBACK_COMPLETED_FOR_NON_CACHED_FILES
                if (patcherItems.Count <= 0 || currentIndex >= patcherItems.Count)
#else
                if (patcherItems.Count <= 0)
#endif
                {
#if !PATCH_HASH128
                    ThreadSafeDispatcher.Instance.Invoke(() =>
                    {
#endif
#if USE_NBG_WHEN_ERROR
                        hasErrorBGWebClient = false;
#endif
#if PATCH_ALL_COMPLETED_WITH_PATCHERITEMS
                        AllCompleted(patcherItems);
#else
                        AllCompleted(patcherItems.Count);
#endif
#if !PATCH_HASH128
                    });
#endif
                }
                else
                {
                    if (OpenConfirmDialog == null)
                    {
                        DownloadNextFileNBG();
                    }
                    else
                    {
#if !PATCH_HASH128
                        ThreadSafeDispatcher.Instance.Invoke(() =>
                        {
#endif
                        OpenConfirmDialog(totalSize, () =>
                        {
                            DownloadNextFileNBG();
                        });
#if !PATCH_HASH128
                        });
#endif
                    }
                }
#if !USE_ASYNC_AWAIT
#if PATCH_HASH128
            });
#endif
#endif
        }


        // Return : true -> call DownloadNextFile() again
        private void DownloadNextFileNBG()
        {
            currentIndex++;
            if (currentIndex >= patcherItems.Count) return;

            PatcherItem item = patcherItems[currentIndex];

            string src = Path.Combine(item.Relative, item.ZipName);
            src = src.Replace('\\', Path.DirectorySeparatorChar);
            if (!Directory.Exists(item.TargetFolder))
            {
                Directory.CreateDirectory(item.TargetFolder);
            }

            string zipPath = Path.Combine(item.TargetFolder, item.ZipName);
            
            if(File.Exists(zipPath)) File.Delete(zipPath);

            if(item.URL == null)
                clientNBG.DownloadFileAsync(new Uri(uri, src), zipPath, item);
            else
                clientNBG.DownloadFileAsync(new Uri(item.URL), zipPath, item);
        }

        private void OnAsyncCompletedNBG(object sender, AsyncCompletedEventArgs e)
        {
            PatcherItem item = (PatcherItem)e.UserState;

            if (e.Cancelled)
            {
                ThreadSafeDispatcher.Instance.Invoke(() => {
                    ErrorOccurred(item.FileName, "Cancelled");
                });
                return;
            }

            if (e.Error != null)
            {
                ThreadSafeDispatcher.Instance.Invoke(() => {
                    ErrorOccurred(item.FileName, e.Error.Message);
                });
                return;
            }

            if (!item.FileName.Equals(item.ZipName))
            {
                string zipPath = Path.Combine(item.TargetFolder, item.ZipName);

                using (ZipFile zip = ZipFile.Read(zipPath))
                {
                    zip.ExtractAll(item.TargetFolder, ExtractExistingFileAction.OverwriteSilently);
                    zip.Dispose();
                }

                try { File.Delete(zipPath); } catch { };
            }

            long received;
            long totalSize;
            int completedCount;
            GetTotalProgressed(out received, out totalSize, out completedCount);

            ThreadSafeDispatcher.Instance.Invoke(() => {
                FileCompleted(item.FileName, currentIndex + 1, patcherItems.Count);
            });

            DownloadNextFileNBG();
            if (currentIndex >= patcherItems.Count)
            {
#if USE_NBG_WHEN_ERROR
                hasErrorBGWebClient = false;
#endif
                ThreadSafeDispatcher.Instance.Invoke(() => {
#if PATCH_ALL_COMPLETED_WITH_PATCHERITEMS
                    AllCompleted(patcherItems);
#else
                    AllCompleted(patcherItems.Count);
#endif
                });
            }
        }

        private void OnDownloadProgressChangedNBG(object sender, DownloadProgressChangedEventArgs e)
        {
            if (e.UserState != null)
            {
                PatcherItem item = (PatcherItem)e.UserState;
                lock (patcherItems)
                {
                    foreach (PatcherItem i in patcherItems)
                    {
                        if (String.Compare(i.ZipName, item.ZipName, false) == 0)
                        {
                            i.ReceivedSize = e.BytesReceived;
                            i.IsCompleted = (i.ZipSize <= e.BytesReceived);
                            break;
                        }
                    }
                }

                long received;
                long totalSize;
                int completedCount;

                GetTotalProgressed(out received, out totalSize, out completedCount);

                ThreadSafeDispatcher.Instance.Invoke(() => {
                    Progressed(item.FileName, e.BytesReceived, e.TotalBytesToReceive, currentIndex + 1, patcherItems.Count);
                });

                ThreadSafeDispatcher.Instance.Invoke(() => {
                    TotalProgressed(received, totalSize, completedCount, patcherItems.Count);
                });
            }
        }
#endif

        private void DownloadFile(PatcherItem item)
        {
            currentIndex++;

            string src = Path.Combine(item.Relative, item.ZipName);
            src = src.Replace('\\', Path.DirectorySeparatorChar);
            if (!Directory.Exists(item.TargetFolder))
            {
                Directory.CreateDirectory(item.TargetFolder);
            }

            string zipPath = Path.Combine(item.TargetFolder, item.ZipName);
            
            if(File.Exists(zipPath)) File.Delete(zipPath);

            if(item.URL == null)
            {
                Uri localUri = new Uri(uri, src);
                client.DownloadFileAsync(localUri, zipPath, item);
            }
            else 
            {
                client.DownloadFileAsync(new Uri(item.URL), zipPath, item);
            }
        }

        private void OnInternetReachabilityChanged(bool reachable)
        {
            ReachabilityChanged(reachable);
        }

#if USE_ASYNC_AWAIT
        private async void OnAsyncCompleted(string url, BGWebClient.ResultCode code)
#else
        private void OnAsyncCompleted(string url, BGWebClient.ResultCode code)
#endif
        {
            if (url.EndsWith(patchFileName) && patchStep == PatchStep.WaitPatchInfo)
            {  // 
                if (code == BGWebClient.ResultCode.Cancelled)
                {
                    //  자체적으로 Cancel 시킨 것은 무시한다.
                    //patchStep = PatchStep.None;
                    //ThreadSafeDispatcher.Instance.Invoke(() =>
                    //{
                    //    ErrorOccurred(patchFileName, "Cancelled");
                    //});
                    return;
                }
                if (code == BGWebClient.ResultCode.Error)
                {
                    patchStep = PatchStep.None;
                    ThreadSafeDispatcher.Instance.Invoke(() =>
                    {
#if USE_NBG_WHEN_ERROR
                        hasErrorBGWebClient = true;
#endif
                        ErrorOccurred(patchFileName, "Error");
                    });
                    return;
                }

                byte[] result = null;
                try
                {
                    result = File.ReadAllBytes(Path.Combine(dstFolder, patchFileName));

                    try { File.Delete(Path.Combine(dstFolder, patchFileName)); } catch {}


                    if (result != null)
                    {
                        XXTea xxtea = new XXTea(key);
                        byte[] decrypted = xxtea.Decrypt(result);
                        long totalSize = 0;

                        using (var stream = new MemoryStream(decrypted))
                        using (var zipStream = new ZlibStream(stream, CompressionMode.Decompress))
                        using (var reader = new StreamReader(zipStream))
                        {
                            string line = null;
                            for(;;)
                            {
                                line = reader.ReadLine();
                                if (line == null) break;

                                string[] tokens = line.Split(new char[] { ',' });
#if PATCH_HASH128
                                if (tokens.Length != 7 && tokens.Length != 8) break;
#else
                                if (tokens.Length != 6 && tokens.Length != 7) break;
#endif
                                PatcherItem item = new PatcherItem();
                                item.URL = null;
                                item.Relative = tokens[0];
                                item.FileName = tokens[1];
                                item.ZipName = tokens[2];
                                item.FileSize = long.Parse(tokens[3]);
                                item.ZipSize = long.Parse(tokens[4]);
                                item.LastWriteTime = new DateTime(long.Parse(tokens[5]));
#if !CREATE_RELATIVE_PATH
                                item.TargetFolder = dstFolder;
#else
                                item.TargetFolder = Path.Combine(dstFolder, item.Relative);
#endif
                                item.Target = Path.Combine(item.TargetFolder, item.FileName);

                                item.IsCompleted = false;
                                item.ReceivedSize = 0;

#if PATCH_HASH128
                                if (tokens.Length == 8)
                                {
                                    item.HasCRC = true;
                                    item.CRC = uint.Parse(tokens[6]);
                                    item.Hash128 = Hash128.Parse(tokens[7]);
                                }
                                else {
                                    item.Hash128 = Hash128.Parse(tokens[6]);
                                }

                                if (IsCached(item.FileName, item.Hash128)) {
                                    continue;
                                }
                                else if(File.Exists(item.Target))
#else
                                if (tokens.Length == 7)
                                {
                                    item.HasCRC = true;
                                    item.CRC = uint.Parse(tokens[6]);
                                }

                                if (File.Exists(item.Target))
#endif
                                {
                                    FileInfo info = new FileInfo(item.Target);
                                    if (info.Length == item.FileSize)
                                    {
#if USE_ASYNC_AWAIT
                                        if (item.HasCRC == false || item.CRC == await Crc.GetCRCAsync(item.Target)) 
#else
                                        if (item.HasCRC == false || item.CRC == Crc.GetCRC(item.Target)) 
#endif

#if CALLBACK_COMPLETED_FOR_NON_CACHED_FILES
                                        {
#if USE_OBB_PRE_PATCH
                                            if(!OBBPrepatched || OpenConfirmDialog == null) continue;                                                
#endif
                                            item.ReceivedSize = item.FileSize = item.ZipSize = 0;
                                            item.IsCompleted = true;

                                            if (FileCompleted != null)
                                            {
                                                long received;
                                                long totalSize2;
                                                int completedCount;
                                                GetTotalProgressed(out received, out totalSize2, out completedCount);

                                                ThreadSafeDispatcher.Instance.Invoke(() =>
                                                {
                                                    FileCompleted(item.FileName, completedCount, patcherItems.Count);
                                                });
                                            }
                                        }
#else
                                            continue;
#endif
                                    }
                                }

                                totalSize += item.ZipSize;
                                lock (patcherItems)
                                {
                                    patcherItems.Add(item);
                                }
                            }
                            if (IsAllFinished())
                            {
                                patchStep = PatchStep.None;
#if USE_NBG_WHEN_ERROR
                                hasErrorBGWebClient = false;
#endif
                                ThreadSafeDispatcher.Instance.Invoke(() =>
                                {
#if PATCH_ALL_COMPLETED_WITH_PATCHERITEMS
                                    AllCompleted(patcherItems);
#else
                                    AllCompleted(patcherItems.Count);
#endif
                                });
                            }
                            else
                            {
                                if (OpenConfirmDialog == null)
                                {
                                    lock (patcherItems)
                                    {
                                        patchStep = PatchStep.WaitDataFiles;
                                        foreach (PatcherItem i in patcherItems)
                                        {
                                            DownloadFile(i);
                                        }
                                    }
                                }
                                else
                                {
                                    ThreadSafeDispatcher.Instance.Invoke(() =>
                                    {
                                        OpenConfirmDialog(totalSize, () =>
                                        {
                                            lock (patcherItems)
                                            {
                                                patchStep = PatchStep.WaitDataFiles;
                                                foreach (PatcherItem i in patcherItems)
                                                {
                                                    DownloadFile(i);
                                                }
                                            }
                                        });
                                    });
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
#if USE_NBG_WHEN_ERROR
                    hasErrorBGWebClient = true;
#endif
#if MDEBUG
                    Debug.Log("Invalid Patch Info (" + e.ToString() + ")");
#endif
                    patchStep = PatchStep.None;
                    ErrorOccurred(patchFileName, "Invalid Patch Info (" + e.Message + ")");
                }
            }
            else if(patchStep == PatchStep.WaitDataFiles)
            {
                PatcherItem item = SearchItem(url);

                if (item != null)
                {
                    if (code == BGWebClient.ResultCode.Cancelled)
                    {
                        //  자체적으로 Cancel 시킨 것은 무시한다.
                        //patchStep = PatchStep.None;
                        //ErrorOccurred(item.FileName, "Cancelled");
                        return;
                    }

                    if (code == BGWebClient.ResultCode.Error)
                    {
#if USE_NBG_WHEN_ERROR
                        hasErrorBGWebClient = true;
#endif
                        patchStep = PatchStep.None;
                        ErrorOccurred(item.FileName, "Error");//e.Error.Message);
                        return;
                    }

                    if(!item.FileName.Equals(item.ZipName)) 
                    {
                        string zipPath = Path.Combine(item.TargetFolder, item.ZipName);

                        using (ZipFile zip = ZipFile.Read(zipPath))
                        {
                            zip.ExtractAll(item.TargetFolder, ExtractExistingFileAction.OverwriteSilently);
                        }

                        try { File.Delete(zipPath); } catch { }
                    }

                    item.ReceivedSize = item.ZipSize;
                    item.IsCompleted = true;

                    if (FileCompleted != null)
                    {
                        long received;
                        long totalSize;
                        int completedCount;
                        GetTotalProgressed(out received, out totalSize, out completedCount);

                        ThreadSafeDispatcher.Instance.Invoke(() =>
                        {
                            FileCompleted(item.FileName, completedCount, patcherItems.Count);
                        });
                    }

                    if (IsAllFinished())
                    {
                        patchStep = PatchStep.None;
#if USE_NBG_WHEN_ERROR
                        hasErrorBGWebClient = false;
#endif
                        ThreadSafeDispatcher.Instance.Invoke(() =>
                        {
#if PATCH_ALL_COMPLETED_WITH_PATCHERITEMS
                            AllCompleted(patcherItems);
#else
                            AllCompleted(patcherItems.Count);
#endif
                        });
                    }
                }
            }
        }

        private void OnDownloadProgressChanged(string url, int writtenBytes, int expectedWrittenBytes)
        {
            PatcherItem item = SearchItem(url);

            if (item != null)
            {
                item.ReceivedSize = writtenBytes;

                long received;
                long totalSize;
                int completedCount;
                GetTotalProgressed(out received, out totalSize, out completedCount);

                ThreadSafeDispatcher.Instance.Invoke(() =>
                {
                    Progressed(item.FileName, writtenBytes, expectedWrittenBytes, completedCount, patcherItems.Count);
                    TotalProgressed(received, totalSize, completedCount, patcherItems.Count);
                });
            }
        }

        private PatcherItem SearchItem(string url)
        {
            lock (patcherItems)
            {
                foreach (PatcherItem i in patcherItems)
                {
                    string itemUrl = i.URL;
                    if(i.URL == null)
                    {
                        itemUrl = Path.Combine(i.Relative, i.ZipName);
                        itemUrl = itemUrl.Replace('\\', Path.DirectorySeparatorChar);
                        Uri localUri = new Uri(uri, itemUrl);
                        itemUrl = localUri.ToString();
                    }
                    if (String.Compare(itemUrl, url, false) == 0)
                    {
                        return i;
                    }
                }
                return null;
            }
        }

        private bool IsAllFinished()
        {
            lock (patcherItems)
            {
                foreach (PatcherItem i in patcherItems)
                {
                    if (!i.IsCompleted)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

#else
        private System.Net.WebClient client = new System.Net.WebClient();

        public PatcherAsyncBG(string url)
        {
            baseUrl = url;

            client = new System.Net.WebClient();

            client.DownloadDataCompleted += new DownloadDataCompletedEventHandler(OnDownloadDataCompleted);
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(OnAsyncCompleted);
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(OnDownloadProgressChanged);
        }

        public void Patch(string patchName, string dstFolder, uint[] key, OnOpenConfirmDialog callback, string patchbin = "Patch.bin", int versionCode = -1, bool deleteObb = false, bool isOnlyUseBaseUrl = false)
        {
            patchFileName = patchbin;
            OpenConfirmDialog = callback;

            currentIndex = -1;
            if (isOnlyUseBaseUrl)
            {
                uri = new Uri(baseUrl);
            }
            else if (string.IsNullOrEmpty(patchName))
            {
                uri = new Uri(baseUrl + AssetBundleUtility.GetPlatformName() + "/");
            }
            else
            {
                uri = new Uri(baseUrl + AssetBundleUtility.GetPlatformName() + "/" + patchName + "/");
            }
            this.dstFolder = string.IsNullOrEmpty(dstFolder) ? string.Empty : Path.GetFullPath(dstFolder);
            this.key = key;

            patcherItems = new List<PatcherItem>();

            client.DownloadDataAsync(new Uri(uri, patchFileName), null);
        }

#if USE_ASYNC_AWAIT
        private async void OnDownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
#else
        private void OnDownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
#endif
        {
            if (e.Cancelled)
            {
                ThreadSafeDispatcher.Instance.Invoke(() =>
                {
                    ErrorOccurred(patchFileName, "Cancelled");
                });
                return;
            }

            if (e.Error != null)
            {
                ThreadSafeDispatcher.Instance.Invoke(() =>
                {
                    ErrorOccurred(patchFileName, e.Error.Message);
                });
                return;
            }

#if !USE_ASYNC_AWAIT
#if PATCH_HASH128
            ThreadSafeDispatcher.Instance.Invoke(() =>
            {
#endif
#endif
                XXTea xxtea = new XXTea(key);
                byte[] decrypted = xxtea.Decrypt(e.Result);
                long totalSize = 0;
                using (var stream = new MemoryStream(decrypted))
                using (var zipStream = new ZlibStream(stream, CompressionMode.Decompress))
                using (var reader = new StreamReader(zipStream))
                {
                    for (; ; )
                    {
                        string line = reader.ReadLine();
                        if (line == null) break;

                        Debug.Log("PatchItemInfo : " + line);

                        string[] tokens = line.Split(new char[] { ',' });
#if PATCH_HASH128
                        if (tokens.Length != 7 && tokens.Length != 8) break;
#else
                        if (tokens.Length != 6 && tokens.Length != 7) break;
#endif
                        Debug.Log("Relative [" + tokens[0] + "]         FileName [" + tokens[1] + "]");

                        PatcherItem item = new PatcherItem();
                        item.URL = null;
                        item.Relative = tokens[0];
                        item.FileName = tokens[1];
                        item.ZipName = tokens[2];
                        item.FileSize = long.Parse(tokens[3]);
                        item.ZipSize = long.Parse(tokens[4]);
                        item.LastWriteTime = new DateTime(long.Parse(tokens[5]));
#if !CREATE_RELATIVE_PATH
                        item.TargetFolder = dstFolder;
#else
                        item.TargetFolder = Path.Combine(dstFolder, item.Relative);
#endif
                        item.Target = Path.Combine(item.TargetFolder, item.FileName);

                        item.IsCompleted = false;
                        item.ReceivedSize = 0;
#if PATCH_HASH128
                        if (tokens.Length == 8)
                        {
                            item.HasCRC = true;
                            item.CRC = uint.Parse(tokens[6]);
                            item.Hash128 = Hash128.Parse(tokens[7]);
                        }
                        else
                        {
                            item.Hash128 = Hash128.Parse(tokens[6]);
                        }
                        if (IsCached(item.FileName, item.Hash128))
                        {
                            continue;
                        }
                        else if (File.Exists(item.Target))
#else
                        if (tokens.Length == 7)
                        {
                            item.HasCRC = true;
                            item.CRC = uint.Parse(tokens[6]);
                        }

                        if (File.Exists(item.Target))
#endif
                        {
                            FileInfo info = new FileInfo(item.Target);
                            if (info.Length == item.FileSize)
                            {
#if USE_ASYNC_AWAIT
                                if (item.HasCRC == false || item.CRC == await Crc.GetCRCAsync(item.Target)) 
#else
                                if (item.HasCRC == false || item.CRC == Crc.GetCRC(item.Target))
#endif

#if CALLBACK_COMPLETED_FOR_NON_CACHED_FILES
                                {
#if USE_OBB_PRE_PATCH
                                    if (!OBBPrepatched || OpenConfirmDialog == null) continue;
#endif
                                    item.ReceivedSize = item.FileSize = item.ZipSize = 0;
                                    item.IsCompleted = true;

                                    if (FileCompleted != null)
                                    {
                                        long received;
                                        long totalSize2;
                                        int completedCount;
                                        GetTotalProgressed(out received, out totalSize2, out completedCount);

                                        ThreadSafeDispatcher.Instance.Invoke(() =>
                                        {
                                            FileCompleted(item.FileName, completedCount, patcherItems.Count);
                                        });
                                    }
                                    currentIndex++;
                                    totalSize += item.FileSize;
                                    patcherItems.Insert(0, item);
                                    continue;
                                }
#else
                                    continue;
#endif
                            }
                        }
                        totalSize += item.ZipSize;
                        patcherItems.Add(item);
                    }
                }
#if CALLBACK_COMPLETED_FOR_NON_CACHED_FILES
                if (patcherItems.Count <= 0 || currentIndex >= patcherItems.Count)
#else
                if (patcherItems.Count <= 0)
#endif
                {
#if !PATCH_HASH128
                    ThreadSafeDispatcher.Instance.Invoke(() =>
                    {
#endif
#if PATCH_ALL_COMPLETED_WITH_PATCHERITEMS
                        AllCompleted(patcherItems);
#else
                    AllCompleted(patcherItems.Count);
#endif
#if !PATCH_HASH128
                    });
#endif
                }
                else
                {
                    if (OpenConfirmDialog == null)
                    {
                        DownloadNextFile();
                    }
                    else
                    {
#if !PATCH_HASH128
                        ThreadSafeDispatcher.Instance.Invoke(() =>
                        {
#endif
                        OpenConfirmDialog(totalSize, () =>
                        {
                            DownloadNextFile();
                        });
#if !PATCH_HASH128
                        });
#endif
                    }
                }
#if !USE_ASYNC_AWAIT
#if PATCH_HASH128
            });
#endif
#endif
        }


        // Return : true -> call DownloadNextFile() again
        private void DownloadNextFile()
        {
            currentIndex++;
            if (currentIndex >= patcherItems.Count) return;

            PatcherItem item = patcherItems[currentIndex];

            string src = Path.Combine(item.Relative, item.ZipName);
            src = src.Replace('\\', Path.DirectorySeparatorChar);

            if (!Directory.Exists(item.TargetFolder))
            {
                Directory.CreateDirectory(item.TargetFolder);
            }

            string zipPath = Path.Combine(item.TargetFolder, item.ZipName);

            if (File.Exists(zipPath)) File.Delete(zipPath);

            if (item.URL == null)
                client.DownloadFileAsync(new Uri(uri, src), zipPath, item);
            else
                client.DownloadFileAsync(new Uri(item.URL), zipPath, item);
        }

        private void OnAsyncCompleted(object sender, AsyncCompletedEventArgs e)
        {
            PatcherItem item = (PatcherItem)e.UserState;

            if (e.Cancelled)
            {
                ThreadSafeDispatcher.Instance.Invoke(() =>
                {
                    ErrorOccurred(item.FileName, "Cancelled");
                });
                return;
            }

            if (e.Error != null)
            {
                ThreadSafeDispatcher.Instance.Invoke(() =>
                {
                    ErrorOccurred(item.FileName, e.Error.Message);
                });
                return;
            }

            if (!item.FileName.Equals(item.ZipName))
            {
                string zipPath = Path.Combine(item.TargetFolder, item.ZipName);

                using (ZipFile zip = ZipFile.Read(zipPath))
                {
                    zip.ExtractAll(item.TargetFolder, ExtractExistingFileAction.OverwriteSilently);
                    zip.Dispose();
                }

                try { File.Delete(zipPath); } catch { };
            }

            long received;
            long totalSize;
            int completedCount;
            GetTotalProgressed(out received, out totalSize, out completedCount);

            ThreadSafeDispatcher.Instance.Invoke(() =>
            {
                FileCompleted(item.FileName, currentIndex + 1, patcherItems.Count);
            });

            DownloadNextFile();
            if (currentIndex >= patcherItems.Count)
            {
                ThreadSafeDispatcher.Instance.Invoke(() =>
                {
#if PATCH_ALL_COMPLETED_WITH_PATCHERITEMS
                    AllCompleted(patcherItems);
#else
                    AllCompleted(patcherItems.Count);
#endif
                });
            }
        }

        private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (e.UserState != null)
            {
                PatcherItem item = (PatcherItem)e.UserState;
                lock (patcherItems)
                {
                    foreach (PatcherItem i in patcherItems)
                    {
                        if (String.Compare(i.ZipName, item.ZipName, false) == 0)
                        {
                            i.ReceivedSize = e.BytesReceived;
                            i.IsCompleted = (i.ZipSize <= e.BytesReceived);
                            break;
                        }
                    }
                }

                long received;
                long totalSize;
                int completedCount;

                GetTotalProgressed(out received, out totalSize, out completedCount);

                ThreadSafeDispatcher.Instance.Invoke(() =>
                {
                    Progressed(item.FileName, e.BytesReceived, e.TotalBytesToReceive, currentIndex + 1, patcherItems.Count);
                });

                ThreadSafeDispatcher.Instance.Invoke(() =>
                {
                    TotalProgressed(received, totalSize, completedCount, patcherItems.Count);
                });
            }
        }
#endif
        private void GetTotalProgressed(out long received, out long totalSize, out int completedCount)
        {
            lock (patcherItems)
            {
                received = 0;
                totalSize = 0;
                completedCount = 0;
                foreach (PatcherItem i in patcherItems)
                {
                    received += i.ReceivedSize;
                    totalSize += i.ZipSize;
                    if (i.IsCompleted)
                    {
                        completedCount++;
                    }
                }
            }
        }

        public void DownloadOBB(OnOpenConfirmDialog callback, int versionCode = -1, string obburl = null, int obbsize = 0)
        {
#if !UNITY_EDITOR && UNITY_ANDROID
//            hasErrorBGWebClient = true;
            OpenConfirmDialog = callback;
            currentIndex = -1;
            patchStep = PatchStep.WaitPatchInfo;

            long manifestOBBFileSize = -1;
#if MDEBUG
            Debug.Log("OBBVALIDATE unity.build-id = [" + client.GetUnityBuildId() + "]");
            Debug.Log("OBBVALIDATE haeginsplitappbinsize = [" + client.GetUnitySplitApplicationBinarySize() + "]");
#endif
            try { manifestOBBFileSize = long.Parse(client.GetUnitySplitApplicationBinarySize()); } catch {}
            if(manifestOBBFileSize > 0) obbsize = (int)manifestOBBFileSize;
            if (File.Exists(client.GetOBBFilePath(versionCode)) && manifestOBBFileSize > 0)
            {
                // OBB File Validation
                try
                {
                    long existOBBFileSize = new FileInfo(client.GetOBBFilePath(versionCode)).Length;
                    bool isValid = false;
                    ZipFile zip = new ZipFile(client.GetOBBFilePath(versionCode));
                    if (zip.ContainsEntry("assets/" + client.GetUnityBuildId()) == false || manifestOBBFileSize != existOBBFileSize) 
                    {
#if MDEBUG
                        Debug.Log("OBBVALIDATE Invalid OBB File [" + client.GetOBBFilePath(versionCode) + "]");
#endif
                        // if obb file is invalid then delete obb file
                        File.Delete(client.GetOBBFilePath(versionCode));
#if MDEBUG
                        Debug.Log("OBBVALIDATE Delete OBB File [" + client.GetOBBFilePath(versionCode) + "]");
#endif
                    }
                    else
                    {
#if MDEBUG
                        Debug.Log("OBBVALIDATE Valid OBB File [" + client.GetOBBFilePath(versionCode) + "]");
#endif
                    }
                }
                catch (Exception ee)
                {
#if MDEBUG
                    Debug.Log("OBBVALIDATE " + ee.ToString());
#endif
                }
            }


            if (!File.Exists(client.GetOBBFilePath(versionCode)))
            {
                string dstFolder = client.GetOBBFilePath(versionCode);
                this.dstFolder = dstFolder.Substring(0, dstFolder.LastIndexOf('/'));
                string obbFileName = dstFolder.Substring(dstFolder.LastIndexOf('/') + 1, dstFolder.Length - dstFolder.LastIndexOf('/') - 1);
#if MDEBUG
                Debug.Log("OBB Filename = " + obbFileName);
#endif
                if (!Directory.Exists(this.dstFolder))
                {
                    Directory.CreateDirectory(this.dstFolder);
                }
#if USE_ONESTORE_IAP
                int count = 0;
                string[] filename = null;
                int[] filesize = null;
                string[] url = null;
#else
                client.GetOBBDownloadInfo(ProjectSettings.base64EncodedPublicKey, (int count, string[] filename, int[] filesize, string[] url) => {
#endif
#if MDEBUG
                    Debug.Log("OBB Count = " + count);
#endif
                    bool available = false;
                    long totalSize = 0;

                    if (count > 0)
                    {
                        for(int i = 0; i < count; i++) {
                            if(filename[i].Equals(obbFileName)) available = true;
                        }
                        if(available) {
                            patcherItems = new List<PatcherItem>();
                            while (count > 0)
                            {
                                PatcherItem item = new PatcherItem();
                                item.Relative = "";
                                item.FileName = filename[count - 1];
                                item.ZipName = item.FileName;
                                item.FileSize = (long)(filesize[count - 1]);
                                item.ZipSize = item.FileSize;
                                item.LastWriteTime = DateTime.UtcNow;
                                item.TargetFolder = client.GetWritableOBBDir();//  this.dstFolder;
                                item.Target = Path.Combine(item.TargetFolder, item.FileName);
                                item.IsCompleted = false;
                                item.ReceivedSize = 0;
                                item.HasCRC = false;
                                item.CRC = 0;
                                item.URL = url[count - 1];
                                totalSize += item.ZipSize;
                                patcherItems.Add(item);

                                count--;
                            }
                        }
                    }
                    else 
                    {
                        if(obburl != null)
                        {
#if MDEBUG
                            Debug.Log("Download OBB from " + obburl);
#endif
                            available = true;
                            patcherItems = new List<PatcherItem>();
                            PatcherItem item = new PatcherItem();
                            item.Relative = "";
                            item.FileName = obbFileName;
                            item.ZipName = item.FileName;
                            item.FileSize = (long)(obbsize);
                            item.ZipSize = item.FileSize;
                            item.LastWriteTime = DateTime.UtcNow;
                            item.TargetFolder = client.GetWritableOBBDir();//   this.dstFolder;
                            item.Target = Path.Combine(item.TargetFolder, item.FileName);
                            item.IsCompleted = false;
                            item.ReceivedSize = 0;
                            item.HasCRC = false;
                            item.CRC = 0;
                            item.URL = obburl;
                            totalSize += item.ZipSize;
                            patcherItems.Add(item);
                        }
                    }

                    if(!available) {
                        // 파일이 없는데 받을 정보를 얻어올 수 없는 상황이면 에러지
                        ThreadSafeDispatcher.Instance.Invoke(() =>
                        {
                            ErrorOccurred("obb", "Error");
                        });
                        return;
                    }

#if USE_NBG_WHEN_ERROR
                    if (hasErrorBGWebClient)
                    {
                        if(clientNBG == null) {
                            clientNBG = new System.Net.WebClient();

                            clientNBG.DownloadDataCompleted += new DownloadDataCompletedEventHandler(OnDownloadDataCompletedNBG);
                            clientNBG.DownloadFileCompleted += new AsyncCompletedEventHandler(OnAsyncCompletedNBG);
                            clientNBG.DownloadProgressChanged += new DownloadProgressChangedEventHandler(OnDownloadProgressChangedNBG);

                            client.DownloadFileCompleted -= OnAsyncCompleted;
                            client.DownloadProgressChanged -= OnDownloadProgressChanged;
                            client.InternaetReachabilityChanged -= OnInternetReachabilityChanged;
                        }
#if !PATCH_HASH128
                        ThreadSafeDispatcher.Instance.Invoke(() =>
                        {
#endif
                            OpenConfirmDialog(totalSize, () =>
                            {
                                DownloadNextFileNBG();
                            });
#if !PATCH_HASH128
                        });
#endif
                    }
                    else
                    {
#endif
                        if (OpenConfirmDialog == null)
                        {
                            lock (patcherItems)
                            {
                                patchStep = PatchStep.WaitDataFiles;
                                foreach (PatcherItem i in patcherItems)
                                {
                                    DownloadFile(i);
                                }
                            }
                        }
                        else
                        {
                            ThreadSafeDispatcher.Instance.Invoke(() =>
                            {
                                OpenConfirmDialog(totalSize, () =>
                                {
                                    lock (patcherItems)
                                    {
                                        patchStep = PatchStep.WaitDataFiles;
                                        foreach (PatcherItem i in patcherItems)
                                        {
                                            DownloadFile(i);
                                        }
                                    }
                                });
                            });
                        }
#if USE_NBG_WHEN_ERROR
                    }
#endif
#if !USE_ONESTORE_IAP
                });
#endif
                return;
            }
#endif
#if PATCH_ALL_COMPLETED_WITH_PATCHERITEMS
            AllCompleted(new List<PatcherItem>());
#else
            AllCompleted(0);
#endif
        }

#if USE_OBB_PRE_PATCH
        private void CopyFromObb(string dst, string obbFilePath, bool deleteObb, OnFinishCopy callback)
        {
            CopyOBB obj = new CopyOBB();
            obj.patcher = this;
            obj.dstFolder = dst;
            obj.obbFilePath = obbFilePath;
            obj.deleteObb = deleteObb;
            obj.callback = callback;
            Thread thread = new Thread(obj.Run);
            thread.Start();
        }

        public void CallCopyProgressed(long totalDownloadedByte, long totalLength, int count, int totalCount)
        {
#if MDEBUG
            Debug.Log("CallCopyProgressed " + totalDownloadedByte + ", " + totalLength + ", " + count + ", " + totalCount);
#endif
            ThreadSafeDispatcher.Instance.Invoke(() =>
            {
                CopyProgressed(totalDownloadedByte, totalLength, count, totalCount);
            });
        }
#endif
    }

#if USE_OBB_PRE_PATCH
class CopyOBB
    {
        public PatcherAsyncBG patcher;
        public string dstFolder;
        public string obbFilePath;
        public bool deleteObb;
        public PatcherAsyncBG.OnFinishCopy callback;

        public void Run()
        {
#if MDEBUG
            Debug.Log("CopyFromOBB " + obbFilePath + " >>> " + dstFolder);
#endif
            int totalCount = 0;
            int count = 0;
            long obbsize = 0;
            long obbread = 0;
            try
            {
                // OBB 파일이 없으면 끝
                if (File.Exists(obbFilePath))
                {
                    // OBB 파일을 이미 처리했으면 끝
                    int offset = obbFilePath.LastIndexOf('/') + 1;
                    string prefs = obbFilePath.Substring(offset, obbFilePath.Length - offset);
                    if (PlayerPrefs.GetInt(prefs, 0) == 0)
                    {
                        PlayerPrefs.SetInt(prefs, 1);
                        PlayerPrefs.Save();

                        // dstFolder를 미리 만들어준다.
                        if (!Directory.Exists(dstFolder))
                        {
                            Directory.CreateDirectory(dstFolder);
                        }

#if CALLBACK_COMPLETED_FOR_NON_CACHED_FILES
#if USE_OBB_PRE_PATCH
                        patcher.OBBPrepatched = true;
#endif
#endif
                        // 본격적으로 OBB 파일을 풀어보자 
                        using (var inStream = File.OpenRead(obbFilePath))
                        {
                            obbsize = inStream.Length;
                            byte[] buf = new byte[14];
                            inStream.Read(buf, 0, 14);

                            if (buf[0] != 0 || buf[1] != 2 || buf[2] != 6 || buf[3] != 9 || buf[4] != 5 || buf[5] != 1 || buf[6] != 5 || buf[7] != 0 || buf[8] != 0 || buf[9] != 1)
                            {
                                count = 0;
                                deleteObb = false;
#if MDEBUG
                                Debug.Log("OBB : invalid obb file " + buf[0] + "," + buf[1] + "," + buf[2] + "," + buf[3] + "," + buf[4] + "," + buf[5] + "," + buf[6] + "," + buf[7] + "," + buf[8] + "," + buf[9]);
#endif
                            }
                            else
                            {
                                totalCount = count = (int)(((int)(buf[10]) << 24) & 0xFF000000) | (int)(((int)(buf[11]) << 16) & 0xFF0000) | (int)(((int)(buf[12]) << 8) & 0xFF00) | (int)((int)buf[13] & 0xFF);
                            }
#if MDEBUG
                            Debug.Log("File count in OBB : " + count);
#endif
                            while (count > 0)
                            {
                                byte[] filename = null;
                                byte[] filenamelength = new byte[4];
                                byte[] filesize = new byte[4];
                                int read = inStream.Read(filenamelength, 0, 4);
                                if (read <= 0) return; // 파일 다 읽어버렸네?
                                obbread += read;
                                int len = (int)(((int)(filenamelength[0]) << 24) & 0xFF000000) | (int)(((int)(filenamelength[1]) << 16) & 0xFF0000) | (int)(((int)(filenamelength[2]) << 8) & 0xFF00) | (int)((int)filenamelength[3] & 0xFF);
                                if (len >= 256 || len <= 0) return; // 파일 이름이 너무 길면 문제가 있는거다.
                                filename = new byte[len];
                                read = inStream.Read(filename, 0, len);
                                if (read <= 0) return; // 파일 다 읽어버렸네?
                                obbread += read;
                                string name = System.Text.Encoding.UTF8.GetString(filename);
                                read = inStream.Read(filesize, 0, 4);
                                if (read <= 0) return; // 파일 다 읽어버렸네?
                                obbread += read;
                                int size = (int)(((int)(filesize[0]) << 24) & 0xFF000000) | (int)(((int)(filesize[1]) << 16) & 0xFF0000) | (int)(((int)(filesize[2]) << 8) & 0xFF00) | (int)((int)filesize[3] & 0xFF);
                                if (size < 0) return; // 파일 사이즈가 음수면 에러..
                                using (var outStream = File.Open(dstFolder + "/" + name, FileMode.Create))
                                {
                                    int toRead = 0;
                                    byte[] fbuf = new byte[81920];
                                    do
                                    {
                                        patcher.CallCopyProgressed(obbread, obbsize, totalCount - count, totalCount);
                                        if (size >= 81920) toRead = 81920; else toRead = size;
                                        read = inStream.Read(fbuf, 0, toRead);
                                        if (read <= 0) return; // 파일 다 읽어버렸네?
                                        obbread += read;
                                        outStream.Write(fbuf, 0, read);
                                        size -= read;
                                        Thread.Sleep(5);
                                    } while (size > 0);
                                    outStream.Flush();
                                    outStream.Close();
                                }
                                count--;
#if MDEBUG
                                Debug.Log("OBB : " + name);
#endif
                                patcher.CallCopyProgressed(obbread, obbsize, totalCount - count, totalCount);
                            }
                        }
#if UNITY_ANDROID
                        if(deleteObb)
                        {
                            File.Delete(obbFilePath);
                        }
#endif
                    }
                    else
                    {
#if MDEBUG
                        Debug.Log("OBB : already processed");
#endif
                    }
                }
                else
                {
#if MDEBUG
                    Debug.Log("OBB : file not found");
#endif
                }
            }
            catch(Exception e)
            {
#if MDEBUG
                Debug.Log(e.ToString());
#endif
            }

            ThreadSafeDispatcher.Instance.Invoke(() =>
            {
                callback();
            });
        }
    }
}
#endif
