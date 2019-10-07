using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;


[CreateAssetMenu]
[Sirenix.OdinInspector.ShowOdinSerializedPropertiesInInspector]
public class CardDataPackage : ScriptableObject{
    public string deviceId;
    public Dictionary<string, CardData> data = new Dictionary<string, CardData>();
    public List<string> checkHumanCard;
    public Dictionary<string, List<string>> rarelityHumanCardNum;
    public Dictionary<string, List<string>> rarelityHumanCardCheck;
    public List<string> checkOrcCard;
    public Dictionary<string, List<string>> rarelityOrcCardNum;
    public Dictionary<string, List<string>> rarelityOrcCardCheck;
}
