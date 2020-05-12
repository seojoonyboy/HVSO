using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IngameSettingModalManager : MonoBehaviour {
    [SerializeField] GameObject basePanel;
    [SerializeField] GameObject settingModal, quitModal;
    [SerializeField] GameObject vibrationButton;
    [SerializeField] Button settingBtn;
    [SerializeField] Slider sfxSlider, bgmSlider;
    [SerializeField] TMPro.TextMeshProUGUI sfxValue, bgmValue;
    SoundManager soundManager;

    private string battleType;
    void Awake() {
        if (settingBtn == null) return;
        settingBtn.onClick.AddListener(() => {
            basePanel.SetActive(true);
            settingModal.SetActive(true);
            PlayMangement.instance.openOption = true;
            PlayMangement.instance.cardInfoCanvas.GetChild(0).GetComponent<CardListManager>().CloseCardInfo();
            PlayMangement.instance.cardInfoCanvas.GetChild(0).GetComponent<CardListManager>().CloseUnitInfo();
        });
        SetUpIngameOption();

    }

    private void Start() {
        battleType = PlayerPrefs.GetString("SelectedBattleType");
    }

    private void SetOption() {
        
    }


    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if(!basePanel.activeSelf) basePanel.SetActive(true);

            if (!settingModal.activeSelf) {
                settingModal.SetActive(true);
            }
            else {
                quitModal.SetActive(true);
            }
        }

        if (bgmSlider.value != PlayerPrefs.GetFloat("BgmVolume")) {
            SoundManager.Instance.bgmController.BGMVOLUME = bgmSlider.value;
            bgmValue.text = ((int)(bgmSlider.value * 100)).ToString();
        }
        if (sfxSlider.value != PlayerPrefs.GetFloat("SoundVolume")) {
            SoundManager.Instance.SOUNDVOLUME = sfxSlider.value;
            sfxValue.text = ((int)(sfxSlider.value * 100)).ToString();
        }

    }

    public void OnSurrendBtn() {
        bool isTutorialFinished = System.Convert.ToBoolean(PlayerPrefs.GetString("IsTutorialFinished", "false"));
        if(!isTutorialFinished) return;
        
        if (PlayMangement.instance.isGame == false) return;
        quitModal.SetActive(true);
    }

    public void OnCancelBtn() {
        OffAllModals();
    }

    public void Surrend() {
        if (PlayMangement.instance.isGame == false) return;
        PlayMangement.instance.isGame = false;
        PlayMangement.instance.SocketHandler.Surrend(null);
        OffAllModals();
    }

    public void OffAllModals() {
        quitModal.SetActive(false);
        settingModal.SetActive(false);
        basePanel.SetActive(false);
        PlayMangement.instance.openOption = false;
    }


    private void SetUpIngameOption() {
        if (SoundManager.Instance == null) return;

        string vibrate = PlayerPrefs.GetString("Vibrate");
        soundManager = SoundManager.Instance;       

        sfxSlider = gameObject.transform.Find("Background/SettingModal/Options/EffectSound/Slider/Slider").gameObject.GetComponent<Slider>();
        sfxValue = gameObject.transform.Find("Background/SettingModal/Options/EffectSound/Header/Value").gameObject.GetComponent<TMPro.TextMeshProUGUI>();
        bgmSlider = gameObject.transform.Find("Background/SettingModal/Options/BGM/Slider/Slider").gameObject.GetComponent<Slider>();
        bgmValue = gameObject.transform.Find("Background/SettingModal/Options/BGM/Header/Value").gameObject.GetComponent<TMPro.TextMeshProUGUI>();

        sfxSlider.value = PlayerPrefs.GetFloat("SoundVolume");
        sfxValue.text = ((int)(sfxSlider.value * 100)).ToString();
        bgmSlider.value = PlayerPrefs.GetFloat("BgmVolume");
        bgmValue.text = ((int)(bgmSlider.value * 100)).ToString();

        bool vibrateActive = (vibrate == "On") ? true : false;
        ButtonActivate(vibrationButton.transform.Find("On").gameObject, vibrateActive);
        ButtonActivate(vibrationButton.transform.Find("Off").gameObject, !vibrateActive);
    }

    public void BgmUpBtn(Slider slider) {
        bgmSlider.value += 0.01f;
    }

    public void BgmDownBtn(Slider slider) {
        bgmSlider.value -= 0.01f;
    }

    public void SfxUpBtn(Slider slider) {
        sfxSlider.value += 0.01f;
    }

    public void SfxDownBtn(Slider slider) {
        sfxSlider.value -= 0.01f;
    }


    private void ButtonActivate(GameObject buttonObject, bool isOn) {
        Color buttonColor = (isOn) ? Color.white : new Color(0.5f, 0.5f, 0.5f);
        buttonObject.GetComponent<Image>().color = buttonColor;
        buttonObject.transform.Find("Text").gameObject.GetComponent<TMPro.TextMeshProUGUI>().color = buttonColor;
        buttonObject.transform.Find("Image").gameObject.GetComponent<Image>().color = buttonColor;
    }



    public void VibrateOn(bool on) {
        if (on)
            PlayerPrefs.SetString("Vibrate", "On");
        else
            PlayerPrefs.SetString("Vibrate", "Off");
        //transform.GetChild(0).Find("Vibration").Find("Off").GetComponent<Button>().interactable = on;
        //transform.GetChild(0).Find("Vibration").Find("On").GetComponent<Button>().interactable = !on;
        ButtonActivate(vibrationButton.transform.Find("On").gameObject, on);
        ButtonActivate(vibrationButton.transform.Find("Off").gameObject, !on);

        OptionSetupManager.vibrateOn = on;
        if (OptionSetupManager.vibrateOn)
            CustomVibrate.Vibrate(1000);
    }


}
