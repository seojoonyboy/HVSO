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

    protected Dictionary<string, string> dictionary;
    [SerializeField] bool addToDictionary;

    public virtual void RequestLocalizationInfo(OnRequestFinishedDelegate callback) {
        var language = PlayerPrefs.GetString("Language", AccountManager.Instance.GetLanguageSetting());
        PlayerPrefs.SetString("Language", language);

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

        HTTPRequest downloadRequest = new HTTPRequest(new Uri(url.ToString()));
        downloadRequest.MethodType = HTTPMethods.Get;

        GetComponent<LocalizationDownloadManager>().Request(downloadRequest, callback);
    }

    public virtual void Download() {
        dictionary = new Dictionary<string, string>();
        RequestLocalizationInfo(OnRequest);
    }

    protected virtual void OnRequest(HTTPRequest req, HTTPResponse res) {
        ProcessFragments(res.Data);
        ReadCsvFile();
        if(addToDictionary) AddToDictionary();
    }

    protected virtual void ProcessFragments(byte[] fragments) {
        string dir = string.Empty;

        if(Application.platform == RuntimePlatform.Android) {
            dir = Application.persistentDataPath;
        }
        else if(Application.platform == RuntimePlatform.IPhonePlayer) {
            dir = Application.persistentDataPath;
        }
        else {
            dir = Application.streamingAssetsPath;
        }

        string filePath = dir + "/" + fileName;
        File.WriteAllBytes(filePath, fragments);
    }

    protected virtual void ReadCsvFile() {
        var pathToCsv = string.Empty;

        if(Application.platform == RuntimePlatform.Android) {
            pathToCsv = Application.persistentDataPath + "/" + fileName;
        }
        else if(Application.platform == RuntimePlatform.IPhonePlayer) {
            pathToCsv = Application.persistentDataPath + "/" + fileName;
        }
        else {
            pathToCsv = Application.streamingAssetsPath + "/" + fileName;
        }

        var lines = File.ReadLines(pathToCsv);

        foreach(string line in lines) {
            var datas = line.Split(',');
            datas[1] = datas[1].Replace("\"", string.Empty);
            dictionary.Add(datas[0], datas[1]);
        }
    }

    protected virtual void AddToDictionary() {
        var translator = AccountManager.Instance.GetComponent<Fbl_Translator>();
        if(translator.localizationDatas.ContainsKey(key)) translator.localizationDatas.Remove(key);

        AccountManager.Instance.GetComponent<Fbl_Translator>().localizationDatas.Add(key, dictionary);
        //MakeEnumScript();   //빌드시에 주석처리 필요함
    }

    protected virtual void MakeEnumScript() {
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
