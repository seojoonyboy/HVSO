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
    [SerializeField] AccountSetup accountSetup;
    // Start is called before the first frame update
    public static bool vibrateOn;
    void Start()
    {
        if (PlayerPrefs.HasKey("Vibrate")) {
            vibrateOn = (PlayerPrefs.GetString("Vibrate") == "On");
        }
        else
            PlayerPrefs.SetString("Vibrate", "On");
        transform.GetChild(0).Find("Vibration").Find("Off").GetComponent<Button>().interactable = PlayerPrefs.GetString("Vibrate") == "On";
        transform.GetChild(0).Find("Vibration").Find("On").GetComponent<Button>().interactable = PlayerPrefs.GetString("Vibrate") == "Off";
        bgmSlider.value = PlayerPrefs.GetFloat("BgmVolume");
        bgmValue.text = ((int)(bgmSlider.value * 100)).ToString();
        soundSlider.value = PlayerPrefs.GetFloat("SoundVolume");
        soundValue.text = ((int)(soundSlider.value * 100)).ToString();
        accountSetup.Init();
    }

    void OnDestroy() {
        accountSetup.Destory();    
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

    public void VibrateOn(bool on) {
        if (on)
            PlayerPrefs.SetString("Vibrate", "On");
        else 
            PlayerPrefs.SetString("Vibrate", "Off");
        transform.GetChild(0).Find("Vibration").Find("Off").GetComponent<Button>().interactable = on;
        transform.GetChild(0).Find("Vibration").Find("On").GetComponent<Button>().interactable = !on;
        vibrateOn = on;
        if (vibrateOn)
            Handheld.Vibrate();
    }
}
