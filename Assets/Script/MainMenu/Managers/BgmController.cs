using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class BgmController : SerializedMonoBehaviour 
{
    public Dictionary<BgmEnum, AudioSource> bgmDictionary;
    public AudioSource mainAudio;

    private void Awake() {
        mainAudio = gameObject.GetComponent<AudioSource>();
        DontDestroyOnLoad(gameObject);
    }

    private void Start() {
        //PlaySoundTrack(BgmEnum.MENU);
    }


    public void PlaySoundTrack(BgmEnum type) {
        if (bgmDictionary.ContainsKey(type) == false || bgmDictionary[type] == null) {
            Logger.LogError("사운드가 없습니다.");
            return;
        }
        mainAudio.clip = bgmDictionary[type].clip;
        mainAudio.Play();
    }

    public void StopSoundTrack() {
        mainAudio.Stop();
    }

    public enum BgmEnum {
        CITY = 1000,
        DEFEAT,
        FOREST,
        MENU,
        VICTORY
    }    
}
