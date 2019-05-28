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
    public Dictionary<string, Sprite> cardBackground;
}
