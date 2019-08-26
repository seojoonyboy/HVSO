using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Haegin
{
    public partial class TextManager
    {
        static string languageSetting;
        static List<string> textTable;

        static TextManager()
        {
#if MDEBUG
            Debug.Log("TextManager static initialize");
#endif
            languageSetting = Application.systemLanguage.ToString();
#if UNITY_EDITOR
            Initialize("Korean");
#endif
        }

        public static string GetLanguageSetting()
        {
            return languageSetting;
        }

        public static void SetLanguageSetting(string ls)
        {
            if (languageSetting == null || !languageSetting.Equals(ls))
            {
                Initialize(ls);
            }
        }

        public static void Initialize(string ls, TextAsset textAsset = null)
        {
            if (ls != null)
            {
                languageSetting = ls;
            }

            uint[] key = new uint[] { 02, 6951, 5001, 08378 };
            G.Util.XXTea xxtea = new G.Util.XXTea(key);

            if (textAsset == null)
            {
#if MDEBUG
                Debug.Log("Load From Resources");
#endif
                textAsset = Resources.Load(languageSetting) as TextAsset;
                if (textAsset == null)
                {
                    textAsset = Resources.Load("English") as TextAsset;
                    if (textAsset == null)
                    {
                        textAsset = Resources.Load("Korean") as TextAsset;
                    }
                }
            }
            if (textAsset == null) Debug.Log("textAsset is null");
            Debug.Log("textAsset " + textAsset);
            byte[] decrypted = xxtea.Decrypt(textAsset.bytes);

            using (MemoryStream stream = new MemoryStream(decrypted))
            {
                byte[] int16Buffer = new byte[2];
                textTable = new List<string>();
                stream.Read(int16Buffer, 0, 2);
                int count = (((int)(int16Buffer[0]) << 8) & 0xFF00) | ((int)int16Buffer[1] & 0xFF);

                for (int i = 0; i < count; i++) {
                    stream.Read(int16Buffer, 0, 2);
                    int length = (((int)(int16Buffer[0]) << 8) & 0xFF00) | ((int)int16Buffer[1] & 0xFF);
                    byte[] buffer = new byte[length];
                    stream.Read(buffer, 0, length);
                    textTable.Add(Encoding.UTF8.GetString(buffer));
                }
            }
        }

        public static string GetString(StringTag tag)
        {
            try {
                return textTable[(int)tag];
            } catch {
                return "Unknown Text";
            }          
        }
    }
}
