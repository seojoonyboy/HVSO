using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
using System.Data;
using System.IO;
using ExcelDataReader;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

public class TextDataConverterWindow : EditorWindow
{
    [MenuItem("Haegin/Text Data Converter")]
    public static void ShowWindow()
    {
        System.Type inspectorType = System.Type.GetType("UnityEditor.InspectorWindow,UnityEditor.dll");
        EditorWindow.GetWindow<TextDataConverterWindow>("Text Converter", new System.Type[] { inspectorType });
    }

    string inputFilePath = "/Haegin/Text/Editor/TextTable.xlsx";

    void OnGUI()
    {
        GUILayout.Label("Settings");
        inputFilePath = EditorGUILayout.TextField("Input File Path (excel)", inputFilePath);

        if (GUILayout.Button("Convert"))
        {
            ConvertExcelToBinary();
        }
    }

    void ConvertExcelToBinary()
    {
#if MDEBUG
        Debug.Log("Start Convert...");            
#endif
        using (FileStream stream = File.Open(Application.dataPath + inputFilePath, FileMode.Open, FileAccess.Read))
        {
            using (IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream))
            {
                DataSet dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
                {
                    ConfigureDataTable = _ => new ExcelDataTableConfiguration
                    {
                        UseHeaderRow = false
                    }
                });
                DataTable dataTable = dataSet.Tables[0];
                for (int col = 0; col < dataTable.Columns.Count; col++) {
                    if(dataTable.Rows[0][col].ToString().Equals("Tag")) {
                        // Tag Enum 소스 파일 생성 
                        using (StreamWriter enumFile = new StreamWriter(Application.dataPath + "/Haegin/Text/TextManager_enum.cs"))
                        {
                            enumFile.WriteLine("using UnityEngine;");
                            enumFile.WriteLine("namespace Haegin");
                            enumFile.WriteLine("{");
                            enumFile.WriteLine("    public partial class TextManager : MonoBehaviour");
                            enumFile.WriteLine("    {");
                            enumFile.WriteLine("        public enum StringTag");
                            enumFile.WriteLine("        {");
                            for (int row = 1; row < dataTable.Rows.Count; row++)
                            {
                                enumFile.WriteLine("            " + dataTable.Rows[row][col].ToString() + ",");
                            }
                            enumFile.WriteLine("            Max");
                            enumFile.WriteLine("        }");
                            enumFile.WriteLine("    }");
                            enumFile.WriteLine("}");
                            enumFile.Close();
                        }
                    }
                    else {
                        // 언어 데이터 파일 생성
                        using(MemoryStream wstream = new MemoryStream())
                        {
                            byte[] count = new byte[2];
                            count[0] = (byte)((dataTable.Rows.Count >> 8) & 0xFF);
                            count[1] = (byte)(dataTable.Rows.Count & 0xFF);
                            wstream.Write(count, 0, 2);

                            for (int row = 1; row < dataTable.Rows.Count; row++)
                            {
                                byte[] stringData = Encoding.UTF8.GetBytes(dataTable.Rows[row][col].ToString());
                                byte[] length = new byte[2];

                                length[0] = (byte)((stringData.Length >> 8) & 0xFF);
                                length[1] = (byte)(stringData.Length & 0xFF);

                                wstream.Write(length, 0, 2);
                                wstream.Write(stringData, 0, stringData.Length);
                            }

                            uint[] key = new uint[] { 02, 6951, 5001, 08378 };
                            G.Util.XXTea xxtea = new G.Util.XXTea(key);
                            xxtea.EncryptToFile(Application.dataPath + "/Resources/" + dataTable.Rows[0][col].ToString() + ".bytes", wstream.GetBuffer(), 0, (int)wstream.Position);
                        }
                    }
                }
                AssetDatabase.Refresh();
            }
        }
    }
}
