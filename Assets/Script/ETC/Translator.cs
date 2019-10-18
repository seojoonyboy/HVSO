using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix;
using Sirenix.OdinInspector;

public class Translator : SerializedMonoBehaviour {
    public Dictionary<string, string> unitCategories;

    public Dictionary<string, string> skillTypeNames;
    public Dictionary<string, string> skillTypeDescs;

    public List<string> GetTranslatedUnitCtg(List<string> data) {
        var keys = data;
        List<string> values = new List<string>();
        foreach(string key in keys) {
            if (unitCategories.ContainsKey(key)) {
                values.Add(unitCategories[key]);
            }
            else {
                string msg = string.Format("{0}에 대한 번역을 찾을 수 없습니다.", key);
                Logger.LogError(msg);
            }
        }
        return values;
    }

    public string GetTranslatedSkillName(string keyword) {
        string result;

        if (skillTypeNames.ContainsKey(keyword)) {
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
        string name = GetTranslatedSkillName(keyword);
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
        List<string> types = GetMiddleText(startType, endType, desc);

        List<string> categories_translated = GetTranslatedUnitCtg(categories);
        
        
        for(int i = 0; i < categories.Count; i++) {
            desc = desc.Replace(categories[i], categories_translated[i]);
        }
        desc = desc.Replace(startCategory, categoryColorStart);
        desc = desc.Replace(endCategory, colorEnd);
        for(int i = 0; i < types.Count; i++) {
            string types_translated = GetTranslatedSkillName(types[i]);
            desc = desc.Replace(types[i], string.Format("<link={0}>{1}</link>", types[i], types_translated));
        }
        desc = desc.Replace(startType, typeColorStart);
        desc = desc.Replace(endType, colorEnd);
        
        return desc;
    }

    private List<string> GetMiddleText(string start, string end, string value) {
        List<string> middles = new List<string>();
        List<int> startList = value.AllIndexesOf(start, System.StringComparison.OrdinalIgnoreCase);
        List<int> endList = value.AllIndexesOf(end, System.StringComparison.OrdinalIgnoreCase);
        for(int i = 0; i < startList.Count; i++)
            middles.Add(value.Substring(startList[i] + 1, endList[i] - startList[i] - 1));
        return middles;
    }
}
