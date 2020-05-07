using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[ShowOdinSerializedPropertiesInInspector]

//맵, 유닛 프리팹 관리 (AccountManager가 너무 무거워져서 인게임에서만 사용하는
//리소스는 점진적으로 이쪽으로 이전할 계획
public class IngameUIResourceManager : SerializedMonoBehaviour {
    public Dictionary<string, GameObject[]> raceUIPrefabs;
    
}
