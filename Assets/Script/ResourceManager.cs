using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Sirenix.OdinInspector;

[Sirenix.OdinInspector.ShowOdinSerializedPropertiesInInspector]
public class ResourceManager : SerializedMonoBehaviour
{
    public Dictionary<string, Sprite> cardPortraite;
    public Dictionary<string, GameObject> cardSkeleton;
    public Dictionary<string, GameObject> cardPreveiwSkeleton;
    public Dictionary<string, Sprite> cardBackground;
    public Dictionary<string, GameObject[]> raceUiPrefabs;
    public Dictionary<string, GameObject> heroPreview;
    public Dictionary<string, GameObject> heroSkeleton;
    public Dictionary<string, Sprite> classImage;
    public Dictionary<string, ClassInfo> classInfo;
    public Dictionary<string, Sprite> infoSprites;
    public Dictionary<string, Sprite> infoPortraite;
    public Dictionary<string, Sprite> skillIcons;
    public Dictionary<string, Sprite> buffSkillIcons;
    public Dictionary<string, Sprite> heroPortraite;
    public Dictionary<string, Sprite> deckPortraite;
    public Dictionary<string, ScenarioUnit> ScenarioUnitResurce;
    public Dictionary<string, Sprite> campBackgrounds;   //진영별 뒷배경
    public Dictionary<string, Sprite> rewardIcon;
    public Dictionary<string, Sprite> rankIcons;

    public Dictionary<string, UnitRace> unitRace;

    public GameObject unitDeadObject;
    public GameObject baseSkillIcon;
    public GameObject hideObject;
    public GameObject touchIcon;
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