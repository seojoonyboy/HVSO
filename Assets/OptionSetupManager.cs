using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionSetupManager : MonoBehaviour
{
    [SerializeField] Slider bgmSlider;
    [SerializeField] TMPro.TextMeshProUGUI bgmValue;
    [SerializeField] Slider soundSlider;
    [SerializeField] TMPro.TextMeshProUGUI soundValue;
    // Start is called before the first frame update
    void Start()
    {
        bgmSlider.value = PlayerPrefs.GetFloat("BgmVolume");
        bgmValue.text = ((int)(bgmSlider.value * 100)).ToString();
        soundSlider.value = PlayerPrefs.GetFloat("SoundVolume");
        soundValue.text = ((int)(soundSlider.value * 100)).ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (bgmSlider.value != PlayerPrefs.GetFloat("BgmVolume")) {
            SoundManager.Instance.bgmController.BGMVOLUME = bgmSlider.value;
            bgmValue.text = ((int)(bgmSlider.value * 100)).ToString();
        }
        if (soundSlider.value != PlayerPrefs.GetFloat("SoundVolume")) {
            SoundManager.Instance.SOUNDVOLUME = soundSlider.value;
            soundValue.text = ((int)(soundSlider.value * 100)).ToString();
        }
    }

    public void VolumeUpBtn(Slider slider) {
        bgmSlider.value += 0.01f;
    }

    public void VolumeDownBtn(Slider slider) {
        bgmSlider.value -= 0.01f;
    }
}
