using System;
using System.Collections;
using System.Collections.Generic;
using BestHTTP;
using Spine.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LoginController : MonoBehaviour {
    NetworkManager networkManager;
    GameObject loadingModal;
    [SerializeField] GameObject logo, textImage;
    [SerializeField] Button loginBtn;
    [SerializeField] GameObject skipbuttons, mmrchange;
    [SerializeField] TMPro.TMP_InputField mmrInputField, rankIdInputField;

    public GameObject obbCanvas, sceneStartCanvas, LoginTypeSelCanvas, EULACanvas, fbl_loginCanvas;
    [SerializeField] private LocalizationDownloadManager localizationDownloadManager;
    
    bool isClicked = false;

    private void Awake() {
        AccountManager.Instance.tokenSetFinished += OnTokenSetFinished;
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_USER_UPDATED, OnRequestUserInfoCallback);
#if UNITY_EDITOR
        skipbuttons.SetActive(true);
#endif
        mmrchange.SetActive(true);
        
        transform.Find("Panel")
            .GetComponent<DelayButton>()
            .instanceCallback.AddListener(() => {
                Hashtable hash = new Hashtable();
                hash.Add("amount", new Vector3(0.15f, 0.15f, 0f));
                hash.Add("time", 1f);
                iTween.PunchScale(logo, hash);
            });
    }

    private void OnDestroy() {
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_USER_UPDATED, OnRequestUserInfoCallback);
        AccountManager.Instance.tokenSetFinished -= OnTokenSetFinished;
    }

    private void OnTokenSetFinished() {
        loginBtn.enabled = true;
    }

    public void Login() {
        AccountManager.Instance.prevSceneName = "Login";

        networkManager = NetworkManager.Instance;
        StartCoroutine(LogoReveal());
    }

    private void Update() {
        if (Input.GetKey(KeyCode.D)) {
            PlayerPrefs.DeleteKey("ReconnectData");
        }
    }

    private void OnRequestUserInfoCallback(Enum Event_Type, Component Sender, object Param) {
        AccountManager accountManager = AccountManager.Instance;

        HTTPResponse res = (HTTPResponse)Param;

        if (res.IsSuccess) {
            if (accountManager.userData.preSupply < 200 && accountManager.userData.supplyTimeRemain > 0) {
                Invoke("ReqInTimer", (float)accountManager.userData.supplyTimeRemain);
            }
            else {
                Logger.Log("Pre Supply??? ??????????????????. Timer??? ???????????? ????????????.");
            }

            if (PlayerPrefs.GetInt("isFirst", 2) == 2) {
                LoginTypeSelCanvas.SetActive(true);
            }
            else {
                accountManager.OnSignInResultModal();
            }
        }
        else {
            isClicked = false;
        }
        
    }

    IEnumerator LogoReveal() {
        var downloaders = NetworkManager.Instance.GetComponents<LocalizationDataDownloader>();
        foreach (LocalizationDataDownloader downloader in downloaders) {
            downloader.Download();
        }
        yield return new WaitUntil(() => !localizationDownloadManager.isDownloading);
        
        yield return new WaitForSeconds(1.5f);
        //Logger.Log("textImage");
        logo.SetActive(true);
        //loginBtn.enabled = true;
        SkeletonGraphic skeletonGraphic = logo.GetComponent<SkeletonGraphic>();
        Spine.AnimationState state = skeletonGraphic.AnimationState;
        state.SetAnimation(0, "animation", false);
        isClicked = false;
        IAPSetup.Instance.Init();
        
        yield return new WaitForSeconds(1.0f);
        textImage.SetActive(true);
        
        //CustomVibrate.Vibrate(1000);
    }

    public void OnStartButton() {
        if (!isClicked) {
            AccountManager accountManager = AccountManager.Instance;
            accountManager.LoadAllCards();
            accountManager.LoadAllHeroes();
            accountManager.RequestHumanTemplates();
            accountManager.RequestOrcTemplates();
            accountManager.RequestUserInfo();
            accountManager.RequestRankTable();
            SoundManager.Instance.PlaySound(SoundType.FIRST_TURN);

        }
        isClicked = true;
    }

    public void OnGuestLoginButtonClick(string param) {
        Firebase.Analytics.FirebaseAnalytics.LogEvent("Guest_Login");
        FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.MAIN_SCENE);
    }

    private void ReqInTimer() {
        AccountManager.Instance.ReqInTimer(AccountManager.Instance.GetRemainSupplySec());
    }

    public void SkipStory(int type) {
        AccountManager accountManager = AccountManager.Instance;
        if(type == 0) {
            accountManager.SkipStoryRequest("human", 1);
        }
        else if(type == 1) {
            accountManager.SkipStoryRequest("human", 1);
            accountManager.SkipStoryRequest("orc", 1);
        }
        else if(type == 2) {
            accountManager.SkipStoryRequest("human", 1);
            accountManager.SkipStoryRequest("orc", 1);
            accountManager.SkipStoryRequest("human", 2);
            accountManager.SkipStoryRequest("orc", 2);
            accountManager.RequestUnlockInTutorial(1, (req, res) => {
                accountManager.RequestQuestInfo((_req, _res) => {
                    var questDatas = dataModules.JsonReader.Read<List<Quest.QuestData>>(_res.DataAsText);

                    Quest.QuestData questData = questDatas.Find(x => x.questDetail.id == "t1");
                    int qid = questData.id;
                    int progress = 2;
                    accountManager.RequestChangeQuestProgress(qid: qid, progress: progress, (__req, __res) => {
                        Logger.Log("!!");
                    });
                });
            });
            Dictionary<string, bool> GameStates = new Dictionary<string, bool>();
            GameStates.Add("AccountLinkTutorialLoaded", false);
            GameStates.Add("NickNameChangeTutorialLoaded", false);
            GameStates.Add("NeedToCallAttendanceBoard", true);
            GameStates.Add("DailyQuestLoaded", false);
            GameStates.Add("IsTutorialFinished", false);
            GameStates.Add("IsQ5Finished", true);

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (KeyValuePair<string, bool> dict in GameStates) {
                sb.Append(dict.Key + "|" + dict.Value + ",");
            }
            PlayerPrefs.SetString("GameStates", sb.ToString());

            MainSceneStateHandler.TutorialMilestone milestone = new MainSceneStateHandler.TutorialMilestone();
            milestone.milestoneType = MainSceneStateHandler.MilestoneType.QUEST;
            milestone.name = MenuTutorialManager.TutorialType.Q5;

            PlayerPrefs.SetString("TutorialMilestone", JsonUtility.ToJson(milestone));
        }
        NewAlertManager._ClearDic();
    }

    public void ChangeLanguageToEnglish() {
        StartCoroutine(nameof(_changeLanguageToEnglish));
    }

    private IEnumerator _changeLanguageToEnglish() {
        AccountManager.Instance.SetLanguageSetting("English");
        
        var downloaders = NetworkManager.Instance.GetComponents<LocalizationDataDownloader>();
        foreach (LocalizationDataDownloader downloader in downloaders) {
            downloader.Download();
        }
        yield return new WaitUntil(() => !localizationDownloadManager.isDownloading);
        
        Modal.instantiate("?????? ????????? ????????? ?????????????????????.", Modal.Type.CHECK);
    }

    public void ChangeMMR() {
        //Modal.instantiate(mmrInputField.text, Modal.Type.CHECK);
        int value = 0;
        int value2 = 0;
        int.TryParse(mmrInputField.text, out value);
        int.TryParse(rankIdInputField.text, out value2);

        AccountManager.Instance.RequestChangeMMRForTest(value, value2);
    }

    public void ChangeDefaultRank() {
        int value = 0;
        int.TryParse(mmrInputField.text, out value);

        if (value >= 0 && value <= 4) rankIdInputField.text = "18";
        else if (value >= 5 && value <= 39) rankIdInputField.text = "17";
        else if (value >= 40 && value <= 99) rankIdInputField.text = "16";
        else if (value >= 100 && value <= 149) rankIdInputField.text = "15";
        else if (value >= 150 && value <= 299) rankIdInputField.text = "14";
        else if (value >= 300 && value <= 449) rankIdInputField.text = "13";
        else if (value >= 450 && value <= 599) rankIdInputField.text = "12";
        else if(value >= 600 && value <= 799) rankIdInputField.text = "11";
        else if (value >= 800 && value <= 999) rankIdInputField.text = "10";
        else if (value >= 1000 && value <= 1199) rankIdInputField.text = "9";
        else if (value >= 1200 && value <= 1399) rankIdInputField.text = "8";
        else if (value >= 1400 && value <= 1699) rankIdInputField.text = "7";
        else if (value >= 1700 && value <= 1999) rankIdInputField.text = "6";
        else if (value >= 2000 && value <= 2299) rankIdInputField.text = "5";
        else if (value >= 2300 && value <= 2599) rankIdInputField.text = "4";
        else if (value >= 2600 && value <= 2999) rankIdInputField.text = "3";
        else rankIdInputField.text = "2";
    }
}
