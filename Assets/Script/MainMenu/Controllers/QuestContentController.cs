using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace Quest {
    public partial class QuestContentController : MonoBehaviour {
        
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private TextMeshProUGUI info;
        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI sliderInfo;
        [SerializeField] private Button getBtn;
        [SerializeField] private Button rerollBtn;
        [SerializeField] private Transform rewardUIParent;

        public QuestData data;
        public QuestManager manager;
        
        private void OnEnable() {
            if(data == null) return;
            title.text = data.questDetail.name;
            info.text = data.questDetail.desc;
            slider.maxValue = (float)data.questDetail.progMax;
            slider.value = (float)data.progress;
            sliderInfo.text = data.progress.ToString() + "/" + data.questDetail.progMax.ToString();
            if(data.cleared) {
                if (!data.rewardGet) {
                    getBtn.enabled = true;
                    getBtn.GetComponentInChildren<TextMeshProUGUI>().text = "획득하기";
                }
                else {
                    getBtn.enabled = false;
                    getBtn.GetComponentInChildren<TextMeshProUGUI>().text = "획득완료";
                }
            }
            else {
                getBtn.enabled = false;
                getBtn.GetComponentInChildren<TextMeshProUGUI>().text = "진행중";
            }

            foreach(Transform slot in rewardUIParent) {
                slot.gameObject.SetActive(false);
            }

            var icons = AccountManager.Instance.resource.rewardIcon;

            for(int i=0; i<data.questDetail.rewards.Length; i++) {
                rewardUIParent.GetChild(i).gameObject.SetActive(true);
                Image rewardImg = rewardUIParent.GetChild(i).GetChild(0).GetComponent<Image>();
                if (icons.ContainsKey(data.questDetail.rewards[i].kind)) {
                    rewardImg.sprite = icons[data.questDetail.rewards[i].kind];
                }
            }

            NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_QUEST_REWARD_RECEIVED, OnRewardReceived);
        }

        private void OnDisable() {
            NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_QUEST_REWARD_RECEIVED, OnRewardReceived);
        }

        private void OnRewardReceived(Enum Event_Type, Component Sender, object Param) {
            var targetObj = (GameObject)Param;
            if(gameObject != targetObj) return;
            Modal.instantiate("보상을 우편으로 발송하였습니다.", Modal.Type.CHECK);
            //targetObj.GetComponent<QuestContentController>().getBtn.GetComponentInChildren<TextMeshProUGUI>().text = "획득완료";
            //targetObj.GetComponent<QuestContentController>().getBtn.enabled = false;
        }

        private void GetReward() {
            gameObject.SetActive(false);
        }

        public void RequestRewardButtonClicked() {
            if (data.cleared && !data.rewardGet) {
                AccountManager.Instance.RequestQuestClearReward(data.id, gameObject);
                AccountManager.Instance.RequestQuestInfo();
            }
        }
    }

    public partial class QuestContentController : MonoBehaviour {
        public void ActiveTutorial() {
            for(int i = 0; i < data.tutorials.Length; i++) {
                if(data.tutorials[i].isShowing == true) continue;
                MethodInfo theMethod = this.GetType().GetMethod(data.tutorials[i].method);
                object[] args = new object[]{data.tutorials[i].args};
                Debug.Log(data.tutorials[i].method);
                object played = theMethod.Invoke(this, args);
                data.tutorials[i].isShowing = (bool)played;
            }
        }

        public bool QuestSubSetShow(string[] args) {
            if(data.progress > 0) return false;
            Type enumType = typeof(MenuTutorialManager.TutorialType);
            MenuTutorialManager.TutorialType questEnum = (MenuTutorialManager.TutorialType)Enum.Parse(enumType, args[0].ToUpper());
            manager.tutoDialog.StartQuestSubSet(questEnum);
            return true;
        }

        public bool QuestIconShow(string[] args) {
            if(data.progress > 0) return false;
            manager.ShowHandIcon();
            return true;
        }

        public bool ShowStoryHand(string[] args) {
            if(data.cleared) return false;
            string camp = args[0];
            int stage = int.Parse(args[1]);
            manager.tutorialSerializeList.scenarioManager.SetTutoQuest(this, stage);
            manager.tutorialSerializeList.playButton.SetActive(true);
            CheckTutorialPlayed(int.Parse(args[1]));
            return true;
        }

        private void CheckTutorialPlayed(int stage) {
            bool isHumanClear = AccountManager.Instance.clearedStages.Exists(x=>(x.stageNumber == stage && x.camp.CompareTo("human") == 0));
            if(!isHumanClear) manager.tutorialSerializeList.HumanFlagIcon.SetActive(true);
            bool isOrcClear = AccountManager.Instance.clearedStages.Exists(x=>(x.stageNumber == stage && x.camp.CompareTo("orc") == 0));
            if(!isOrcClear) manager.tutorialSerializeList.OrcFlagIcon.SetActive(true);
        }

        public bool QuestClearShow(string[] args) {
            if(!data.cleared) return false;
            manager.tutoDialog.StartQuestSubSet(MenuTutorialManager.TutorialType.QUEST_SUB_SET_2);
            manager.ShowHandIcon();
            ShowHandIcon();
            return true;
        }

        private void ShowHandIcon() {
            AddSpinetoButtonAndRemoveClick(getBtn, GetQuestItem);
        }

        private void GetQuestItem() {
            manager.tutoDialog.StartQuestSubSet(MenuTutorialManager.TutorialType.QUEST_SUB_SET_3);
            PlayerPrefs.SetInt("FirstTutorialClear", 1);
            PlayerPrefs.Save();
        }

        public bool StartMailTutorial(string[] args) {
            GetPostOffice();
            if(!manager.tutorialSerializeList.backButton.gameObject.activeInHierarchy) return false;
            AddSpinetoButtonAndRemoveClick(manager.tutorialSerializeList.backButton);
            return true;
        }

        private async void GetPostOffice() {
            await Task.Delay(500);
            manager.tutorialSerializeList.newMail.SetActive(true);
            manager.tutorialSerializeList.mailAllGetButton.interactable = false;
            manager.tutorialSerializeList.mailBoxManager.tutoQuest = new MailBoxManager.TutorialQuest();
            manager.tutorialSerializeList.mailBoxManager.tutoQuest.quest = this;
        }

        public void MailOpen() {
            AddSpinetoButtonAndRemoveClick(manager.tutorialSerializeList.mailBoxManager.tutoQuest.receiveBtn, SubSet4);
            manager.tutorialSerializeList.mailBoxManager.tutoQuest.openBtn.enabled = false;
        }

        public void ResetMailOpen(Button btn) {
            btn.onClick.RemoveListener(SubSet4);
            Transform hand = btn.transform.Find("tutorialHand");
            if(hand == null) return;
            Destroy(hand.gameObject);
        }

        private void SubSet4() {
            manager.tutorialSerializeList.newMail.SetActive(false);
            manager.tutorialSerializeList.mailBoxManager.tutoQuest.openBtn.enabled = true;
            manager.tutorialSerializeList.mailAllGetButton.interactable = true;
            manager.tutoDialog.StartQuestSubSet(MenuTutorialManager.TutorialType.QUEST_SUB_SET_4);
            manager.tutorialSerializeList.mailBoxManager.tutoQuest = null;
            AddSpinetoButtonAndRemoveClick(manager.tutorialSerializeList.backButton, BreakCardDictionaryTab);
        }

        public void BreakCardDictionaryTab() {
            manager.tutoDialog.StartQuestSubSet(MenuTutorialManager.TutorialType.QUEST_SUB_SET_9);
            PlayerPrefs.DeleteKey("FirstTutorialClear");
            PlayerPrefs.Save();
        }

        public void AddSpinetoButtonAndRemoveClick(Button button, UnityAction moreAction = null) {
            Instantiate(manager.handSpinePrefab, button.transform, false).name = "tutorialHand";
            UnityAction deleteHand = null;
            if(moreAction != null) deleteHand += moreAction;
            deleteHand += () => {
                button.onClick.RemoveListener(deleteHand);
                Transform hand = button.transform.Find("tutorialHand");
                if(hand == null) return;
                Destroy(hand.gameObject);
            };
            button.onClick.AddListener(deleteHand);
        }

        public bool MenuDictionaryShowHand(string[] args) {
            if(data.cleared) return false;
            MenuSceneController menu = MenuSceneController.menuSceneController;
            menu.DictionaryShowHand(this, args);
            manager.tutorialSerializeList.newCardMenu.SetActive(true);
            return true;
        }

        public void ReadyEnterCardMenu() {
            manager.tutorialSerializeList.newCardMenu.SetActive(false);
            manager.tutoDialog.StartQuestSubSet(MenuTutorialManager.TutorialType.QUEST_SUB_SET_5);
        }

        public void DictionaryCardHand(string[] args) {
            if(data.cleared) return;
            CardDictionaryManager cardManager = CardDictionaryManager.cardDictionaryManager;
            cardManager.cardShowHand(this, args);
        }

        public bool CreateCardCheck(string[] args) {
            if(data.cleared) return false;
            OnEvent theEvent = null;
            theEvent = (type, Sender, Param) => {
                var card = Array.Find(AccountManager.Instance.myCards, x=>x.cardId.CompareTo(args[0])==0);
                if(card == null) return;
                if(card.cardCount != 4) return;
                createCardDone();
                NoneIngameSceneEventHandler.Instance.RemoveListener(
                    NoneIngameSceneEventHandler.EVENT_TYPE.API_INVENTORIES_UPDATED, 
                    theEvent
                );
            };
            NoneIngameSceneEventHandler.Instance.AddListener(
                NoneIngameSceneEventHandler.EVENT_TYPE.API_INVENTORIES_UPDATED, 
                theEvent);
            return true;
        }

        public void CloseDictionary() {
            manager.tutorialSerializeList.menuLockController.Unlock("DeckEdit", true);
        }

        private async void createCardDone() {
            await Task.Delay(2000);
            GameObject hand;
            while(true) {
                hand = GameObject.Find("tutorialHand");
                if(hand == null) break;
                DestroyImmediate(hand);
            }
            //튜토리얼 완료
            MenuSceneController menu = MenuSceneController.menuSceneController;
            CardDictionaryManager card = CardDictionaryManager.cardDictionaryManager;
            menu.DictionaryRemoveHand();
            card.closingToShowEditDeckLock = true;
            AddSpinetoButtonAndRemoveClick(card.transform.Find("UIbar/ExitBtn").GetComponent<Button>());
            manager.tutoDialog.StartQuestSubSet(MenuTutorialManager.TutorialType.QUEST_SUB_SET_6);
            AccountManager.Instance.RequestUnlockInTutorial(4);
            AccountManager.Instance.RequestQuestInfo();
        }

        public bool MenuDeckSettingShowHand(string[] args) {
            if(data.cleared) return false;
            AddSpinetoButtonAndRemoveClick(manager.tutorialSerializeList.ScrollDeckButton);
            manager.tutorialSerializeList.newDeckMenu.SetActive(true);
            manager.tutorialSerializeList.horizontalScrollSnap.OnSelectionChangeEndEvent.AddListener(x=>{if(x==0) manager.tutorialSerializeList.newDeckMenu.SetActive(false);});
            DeckHandler[] decks = manager.tutorialSerializeList.deckSettingManager.transform.GetComponentsInChildren<DeckHandler>();
            Array.ForEach(decks, x=> x.TutorialHandShow(this));
            return true;
        }

        public bool DeckSettingRemoveCard(string[] args) {
            if(data.cleared) return false;
            if(EditCardHandler.questInfo == null)
                EditCardHandler.questInfo = new EditCardHandler.QuestInfo();
            EditCardHandler.QuestInfo questInfo = EditCardHandler.questInfo;
            questInfo.quest = this;
            questInfo.removeId = args[0];
            return true;
        }

        public bool DeckSettingAddCard(string[] args) {
            if(data.cleared) return false;
            if(EditCardHandler.questInfo == null)
                EditCardHandler.questInfo = new EditCardHandler.QuestInfo();
            EditCardHandler.QuestInfo questInfo = EditCardHandler.questInfo;
            questInfo.quest = this;
            questInfo.addId = args[0];
            return true;
        }

        public bool BattleShow(string[] args) {
            if(data.cleared) return false;
            manager.tutorialSerializeList.modeSelect.onClick.AddListener(ModeClicked);
            manager.tutorialSerializeList.newBattleMenu.SetActive(true);
            manager.tutorialSerializeList.BattleButton.onClick.AddListener(BattleClicked);
            manager.tutorialSerializeList.modeGlow.SetActive(true);
            return true;
        }

        public async void ModeClicked() {
            manager.tutorialSerializeList.modeSelect.GetComponent<Button>().onClick.RemoveListener(ModeClicked);
            await Task.Delay(400);
            manager.tutorialSerializeList.menuLockController.Unlock("League", true);
            GameObject battle = manager.tutorialSerializeList.BattleButton.gameObject;
            Instantiate(manager.handSpinePrefab, battle.transform, false);
        }

        private void BattleClicked() {
            manager.tutoDialog.StartQuestSubSet(MenuTutorialManager.TutorialType.QUEST_SUB_SET_8);
            manager.tutorialSerializeList.newBattleMenu.SetActive(false);
            manager.tutorialSerializeList.BattleButton.GetComponent<Button>().onClick.RemoveListener(BattleClicked);
            manager.tutorialSerializeList.modeGlow.SetActive(false);
            //AccountManager.Instance.RequestQuestProgress(data.id);
        }
    }
}