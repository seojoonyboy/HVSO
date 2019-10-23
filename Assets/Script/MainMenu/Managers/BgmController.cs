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
        mainAudio.volume = 0.6f;
        mainAudio.clip = bgmDictionary[type].clip;
        mainAudio.Play();
    }

    private IEnumerator SoundVolumeDown() {
        float speed = 0.1f;

        while(mainAudio.volume > 0) {
            yield return new WaitForSeconds(0.5f);
            mainAudio.volume -= speed;
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
