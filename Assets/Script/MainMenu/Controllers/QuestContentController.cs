using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine.Events;
using Spine.Unity;

namespace Quest {
    public partial class QuestContentController : MonoBehaviour {
        
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private TextMeshProUGUI info;
        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI sliderInfo;
        [SerializeField] private Button getBtn;
        [SerializeField] private Button rerollBtn;
        [SerializeField] private Transform rewardUIParent;
        [SerializeField] public GameObject fakeItem, poolObject;

        [SerializeField] private Transform endPosition;
        [SerializeField] private Transform scrollViewContent;
        [SerializeField] private Button hudBackButton;

        public QuestData data;
        public QuestManager manager;

        GameObject clone;
        private void OnEnable() {
            MakeQuest();

            NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_QUEST_REWARD_RECEIVED, OnRewardReceived);
            NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_QUEST_REFRESHED, OnRerollComplete);
            rerollBtn.onClick.AddListener(RerollQuest);
        }

        private void OnDisable() {
            NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_QUEST_REWARD_RECEIVED, OnRewardReceived);
            NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_QUEST_REFRESHED, OnRerollComplete);
            rerollBtn.onClick.RemoveListener(RerollQuest);
        }


        public void MakeQuest() {
            if (data == null) return;

            if (hudBackButton != null) hudBackButton.enabled = true;
            if (data.questDetail != null) title.text = data.questDetail.name;
            info.text = data.questDetail.desc;
            slider.maxValue = (float)data.questDetail.progMax;
            slider.value = (float)data.progress;
            sliderInfo.text = data.progress.ToString() + "/" + data.questDetail.progMax.ToString();

            getBtn.GetComponent<Image>().material = null;
            getBtn.GetComponent<Image>().color = Color.white;

            var animator = getBtn.transform.parent.GetComponent<Animator>();
            AccountManager accountManager = AccountManager.Instance;
            Fbl_Translator translator = accountManager.GetComponent<Fbl_Translator>();

            TextMeshProUGUI getBtnText = getBtn.GetComponentInChildren<TextMeshProUGUI>();
            getBtnText.GetComponent<FblTextConverter>().SetFont(ref getBtnText, true);

            if (data.cleared) {
                if (!data.rewardGet) {
                    getBtn.enabled = true;

                    getBtnText.text = translator.GetLocalizedText(
                        "MainUI",
                        "ui_page_quest_complete");

                    animator.enabled = true;
                    animator.Play("Glow");
                }
            }
            else {
                getBtn.enabled = false;
                getBtnText.text = translator.GetLocalizedText(
                    "MainUI",
                    "ui_page_quest_inprogress");

                animator.Play("Default");
                animator.enabled = false;
            }

            foreach (Transform slot in rewardUIParent) {
                slot.gameObject.SetActive(false);
            }

            var icons = AccountManager.Instance.resource.rewardIcon;

            for (int i = 0; i < data.questDetail.rewards.Length; i++) {
                rewardUIParent.GetChild(i).gameObject.SetActive(true);
                Image rewardImg = rewardUIParent.GetChild(i).GetChild(0).GetComponent<Image>();
                
                var rewardDescriptionHandler = RewardDescriptionHandler.instance;
                var keyword = data.questDetail.rewards[i].kind;
                
                if (icons.ContainsKey(data.questDetail.rewards[i].kind)) {
                    rewardImg.sprite = icons[data.questDetail.rewards[i].kind];
                    var parent = rewardImg.transform.parent;
                    parent.GetComponent<Button>().onClick.RemoveAllListeners();
                    parent.GetComponent<Button>().onClick.AddListener(() => {
                        rewardDescriptionHandler.RequestDescriptionModal(keyword);
                    });
                }
            }
        }

        public void RerollQuest() {
            AccountManager.Instance.RequestQuestRefresh(data.id, gameObject);
        }

        private void OnRerollComplete(Enum Event_Type, Component Sender, object Param) {
            if (AccountManager.Instance.refreshObj != gameObject) return;
            data = AccountManager.Instance.rerolledQuest;
            AccountManager.Instance.RequestQuestRefreshTime();
            MakeQuest();
        }

        private void OnRewardReceived(Enum Event_Type, Component Sender, object Param) {
            var targetObj = (GameObject)Param;
            if(gameObject != targetObj) return;

            var translator = AccountManager.Instance.GetComponent<Fbl_Translator>();
            string message = translator.GetLocalizedText("UIPopup", "ui_popup_mailsent");
            string okBtn = translator.GetLocalizedText("UIPopup", "ui_popup_check");
            string header = translator.GetLocalizedText("UIPopup", "ui_popup_check");

            //if (data.questDetail.id.CompareTo("t1")== 0) Modal.instantiate(message, Modal.Type.CHECK, GetQuestItem, btnTexts: new string[] { okBtn }, headerText: header);
            //else Modal.instantiate(message, Modal.Type.CHECK, btnTexts: new string[] { okBtn }, headerText: header);
            if (data.questDetail.id.CompareTo("t1")== 0) GetQuestItem();
            //targetObj.GetComponent<QuestContentController>().getBtn.GetComponentInChildren<TextMeshProUGUI>().text = "획득완료";
            //targetObj.GetComponent<QuestContentController>().getBtn.enabled = false;
            gameObject.SetActive(false);
            if(hudBackButton != null) hudBackButton.enabled = true;
        }

        private void GetReward() {
            gameObject.SetActive(false);
        }

        public void RequestRewardButtonClicked() {
            clone = Instantiate(fakeItem, scrollViewContent);
            clone.GetComponent<QuestContentController>().data = data;
            clone.gameObject.SetActive(true);
            clone.transform.localScale = Vector3.one;
            clone.transform.localPosition = transform.localPosition;

            GetComponent<Image>().enabled = false;
            foreach (Transform tf in transform) {
                tf.gameObject.SetActive(false);
            }

            StartCoroutine(StartEffect());
        }

        IEnumerator StartEffect() {
            if(hudBackButton != null) hudBackButton.enabled = false;

            yield return _stampEffect();
            yield return _SlideEffect();
            yield return _ScaleEffect();

            if (data.cleared && !data.rewardGet) {
                AccountManager.Instance.RequestQuestClearReward(data.id, gameObject);
                AccountManager.Instance.RequestQuestInfo();
            }
        }

        IEnumerator _stampEffect() {
            clone.transform.Find("Stamp").gameObject.SetActive(true);

            SkeletonGraphic skeletonGraphic = clone.transform.Find("Stamp").GetComponent<SkeletonGraphic>();
            skeletonGraphic.Initialize(false);
            skeletonGraphic.Update(0);
            skeletonGraphic.AnimationState.SetAnimation(0, "animation", false);
            yield return new WaitForSeconds(0.5f);
        }

        IEnumerator _SlideEffect() {
            float moveTime = 0.3f;
            Vector3 targetPos2 = new Vector3(endPosition.position.x, transform.position.y, 0);
            //yield return new WaitForSeconds(0.2f);
            iTween.MoveTo(clone, iTween.Hash(
                "position", targetPos2,
                "time", moveTime,
                "easetype", iTween.EaseType.easeInBack
            ));
            yield return new WaitForSeconds(moveTime);
            //Destroy(clone.gameObject);
            clone.transform.Find("Stamp").gameObject.SetActive(false);
        }

        IEnumerator _ScaleEffect() {
            var animator = GetComponent<Animator>();
            animator.enabled = true;
            animator.Play("ScaleToZero");
            yield return new WaitForSeconds(0.8f);
            
            Destroy(clone);
            animator.enabled = false;
        }
    }
