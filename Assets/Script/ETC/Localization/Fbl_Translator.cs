using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix;
using Sirenix.OdinInspector;

public class Fbl_Translator : SerializedMonoBehaviour {
    public Dictionary<string, string> skillTypeNames;
    public Dictionary<string, string> skillTypeDescs;
    public Dictionary<string, Dictionary<string, string>> localizationDatas;

    public List<string> GetTranslatedUnitCtg(List<string> data) {
        var keys = data;
        var translator = AccountManager.Instance.GetComponent<Fbl_Translator>();
        
        return keys.Select(key => translator.GetLocalizedText("Category", "category_name_" + key)).ToList();
    }

    public string GetTranslatedSkillName(string keyword, bool withLink = true) {
        string result;

        if (skillTypeNames.ContainsKey(keyword)) {
            if(withLink)
                result = string.Format("<color=#149AE9><link={0}>{1}</link></color>", keyword, skillTypeNames[keyword]);
            else
                result = skillTypeNames[keyword];
        }
        else { result = ""; }
        return result;
    }

    public string GetTranslatedSkillTypeDesc(string keyword) {
        string result;

        if (skillTypeDescs.ContainsKey(keyword)) {
            result = skillTypeDescs[keyword];
        }
        else { result = ""; }
        return result;
    }

    public string[] GetTranslatedSkillSet(string keyword) {
        string name = GetTranslatedSkillName(keyword, false);
        string desc = GetTranslatedSkillTypeDesc(keyword);

        string[] set = new string[] { name, desc };
        return set;
    }

    public string DialogSetRichText(string desc) {
        const string startCategory = "[";
        const string endCategory = "]";
        const string startType = "{";
        const string endType = "}";
        const string categoryColorStart = "<color=#ECC512>";
        const string typeColorStart = "<color=#149AE9>";
        const string colorEnd = "</color>";

        List<string> categories = GetMiddleText(startCategory, endCategory, desc);
        List<string> types = GetMiddleText(startType, endType, desc) ??
                             throw new ArgumentNullException("GetMiddleText(startType, endType, desc)");

        List<string> categoriesTranslated = GetTranslatedUnitCtg(categories);


        for (var i = 0; i < categoriesTranslated.Count; i++) {
            desc = desc?.Replace(categories[i], categoriesTranslated[i]);
        }

        if (desc == null) return desc;
        desc = desc.Replace(startCategory, categoryColorStart);
        desc = desc.Replace(endCategory, colorEnd);
        foreach (var t in types) {
            var typesTranslated = GetTranslatedSkillName(t);
            desc = desc.Replace(t, $"<link={t}>{typesTranslated}</link>");
        }

        desc = desc.Replace(startType, typeColorStart);
        desc = desc.Replace(endType, colorEnd);
        return desc;
    }

    private List<string> GetMiddleText(string start, string end, string value) {
        List<string> middles = new List<string>();
        List<int> startList = value.AllIndexesOf(start, System.StringComparison.OrdinalIgnoreCase);
        List<int> endList = value.AllIndexesOf(end, System.StringComparison.OrdinalIgnoreCase);
        for (int i = 0; i < startList.Count; i++)
            middles.Add(value.Substring(startList[i] + 1, endList[i] - startList[i] - 1));
        return middles;
    }

    public string GetLocalizedText(string category, string key) {
        string result = string.Empty;
        if (category == null || key == null) return result;
        if (localizationDatas.ContainsKey(category)) {
            var dict = localizationDatas[category];
            dict.TryGetValue(key, out result);
        }

        if (result == null) {
            Logger.LogWarning(key + "에 대한 번역 값을 찾을 수 없습니다.");
        }
        return result;
    }
}
