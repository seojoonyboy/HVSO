using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
using System.Data;
using System.IO;
using ExcelDataReader;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

public class HelpDataConverterWindow : EditorWindow
{
    [MenuItem("Haegin/Help Data Converter")]
    public static void ShowWindow()
    {
        System.Type inspectorType = System.Type.GetType("UnityEditor.InspectorWindow,UnityEditor.dll");
        EditorWindow.GetWindow<HelpDataConverterWindow>("Help Data Converter", new System.Type[] { inspectorType });
    }

    string inputFilePath = "/Haegin/Help/Editor/Help.xlsx";
    string css = "body { -webkit-text-size-adjust: none; font-size: 100%; font-family: Verdana, Arial, Tahoma, sans-serif; margin: 0; padding: 0; border: 0; background: #ebebeb; }\nheader { display: block; width: 100%; height: 70px; border-bottom: 1px solid #b9b9b9; background: #ffffff; font-size: 170%; line-height: 90%; font-weight: normal; position: fixed; color: #000000; text-align: center; }\nheader a { position: fixed; left: 30px; top: 20px; text-decoration: none; color: #606060; }\nul { display: block; padding: 0; margin: 0; padding-top: 70px; }\nul a li { display: block; width: 100%; border-bottom: 1px solid #b9b9b9; border-top: 1px solid #f7f7f7; background: #f0f0f0; }\nul li h2 { font-size: 150%; line-height: 170%; font-weight: normal; padding-top: 2px; padding-bottom: 2px; padding-left: 30px; color: #000000; }\nul li h3 { font-size: 150%; line-height: 170%; font-stretch: 50%; font-weight: normal; padding-top: 0px; padding-bottom: 0px; padding-right: 30px; color: #C0C0C0; }\na { text-decoration: none; }\ntable { border: 0px; border-spacing: 0px; padding: 0px; }\np { font-size: 150%; line-height: 170%; font-weight: normal; margin: 0; padding-top: 70px; padding-left: 30px; padding-right: 30px; color: #000000; }\n*.unselectable {\n   -moz-user-select: -moz-none;\n   -khtml-user-select: none;\n   -webkit-user-select: none;\n   -ms-user-select: none;\n   user-select: none;\n}";
    /*
body { -webkit-text-size-adjust: none; font-size: 100%; font-family: Verdana, Arial, Tahoma, sans-serif; margin: 0; padding: 0; border: 0; background: #ebebeb; }
header { display: block; width: 100%; height: 70px; border-bottom: 1px solid #b9b9b9; background: #ffffff; font-size: 170%; line-height: 90%; font-weight: normal; position: fixed; color: #000000; text-align: center; }
header a { position: fixed; left: 30px; top: 20px; text-decoration: none; color: #606060; }
ul { display: block; padding: 0; margin: 0; padding-top: 70px; }
ul a li { display: block; width: 100%; border-bottom: 1px solid #b9b9b9; border-top: 1px solid #f7f7f7; background: #f0f0f0; }
ul li h2 { font-size: 150%; line-height: 170%; font-weight: normal; padding-top: 2px; padding-bottom: 2px; padding-left: 30px; color: #000000; }
ul li h3 { font-size: 150%; line-height: 170%; font-stretch: 50%; font-weight: normal; padding-top: 0px; padding-bottom: 0px; padding-right: 30px; color: #C0C0C0; }
a { text-decoration: none; }
table { border: 0px; border-spacing: 0px; padding: 0px; }
p { font-size: 150%; line-height: 170%; font-weight: normal; margin: 0; padding-top: 70px; padding-left: 30px; padding-right: 30px; color: #000000; }

*.unselectable {
   -moz-user-select: -moz-none;
   -khtml-user-select: none;
   -webkit-user-select: none;
   -ms-user-select: none;
   user-select: none;
}
    */

    private void Awake()
    {
        string dstPath = Application.dataPath + "/../HelpDocuments";
        if(File.Exists(dstPath + "/style.css")) {
            StreamReader cssFile = new StreamReader(dstPath + "/style.css");
            css = cssFile.ReadToEnd();
            cssFile.Close();
        }
    }

    void OnGUI()
    {
        GUILayout.Label("Settings");
        inputFilePath = EditorGUILayout.TextField("Input File Path (excel)", inputFilePath);

        GUILayout.Label("Stylesheet");
        css = EditorGUILayout.TextArea(css, GUILayout.Height(150));

        if (GUILayout.Button("Convert"))
        {
            ConvertExcelToBinary();
        }
    }

