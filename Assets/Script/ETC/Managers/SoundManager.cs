using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UniRx;
using Sirenix.OdinInspector;

[ShowOdinSerializedPropertiesInInspector]
public class SoundManager : SerializedMonoBehaviour {
    public Dictionary<SoundType, AudioSource> sounds;
    public Dictionary<string, AudioSource> unitSfx;
    public Dictionary<string, AudioSource> magicSfx;
    
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
    public GameObject emptySource;

    public BgmController bgmController;

    void Awake() {
        _instance = GetComponent<SoundManager>();
        DontDestroyOnLoad(gameObject);    
    }
    public void PlayMagicSound(string id) {
        if (!magicSfx.ContainsKey(id) || magicSfx[id] == null) {
            AttackSound(magicSfx["ac10015"]);
            return;
        }
        if (id == "ac10005")
            return;
        AttackSound(magicSfx[id]);
    }


    public void PlayAttackSound(string id) {
        if (!unitSfx.ContainsKey(id) || unitSfx[id] == null) {
            AttackSound(unitSfx["ac10001"]);
            return;
        }
        if (id == "ac10005")
            return;
        AttackSound(unitSfx[id]);
    }

    private void AttackSound(AudioSource sfxSource) {
        GameObject soundObject = GetUnusedAudio();
        soundObject.SetActive(true);
        AudioSource sound = soundObject.GetComponent<AudioSource>();
        sound.clip = sfxSource.clip;
        sound.Play();
        StartCoroutine(SoundAfterOff(sfxSource, soundObject));
    }


    public void PlaySound(SoundType type) {
        if (!sounds.ContainsKey(type) || sounds[type] == null) {
            Logger.LogError(string.Format("{0}에 대한 음원을 찾을 수 없습니다.", type));
            return;
        }
        PlaySfx(sounds[type]);
    }

    private void PlaySfx(AudioSource sfxSource) {
        GameObject soundObject = GetUnusedAudio();
        soundObject.SetActive(true);
        AudioSource sound = soundObject.GetComponent<AudioSource>();
        sound.clip = sfxSource.clip;
        sound.time = 0;
        sound.Play();
        StartCoroutine(SoundAfterOff(sfxSource, soundObject));
    }

    //미사용 오브젝트 찾기
    private GameObject GetUnusedAudio() {
        foreach(Transform child in gameObject.transform) {
            if (child.gameObject.activeSelf == false)
                return child.gameObject;
        }

        GameObject empty = Instantiate(emptySource, gameObject.transform);
        empty.transform.SetAsLastSibling();
        return empty;
    }

    //임시로 이리 쓰는데, Unirx를 써서 처리 예정, 오디오 시간만큼 기달리고, 그 후에 턴 off
    private IEnumerator SoundAfterOff(AudioSource audio, GameObject soundObject) {
        yield return new WaitForSeconds(audio.clip.length);
        soundObject.GetComponent<AudioSource>().clip.UnloadAudioData();
        soundObject.SetActive(false);
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
