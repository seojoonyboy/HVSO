using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using dataModules;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using UnityEngine.SceneManagement;

namespace Quest {
    public class QuestManager : MonoBehaviour
    {
        public GameObject QuestCanvas;
        public Transform content;
        [SerializeField] protected HUDController HUDController;
        [SerializeField] protected Transform header;
        [SerializeField] protected Transform achievementList;
        [SerializeField] GameObject newIcon;
        [SerializeField] private TMPro.TextMeshProUGUI clearNumText;
        [SerializeField] protected Transform windowList;
        [SerializeField] protected GameObject allClearTextObject;
        public MenuSceneController tutoDialog;

        public GameObject handSpinePrefab;
        protected Tutorials[] tutorialJson;
        private int clearNum;
        private string localSaveData;
        int clearedTargetIndex;
        public bool alertSettingFinished = false;
        
        public static bool onAnimation = false;
        private void Start() {
            alertSettingFinished = false;
            NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_QUEST_UPDATED, ShowQuest);
            NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_ACHIEVEMENT_UPDATED, ShowAcievement);
            NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_ACHIEVEMENT_REWARD_RECEIVED, RefrechCleardedAchievement);
            AccountManager.Instance.RequestQuestInfo();
            AccountManager.Instance.RequestAchievementInfo();
            LoadQuestDataLocal();
        }

        public void SwitchPanel(int page) {
            for (int i = 0; i < 3; i++) {
                if (i == 1) continue;
                QuestCanvas.transform.Find("InnerCanvas/MainPanel").GetChild(i).gameObject.SetActive(i == page);
                header.GetChild(i).GetComponent<Button>().interactable = !(i == page);
            }
        }

        protected virtual void OnBackBtnClicked() {
            if (onAnimation) return;
            SoundManager.Instance.PlaySound(UISfxSound.BUTTON1);
            EscapeKeyController.escapeKeyCtrl.RemoveEscape(OnBackBtnClicked);
            HUDController.SetHeader(HUDController.Type.SHOW_USER_INFO);
            AccountManager.Instance.RequestMailBoxNum();
            QuestCanvas.SetActive(false);
        }

        public void OpenWindow(GameObject obj) {
            if (onAnimation) return;
            obj.SetActive(true);
            Button pressed = null;
            switch (obj.name) {
                case "QuestPanel":
                    pressed = header.Find("day").GetComponent<Button>();
                    break;
                case "WeeklyWuestPanel":
                    pressed = header.Find("Week").GetComponent<Button>();
                    break;
                case "AchievementPanel":
                    pressed = header.Find("Achievement").GetComponent<Button>();

                    break;
                default:
                    pressed = header.Find("day").GetComponent<Button>();
                    break;
            }
            pressed.interactable = false;
            for (int i = 0; i < windowList.childCount; i++) {
                if (windowList.GetChild(i).gameObject != obj) {
                    windowList.GetChild(i).gameObject.SetActive(false);
                    header.GetChild(i).GetComponent<Button>().interactable = true;
                }
            }
        }

        public virtual void OpenQuestCanvas() {
            AccountManager.Instance.RequestQuestInfo();
            HUDController.SetHeader(HUDController.Type.RESOURCE_ONLY_WITH_BACKBUTTON);
            HUDController.SetBackButton(OnBackBtnClicked);

            EscapeKeyController.escapeKeyCtrl.AddEscape(OnBackBtnClicked);
            OpenWindow(windowList.Find("QuestPanel").gameObject);
            QuestCanvas.SetActive(true);
            SwitchPanel(0);
            showNewIcon(false);
        }

        protected void OnDestroy() {
            NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_QUEST_UPDATED, ShowQuest);
            NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_ACHIEVEMENT_UPDATED, ShowAcievement);
            NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_ACHIEVEMENT_REWARD_RECEIVED, RefrechCleardedAchievement);
        }

        public void AddQuest(QuestData data) {
            QuestContentController quest = null;
            foreach(Transform item in content) {
                if(!item.gameObject.activeSelf) {
                    quest = item.GetComponent<QuestContentController>();
                    break;
                }
            }

            if (quest == null) return;

            quest.GetComponent<RectTransform>().sizeDelta = new Vector2(
                quest.fakeItem.GetComponent<RectTransform>().sizeDelta.x,
                quest.fakeItem.GetComponent<RectTransform>().sizeDelta.y
            );

            quest.GetComponent<Image>().enabled = true;
            foreach (Transform tf in quest.transform) {
                tf.gameObject.SetActive(true);
            }

            quest.data = data;
            quest.manager = this;
            quest.gameObject.SetActive(true);
            if(data.cleared) clearNum++;

            onAnimation = false;
        }

        public void AddAchievement(AchievementData data, int index) {
            Transform target = achievementList.GetChild(index);
            target.gameObject.SetActive(true);

            if (data.check.Contains("hero")) {
                if(data.check.Contains("h10001"))
                    target.Find("Icon").GetComponent<Image>().sprite = AccountManager.Instance.resource.achievementIcon["hero_h10001"];
                if (data.check.Contains("h10002"))
                    target.Find("Icon").GetComponent<Image>().sprite = AccountManager.Instance.resource.achievementIcon["hero_h10002"];
                if (data.check.Contains("h10003")) 
                    target.Find("Icon").GetComponent<Image>().sprite = AccountManager.Instance.resource.achievementIcon["hero_h10003"];
                if (data.check.Contains("h10004"))
                    target.Find("Icon").GetComponent<Image>().sprite = AccountManager.Instance.resource.achievementIcon["hero_h10004"];
            }
            else  if(data.check.Contains("rank_best"))
                target.Find("Icon").GetComponent<Image>().sprite = AccountManager.Instance.resource.achievementIcon["rank_best"];
            else if(data.check.Contains("battle_count"))
                target.Find("Icon").GetComponent<Image>().sprite = AccountManager.Instance.resource.achievementIcon["battle_count"];
            else if (data.check.Contains("box_open_count"))
                target.Find("Icon").GetComponent<Image>().sprite = AccountManager.Instance.resource.achievementIcon["box_open_count"];
            else if (data.check.Contains("card_count_best"))
                target.Find("Icon").GetComponent<Image>().sprite = AccountManager.Instance.resource.achievementIcon["card_count_best"];
            else if (data.check.Contains("card_break_count"))
                target.Find("Icon").GetComponent<Image>().sprite = AccountManager.Instance.resource.achievementIcon["card_break_count"];
            else if (data.check.Contains("card_craft_count"))
                target.Find("Icon").GetComponent<Image>().sprite = AccountManager.Instance.resource.achievementIcon["card_craft_count"];

            target.Find("Title/Text").GetComponent<TMPro.TextMeshProUGUI>().text = data.name;
            target.Find("Info").GetComponent<TMPro.TextMeshProUGUI>().text = data.desc;
            target.Find("Progressing").gameObject.SetActive(data.progress < data.progMax);
            target.Find("Progressing/Rate").GetComponent<TMPro.TextMeshProUGUI>().text = data.progress + "/" + data.progMax;
            target.Find("Rewards").GetChild(0).Find("Image").GetComponent<Image>().sprite = AccountManager.Instance.resource.rewardIcon[data.reward.kind];
            target.Find("Rewards").GetChild(0).Find("Amount").GetComponent<TMPro.TextMeshProUGUI>().text= data.reward.amount;
            target.Find("Rewards").GetChild(0).GetComponent<Button>().onClick.RemoveAllListeners();
            target.Find("Rewards").GetChild(0).GetComponent<Button>().onClick.AddListener(() => RewardDescriptionHandler.instance.RequestDescriptionModalWithBg(data.reward.kind));
            target.Find("GetBtn").GetComponent<Button>().onClick.AddListener(() => RecieveAchievement(data.acvId, index));
        }

        void RecieveAchievement(string id, int index) {
            clearedTargetIndex = index;
            AccountManager.Instance.RequestAchievementClearReward(id);
        }

        protected void RefrechCleardedAchievement(Enum type, Component Sender, object Param) {
            if(SceneManager.GetActiveScene().name != "MenuScene") return;
            AddAchievement(AccountManager.Instance.updatedAchievement, clearedTargetIndex);
        }

        public void ResetQuest() {
            foreach(Transform item in content) {
                item.gameObject.SetActive(false);
            }
        }

        protected void ShowQuest(Enum type, Component Sender, object Param) {
            AccountManager.Instance.RequestQuestRefreshTime();
            ResetQuest();
            clearNum = 0;
            AccountManager accountManager = AccountManager.Instance;
            var questDatas = accountManager.questDatas;
            StringBuilder file = new StringBuilder();
            foreach(QuestData questData in questDatas) {
                AddQuest(questData);
                CheckNewQuest(questData.questDetail.id);
                file.Append(questData.questDetail.id);
                file.Append(',');
            }
            bool questAllClear = questDatas.Count == 0;
            if(allClearTextObject != null) 
                allClearTextObject.SetActive(questAllClear);
            if(questAllClear) {
                onAnimation = false;
            }
            ShowNotice();
            SaveQuestDataLocal(file.ToString());
        }

        protected void ShowAcievement(Enum type, Component Sender, object Param) {
            if(SceneManager.GetActiveScene().name != "MenuScene") return;
            
            for(int i = 0; i < AccountManager.Instance.achievementDatas.Count; i++) {
                AddAchievement(AccountManager.Instance.achievementDatas[i], i);
            }
        }


        private void CheckNewQuest(string id) {
            bool isNew = !localSaveData.Contains(id);
            if(isNew) showNewIcon(true);
        }

        private void ShowNotice() {
            alertSettingFinished = true;
            if(clearNumText == null) return;
            if(clearNum > 0) {
                clearNumText.transform.parent.gameObject.SetActive(true);
                clearNumText.text = clearNum.ToString();
                showNewIcon(false);
            }
            else {
                clearNumText.transform.parent.gameObject.SetActive(false);
            }
        }

        private void SaveQuestDataLocal(string file) {
            if(string.IsNullOrEmpty(file)) return;
            localSaveData = file;
            PlayerPrefs.SetString("QuestManagerData", file.RemoveLast(1));
        }

        private void LoadQuestDataLocal() {
            localSaveData = PlayerPrefs.GetString("QuestManagerData", "none");
        }

        public void showNewIcon(bool yesno) {
            newIcon.SetActive(yesno);
        }

        public void DailyQuestInformation() {
            string[] check = new string[1];
            check[0] = AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText("UIPopup", "ui_popup_check");
            Modal.instantiate(AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText("UIPopup", "ui_popup_quest_qchangehour"), Modal.Type.CHECK, null, null, null, check, null);
        }


        public TutorialSerializeList tutorialSerializeList;
        /// <summary>
        /// ??????????????? ???????????? ?????? Inspector ?????? ????????? GameObject ?????? Component??? ???????????? ?????? ????????? ?????????
        /// </summary>
        [Serializable] public class TutorialSerializeList {
            public GameObject playButton;   //????????? ?????? ?????????
            public GameObject HumanFlagIcon; //?????? ????????? ??????, ????????? ????????? ?????? 
            public GameObject OrcFlagIcon;  //?????? ????????? ??????, ????????? ????????? ??????
            public GameObject newMail;  //?????????, ????????? ????????? ??????
            public ScenarioManager scenarioManager; //?????????????????????, 0-2 ?????? ????????? ????????? ??????
            public DeckSettingManager deckSettingManager; //??????????????????, ???????????? ?????? ?????? ????????? ???????????? ??????
            public Button BattleButton; //Mode??? ?????? ??????, ????????? ????????? ????????? ??? ?????? ?????? ??????
            public Button backButton; //??? ??????, ????????? ????????? ??????
            public Button mailAllGetButton; //?????? ?????? ?????? ??????, ????????? ?????? ?????? ?????? ???????????? ????????? ??????????
            public MailBoxManager mailBoxManager; //?????????????????????, ???????????? ????????? ?????? ????????? ???????????? ??????
            public MenuLockController menuLockController; //?????????????????????, ???????????? ??? ???
            public HorizontalScrollSnap horizontalScrollSnap; //???????????????(??????????????????), ??????????????? ????????? ?????? ?????? ??????
            public GameObject newCardMenu;  //?????? ?????? 4?????? ??????????????? ?????????
            public Button ScrollDeckButton; //?????? ?????? 2?????? ????????? ??????
            public GameObject newDeckMenu;  //?????? ?????? 2?????? ???????????? ?????????
            public GameObject newBattleMenu;//Mode??? ?????????
            public Button modeSelect;   //Mode ??????
            public GameObject modeGlow; //Mode??? ????????? 
        }

    }

    [Serializable] public class QuestData {
        public int id;
        public int progress;
        public QuestDetail questDetail;
        public bool cleared = false;
        public bool rewardGet = false;
        public TutorialShowList[] tutorials;
    }

    public class RefreshRemain {
        public int remainTime;
    }

    public class QuestDetail {
        public string id;
        public string type;
        public string name;
        public string desc;
        public int progMax;
        public Reward[] rewards;
    }

    [Serializable]
    public class Reward {
        public string kind;
        public int amount;
    }
    /// <summary>
    /// Resource??? questData json??? Deserialize??? ?????? ?????????
    /// </summary>
    public class Tutorials {
        public string id;
        public TutorialShowList[] tutorials;
    }
    /// <summary>
    /// ????????? ?????????????????? ????????????
    /// </summary>
    public class TutorialShowList {
        public string method;   //???????????? ????????????
        public string[] args;   //???????????? ????????????
        public bool isShowing = false;  //?????? ???????????? ?????? ???????????? ?????? ?????????
    }
}