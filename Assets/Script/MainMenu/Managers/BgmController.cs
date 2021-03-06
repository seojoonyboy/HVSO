using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class BgmController : SerializedMonoBehaviour 
{
    public Dictionary<BgmEnum, AudioSource> bgmDictionary;
    public AudioSource mainAudio;
    float bgmVolume;

    public float BGMVOLUME {
        get { return bgmVolume; }
        set { 
            bgmVolume = value;
            PlayerPrefs.SetFloat("BgmVolume", value);
            mainAudio.volume = bgmVolume;
        }
    }

    private void Awake() {
        mainAudio = gameObject.GetComponent<AudioSource>();
        if (!PlayerPrefs.HasKey("BgmVolume"))
            PlayerPrefs.SetFloat("BgmVolume", 0.5f);
        mainAudio.volume = PlayerPrefs.GetFloat("BgmVolume");
        DontDestroyOnLoad(gameObject);
    }

    private void Start() {
        //PlaySoundTrack(BgmEnum.MENU);
    }

    public void SoundTrackLoopOn() {
        //mainAudio.loop = true;
    }

    public void SoundTrackLoopOff() {
        //mainAudio.loop = false;
    }

    public void PlaySoundTrack(BgmEnum type) {
        if (bgmDictionary.ContainsKey(type) == false || bgmDictionary[type] == null) {
            //Logger.LogError("사운드가 없습니다.");
            return;
        }
        StopAllCoroutines();
        mainAudio.volume = PlayerPrefs.GetFloat("BgmVolume");


        if (type == BgmEnum.VICTORY || type == BgmEnum.DEFEAT) {
            mainAudio.loop = false;            
        }
        else
            mainAudio.loop = true;

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
