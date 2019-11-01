using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour {
    private SoundManager soundManager;

    private void Start() {
        soundManager = SoundManager.Instance;
    }

    public void PlaySound(string name) {
        if(soundManager == null) return;
        //soundManager.PlaySound(name);
    }
}