    string GetIDListName(DataTable dataTable, int[,] idtable, int row, int col, string prefix)
    {
        if(dataTable.Rows[row][col - 1].ToString().Equals("Privacy Policy") || dataTable.Rows[row][col - 1].ToString().Equals("개인정보보호정책")) 
        {
            return prefix + "_PP";
        }
        else if(dataTable.Rows[row][col - 1].ToString().Equals("Terms of Service") || dataTable.Rows[row][col - 1].ToString().Equals("이용약관")) 
        {
            return prefix + "_ToS";
        }
        else if (dataTable.Rows[row][col - 1].ToString().Equals("Acquisition probability") || dataTable.Rows[row][col - 1].ToString().Equals("Acquisition Probability") || dataTable.Rows[row][col - 1].ToString().Equals("획득 확률!") || dataTable.Rows[row][col - 1].ToString().Equals("획득 확률"))
        {
            return prefix + "_AP";
        }
        else
        {
            string str = prefix;
            for (int i = 1; i < col; i++)
            {
                str = str + "_" + idtable[row, i];
            }
            return str;
        }
    }

    void CloseFile(StreamWriter htmlFile, bool IsMenu)
    {
        if (htmlFile != null)
        {
            if (IsMenu)
            {
                htmlFile.WriteLine("    </ul>");
            }
            htmlFile.WriteLine("  </body>");
            htmlFile.WriteLine("</html>");
            htmlFile.Close();
            htmlFile = null;
        }
    }

    string GetTitle(DataTable dataTable, int row, int col)
    {
        if(col == 1) {
            return dataTable.Rows[0][0].ToString();
        }
        else {
            while(true) {
                if(!string.IsNullOrEmpty(dataTable.Rows[row][col - 1].ToString())) {
                    return dataTable.Rows[row][col - 1].ToString();
                }
                row--;
            }
        }
    }

