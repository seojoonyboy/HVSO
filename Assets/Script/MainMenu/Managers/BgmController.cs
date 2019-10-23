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
        mainAudio.volume = 1f;
        mainAudio.clip = bgmDictionary[type].clip;
        mainAudio.Play();
    }

    private IEnumerator SoundVolumeDown() {
        float soundVolumne = mainAudio.volume;
        float speed = 0.1f;

        while(soundVolumne <= 0) {
            yield return new WaitForSeconds(0.05f);
            soundVolumne -= speed;
        }
        mainAudio.Stop();
    }

    public void SoundDownAfterStop() {
        StartCoroutine(SoundVolumeDown());
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
