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
    public BgmController bgmController;

    void Awake() {
        _instance = GetComponent<SoundManager>();
        DontDestroyOnLoad(gameObject);    
    }

    public void PlaySound(SoundType type) {
        if (!sounds.ContainsKey(type) || sounds[type] == null) {
            Logger.LogError(string.Format("{0}에 대한 음원을 찾을 수 없습니다.", type));
            return;
        }
        sounds[type].Play();
    }

    public GameObject GetUnusedAudio() {
        foreach(Transform child in gameObject.transform) {

        }
        return null;
    }








}


public enum SoundType {
    NEXT_TURN,
    FIRST_TURN,
    NORMAL_ATTACK,
    MIDDLE_ATTACK,
    LARGE_ATTACK,
    APPEAR_UNIT,
    DEAD
}
