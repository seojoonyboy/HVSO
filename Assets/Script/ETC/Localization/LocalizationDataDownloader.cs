using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BestHTTP;
using System.IO;
using System.Linq;
using System.Text;

public class LocalizationDataDownloader : MonoBehaviour {
    public string subUrl = "";
    public string fileName;
    public string key;

    Dictionary<string, string> dictionary;

    void Awake() {
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_USER_UPDATED, OnRequestUserInfoCallback);
    }

    void OnDestroy() {
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_USER_UPDATED, OnRequestUserInfoCallback);
    }

    private void OnRequestUserInfoCallback(Enum Event_Type, Component Sender, object Param) {
        dictionary = new Dictionary<string, string>();

        RequestLocalizationInfo(OnRequest);
    }

    public void RequestLocalizationInfo(OnRequestFinishedDelegate callback) {
        var language = AccountManager.Instance.GetLanguageSetting();
        //language = "English";   //테스트 코드

        StringBuilder url = new StringBuilder();
        var networkManager = NetworkManager.Instance;
        string base_url = networkManager.baseUrl;

        string prev = subUrl.Substring(0, subUrl.IndexOf("{"));
        string next = subUrl.Substring(subUrl.IndexOf("}") + 1);
        url
            .Append(base_url)
            .Append(prev)
            .Append(language)
            .Append(next);

        HTTPRequest request = new HTTPRequest(new Uri(url.ToString()));
        request.MethodType = HTTPMethods.Get;

        networkManager.Request(request, callback, "Localization 정보를 불러오는 중");
    }

    private void OnRequest(HTTPRequest req, HTTPResponse res) {
        switch (req.State) {
            case HTTPRequestStates.Processing:
                break;
            case HTTPRequestStates.Finished:
                if (res.IsSuccess) {
                    ProcessFragments(res.Data);
                    ReadCsvFile();
                    WriteToScriptableObject();

                    Logger.Log(fileName + "Request Finished");
                }
                else {
                    Modal.instantiate("Localization 정보를 불러오는 중에 문제가 발생하였습니다.", Modal.Type.CHECK);
                }
                break;
            case HTTPRequestStates.Error:
                break;
            case HTTPRequestStates.Aborted:
                break;
            case HTTPRequestStates.ConnectionTimedOut:
                break;
            case HTTPRequestStates.TimedOut:
                break;
        }
    }

    private void ProcessFragments(byte[] fragments) {
        string dir = Application.dataPath + "/StreamingAssets/";
        string filePath = dir + fileName;

        File.WriteAllBytes(filePath, fragments);
    }

    private void ReadCsvFile() {
        var pathToCsv = Application.dataPath + "/StreamingAssets/" + fileName;
        var lines = File.ReadLines(pathToCsv);

        foreach(string line in lines) {
            var datas = line.Split(',');
            dictionary.Add(datas[0], datas[1]);
        }
    }

    private void WriteToScriptableObject() {
        AccountManager.Instance.GetComponent<fbl_Translator>().localizationDatas.Add(key, dictionary);
        //MakeEnumScript();   //빌드시에 주석처리 필요함
    }

    private void MakeEnumScript() {
        using (StreamWriter enumFile = new StreamWriter(Application.dataPath + "/Script/ETC/Localization/Fbl_" + key + "_enum.cs")) {
            enumFile.WriteLine("using UnityEngine;");
            enumFile.WriteLine("namespace Haegin");
            enumFile.WriteLine("{");
            enumFile.WriteLine("    public partial class TextManager : MonoBehaviour");
            enumFile.WriteLine("    {");
            enumFile.WriteLine("        public enum " + key);
            enumFile.WriteLine("        {");
            foreach (KeyValuePair<string, string> items in dictionary) {
                enumFile.WriteLine("            " + items.Key.ToString() + ",");
            }
            enumFile.WriteLine("            Max");
            enumFile.WriteLine("        }");
            enumFile.WriteLine("    }");
            enumFile.WriteLine("}");
            enumFile.Close();
        }
    }
}

[Serializable]
public class LocalTextData {
    public string tag;
    public string text;

    public LocalTextData(string tag, string text) {
        this.tag = tag;
        this.text = text;
    }
}
