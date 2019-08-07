using System;
using System.Text;
using System.Net;
using System.IO;

namespace G.Util
{
	public class XXTea
	{
		private static readonly uint DELTA = 0x9e3779b9;

		private uint[] k;
		public uint[] Key { get { return k; } }

		public XXTea()
		{
			SetKey();
		}

		public XXTea(uint[] key)
		{
			SetKey(key);
		}

		public XXTea(string key)
		{
			SetKey(key);
		}

		public void SetKey()
		{
			k = new uint[4];

			Random random = new Random();
			k[0] = (uint)random.Next();
			k[1] = (uint)random.Next();
			k[2] = (uint)random.Next();
			k[3] = (uint)random.Next();
		}

		public void SetKey(uint[] key)
		{
			int len = key.Length;
			int fixedLen = 4;

			k = new uint[fixedLen];
			for (int i = 0; i < fixedLen; i++)
			{
				if (i < len)
					k[i] = key[i];
				else
					k[i] = 0;
			}
		}

		public bool SetKey(string base62Key)
		{
			if (base62Key == null) return false;

			try
			{
				SetKey(ConvertEx.ToKey(base62Key));
			}
			catch (Exception)
			{
				return false;
			}

			return true;
		}

		public byte[] Encrypt(byte[] data, int offset, int count)
		{
            try
            {
                uint[] v = new uint[(int)Math.Ceiling((double)(count + 4) / 4)];

				if(v == null) return null;  // IL2CPP 버그를 피하자.

                v[0] = (uint)count;
                Buffer.BlockCopy(data, offset, v, 4, count);

                _Encrypt(v, k);

				if(v.Length * 4 <= 0) return null;   // IL2CPP 버그를 피하자.

                byte[] enc = new byte[v.Length * 4];

				if(enc == null) return null;    // IL2CPP 버그를 피하자. 

                Buffer.BlockCopy(v, 0, enc, 0, enc.Length);

                return enc;
            }
            catch(Exception)
            {
                return null;
            }
        }

		public byte[] Decrypt(byte[] data, int offset, int count)
		{
            try
            {
                uint[] v = new uint[(int)Math.Ceiling((double)count / 4)];

				if(v == null) return null;  // IL2CPP 버그를 피하자.

                Buffer.BlockCopy(data, offset, v, 0, count);

                _Decrypt(v, k);

				if(v[0] <= 0) return null;   // IL2CPP 버그를 피하자.
                
				byte[] dec = new byte[v[0]];
				
				if(dec == null) return null;   // IL2CPP 버그를 피하자.
                
				Buffer.BlockCopy(v, 4, dec, 0, dec.Length);    // 요기서 죽을 수 있네

                return dec;
            }
            catch(Exception)
            {
                return null;
            }
		}

		public byte[] Encrypt(byte[] data)
		{
			return Encrypt(data, 0, data.Length);
		}

		public byte[] Decrypt(byte[] data)
		{
			return Decrypt(data, 0, data.Length);
		}

		public byte[] Encrypt(string text, Encoding encoding)
		{
			byte[] data = encoding.GetBytes(text);
            if(data != null)
    			return Encrypt(data, 0, data.Length);
            return null;
		}

		public string Decrypt(byte[] data, Encoding encoding)
		{
			byte[] decrypted = Decrypt(data, 0, data.Length);
            if (decrypted != null)
    			return encoding.GetString(decrypted);
            return null;
		}

		public string Decrypt(byte[] data, int offset, int count, Encoding encoding)
		{
			byte[] decrypted = Decrypt(data, offset, count);
            if (decrypted != null)
                return encoding.GetString(decrypted);
            return null;
		}

		public void EncryptToFile(string path, byte[] data)
		{
			byte[] encrypted = Encrypt(data, 0, data.Length);
			File.WriteAllBytes(path, encrypted);
		}

		public void EncryptToFile(string path, byte[] data, int offset, int count)
		{
			byte[] encrypted = Encrypt(data, offset, count);
			File.WriteAllBytes(path, encrypted);
		}

		public byte[] DecryptFromFile(string path)
		{
			byte[] encrypted = File.ReadAllBytes(path);
            if(encrypted != null)
    			return Decrypt(encrypted, 0, encrypted.Length);
            return null;
		}

		public string EncryptString(string text)
		{
			if (text == null) return null;
            byte[] encrypted = Encrypt(text, Encoding.UTF8);
            if(encrypted != null)
                return Convert.ToBase64String(encrypted);
            return null;
		}

		public string DecryptString(string text)
		{
			if (text == null) return null;
			return Decrypt(Convert.FromBase64String(text), Encoding.UTF8);
		}

		private static void _Encrypt(uint[] v, uint[] k)
		{
			int n = v.Length;
			uint y = 0, sum = 0, p = 0, e = 0;
			uint rounds = (uint)(6 + 52 / n);
			uint z = v[n - 1];

			do
			{
				sum += DELTA;
				e = (sum >> 2) & 3;
				for (p = 0; p < n - 1; p++)
				{
					y = v[p + 1];
					z = v[p] += (((z >> 5 ^ y << 2) + (y >> 3 ^ z << 4)) ^ ((sum ^ y) + (k[(p & 3) ^ e] ^ z)));
				}
				y = v[0];
				z = v[n - 1] += (((z >> 5 ^ y << 2) + (y >> 3 ^ z << 4)) ^ ((sum ^ y) + (k[(p & 3) ^ e] ^ z)));
			} while (--rounds > 0);
		}

		private static void _Decrypt(uint[] v, uint[] k)
		{
			int n = v.Length;
			uint z = 0, p = 0, e = 0;
			uint rounds = (uint)(6 + 52 / n);
			uint sum = rounds * DELTA;
			uint y = v[0];

			do
			{
				e = (sum >> 2) & 3;
				for (p = (uint)(n - 1); p > 0; p--)
				{
					z = v[p - 1];
					y = v[p] -= (((z >> 5 ^ y << 2) + (y >> 3 ^ z << 4)) ^ ((sum ^ y) + (k[(p & 3) ^ e] ^ z)));
				}
				z = v[n - 1];
				y = v[0] -= (((z >> 5 ^ y << 2) + (y >> 3 ^ z << 4)) ^ ((sum ^ y) + (k[(p & 3) ^ e] ^ z)));
			} while ((sum -= DELTA) != 0);
		}
	}
}
