using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SoundController : MonoBehaviour {
    private SoundManager soundManager;

    private void Start() {
        soundManager = SoundManager.Instance;
    }

    public void PlaySound(string name) {
        if(soundManager == null) return;
        try {
            UISfxSound sound = (UISfxSound)Enum.Parse(typeof(UISfxSound), name.ToUpper());
            soundManager.PlaySound(sound);
        }
        catch(Exception e) {
            Debug.Log("사운드 이름 오류 : " + name + "\n" + e);
        }
    }
}
