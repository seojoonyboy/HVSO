using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Sirenix.OdinInspector;

[Sirenix.OdinInspector.ShowOdinSerializedPropertiesInInspector]
public class ResourceManager : SerializedMonoBehaviour {
    public Dictionary<string, Sprite> cardPortraite;
    public Dictionary<string, Sprite> cardBackground;
    public Dictionary<string, GameObject> heroPreview;
    public Dictionary<string, GameObject> heroSkeleton;
    public Dictionary<string, Sprite> classImage;
    public Dictionary<string, ClassInfo> classInfo;
    public Dictionary<string, Sprite> infoSprites;
    public Dictionary<string, Sprite> infoPortraite;
    [SerializeField] Dictionary<string, Sprite> skillIcons;
    public Dictionary<string, Sprite> buffSkillIcons;
    public Dictionary<string, Sprite> heroPortraite;
    public Dictionary<string, Sprite> deckPortraite;
    public Dictionary<string, ScenarioUnit> ScenarioUnitResource;
    public Dictionary<string, Sprite> campBackgrounds;   //진영별 뒷배경
    public Dictionary<string, Sprite> rewardIcon;
    public Dictionary<string, Sprite> achievementIcon;
    public Dictionary<string, Sprite> rewardIconsInDescriptionModal;
    public Dictionary<string, Sprite> rankIcons;
    public Dictionary<string, Sprite> traitIcons;
    public Dictionary<string, Sprite> packageImages;
    public Dictionary<string, GameObject> toolObject;

    public Dictionary<string, Sprite> localizeImage;

    public Dictionary<string, UnitRace> unitRace;
    public Dictionary<string, TMPro.TMP_FontAsset> tmp_fonts;
    public Dictionary<string, Font> fonts;
    public GameObject baseSkillIcon;
    public GameObject touchIcon;
    
    public Sprite GetSkillIcons(string keyword) {
        return skillIcons.ContainsKey(keyword) ? skillIcons[keyword] : skillIcons["default"];
    }

    public bool FindSkillNames(string keyword) {
        return skillIcons.ContainsKey(keyword);
    }

    public Sprite GetRewardIconWithBg(string keyword) {
        var filteredKeyword = FilteringKeyword(keyword);
        try {
            return rewardIconsInDescriptionModal[filteredKeyword];
        }
        catch (Exception ex) {
            Logger.LogError(filteredKeyword + "에 대한 보상 아이콘을 찾을 수 없음");
            return null;
        }
    }
    
    public string FilteringKeyword(string _keyword) {
        string keyword = _keyword.ToLower();
        if (keyword.Contains("x2")) return "supplyX2Coupon";
        if (keyword.Contains("crystal")) return "magiccrystal";
        if (keyword.Contains("reinforcedbox")) return "enhancebox";
        if (keyword.Contains("extralargebox")) return "enormousbox";
        if (keyword.Contains("largebox") && !keyword.Contains("extra")) return keyword;
        if (keyword.Contains("supplybox")) return "enhancebox";
        if (keyword.Contains("gold")) return "gold";
        if (keyword.Equals("supply")) return "presupply";
        return _keyword;
    }
}

public class ScenarioUnit {
    public string name;
    public Sprite sprite;
}

public class ClassInfo {
    public string name;
    public string info;
}

public enum UnitRace {
    HUMAN_ELDER_MAN = 500,
    HUMAN_ELDER_WOMEN,
    HUMAN_MAN,
    HUMAN_WOMAN,
    HUMAN_MIDDLE_MAN,
    HUMAN_MIDDLE_WOMAN,
    ORC_ELDER_MAN,
    ORC_ELDER_WOMAN,
    ORC_MAN,
    ORC_WOMAN,
    ORC_MIDDLE_MAN,
    ORC_MIDDLE_WOMAN,
    MACHINE,
    MONSTER,
    ANIMAL,
    CRYSTAL,
    BEAST,
    HUMAN_BOY
}