    void ConvertExcelToBinary()
    {
#if MDEBUG
        Debug.Log("Start Convert...");
#endif
        string dstPath = Application.dataPath + "/../HelpDocuments";

        if(Directory.Exists(dstPath)) {
            Directory.Delete(dstPath, true);
        }
        Directory.CreateDirectory(dstPath);

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

                foreach(DataTable dataTable in dataSet.Tables)
                {
                    string Filename = dataTable.TableName;
                    int[,] idtable = new int[dataTable.Rows.Count, dataTable.Columns.Count];

                    for (int col = 1; col < dataTable.Columns.Count; col++)
                    {
                        int id = 1;
                        string prev = null;
                        for (int row = 0; row < dataTable.Rows.Count; row++)
                        {
                            if (prev == null)
                            {
                                prev = dataTable.Rows[row][col].ToString();
                            }
                            if (string.IsNullOrEmpty(dataTable.Rows[row][col].ToString()) || prev.Equals(dataTable.Rows[row][col].ToString())) {
                                if (row == 0 || (col > 1 && idtable[row, col - 1] != idtable[row - 1, col - 1]))
                                {
                                    id = 1;
                                }
                                idtable[row, col] = id;
                            }
                            else {
                                prev = dataTable.Rows[row][col].ToString();
                                id++;
                                if (row == 0 || (col > 1 && idtable[row, col - 1] != idtable[row - 1, col - 1]))
                                {
                                    id = 1;
                                }
                                idtable[row, col] = id;
                            }
                        }
                    }

                    StreamWriter htmlFile = null;
                    bool IsMenu = false;
                    for (int col = 1; col < dataTable.Columns.Count; col++)
                    {
                        for (int row = 0; row < dataTable.Rows.Count; row++)
                        {
                            if (!string.IsNullOrEmpty(dataTable.Rows[row][col].ToString()))
                            {
                                if (row == 0 || !GetIDListName(dataTable, idtable, row, col, Filename).Equals(GetIDListName(dataTable, idtable, row - 1, col, Filename)))
                                {
                                    // 이전 파일이 있으면 닫아야지
                                    CloseFile(htmlFile, IsMenu);

                                    // 새로운 파일 시작
                                    htmlFile = new StreamWriter(dstPath + "/" + GetIDListName(dataTable, idtable, row, col, Filename) + ".html");
                                    htmlFile.WriteLine("<!doctype html>");
                                    htmlFile.WriteLine("<html>");
                                    htmlFile.WriteLine("  <head>");
                                    htmlFile.WriteLine("    <meta name=\"viewport\" content=\"user-scalable=no\"/>");
                                    htmlFile.WriteLine("    <meta http-equiv=\"Pragma\" content=\"no-cache\">");
                                    htmlFile.WriteLine("    <meta http-equiv=\"Expires\" content=\"-1\">");
                                    htmlFile.WriteLine("    <meta http-equiv=\"Cache-Control\" content=\"No-Cache\">");
                                    htmlFile.WriteLine("    <meta http-equiv=\"Content-Type\" content=\"text/html; charset = utf-8\">");
                                    htmlFile.WriteLine("    <meta http-equiv=\"Cache-Control\" content=\"max-age=no-cache\" forua=\"true\"/>");
                                    htmlFile.WriteLine("    <meta http-equiv=\"Cache-Control\" content=\"must-revalidate\" forua=\"true\"/>");
                                    htmlFile.WriteLine("    <link rel=\"stylesheet\" href=\"style.css\">");
                                    if(col == 1) 
                                    {
                                        htmlFile.WriteLine("    <title>HaeginHelpRoot</title>");
                                    }
                                    else 
                                    {
                                        htmlFile.WriteLine("    <title>HaeginHelpNode</title>");
                                    }
                                    htmlFile.WriteLine("  </head>");
                                    htmlFile.WriteLine("  <body oncontextmenu=\"return false\" ondragstart=\"return false\" onselectstart=\"return false\" unselectable=\"on\" class=\"unselectable\">");
                                    htmlFile.WriteLine("    <header>");
                                    if(col == 1) {
                                        htmlFile.WriteLine("        <table width=100% height=70px><tr><td width=8%></td><td width=84%>" + GetTitle(dataTable, row, col) + "</td><td width=8%></td></tr></table>");
                                    }
                                    else {
//                                        htmlFile.WriteLine("        " + GetTitle(dataTable, row, col) + "<a href=\"" + GetIDListName(dataTable, idtable, row, col - 1, Filename) + ".html\">" + dataTable.Rows[2][0].ToString() + "</a>");
                                        htmlFile.WriteLine("        <table width=100% height=70px><tr><td width=8%></td><td width=84%>" + GetTitle(dataTable, row, col) + "</td><td width=8%></td></tr></table><a href=\"" + GetIDListName(dataTable, idtable, row, col - 1, Filename) + ".html\"><</a>");
                                    }
                                    htmlFile.WriteLine("    </header>");
                                    if (col + 1 < dataTable.Columns.Count && !string.IsNullOrEmpty(dataTable.Rows[row][col + 1].ToString()))
                                    {
                                        // 메뉴 페이지
                                        IsMenu = true;
                                        htmlFile.WriteLine("    <ul>");
                                    }
                                    else
                                    {
                                        IsMenu = false;
                                    }
                                }
                                if (col + 1 >= dataTable.Columns.Count || string.IsNullOrEmpty(dataTable.Rows[row][col + 1].ToString()))
                                {
                                    // 컨텐츠 
                                    htmlFile.WriteLine("     <table><tr><td><p>" + dataTable.Rows[row][col].ToString().Replace("\n", "<br>") + "</p></td></tr></table>");
                                }
                                else
                                {
                                    // 메뉴 아이템
                                    htmlFile.WriteLine("     <a href=\"" + GetIDListName(dataTable, idtable, row, col + 1, Filename) + ".html\"><li><table><tr><td width = 100%><h2>" + dataTable.Rows[row][col].ToString() + "</h2></td><td><h3>></h3></td></tr></table></li></a>");
                                }
                            }
                        }

                        if(col == 1 && dataTable.Rows.Count >= 4 && !string.IsNullOrEmpty(dataTable.Rows[3][0].ToString())) 
                        {
                            htmlFile.WriteLine("     <a id=\"haeginsupport\" href=\"mailto:support@haegin.kr\"><li><table><tr><td width = 100%><h2>" + dataTable.Rows[3][0].ToString() + "</h2></td><td><h3>></h3></td></tr></table></li></a>");
                        }
                    }
                    CloseFile(htmlFile, IsMenu);
                }

                StreamWriter cssFile = new StreamWriter(dstPath + "/style.css");
                cssFile.WriteLine(css);
                cssFile.Close();
                cssFile = null;

                AssetDatabase.Refresh();
            }
        }
    }
}
