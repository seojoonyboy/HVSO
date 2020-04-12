using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Sirenix;
using Sirenix.OdinInspector;

public class Fbl_Translator : SerializedMonoBehaviour {
    public Dictionary<string, string> skillTypeDescs;
    public Dictionary<string, Dictionary<string, string>> localizationDatas;

    public List<string> GetTranslatedUnitCtg(List<string> data) {
        var keys = data;
        var translator = AccountManager.Instance.GetComponent<Fbl_Translator>();
        
        return keys.Select(key => translator.GetLocalizedText("Category", "category_name_" + key)).ToList();
    }
    
    public string GetTranslatedUnitCtg(string keyword) {
        var translator = AccountManager.Instance.GetComponent<Fbl_Translator>();
        string translatedKeyword = translator.GetLocalizedText("Category", "category_name_" + keyword);
        string result = string.Format("<color=#ECC512>{0}</color>", translatedKeyword);
        return result;
    }

    public string GetTranslatedSkillName(string keyword, bool withLink = true) {
        var translator = AccountManager.Instance.GetComponent<Fbl_Translator>();
        string translatedKeyword = translator.GetLocalizedText("Skill", "skill_name_" + keyword);
        string result = string.Format("<color=#149AE9><link={0}>{1}</link></color>", keyword, translatedKeyword);
        return result;
    }

    public string GetTranslatedSkillTypeDesc(string keyword) {
        var translator = AccountManager.Instance.GetComponent<Fbl_Translator>();
        string result = translator.GetLocalizedText("Skill", "skill_txt_" + keyword);
        return result;
    }

    public string[] GetTranslatedSkillSet(string keyword) {
        string name = GetTranslatedSkillName(keyword, false);
        string desc = GetTranslatedSkillTypeDesc(keyword);

        string[] set = new string[] { name, desc };
        return set;
    }

    public string DialogSetRichText(string desc) {
        // StringBuilder sb = new StringBuilder();
        // var lastItem = attributes.Last();
        // foreach (var attr in attributes) {
        //     string translatedKeyword = GetTranslatedSkillName(attr.name);
        //     sb.Append(translatedKeyword);
        //     if(attr != lastItem) sb.Append(",");
        // }
        //
        // return sb.ToString();
        Regex regex = new Regex(@"{([^{}]+)}");
        MatchCollection collection = regex.Matches(desc);
        foreach (Match match in collection) {
             Group group = match.Groups[1];
             var translated = GetTranslatedSkillName(group.Value);
             desc = desc.Replace(group.Value, translated);
             desc = desc.Replace("{", "");
             desc = desc.Replace("}", "");
        }
        
        Regex regex2 = new Regex(@"\[([^\[\]]+)\]");
        MatchCollection collection2 = regex2.Matches(desc);
        foreach (Match match in collection2) {
            Group group = match.Groups[1];
            var translated = GetTranslatedUnitCtg(group.Value);
            desc = desc.Replace(group.Value, translated);
            desc = desc.Replace("[", "");
            desc = desc.Replace("]", "");
        }
        
        return desc;
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
