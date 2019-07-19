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
}
