using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionSetupManager : MonoBehaviour {
    [SerializeField] Slider bgmSlider;
    [SerializeField] TMPro.TextMeshProUGUI bgmValue;
    [SerializeField] Slider soundSlider;
    [SerializeField] TMPro.TextMeshProUGUI soundValue;
    [SerializeField] AccountSetup accountSetup;
    [SerializeField] LanguageSetup languageSetup;
    [SerializeField] Transform goldTrackBoard;
    [SerializeField] MenuSceneController menuSceneController;

    // Start is called before the first frame update
    public static bool vibrateOn;
    void Start() {
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
        languageSetup.Init();
    }

    // Update is called once per frame
    void Update() {
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
        slider.value += 0.01f;
    }

    public void VolumeDownBtn(Slider slider) {
        slider.value -= 0.01f;
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
            CustomVibrate.Vibrate(1000);
    }

    public void LanguageChange(string language) {
        var prevLanguage = PlayerPrefs.GetString("Language", AccountManager.Instance.GetLanguageSetting());
        if (language == prevLanguage) {
            Logger.Log("??????" + language + "??? ????????????????????????.");
            return;
        }

        //TODO
        //Scene ?????? ????????????
        //SingleTon Destroy?
        AccountManager.Instance.RequestLocaleSetting(false, language, (req, res) => {
            if (res.IsSuccess) {
                AccountManager.Instance.prevSceneName = "Login";
                StartCoroutine(_languageChange(language));
            }
            else {
                Modal.instantiate("?????? ?????? ?????? ??????", Modal.Type.CHECK);
            }
        });
    }

    IEnumerator _languageChange(string language) {
        AccountManager.Instance.SetLanguageSetting(language);

        AccountManager.Instance.GetComponent<Fbl_Translator>().localizationDatas.Clear();
        var downloaders = NetworkManager.Instance.GetComponents<LocalizationDataDownloader>();
        foreach (LocalizationDataDownloader downloader in downloaders) {
            downloader.Download();
        }
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => NetworkManager.Instance.GetComponent<LocalizationDownloadManager>().isDownloading);

        AccountManager.Instance.LoadAllCards();
        yield return new WaitForSeconds(1.0f);  //??????????????? ???????????? ??????
        
        FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.MAIN_SCENE);
    }

    public void OpenGoldTracking() {
        goldTrackBoard.gameObject.SetActive(true);
        goldTrackBoard.Find("Paid").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.userData._goldPaid.ToString();
        goldTrackBoard.Find("Free").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.userData._goldFree.ToString();
        EscapeKeyController.escapeKeyCtrl.AddEscape(CloseGoldTracking);
    }

    public void CloseGoldTracking() {
        goldTrackBoard.gameObject.SetActive(false);
        EscapeKeyController.escapeKeyCtrl.RemoveEscape(CloseGoldTracking);
    }
}
