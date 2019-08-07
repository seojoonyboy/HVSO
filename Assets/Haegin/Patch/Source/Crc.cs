using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace G.Util
{
    public class Crc
    {
        private static readonly uint[] table = new uint[256];
        private uint crc;

        static Crc()
        {
            MakeTable();
        }

        public Crc()
        {
            Reset();
        }

        private static void MakeTable()
        {
            for (int n = 0; n < 256; n++)
            {
                uint c = (uint)n;
                for (int k = 0; k < 8; k++)
                {
                    if ((c & 1) == 1) { c = 0xedb88320U ^ (c >> 1); }
                    else { c = c >> 1; }
                }
                table[n] = c;
            }
        }

        public void Reset()
        {
            crc = 0xffffffff;
        }

        public void Update(byte[] buffer)
        {
            Update(buffer, buffer.Length);
        }

        public void Update(byte[] buffer, int length)
        {
            for (int i = 0; i < length; i++)
            {
                crc = table[(crc ^ buffer[i]) & 0xff] ^ (crc >> 8);
            }
        }

        public uint GetCRC()
        {
            return (crc ^ 0xffffffff);
        }

        public static uint GetCRC(byte[] buffer)
        {
            return GetCRC(buffer, buffer.Length);
        }

        public static uint GetCRC(byte[] buffer, int length)
        {
            Crc c = new Crc();
            c.Reset();
            c.Update(buffer, length);
            return c.GetCRC();
        }

        public static uint GetCRC(string text, Encoding enc)
        {
            byte[] buffer = enc.GetBytes(text);
            return GetCRC(buffer);
        }
        
		public static uint GetCRC(string filePath)
		{
			byte[] buffer = File.ReadAllBytes(filePath);
			return GetCRC(buffer);
		}

#if USE_ASYNC_AWAIT
        public static async System.Threading.Tasks.Task<uint> GetCRCAsync(string filepath)
        {
            try
            {
                using (FileStream sourceStream = File.Open(filepath, FileMode.Open))
                {
                    int offset = 0;
                    byte[] buffer = new byte[sourceStream.Length];
                    do
                    {
                        int readbytes = await sourceStream.ReadAsync(buffer, offset, ((int)sourceStream.Length - offset));
                        if (readbytes > 0) offset += readbytes;
                    } while (offset < sourceStream.Length);
                    return GetCRC(buffer);
                };
            }
            catch (Exception e)
            {
                return 0;
            }
        }
#endif
    }
}
