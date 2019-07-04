using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix;
using Sirenix.OdinInspector;

public class Translator : SerializedMonoBehaviour {
    public Dictionary<string, string> unitCategories;

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
}
