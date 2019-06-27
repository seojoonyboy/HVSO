using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[ShowOdinSerializedPropertiesInInspector]
public class SoundManager : SerializedMonoBehaviour {
    public Dictionary<SoundType, AudioSource> sounds;

    private static SoundManager _instance;
    public static SoundManager Instance {
        get {
            if (_instance == null) {
                Logger.LogError("SoundManager를 찾을 수 없습니다.");
                return null;
            }
            else {
                return _instance;
            }
        }
    }

    void Awake() {
        _instance = GetComponent<SoundManager>();
        DontDestroyOnLoad(gameObject);    
    }

    public void PlaySound(SoundType type) {
        Logger.Log(type);
        sounds[type].Play();
    }
}

public enum SoundType {
    NEXT_TURN,
    FIRST_TURN,
    NORMAL_ATTACK,
    MIDDLE_ATTACK,
    LARGE_ATTACK,
    APPEAR_UNIT
}
