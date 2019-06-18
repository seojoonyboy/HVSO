using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[Sirenix.OdinInspector.ShowOdinSerializedPropertiesInInspector]
public class DebugData : SerializedMonoBehaviour {
    public Dictionary<string, CardData> cardData;
    

    public static DebugData Instance { get; private set; }
    private void Start() {
        Instance = this;
        if (AccountManager.Instance == null) return;
        cardData = AccountManager.Instance.cardPackage.data;     
    }

    private void OnDestroy() {
        Instance = null;
    }

    

}
