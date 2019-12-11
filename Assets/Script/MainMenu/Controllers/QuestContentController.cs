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
            title.text = data.name;
            info.text = data.desc;
            slider.maxValue = (float)data.progMax;
            slider.value = (float)data.prog;
            sliderInfo.text = data.prog.ToString() + "/" + data.progMax.ToString();
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

            for(int i=0; i<data.rewards.Length; i++) {
                rewardUIParent.GetChild(i).gameObject.SetActive(true);
                Image rewardImg = rewardUIParent.GetChild(i).GetChild(0).GetComponent<Image>();
                if (icons.ContainsKey(data.rewards[i].kind)) {
                    rewardImg.sprite = icons[data.rewards[i].kind];
                }
            }

            NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_QUEST_REWARD_RECEIVED, OnRewardReceived);
        }

        private void OnDisable() {
            NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_QUEST_REWARD_RECEIVED, OnRewardReceived);
        }

        private void OnRewardReceived(Enum Event_Type, Component Sender, object Param) {
            var targetObj = (GameObject)Param;

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
                MethodInfo theMethod = this.GetType().GetMethod(data.tutorials[i].method);
                object[] args = new object[]{data.tutorials[i].args};
                Debug.Log(data.tutorials[i].method);
                theMethod.Invoke(this, args);
            }
        }

        public void QuestSubSetShow(string[] args) {
            if(data.prog > 0) return;
            Type enumType = typeof(MenuTutorialManager.TutorialType);
            MenuTutorialManager.TutorialType questEnum = (MenuTutorialManager.TutorialType)Enum.Parse(enumType, args[0].ToUpper());
            manager.tutoDialog.StartQuestSubSet(questEnum);
        }

        public void QuestIconShow(string[] args) {
            if(data.prog > 0) return;
            manager.ShowHandIcon();
        }

        public void ShowStoryHand(string[] args) {
            if(data.cleared) return;
            string camp = args[0];
            int stage = int.Parse(args[1]);
            manager.tutorialSerializeList.scenarioManager.SetTutoQuest(this, stage);
            manager.tutorialSerializeList.playButton.SetActive(true);
            CheckTutorialPlayed(int.Parse(args[1]));
        }

        private void CheckTutorialPlayed(int stage) {
            bool isHumanClear = AccountManager.Instance.clearedStages.Exists(x=>(x.stageNumber == stage && x.camp.CompareTo("human") == 0));
            if(!isHumanClear) StoryHandInstance(manager.tutorialSerializeList.HumanFlag);
            bool isOrcClear = AccountManager.Instance.clearedStages.Exists(x=>(x.stageNumber == stage && x.camp.CompareTo("orc") == 0));
            if(!isOrcClear) StoryHandInstance(manager.tutorialSerializeList.OrcFlag);
        }

        private void StoryHandInstance(Transform parent) {
            Instantiate(manager.handSpinePrefab, parent, false).name = "tutorialHand";
        }

        public void QuestClearShow(string[] args) {
            if(!data.cleared) return;
            manager.tutoDialog.StartQuestSubSet(MenuTutorialManager.TutorialType.QUEST_SUB_SET_2);
            manager.ShowHandIcon();
            ShowHandIcon();
           
        }

        private void ShowHandIcon() {
            AddSpinetoButtonAndRemoveClick(getBtn, GetQuestItem);
        }

        private void GetQuestItem() {
            manager.tutoDialog.StartQuestSubSet(MenuTutorialManager.TutorialType.QUEST_SUB_SET_3);
            PlayerPrefs.SetInt("FirstTutorialClear", 1);
            PlayerPrefs.Save();
        }

        public void StartMailTutorial(string[] args) {
            GetPostOffice();
            if(!manager.tutorialSerializeList.backButton.gameObject.activeInHierarchy) return;
            AddSpinetoButtonAndRemoveClick(manager.tutorialSerializeList.backButton);
        }

        private async void GetPostOffice() {
            await Task.Delay(500);
            manager.tutorialSerializeList.newMail.SetActive(true);
            
            manager.tutorialSerializeList.mailBoxManager.quest = this;
        }

        public void MailOpen() {
            AddSpinetoButtonAndRemoveClick(manager.tutorialSerializeList.mailReceiveButton, SubSet4);
            manager.tutorialSerializeList.openMailButton.enabled = false;
        }

        private void SubSet4() {
            manager.tutorialSerializeList.mailBoxManager.quest = null;
            manager.tutorialSerializeList.newMail.SetActive(false);
            manager.tutorialSerializeList.openMailButton.enabled = true;
            manager.tutoDialog.StartQuestSubSet(MenuTutorialManager.TutorialType.QUEST_SUB_SET_4);
            AddSpinetoButtonAndRemoveClick(manager.tutorialSerializeList.mailBackButton, BreakCardDictionaryTab);
        }

        private void BreakCardDictionaryTab() {
            manager.tutoDialog.StartQuestSubSet(MenuTutorialManager.TutorialType.QUEST_SUB_SET_9);
            PlayerPrefs.DeleteKey("FirstTutorialClear");
            PlayerPrefs.Save();
        }

        private void AddSpinetoButtonAndRemoveClick(Button button, UnityAction moreAction = null) {
            Instantiate(manager.handSpinePrefab, button.transform, false).name = "tutorialHand";
            UnityAction deleteHand = null;
            if(moreAction != null) deleteHand += moreAction;
            deleteHand += () => {
                deleteHand -= deleteHand;
                Transform hand = button.transform.Find("tutorialHand");
                if(hand == null) return;
                Destroy(hand.gameObject);
            };
            button.onClick.AddListener(deleteHand);
        }

        public void MenuDictionaryShowHand(string[] args) {
            if(data.cleared) return;
            MenuSceneController menu = MenuSceneController.menuSceneController;
            menu.DictionaryShowHand(this, args);
            manager.tutorialSerializeList.newCardMenu.SetActive(true);
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

        public void CreateCardCheck(string[] args) {
            if(data.cleared) return;
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
        }

        public void CloseDictionary() {
            manager.tutorialSerializeList.menuLockController.Unlock("DeckEdit", false);
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
            menu.DictionaryRemoveHand();
            CardDictionaryManager.cardDictionaryManager.closingToShowEditDeckLock = true;
            manager.tutoDialog.StartQuestSubSet(MenuTutorialManager.TutorialType.QUEST_SUB_SET_6);
            AccountManager.Instance.RequestUnlockInTutorial(3);
            AccountManager.Instance.RequestQuestInfo();
        }

        public void MenuDeckSettingShowHand(string[] args) {
            if(data.cleared) return;
            manager.tutorialSerializeList.newDeckMenu.SetActive(true);
            manager.tutorialSerializeList.horizontalScrollSnap.OnSelectionChangeEndEvent.AddListener(x=>{if(x==0) manager.tutorialSerializeList.newDeckMenu.SetActive(false);});
            DeckHandler[] decks = manager.tutorialSerializeList.deckSettingManager.transform.GetComponentsInChildren<DeckHandler>();
            Array.ForEach(decks, x=> x.TutorialHandShow(this));
        }

        public void DeckSettingRemoveCard(string[] args) {
            if(data.cleared) return;
            if(EditCardHandler.questInfo == null)
                EditCardHandler.questInfo = new EditCardHandler.QuestInfo();
            EditCardHandler.QuestInfo questInfo = EditCardHandler.questInfo;
            questInfo.quest = this;
            questInfo.removeId = args[0];
        }

        public void DeckSettingAddCard(string[] args) {
            if(data.cleared) return;
            if(EditCardHandler.questInfo == null)
                EditCardHandler.questInfo = new EditCardHandler.QuestInfo();
            EditCardHandler.QuestInfo questInfo = EditCardHandler.questInfo;
            questInfo.quest = this;
            questInfo.addId = args[0];
        }

        public void BattleShow(string[] args) {
            GameObject battle = manager.tutorialSerializeList.BattleObject;
            Instantiate(manager.handSpinePrefab, battle.transform, false);

            battle.GetComponent<Button>().onClick.AddListener(BattleClicked);
        }

        private void BattleClicked() {
            GameObject battle = manager.tutorialSerializeList.BattleObject;
            battle.GetComponent<Button>().onClick.RemoveListener(BattleClicked);
            AccountManager.Instance.RequestQuestProgress(data.id);
        }
    }
}