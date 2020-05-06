using System.IO;
using MessagePack;
using G.Util;

namespace G.MessagePack
{
    public class DataLoader
    {
        public static T Load<T>(string path, string base62Key)
        {
            byte[] bytes = File.ReadAllBytes(path);
            return Load<T>(bytes, base62Key);
        }

        public static T Load<T>(byte[] bytes, string base62Key = null)
        {
            if (!string.IsNullOrEmpty(base62Key))
            {
                XXTea xxtea = new XXTea(base62Key);
                bytes = xxtea.Decrypt(bytes);
            }

            return MessagePackSerializer.Deserialize<T>(bytes);
        }

        public static void Save<T>(string path, T data, string base62Key)
        {
            byte[] bytes = MessagePackSerializer.Serialize(data);

            if (!string.IsNullOrEmpty(base62Key))
            {
                XXTea xxtea = new XXTea(base62Key);
                bytes = xxtea.Encrypt(bytes);
            }

            var dir = FileEx.GetDirectory(path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllBytes(path, bytes);
        }
    }
}
