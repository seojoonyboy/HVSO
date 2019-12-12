using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UniRx;
using Sirenix.OdinInspector;

public class SoundManager : SerializedMonoBehaviour {
    public Dictionary<SoundType, AudioClip> sounds;
    public Dictionary<string, AudioClip> unitSfx;
    public Dictionary<string, AudioClip> magicSfx;
    public Dictionary<UISfxSound, AudioClip> uiSfx;
    public Dictionary<IngameSfxSound, AudioClip> ingameSfx;
    public Dictionary<UnitRace, Dictionary<VoiceType, AudioClip>> unitSound;
    float soundVolume;

    public float SOUNDVOLUME {
        get { return soundVolume; }
        set {
            soundVolume = value;
            PlayerPrefs.SetFloat("SoundVolume", soundVolume);
        }
    }

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
        if (!PlayerPrefs.HasKey("SoundVolume")) 
            PlayerPrefs.SetFloat("SoundVolume", 0.5f);
        soundVolume = PlayerPrefs.GetFloat("SoundVolume");
            
        DontDestroyOnLoad(gameObject);    
    }

    public void PlayIngameSfx(IngameSfxSound id) {
        if (!ingameSfx.ContainsKey(id) || ingameSfx[id] == null) {
            AttackSound(ingameSfx[IngameSfxSound.CARDDRAW]);
            return;
        }
        PlaySfx(ingameSfx[id]);
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

    public void PlayUnitVoice(string id, VoiceType voice) {
        if (AccountManager.Instance.resource.unitRace.ContainsKey(id) == false) return;
        UnitRace race = AccountManager.Instance.resource.unitRace[id];
        AudioClip unitAudio = unitSound[race][voice];
        PlaySfx(unitAudio);
    }

    public async void PlayShieldChargeCount(int num) {
        for (int i = 0; i < num; i++) {
            await System.Threading.Tasks.Task.Delay(100);
            PlaySfx(ingameSfx[IngameSfxSound.SHIELDCHARGECOUNT]);            
        }
        PlayIngameSfx(IngameSfxSound.SHIELDCHARGE);
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

    private void AttackSound(AudioClip sfxSource) {
        GameObject soundObject = GetUnusedAudio();
        soundObject.SetActive(true);
        AudioSource sound = soundObject.GetComponent<AudioSource>();
        sound.clip = sfxSource;
        sound.volume = soundVolume;
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

    public void PlaySound(UISfxSound sfxSound) {
        if(!uiSfx.ContainsKey(sfxSound) || uiSfx[sfxSound] == null) {
            Logger.LogError(string.Format("{0}에 대한 음원을 찾을 수 없습니다."));
            return;
        }
        PlaySfx(uiSfx[sfxSound]);
    }

    private void PlaySfx(AudioClip sfxSource) {
        GameObject soundObject = GetUnusedAudio();
        soundObject.SetActive(true);
        AudioSource sound = soundObject.GetComponent<AudioSource>();
        sound.clip = sfxSource;
        sound.time = 0;
        sound.volume = soundVolume;
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
    private IEnumerator SoundAfterOff(AudioClip audio, GameObject soundObject) {
        yield return new WaitForSeconds(audio.length + 0.1f);
        soundObject.GetComponent<AudioSource>().clip = null;
        yield return new WaitForSeconds(0.1f);
        soundObject.SetActive(false);
    }
}

public enum VoiceType {
    ATTACK = 1500,
    CHARGE,
    DAMAGE,
    DIE
}

public enum UISfxSound {
    BOX_EPIC = 3500,
    BOX_NORMAL,
    BOX_RARE,
    BOX_SUPERRARE,
    BOXOPEN,
    BOXOPEN_2,
    BUTTON1,
    BUTTON2,
    BUTTON3,
    BUTTON4,
    CARDCHOICE1,
    CARDCHOICE2,
    CARDCHOICE3,
    CARDCHOICE_HERO,
    CARDCHOICE_UNIT,
    MENUSLIDE1,
    MENUSLIDE2,
    MENUSLIDE3,
    CARD_USE_NORMAL,
    CARD_USE_RARE,
    CARD_USE_SUPERRARE,
    CARD_USE_LEGEND
}

public enum IngameSfxSound {
    CARDDRAW = 4500,
    CARDERROR,
    GAMEMATCH,
    GETMANA,
    MULLIGANBUTTON,
    SHIELDACTION,
    SHIELDCHARGE,
    TEXTTYPING,
    TURNBUTTON,
    TURNSTART,
    HUMANTURN,
    ORCTURN,
    ORCMAGICTURN,
    SHIELDCHARGECOUNT
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
