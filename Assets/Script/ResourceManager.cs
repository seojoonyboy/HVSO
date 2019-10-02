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
    public Dictionary<string, Sprite> heroPortraite;
    public Dictionary<string, Sprite> deckPortraite;
    public Dictionary<string, ScenarioUnit> ScenarioUnitResurce;
    public GameObject unitDeadObject;
    public GameObject previewUnit;
    public GameObject baseSkillIcon;
    public GameObject hideObject;
}

public class ScenarioUnit {
    public string name;
    public Sprite sprite;
}

public class ClassInfo {
    public string name;
    public string info;
}