/* ***********************8
퀘스트 튜토리얼 정리

- 기본
    - 메인화면 Quest버튼에 QuestManager Component 부여
    - 메인화면 QuestCanvas->InnerCanvas->MainPanel->QuestPanel->ViewPort->Content->QuestContent들 마다 QuestContentController Component 부여
    - Resources/TutorialDatas/questData.json 에 퀘스트 관련 튜토리얼 함수와 매개변수 표기
    - RequestQuestInfo -> QuestManager (데이터 가공) ->  QuestContentController (각각 한개의 퀘스트 담기) -> 아이디가 t1~t4인 경우 tutorial 변수에 TutorialJson 담기 -> tutorial 있으면 Reflection으로 해당 함수 실행하기
    - QuestManager의 TutorialSerializeList 클래스는 튜토리얼에 필요한 GameObject들을 담음


- (T1) 0-2 퀘스트 
    - QuestSubSetShow(quest_sub_set_1)
    - QuestIconShow
    - ShowStoryHand(human, 2)
    - QuestClearShow
- (T0) 메일 받기
    - StartMailTutorial
    - FinishMailTutorial
- (T2) 카드 제작하기
    - MenuDictionaryShowHand (ac10055)
    - CreateCardCheck (ac10055)
- (T3) 덱편집하기
    - MenuDeckSettingShowHand
    - DeckSettingRemoveCard (ac10005)
    - DeckSettingAddCard (ac10055)
- (T4) 배틀 진행하기
    - BattleShow


************************/
    public partial class QuestContentController : MonoBehaviour {
        public void ActiveTutorial() {//강제 튜토리얼 완성떄까지 튜토리얼 막기 다른 함수는 참고용으로 냅두기
            // for(int i = 0; i < data.tutorials.Length; i++) {
            //     if(data.tutorials[i].isShowing == true) continue;
            //     MethodInfo theMethod = this.GetType().GetMethod(data.tutorials[i].method);
            //     object[] args = new object[]{data.tutorials[i].args};
            //     Debug.Log(data.tutorials[i].method);
            //     object played = theMethod.Invoke(this, args);
            //     data.tutorials[i].isShowing = (bool)played;
            // }
        }

        public bool QuestSubSetShow(string[] args) {
            if(data.progress > 0) return false;
            Type enumType = typeof(MenuTutorialManager.TutorialType);
            MenuTutorialManager.TutorialType questEnum = (MenuTutorialManager.TutorialType)Enum.Parse(enumType, args[0].ToUpper());
            manager.tutoDialog.StartQuestSubSet(questEnum);
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

        private void GetQuestItem() {
            //manager.tutoDialog.StartQuestSubSet(MenuTutorialManager.TutorialType.QUEST_SUB_SET_3);
            PlayerPrefs.SetInt("FirstTutorialClear", 1);
            PlayerPrefs.Save();
            //manager.TutorialNoQuestShow();
        }

        public bool StartMailTutorial(string[] args) {
            if(data.cleared) return false;
            GetPostOffice();
            if(!manager.tutorialSerializeList.backButton.gameObject.activeInHierarchy) return false;
            AddSpinetoButtonAndRemoveClick(manager.tutorialSerializeList.backButton);
            return true;
        }

        private async void GetPostOffice() {
            await Task.Delay(500);
            manager.tutorialSerializeList.newMail.SetActive(true);
            manager.tutorialSerializeList.mailBoxManager.tutoQuest = new MailBoxManager.TutorialQuest();
            manager.tutorialSerializeList.mailBoxManager.tutoQuest.quest = this;
        }

        public void SubSet4() {
            Transform hand = manager.tutorialSerializeList.mailBoxManager.tutoQuest.receiveBtn.transform.Find("tutorialHand");
            if(hand != null) Destroy(hand.gameObject);
            //manager.tutoDialog.StartQuestSubSet(MenuTutorialManager.TutorialType.QUEST_SUB_SET_4);
        }

        public void BreakCardDictionaryTab() {
            manager.tutorialSerializeList.newMail.SetActive(false);
            manager.tutorialSerializeList.mailBoxManager.tutoQuest.openBtn.enabled = true;
            manager.tutorialSerializeList.mailBoxManager.tutoQuest = null;
            FinishMailTutorial(null);
        }

        public bool FinishMailTutorial(string[] args) {
            if(!data.cleared) return false;
            //manager.tutoDialog.StartQuestSubSet(MenuTutorialManager.TutorialType.QUEST_SUB_SET_9);
            PlayerPrefs.DeleteKey("FirstTutorialClear");
            PlayerPrefs.Save();
            return true;
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
            //manager.tutoDialog.StartQuestSubSet(MenuTutorialManager.TutorialType.QUEST_SUB_SET_5);
        }

        public void DictionaryCardHand(string[] args) {
            if(data.cleared) return;
            CardDictionaryManager cardManager = CardDictionaryManager.cardDictionaryManager;
            //cardManager.cardShowHand(this, args);
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
                BlockerController.blocker.gameObject.SetActive(false);
            }
            //튜토리얼 완료
            MenuSceneController menu = MenuSceneController.menuSceneController;
            CardDictionaryManager card = CardDictionaryManager.cardDictionaryManager;
            menu.DictionaryRemoveHand();
            card.closingToShowEditDeckLock = true;
            AddSpinetoButtonAndRemoveClick(card.transform.Find("UIbar/ExitBtn").GetComponent<Button>());
        }

        public bool MenuDeckSettingShowHand(string[] args) {
            if(data.cleared) return false;
            AddSpinetoButtonAndRemoveClick(manager.tutorialSerializeList.ScrollDeckButton);
            manager.tutorialSerializeList.newDeckMenu.SetActive(true);
            manager.tutorialSerializeList.horizontalScrollSnap.OnSelectionChangeEndEvent.AddListener(x=>{if(x==0) manager.tutorialSerializeList.newDeckMenu.SetActive(false);});
            DeckHandler[] decks = manager.tutorialSerializeList.deckSettingManager.transform.GetComponentsInChildren<DeckHandler>();
            //Array.ForEach(decks, x=> x.TutorialHandShow(this));
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
            //manager.tutoDialog.StartQuestSubSet(MenuTutorialManager.TutorialType.QUEST_SUB_SET_8);
            manager.tutorialSerializeList.newBattleMenu.SetActive(false);
            manager.tutorialSerializeList.BattleButton.GetComponent<Button>().onClick.RemoveListener(BattleClicked);
            manager.tutorialSerializeList.modeGlow.SetActive(false);
            //AccountManager.Instance.RequestQuestProgress(data.id);
        }
    }
}