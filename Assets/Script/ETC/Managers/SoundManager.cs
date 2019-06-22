using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {
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

        sounds = new Dictionary<SoundType, AudioSource>();
        sounds[SoundType.FIRST_TURN] = transform.GetChild(0).GetComponent<AudioSource>();
        sounds[SoundType.LARGE_ATTACK] = transform.GetChild(1).GetComponent<AudioSource>();
        sounds[SoundType.NEXT_TURN] = transform.GetChild(2).GetComponent<AudioSource>();
        sounds[SoundType.NORMAL_ATTACK] = transform.GetChild(3).GetComponent<AudioSource>();
        sounds[SoundType.MIDDLE_ATTACK] = transform.GetChild(4).GetComponent<AudioSource>();
        sounds[SoundType.APPEAR_POOF] = transform.GetChild(5).GetComponent<AudioSource>();

        DontDestroyOnLoad(gameObject);    
    }

    public void PlaySound(SoundType type) {
        if (!sounds.ContainsKey(type) || sounds[type] == null) return;
        sounds[type].Play();
    }
}

public enum SoundType {
    NEXT_TURN,
    FIRST_TURN,
    NORMAL_ATTACK,
    MIDDLE_ATTACK,
    LARGE_ATTACK,
    APPEAR_POOF
}
