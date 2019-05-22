using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;


[CreateAssetMenu]
[Sirenix.OdinInspector.ShowOdinSerializedPropertiesInInspector]
public class CardDataPackage : ScriptableObject{
    public string ClientVersion;
    public Dictionary<string, CardData> data = new Dictionary<string, CardData>();
}

