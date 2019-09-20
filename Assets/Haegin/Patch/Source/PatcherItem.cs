#define PATCH_HASH128
using System;

namespace G.Network
{
    public class PatcherItem
    {
        public string FileName { get; set; }
        public string ZipName { get; set; }
        public string Relative { get; set; }
        public long FileSize { get; set; }
        public long ZipSize { get; set; }
        public DateTime LastWriteTime { get; set; }
        public string TargetFolder { get; set; }
        public string Target { get; set; }
        public bool HasCRC { get; set; }
        public uint CRC { get; set; }
        public bool IsCompleted { get; set; }
        public long ReceivedSize { get; set; }
#if PATCH_HASH128
        public UnityEngine.Hash128 Hash128 { get; set; }
#endif
        public string URL { get; set; }
    }
}
