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
    public class QuestContentController : MonoBehaviour {
        
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private TextMeshProUGUI info;
        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI sliderInfo;
        [SerializeField] private Button getBtn;
        [SerializeField] private Button rerollBtn;
        [SerializeField] private Transform rewardUIParent;

        public QuestData data;
        public QuestManager manager;
        
        private void Start() {
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

            targetObj.GetComponent<QuestContentController>().getBtn.GetComponentInChildren<TextMeshProUGUI>().text = "획득완료";
            targetObj.GetComponent<QuestContentController>().getBtn.enabled = false;
        }

        private void GetReward() {
            gameObject.SetActive(false);
        }

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
        }

        public void StoryCleared(string[] args) {
            if(!data.cleared) return;
            bool isBoxGet = AccountManager.Instance.userData.etcInfo.Exists(x=>x.key.CompareTo("tutorialBox")==0);
            if(isBoxGet) return;
            Type enumType = typeof(MenuTutorialManager.TutorialType);
            MenuTutorialManager.TutorialType questEnum = (MenuTutorialManager.TutorialType)Enum.Parse(enumType, args[0].ToUpper());
            manager.tutoDialog.StartQuestSubSet(questEnum);
        }

        /// <summary>
        /// 메뉴 화면 카드 도감 휴먼 영웅에 손가락 표시
        /// </summary>
        /// <param name="args"></param>
        public void MenuDictionaryShowHand(string[] args) {
            if(data.cleared) return;
            MenuSceneController menu = MenuSceneController.menuSceneController;
            menu.DictionaryShowHand(this, args);
        }

        /// <summary>
        /// 도감 메뉴 해당 카드에 손가락 표시
        /// cardManager.cardShowHand 함수 별개로 만들어서 거기 안에 손가락 생성 별도로 만들게 하기
        /// </summary>
        /// <param name="args">해당 카드 id</param>
        public void DictionaryCardHand(string[] args) {
            if(data.cleared) return;
            CardDictionaryManager cardManager = CardDictionaryManager.cardDictionaryManager;
            cardManager.cardShowHand(this, args);
        }

        /// <summary>
        /// 카드 생성 됐을 때 확인 하는 용도
        /// </summary>
        /// <param name="args">해당 카드 id</param>
        public void CreateCardCheck(string[] args) {
            if(data.cleared) return;
            OnEvent theEvent = null;
            theEvent = (type, Sender, Param) => {
                var card = Array.Find(AccountManager.Instance.myCards, x=>x.cardId.CompareTo(args[0])==0);
                if(card == null) return;
                Start();
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
            manager.tutoDialog.StartQuestSubSet(MenuTutorialManager.TutorialType.QUEST_SUB_SET_4);
            AccountManager.Instance.RequestUnlockInTutorial(3);
            AccountManager.Instance.RequestQuestInfo();
        }

        /// <summary>
        /// 받기 버튼 클릭
        /// </summary>
        public void RequestRewardButtonClicked() {
            if (data.cleared && !data.rewardGet) {
                AccountManager.Instance.RequestQuestClearReward(data.id, gameObject);
            }
        }

        public void MenuDeckSettingShowHand(string[] args) {
            if(data.cleared) return;
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