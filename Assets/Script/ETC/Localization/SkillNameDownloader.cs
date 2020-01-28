using BestHTTP;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SkillNameDownloader : LocalizationDataDownloader {
    Fbl_Translator fbl_Translator;

    void Awake() {
        fbl_Translator = AccountManager.Instance.GetComponent<Fbl_Translator>();
    }

    public override void RequestLocalizationInfo(OnRequestFinishedDelegate callback) {
        base.RequestLocalizationInfo(callback);
    }

    public override void Download() {
        base.Download();
    }

    protected override void AddToDictionary() {
        foreach (KeyValuePair<string, string> items in dictionary) {
            fbl_Translator.skillTypeNames.Add(items.Value, items.Key);
        }
    }

    protected override void OnRequest(HTTPRequest req, HTTPResponse res) {
        base.OnRequest(req, res);
    }

    protected override void ProcessFragments(byte[] fragments) {
        base.ProcessFragments(fragments);
    }

    protected override void ReadCsvFile() {
        var pathToCsv = string.Empty;

        if (Application.platform == RuntimePlatform.Android) {
            pathToCsv = Application.persistentDataPath + "/" + fileName;
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer) {
            pathToCsv = Application.persistentDataPath + "/" + fileName;
        }
        else {
            pathToCsv = Application.streamingAssetsPath + "/" + fileName;
        }

        var lines = File.ReadLines(pathToCsv);

        foreach (string line in lines) {
            var datas = line.Split(',');
            datas[1] = datas[1].Replace("\"", string.Empty);
            datas[1] = datas[1].Replace("{{", string.Empty);
            datas[1] = datas[1].Replace("}}", string.Empty);
            dictionary.Add(datas[0], datas[1]);
        }
    }

    protected override void MakeEnumScript() {
        base.MakeEnumScript();
    }
}
