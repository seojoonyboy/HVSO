using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;
using Spine.Unity;
using BestHTTP;
using dataModules;
using Spine;
using UnityEngine.UI.Extensions;
using System.Threading.Tasks;
using System.Linq;

namespace MenuTutorialModules {
    public class MenuExecute : MonoBehaviour {
        public MenuExecuteHandler handler;
        public HandUIController HandUIController;

        public List<string> args;

        public virtual void Initialize(List<string> args) {
            this.args = args;
            handler = GetComponent<MenuExecuteHandler>();
            HandUIController = GetComponentInChildren<HandUIController>();
        }

        public virtual void Execute() { }

        void OnDestroy() {
            StopAllCoroutines();
        }
    }

    public class Wait_Click : MenuExecute {
        public Wait_Click() : base() { }

        IDisposable clickStream;

        //args[0] screen 또는 Dictionary 키값, args[1] 예비
        public override void Execute() {
            GameObject target = null;

            if (args[0] == "screen")
                target = null;
            else if (args.Count > 1)
                target = MenuMask.Instance.GetMenuObject(args[0], args[1]);
            else
                target = MenuMask.Instance.GetMenuObject(args[0]);

            Button button = (target != null) ? target.GetComponent<Button>() : null;

            //if (button != null)
            //    ShowTouchIcon(button.transform.gameObject);

            clickStream = (button != null) ? button.OnClickAsObservable().Subscribe(_ => CheckButton()) : Observable.EveryUpdate().Where(_ => Input.GetMouseButtonDown(0)).Subscribe(_ => CheckClick(target));
        }

        private void CheckClick(GameObject target) {
            if (target == null) {
                clickStream.Dispose();
                handler.isDone = true;
            }
        }

        private void CheckButton() {
            clickStream.Dispose();
            //MenuGlow.Instance.StopEveryGlow();
            handler.isDone = true;
        }

        private void ShowTouchIcon(GameObject targetButton) {
            GameObject handIcon = Instantiate(AccountManager.Instance.resource.touchIcon, targetButton.transform);
            handIcon.name = "handIcon";
            handIcon.transform.position = targetButton.transform.position;
            SkeletonGraphic touchIcon = handIcon.GetComponent<SkeletonGraphic>();
            touchIcon.Skeleton.SetSlotsToSetupPose();
            touchIcon.Initialize(true);
            touchIcon.Update(0);
            touchIcon.AnimationState.SetAnimation(0, "TOUCH", false);
        }

        private void HideTouchIcon(Button button) {
            GameObject handIcon = button.gameObject.transform.Find("handIcon").gameObject;
            Destroy(handIcon);
        }
    }

    //args[0] screen 또는 Dictionary 키값, args[1] 예비 wait_click이랑 똑같이.
    public class Menu_Glowing : MenuExecute {
        public Menu_Glowing() : base() { }

        public override void Execute() {
            GameObject target = null;

            if (args[0] == "screen")
                target = null;
            else if (args.Count > 1)
                target = MenuMask.Instance.GetMenuObject(args[0], args[1]);
            else
                target = MenuMask.Instance.GetMenuObject(args[0]);

            if (target != null)
                StartGlow(target);
        }

        public void StartGlow(GameObject target) {
            //Logger.Log("Glow Target : " + target);
            //MenuGlow.Instance.StartGlow(target);
        }
    } 

    public class Stop_Menu_Glowing : MenuExecute {
        public Stop_Menu_Glowing() : base() { }

        public override void Execute() {
            //MenuGlow.Instance.StopEveryGlow();
        }
    }

    public class Menu_NPC_Talk : MenuExecute {
        public Menu_NPC_Talk() : base() { }

        //args[0] 유닛 id, args[1] 대사내용, args[2] my,enemy
        public override void Execute() {
            MenuMask menuMask = MenuMask.Instance;
            MenuMask.Instance.gameObject.SetActive(true);
            menuMask.menuTalkPanel.SetActive(true);

            Transform CharacterImage = menuMask.menuTalkPanel.transform.Find("CharacterImage");
            Vector3 prevCharacterImagePos = CharacterImage.localPosition;
            Transform MainText = menuMask.menuTalkPanel.transform.Find("MainText");
            Vector3 prevMainTextPos = MainText.localPosition;
            Transform NameObject = menuMask.menuTalkPanel.transform.Find("NameObject");
            Vector3 prevNameObjectPos = NameObject.localPosition;

            Vector3 originPos = menuMask.menuTalkPanel.transform.Find("OriginPos").localPosition;
            Vector3 originNameObjectPos = menuMask.menuTalkPanel.transform.Find("OriginPosNameObject").localPosition;
            menuMask.menuTalkPanel.transform.parent.GetComponent<Canvas>().sortingOrder = 83;

            if (args.Count == 4) {
                int imgIndex = 0;
                int.TryParse(args[3], out imgIndex);
                var tutorialHelpImages = GetComponent<MenuTutorialManager>().tutorialHelpImages;
                menuMask.menuTalkPanel.transform.Find("HelperImage").gameObject.SetActive(true);
                menuMask.menuTalkPanel.transform.Find("HelperImage/Image").GetComponent<Image>().sprite = tutorialHelpImages[imgIndex];
            }
            else {
                menuMask.menuTalkPanel.transform.Find("HelperImage").gameObject.SetActive(false);
            }

            bool isPlayer = args[2] != "enemy";
            menuMask.menuTalkPanel.transform.Find("CharacterImage/Player").gameObject.SetActive(isPlayer);
            menuMask.menuTalkPanel.transform.Find("CharacterImage/Enemy").gameObject.SetActive(!isPlayer);
            menuMask.menuTalkPanel.transform.Find("NameObject/PlayerName").gameObject.SetActive(isPlayer);
            menuMask.menuTalkPanel.transform.Find("NameObject/EnemyName").gameObject.SetActive(!isPlayer);
            if (isPlayer) {
                //Logger.Log(args[0]);
                menuMask.menuTalkPanel.transform.Find("CharacterImage/Player").GetComponent<Image>().sprite = AccountManager.Instance.resource.ScenarioUnitResource[args[0]].sprite;
                menuMask.menuTalkPanel.transform.Find("NameObject/PlayerName").GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.resource.ScenarioUnitResource[args[0]].name;
            }
            else {
                menuMask.menuTalkPanel.transform.Find("CharacterImage/Enemy").GetComponent<Image>().sprite = AccountManager.Instance.resource.ScenarioUnitResource[args[0]].sprite;
                menuMask.menuTalkPanel.transform.Find("NameObject/EnemyName").GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.resource.ScenarioUnitResource[args[0]].name;
            }

            string convertedText = AccountManager
                .Instance
                .GetComponent<Fbl_Translator>()
                .GetLocalizedText("MainTutorial", args[1]);

            if (convertedText == null) Logger.Log(args[1] + "에 대한 번역을 찾을 수 없습니다!");

            menuMask.menuTalkPanel.GetComponent<TextTyping>().StartTyping(convertedText, handler);
            menuMask.menuTalkPanel.transform.Find("StopTypingTrigger").gameObject.SetActive(true);
        }
    }

    public class Menu_Scope_Object : MenuExecute {
        public Menu_Scope_Object() : base() { }

        //args[0] dictionary 키값
        public override void Execute() {
            GameObject target;

            target = MenuMask.Instance.GetMenuObject(args[0]);
            if (target != null)
                MenuMask.Instance.ScopeMenuObject(target);

            handler.isDone = true;
        }
    }

    public class Menu_Block_Screen : MenuExecute {
        public Menu_Block_Screen() : base() { }

        // 단순한 화면 막기
        public override void Execute() {
            MenuMask.Instance.BlockScreen();
            handler.isDone = true;
        }
    }

    public class Wait_Until : MenuExecute {
        public Wait_Until() : base() { }

        //몇초간 대기
        public override void Execute() {
            MenuMask.Instance.BlockWithTransparent();
            var parms = args;
            float sec = 0;
            float.TryParse(parms[0], out sec);
            StartCoroutine(WaitSec(sec));
        }

        IEnumerator WaitSec(float sec = 0) {
            yield return new WaitForSeconds(sec);
            MenuMask.Instance.ResetTransparentMask();
            handler.isDone = true;
        }
    }

    public class Menu_Hide_Message : MenuExecute {
        public Menu_Hide_Message() : base() { }

        public override void Execute() {
            MenuMask.Instance.HideText();
            handler.isDone = true;
        }
    }

    public class Highlight : MenuExecute {
        //버튼 하이라이트
        public override void Execute() {
            handler.isDone = true;
        }
    }

    public class OffHighlight : MenuExecute {
        public override void Execute() {
            MenuMask.Instance.UnBlockScreen();
            handler.isDone = true;
        }
    }

    public class Dimmed : MenuExecute {
        public override void Execute() {
            var menuMask = MenuMask.Instance;
            
            string objectName = args[0];
            //Logger.Log(objectName);

            var targetObject = menuMask.GetMenuObject(objectName);

            menuMask.UnBlockScreen();
            switch (args[1]) {
                case "on":
                    if(args.Count > 2) {
                        if(objectName == "orc_story_tutorial_1" && args[2] == "scrollArea") {
                            GameObject clone = Instantiate(targetObject);
                            clone.name = objectName;
                            clone.transform.SetParent(targetObject.transform.parent, true);
                            clone.transform.localScale = Vector3.one;
                            clone.transform.SetAsFirstSibling();
                            clone.transform.Find("New_Image").gameObject.SetActive(true);
                        }
                        else if(objectName == "human_story_tutorial_1" && args[2] == "scrollArea") {
                            GameObject clone = Instantiate(targetObject);
                            clone.name = objectName;
                            clone.transform.SetParent(targetObject.transform.parent, true);
                            clone.transform.localScale = Vector3.one;
                            clone.transform.SetSiblingIndex(1);
                            clone.transform.Find("New_Image").gameObject.SetActive(true);
                        }
                        else if(objectName == "orc_story_tutorial_2" && args[2] == "scrollArea") {
                            GameObject clone = Instantiate(targetObject);
                            clone.name = objectName;
                            clone.transform.SetParent(targetObject.transform.parent, true);
                            clone.transform.localScale = Vector3.one;
                            clone.transform.SetSiblingIndex(2);
                            clone.transform.Find("New_Image").gameObject.SetActive(true);
                        }
                        else if(objectName == "human_story_tutorial_2" && args[2] == "scrollArea") {
                            GameObject clone = Instantiate(targetObject);
                            clone.name = objectName;
                            clone.transform.SetParent(targetObject.transform.parent, true);
                            clone.transform.localScale = Vector3.one;
                            clone.transform.SetSiblingIndex(2);
                            clone.transform.Find("New_Image").gameObject.SetActive(true);
                        }
                        else if(objectName == "ModeButton") {
                            targetObject = MenuMask.Instance.transform.Find("StoryFakeImg").gameObject;
                            targetObject.gameObject.SetActive(true);
                            MenuMask.Instance.transform.Find("Dimmed").GetComponent<Image>().raycastTarget = false;

                            MenuMask.Instance.GetMenuObject("StoryModeSpine").gameObject.SetActive(false);
                            MenuMask.Instance.GetMenuObject("BattleSpine").gameObject.SetActive(false);
                        }
                    }
                    menuMask.OnDimmed(targetObject.transform.parent, targetObject);
                    break;
                case "off":
                    if (args.Count > 2) {
                        menuMask.OffDimmed(targetObject, objectName);

                        //추가 처리
                        if (objectName == "ModeButton") {
                            MenuMask.Instance.transform.Find("Dimmed").GetComponent<Image>().raycastTarget = true;
                            MenuMask.Instance.GetMenuObject("StoryFakeImg").gameObject.SetActive(false);
                        }
                    }
                    else {
                        menuMask.OffDimmed(targetObject);
                    }
                    break;
            }
            handler.isDone = true;
        }
    }

    public class Dimmed2 : MenuExecute {
        public override void Execute() {
            var menuMask = MenuMask.Instance;
            string objectName = args[0];
            var targetObject = menuMask.GetMenuObject(objectName);
            if(targetObject == null) { Logger.LogError(objectName + "을 찾을 수 없음!"); }
            if (args[1] == "on")
                BlockerController.blocker.SetBlocker(targetObject);
            else
                BlockerController.blocker.gameObject.SetActive(false);

            handler.isDone = true;
        }
    }

    public class ChangeSelectBtnImage : MenuExecute {
        public override void Execute() {
            string target = args[0];

            switch (target) {
                case "story_orc_btn":
                    var orcButton = MenuMask.Instance.GetMenuObject("story_orc_button");
                    orcButton.GetComponent<Image>().sprite = GetComponent<MenuTutorialManager>().scenarioManager.orc.activeSprite;
                    break;
            }

            handler.isDone = true;
        }
    }

    public class AutoSelectStoryDeck : MenuExecute {
        public override void Execute() {
            ScenarioManager scenarioManager = GetComponent<MenuTutorialManager>().scenarioManager;

            int chapterNum = 0;
            int.TryParse(args[1], out chapterNum);
            chapterNum -= 1;
            if (args[0] == "human") {
                ScenarioGameManagment.chapterData = scenarioManager.human_chapterDatas[chapterNum];
                ScenarioGameManagment.challengeDatas = scenarioManager.human_challengeDatas[chapterNum].challenges;
                string heroId = "h10001";
                switch (chapterNum) {
                    default:
                    case 1:
                    case 2:
                        heroId = "h10001";
                        break;
                }
                PlayerPrefs.SetString("selectedHeroId", heroId);
            }
            else if(args[0] == "orc") {
                ScenarioGameManagment.chapterData = scenarioManager.orc_chapterDatas[chapterNum];
                ScenarioGameManagment.challengeDatas = scenarioManager.orc_challengeDatas[chapterNum].challenges;

                string heroId = "h10002";
                switch (chapterNum) {
                    default:
                    case 1:
                    case 2:
                        heroId = "h10002";
                        break;
                }
                PlayerPrefs.SetString("selectedHeroId", heroId);
            }

            scenarioManager.stageCanvas
                    .transform
                    .Find("DeckSelectPanel/StagePanel/Scroll View/Viewport/Content")
                    .GetChild(0)
                    .Find("Deck")
                    .GetComponent<Button>().onClick.Invoke();
            handler.isDone = true;
        }
    }

    public class RequestQuestBoxReward : MenuExecute {
        public override void Execute() {
            AccountManager.Instance.RequestTutorialBox(callback);
        }

        private void callback(HTTPRequest originalRequest, HTTPResponse response) {
            if (response.DataAsText.Contains("already")) {
                handler.isDone = true;
            }
            else {
                GetComponent<MenuTutorialManager>().BoxRewardPanel.transform.Find("ExitButton").GetComponent<Button>().onClick.AddListener(OnClick);
                GetComponent<MenuTutorialManager>().menuTextCanvas.SetActive(false);
            }
        }

        private void OnClick() {
            GetComponent<MenuTutorialManager>().BoxRewardPanel.transform.Find("ExitButton").GetComponent<Button>().onClick.RemoveListener(OnClick);
            GetComponent<MenuTutorialManager>().menuTextCanvas.SetActive(true);
            handler.isDone = true;
        }
    }

    public class RequestReward : MenuExecute {
        IDisposable clickStream;
        IEnumerator coroutine;
        public override void Execute() {
            string camp = args[0];
            AccountManager.Instance.RequestIngameTutorialReward(ReqCallback, camp);
        }

        private void ReqCallback(HTTPRequest originalRequest, HTTPResponse response) {
            var resText = response.DataAsText;
            Response _res = dataModules.JsonReader.Read<Response>(resText);
            if (!string.IsNullOrEmpty(_res.claimComplete)) {
                //보상 이펙트 보여주기
                coroutine = Proceed();
                StartCoroutine(coroutine);
            }
            else {
                handler.isDone = true;
            }
        }

        IEnumerator Proceed() {
            GameObject target = null;
            
            NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_DECKS_UPDATED, OnDecksUpdated);
            GetComponent<MenuTutorialManager>().ActiveRewardPanel();

            SkeletonGraphic skeletonGraphic = GetComponent<MenuTutorialManager>().rewardPanel.transform.Find("Anim").GetComponent<SkeletonGraphic>();

            skeletonGraphic.Initialize(true);

            skeletonGraphic.Skeleton.SetSkin(args[0]);
            skeletonGraphic.Skeleton.SetSlotsToSetupPose();

            yield return new WaitForEndOfFrame();
            skeletonGraphic.transform.parent.Find("SubBackground").gameObject.SetActive(false);
            skeletonGraphic.AnimationState.SetAnimation(0, "sampledeck", false);

            yield return new WaitForSeconds(0.8f);

            string convertedHeaderText = AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText("MainTutorialUI", "txt_ui_tuto_deckacquired");
            skeletonGraphic.transform.Find("Header/Text").GetComponent<TMPro.TextMeshProUGUI>().text = convertedHeaderText;

            if (args[0] == "human") {
                string convertedText = AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText("MainTutorialUI", "txt_stageselect_tuto_acqdeck_human");
                skeletonGraphic.transform.Find("Description/Text").GetComponent<TMPro.TextMeshProUGUI>().text = convertedText;
            }
            else {
                string convertedText = AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText("MainTutorialUI", "txt_stageselect_tuto_acqdeck_orc");
                skeletonGraphic.transform.Find("Description/Text").GetComponent<TMPro.TextMeshProUGUI>().text = convertedText;
            }

            yield return new WaitForSeconds(1.0f);

            clickStream = Observable.EveryUpdate()
                .Where(_ => Input.GetMouseButtonDown(0))
                .Subscribe(_ => CheckClick(target));

            AccountManager.Instance.RequestUserInfo();
            AccountManager.Instance.RequestMyDecks();
            AccountManager.Instance.RequestInventories();
        }

        private void OnDecksUpdated(Enum Event_Type, Component Sender, object Param) {
            HTTPResponse res = (HTTPResponse)Param;
            if (res != null) {
                if (res.StatusCode == 200 || res.StatusCode == 304) {
                    var result = JsonReader.Read<Decks>(res.DataAsText);
                    AccountManager.Instance.orcDecks = result.orc;
                    AccountManager.Instance.humanDecks = result.human;
                }
            }
            else {
                Logger.Log("Something is wrong");
            }
        }

        private void CheckClick(GameObject target) {
            if (target == null) {
                GetComponent<MenuTutorialManager>().DeactiveRewardPanel();
                clickStream.Dispose();
                handler.isDone = true;
            }
        }

        public class Response {
            public string claimComplete;
            public string error;
        }

        void OnDestroy() {
            StopAllCoroutines();
            NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_DECKS_UPDATED, OnDecksUpdated);
        }
    }

    public class DestroyLoadingModal : MenuExecute {
        public override void Execute() {
            var loadingModal = GetComponent<MenuTutorialManager>().menuSceneController.hideModal;
            loadingModal.SetActive(false);
            handler.isDone = true;
        }
    }

    /// <summary>
    /// 오크 스토리 해금
    /// </summary>
    public class UnlockOrcAnim : MenuExecute {
        IDisposable clickStream;
        IEnumerator coroutine;

        public override void Execute() {
            coroutine = Proceed();
            StartCoroutine(coroutine);
        }

        IEnumerator Proceed() {
            GameObject target = null;

            GetComponent<MenuTutorialManager>().ActiveRewardPanel();
            SkeletonGraphic skeletonGraphic = GetComponent<MenuTutorialManager>().rewardPanel.transform.Find("Anim").GetComponent<SkeletonGraphic>();

            skeletonGraphic.Initialize(true);

            skeletonGraphic.Skeleton.SetSkin("orc");
            skeletonGraphic.Skeleton.SetSlotsToSetupPose();

            yield return new WaitForEndOfFrame();
            skeletonGraphic.transform.parent.Find("SubBackground").gameObject.SetActive(false);
            skeletonGraphic.AnimationState.SetAnimation(0, "story_details", false);

            var fbl_translator = AccountManager.Instance.GetComponent<Fbl_Translator>();

            string headerText = fbl_translator.GetLocalizedText("MainTutorialUI", "txt_ui_tuto__orcstoryunlock");
            string descText = fbl_translator.GetLocalizedText("MainTutorialUI", "txt_stageselect_tuto_openorcstory01");
            skeletonGraphic.transform.Find("Header/Text").GetComponent<TMPro.TextMeshProUGUI>().text = headerText;
            skeletonGraphic.transform.Find("Description/Text").GetComponent<TMPro.TextMeshProUGUI>().text = descText;

            yield return new WaitForSeconds(1.0f);

            clickStream = Observable.EveryUpdate()
                .Where(_ => Input.GetMouseButtonDown(0))
                .Subscribe(_ => CheckClick(target));
        }

        private void CheckClick(GameObject target) {
            if (target == null) {
                GetComponent<MenuTutorialManager>().DeactiveRewardPanel();
                clickStream.Dispose();
                handler.isDone = true;
            }
        }
    }

    /// <summary>
    /// 오크 진영 튜토리얼이 개방 되었습니다!
    /// </summary>
    public class UnlockOrcStoryAnim : MenuExecute {
        IDisposable clickStream;
        IEnumerator coroutine;

        public override void Execute() {
            coroutine = Proceed();
            StartCoroutine(coroutine);
        }

        IEnumerator Proceed() {
            GameObject target = null;

            GetComponent<MenuTutorialManager>().ActiveRewardPanel();
            SkeletonGraphic skeletonGraphic = GetComponent<MenuTutorialManager>().rewardPanel.transform.Find("Anim").GetComponent<SkeletonGraphic>();

            skeletonGraphic.Initialize(true);

            skeletonGraphic.Skeleton.SetSkin("orc");
            skeletonGraphic.Skeleton.SetSlotsToSetupPose();
            skeletonGraphic.transform.Find("Header").GetComponent<BoneFollowerGraphic>().SetBone("text1");

            yield return new WaitForEndOfFrame();
            skeletonGraphic.transform.parent.Find("SubBackground").gameObject.SetActive(false);
            skeletonGraphic.AnimationState.SetAnimation(0, "story_reward1", false);

            skeletonGraphic.transform.Find("Header/Text").GetComponent<TMPro.TextMeshProUGUI>().text = "오크 스토리 개방";
            skeletonGraphic.transform.Find("Description/Text").GetComponent<TMPro.TextMeshProUGUI>().text = "오크 진영 스토리가 개방 되었습니다!";

            yield return new WaitForSeconds(1.0f);

            clickStream = Observable.EveryUpdate()
                .Where(_ => Input.GetMouseButtonDown(0))
                .Subscribe(_ => CheckClick(target));
        }

        private void CheckClick(GameObject target) {
            if (target == null) {
                GetComponent<MenuTutorialManager>().DeactiveRewardPanel();
                clickStream.Dispose();
                handler.isDone = true;
            }
        }
    }

    public class ShowNewImage : MenuExecute {
        public override void Execute() {
            GameObject target = MenuMask.Instance.GetMenuObject(args[0]);
            target.SetActive(true);

            handler.isDone = true;
        }
    }

    public class HideNewImage : MenuExecute {
        public override void Execute() {
            GameObject target = MenuMask.Instance.GetMenuObject(args[0]);
            target.SetActive(false);

            handler.isDone = true;
        }
    }

    public class ShowHandUI : MenuExecute {
        public override void Execute() {
            GameObject target = null;
            if(args.Count > 2) {
                foreach(Transform child in MenuMask.Instance.transform.Find("Dimmed")) {
                    if(child.name == args[0]) {
                        target = child.gameObject;
                    }
                }
            }
            else {
                target = MenuMask.Instance.GetMenuObject(args[0]);
            }

            string animType = args[1];
            GameObject handUI = HandUIController.ActiveHand(target.GetComponent<RectTransform>(), args[0]);
            SkeletonGraphic skeletonGraphic = handUI.GetComponent<SkeletonGraphic>();
            skeletonGraphic.Initialize(true);
            skeletonGraphic.Update(0);
            skeletonGraphic.Skeleton.SetSlotsToSetupPose();

            switch (animType) {
                case "touch":
                    skeletonGraphic.AnimationState.SetAnimation(0, "TOUCH", true);
                    break;
                case "drag":
                    skeletonGraphic.AnimationState.SetAnimation(0, "DRAG", true);
                    break;
            }
            handler.isDone = true;
        }
    }

    public class HideHandUI : MenuExecute {
        public override void Execute() {
            HandUIController.DeactiveHand(args[0]);

            handler.isDone = true;
        }
    }

    public class ResetBtnImage : MenuExecute {
        public override void Execute() {
            string target = args[0];

            switch (target) {
                case "story_orc_btn":
                    var orcButton = MenuMask.Instance.GetMenuObject("story_orc_button");
                    orcButton.GetComponent<Image>().sprite = GetComponent<MenuTutorialManager>().scenarioManager.orc.deactiveSprite;
                    break;
            }
            handler.isDone = true;
        }
    }
    public class ForceOrcStorySocketConnect : MenuExecute {
        public override void Execute() {
            string stageNum = args[1];

            PlayerPrefs.SetString("SelectedRace", "orc");
            PlayerPrefs.SetString("SelectedBattleType", "story");
            PlayerPrefs.SetString("StageNum", stageNum);

            handler.isDone = true;
            FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.CONNECT_MATCHING_SCENE);
        }
    }

    public class ForceHumanStorySocketConnect : MenuExecute {
        public override void Execute() {
            string stageNum = args[1];

            PlayerPrefs.SetString("SelectedRace", "human");
            PlayerPrefs.SetString("SelectedBattleType", "story");
            PlayerPrefs.SetString("StageNum", stageNum);

            handler.isDone = true;
            FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.CONNECT_MATCHING_SCENE);
        }
    }

    public class MainMenuButtonsGlow : MenuExecute {
        public override void Execute() {
            GetComponent<MenuTutorialManager>().FixedMenuCanvas.SetActive(false);

            var glowCanvas = transform.Find("MainMenuGlowCanvas");
            glowCanvas.gameObject.SetActive(true);
            var bottomPanel = glowCanvas.Find("InnerCanvas/BottomPanel");
            MenuMask.Instance.transform.Find("Dimmed").gameObject.SetActive(true);
            for(int i=0; i<5; i++) {
                bottomPanel.GetChild(i).Find("Glow").gameObject.SetActive(true);
            }
            handler.isDone = true;
        }
    }

    public class OffMainMenuButtonsGlow : MenuExecute {
        public override void Execute() {
            if (!GetComponent<MenuTutorialManager>().FixedMenuCanvas.activeSelf) {
                GetComponent<MenuTutorialManager>().FixedMenuCanvas.SetActive(true);
            }
            var glowCanvas = transform.Find("MainMenuGlowCanvas");
            var bottomPanel = glowCanvas.Find("InnerCanvas/BottomPanel");
            MenuMask.Instance.transform.Find("Dimmed").gameObject.SetActive(false);

            for (int i = 0; i < 5; i++) {
                bottomPanel.GetChild(i).Find("Glow").gameObject.SetActive(false);
            }
            glowCanvas.gameObject.SetActive(false);
            handler.isDone = true;
            glowCanvas.Find("InnerCanvas/Dimmed").gameObject.SetActive(false);
        }
    }

    public class MainMenuButtonGlow : MenuExecute {
        public override void Execute() {
            GetComponent<MenuTutorialManager>().FixedMenuCanvas.SetActive(false);
            //Logger.Log("MainMenuButtonGlow : " + args[0]);
            string buttonName = args[0];
            int index = 0;
            switch (buttonName) {
                case "Card":
                    index = 0;
                    break;
                case "DeckEdit":
                    index = 1;
                    break;
                case "League":
                    index = 2;
                    break;
                case "Store":
                    index = 3;
                    break;
                case "Guild":
                    index = 4;
                    break;
            }

            var glowCanvas = transform.Find("MainMenuGlowCanvas");
            glowCanvas.gameObject.SetActive(true);
            var bottomPanel = glowCanvas.Find("InnerCanvas/BottomPanel");
            for (int i = 0; i < 5; i++) {
                if(index == i) {
                    bottomPanel.GetChild(i).Find("Text").gameObject.SetActive(true);
                    bottomPanel.GetChild(i).GetComponent<Image>().color = new Color32(255, 255, 255, 255);
                    bottomPanel.GetChild(i).Find("Image").GetComponent<Image>().color = new Color32(255, 255, 255, 255);
                    continue;
                }
                bottomPanel.GetChild(i).Find("Text").gameObject.SetActive(false);
                bottomPanel.GetChild(i).Find("Glow").gameObject.SetActive(false);
                bottomPanel.GetChild(i).GetComponent<Image>().color = new Color32(34, 34, 34, 255);
                bottomPanel.GetChild(i).Find("Image").GetComponent<Image>().color = new Color32(34, 34, 34, 255);
            }
            glowCanvas.Find("InnerCanvas/Dimmed").gameObject.SetActive(true);
            bottomPanel.GetChild(index).Find("Glow").gameObject.SetActive(true);
            handler.isDone = true;
        }
    }

    public class EndTutorial : MenuExecute {
        public override void Execute() {
            MenuMask.Instance.UnBlockScreen();

            try {
                GetComponent<MenuTutorialManager>().EndTutorial();
                GetComponent<MenuTutorialManager>().enabled = false;
            }
            catch(Exception ex) { }
            handler.isDone = true;
        }
    }

    public class StartNextTutorial : MenuExecute {
        public override void Execute() {
            var enum_val = (MenuTutorialManager.TutorialType)Enum.Parse(typeof(MenuTutorialManager.TutorialType), args[0]);
            GetComponent<MenuTutorialManager>().MainSceneStateHandler.SetMilestone(MainSceneStateHandler.MilestoneType.QUEST, enum_val);
            GetComponent<MenuTutorialManager>().StartQuestSubSet(enum_val);
            handler.isDone = true;
        }
    }

    public class ForceToPage : MenuExecute {
        public override void Execute() {
            string pageName = args[0];

            switch (pageName) {
                case "StoryLobby":
                    GetComponent<MenuTutorialManager>().scenarioManager.gameObject.SetActive(true);
                    break;
            }

            handler.isDone = true;
        }
    }

    public class ForceToStory : MenuExecute {
        public override void Execute() {
            GetComponent<MenuTutorialManager>().scenarioManager.gameObject.SetActive(true);
            handler.isDone = true;
        }
    }

    public class ForceToBattleReady : MenuExecute {
        public override void Execute() {
            var needToReturnBattleReadyScene = AccountManager.Instance.needToReturnBattleReadyScene;

            if (needToReturnBattleReadyScene) {
                GetComponent<MenuTutorialManager>().BattleReadydeckListPanel.transform.root.gameObject.SetActive(true);
                handler.isDone = true;
            }
            else {
                handler.isDone = true;
            }
        }
    }

    public class UnlockCardMenu : MenuExecute {
        public override void Execute() {
            AccountManager.Instance.RequestUnlockInTutorial(3);

            var menuLockController = GetComponent<MenuTutorialManager>().lockController;
            NewAlertManager
                .Instance
                .SetUpButtonToAlert(
                    menuLockController.GetMenu("Dictionary"),
                    NewAlertManager.ButtonName.DICTIONARY
                );

            handler.isDone = true;
        }
    }

    public class RequestUnlockQuest : MenuExecute {
        public override void Execute() {
            int id = -1;
            int.TryParse(args[0], out id);
            if(id == -1) {
                handler.isDone = true;
                return;
            }
            AccountManager.Instance.RequestUnlockInTutorial(id);
            SetUpButtonToAlert(id);

            handler.isDone = true;
        }

        /// <summary>
        /// TODO : App을 종료하거나, 다른 Scene으로 이동하는 경우 느낌표 정보를 유지할 필요가 있음.
        /// </summary>
        /// <param name="id"></param>
        private void SetUpButtonToAlert(int id) {
            NewAlertManager newAlertManager = NewAlertManager.Instance;
            MenuLockController menuLockController = GetComponent<MenuTutorialManager>().lockController;
            switch (id) {
                case 3:
                    newAlertManager
                        .SetUpButtonToAlert(
                            menuLockController.GetMenu("Dictionary"),
                            NewAlertManager.ButtonName.DICTIONARY
                        );
                    break;
                case 5:
                    newAlertManager
                        .SetUpButtonToAlert(
                            menuLockController.GetMenu("DeckEdit"),
                            NewAlertManager.ButtonName.DECK_EDIT
                        );
                    break;
                case 9:
                    newAlertManager
                        .SetUpButtonToAlert(
                            menuLockController.GetMenu("Mode"),
                            NewAlertManager.ButtonName.MODE
                        );
                    break;
            }
        }
    }

    public class SetPlayerPrefabStoryUnlocked : MenuExecute {
        public override void Execute() {
            PlayerPrefs.SetString("StoryUnlocked", "true");
            handler.isDone = true;
        }
    }

    public class UnlockRewardBoxMenu : MenuExecute {
        public override void Execute() {
            StartCoroutine(Proceed());
        }

        IEnumerator Proceed() {
            yield return new WaitForSeconds(0.2f);
            MenuMask.Instance.HideText();

            MenuLockController menuLockController = GetComponent<MenuTutorialManager>().lockController;
            HorizontalScrollSnap horizontalScrollSnap = GetComponent<MenuTutorialManager>().scrollSnap;
            //horizontalScrollSnap.GoToScreen()

            var lockObj = menuLockController.FindButtonLockObject("RewardBox");
            if (lockObj.activeInHierarchy) {
                SkeletonGraphic skeletonGraphic = lockObj.GetComponent<SkeletonGraphic>();

                AccountManager.Instance.RequestUnlockInTutorial(7);
                menuLockController.Unlock("RewardBox");
                yield return new WaitForSeconds(1.5f);
                handler.isDone = true;
                //skeletonGraphic.AnimationState.End += delegate (TrackEntry trackEntry) {
                //    handler.isDone = true;
                //};
            }
            else {
                handler.isDone = true;
            }
        }

        void OnDestroy() {
            StopAllCoroutines();
        }
    }

    public class UnlockShopMenu : MenuExecute {
        public override void Execute() {
            StartCoroutine(Proceed());
        }

        IEnumerator Proceed() {
            yield return new WaitForSeconds(0.2f);
            MenuMask.Instance.HideText();

            MenuLockController menuLockController = GetComponent<MenuTutorialManager>().lockController;
            HorizontalScrollSnap horizontalScrollSnap = GetComponent<MenuTutorialManager>().scrollSnap;

            AccountManager.Instance.RequestUnlockInTutorial(8);
            AccountManager.Instance.prevSceneName = "Main";

            var lockObj = menuLockController.FindButtonLockObject("Shop");
            if (lockObj.activeInHierarchy) {
                SkeletonGraphic skeletonGraphic = lockObj.GetComponent<SkeletonGraphic>();
                menuLockController.Unlock("상점");

                NewAlertManager
                    .Instance
                    .SetUpButtonToAlert(
                        menuLockController.GetMenu("Shop"),
                        NewAlertManager.ButtonName.SHOP
                    );

                yield return new WaitForSeconds(2.0f);
                handler.isDone = true;
            }
            else {
                handler.isDone = true;
            }
        }

        void OnDestroy() {
            StopAllCoroutines();
        }
    }

    public class AccountLinkTutorialFinish : MenuExecute {
        public override void Execute() {
            MainSceneStateHandler.Instance.ChangeState("AccountLinkTutorialLoaded", true);
            handler.isDone = true;
        }
    }

    public class Wait_QuestListLoaded : MenuExecute {
        public override void Execute() {
            NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_QUEST_UPDATED, QuestUpdated);
        }

        private void QuestUpdated(Enum Event_Type, Component Sender, object Param) {
            NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_QUEST_UPDATED, QuestUpdated);
            handler.isDone = true;
        }

        void OnDestroy() {
            NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_QUEST_UPDATED, QuestUpdated);
        }
    }

    public class Wait_Quest_RewardReceive : MenuExecute {
        IDisposable clickStream;
        Quest.QuestManager questManager;
        GameObject target = null;
        GameObject handUI;

        void Awake() {
            questManager = GetComponent<MenuTutorialManager>().questManager;
        }

        public override void Execute() {
            if(args == null) {
                Logger.LogError("Wait_Quest_RewardReceive Args 없음");
                handler.isDone = true;
                return;
            }
            string targetName = args[0];
            var menuMask = MenuMask.Instance;
            switch (targetName) {
                case "t1":
                    //0-2 완료 퀘스트
                    Transform content = GetComponent<MenuTutorialManager>().questManager.content;
                    foreach(Transform child in content) {
                        Logger.Log(child.name);
                        if (child.gameObject.activeSelf) {
                            var questController = child.GetComponent<Quest.QuestContentController>();
                            var data = questController.data;
                            if(targetName == data.questDetail.id) {
                                target = child.Find("GetBtn").gameObject;
                            }
                        }
                    }
                    menuMask.OnDimmed(target.transform.parent, target);
                    break;
            }
            Button button = target.GetComponent<Button>();
            handUI = HandUIController.ActiveHand(target.GetComponent<RectTransform>(), args[0]);

            SkeletonGraphic skeletonGraphic = handUI.GetComponent<SkeletonGraphic>();
            skeletonGraphic.Initialize(true);
            skeletonGraphic.Update(0);
            skeletonGraphic.Skeleton.SetSlotsToSetupPose();
            skeletonGraphic.AnimationState.SetAnimation(0, "TOUCH", true);

            clickStream = button.OnClickAsObservable().Subscribe(_ => CheckClick());
        }

        private void CheckClick() {
            HandUIController.DeactiveHand(args[0]);
            var menuMask = MenuMask.Instance;
            menuMask.OffDimmed(target);
            if (args[0] == "t1") {
                target.gameObject.SetActive(false);
            }

            StartCoroutine(Proceed());
        }

        private IEnumerator Proceed() {
            yield return new WaitUntil(() => FindObjectOfType<Modal>() != null);

            Button okBtn = FindObjectOfType<Modal>()
                .transform
                .GetChild(0)
                .GetChild(0)
                .Find("Buttons/YesButton")
                .GetComponent<Button>();

            clickStream = okBtn.OnClickAsObservable().Subscribe(_ => OkBtnClicked());
        }

        private void OkBtnClicked() {
            clickStream.Dispose();
            handler.isDone = true;
        }
    }

    public class SetMilestone : MenuExecute {
        public override void Execute() {
            var enum_val = (MenuTutorialManager.TutorialType)Enum.Parse(typeof(MenuTutorialManager.TutorialType), args[0]);
            GetComponent<MenuTutorialManager>().MainSceneStateHandler.SetMilestone(MainSceneStateHandler.MilestoneType.QUEST, enum_val);
            if(enum_val == MenuTutorialManager.TutorialType.Q5) {
                MainSceneStateHandler.Instance.ChangeState("IsQ5Finished", true);
            }
            handler.isDone = true;
        }
    }

    public class Wait_Mail_RewardReceive : MenuExecute {
        IDisposable clickStream;
        GameObject target = null;
        MailBoxManager mailBoxManager;
        GameObject handUI;
        void Awake() {
            mailBoxManager = GetComponent<MenuTutorialManager>().MailBoxManager;
        }

        public override void Execute() {
            if (args == null) {
                Logger.LogError("Wait_Quest_RewardReceive Args 없음");
                handler.isDone = true;
                return;
            }
            string targetName = args[0];
            var menuMask = MenuMask.Instance;
            switch (targetName) {
                case "t1":
                    //0-2 완료 퀘스트
                    Transform content = mailBoxManager.transform.Find("Content/MailListMask/MailList");
                    target = content.GetChild(0).Find("RecieveBtn").gameObject;
                    menuMask.OnDimmed(target.transform.parent, target);

                    break;
            }
            Button button = (target != null) ? target.GetComponent<Button>() : null;

            handUI = HandUIController.ActiveHand(target.GetComponent<RectTransform>(), args[0]);
            clickStream = (button != null) ? button.OnClickAsObservable().Subscribe(_ => CheckButton()) : Observable.EveryUpdate().Where(_ => Input.GetMouseButtonDown(0)).Subscribe(_ => CheckClick(target));

            SkeletonGraphic skeletonGraphic = handUI.GetComponent<SkeletonGraphic>();
            skeletonGraphic.Initialize(true);
            skeletonGraphic.Update(0);
            skeletonGraphic.Skeleton.SetSlotsToSetupPose();
            skeletonGraphic.AnimationState.SetAnimation(0, "TOUCH", true);

        }

        private void CheckClick(GameObject target) {
            if (target == null) {
                Logger.LogError("Target Button을 찾을 수 없음.");
                clickStream.Dispose();
                handler.isDone = true;
            }
        }

        private void CheckButton() {
            clickStream.Dispose();

            HandUIController.DeactiveHand(args[0]);
            var menuMask = MenuMask.Instance;
            menuMask.OffDimmed(target);
            handler.isDone = true;
        }
    }

    //우편 받기 버튼 클릭 이후 결과 모달에서 확인을 눌렀을 때...
    public class Wait_Mail_RewardReceive2 : MenuExecute {
        IDisposable clickStream;
        GameObject handUI;

        public override void Execute() {
            MenuMask.Instance.UnBlockScreen();

            var menuMask = MenuMask.Instance;
            GameObject target = menuMask.GetMenuObject("MailCloseBtn");

            Button button = (target != null) ? target.GetComponent<Button>() : null;
            BlockerController.blocker.SetBlocker(target.gameObject);

            clickStream = button.OnClickAsObservable().Subscribe(_ => CheckButton());
        }

        private void CheckButton() {
            clickStream.Dispose();
            BlockerController.blocker.gameObject.SetActive(false);

            handler.isDone = true;
        }

        void OnDestroy() {
            if(clickStream != null) clickStream.Dispose();
        }
    }

    /// <summary>
    /// 도감 화면에서 카드 클릭 대기
    /// </summary>
    public class Wait_Click_Card : MenuExecute {
        public override void Execute() {
            CardDictionaryManager cardManager = CardDictionaryManager.cardDictionaryManager;
            cardManager.cardShowHand(new string[] { args[0] }, () => { handler.isDone = true; }, true);
        }
    }

    public class Wait_Create_Card : MenuExecute {
        public override void Execute() {
            var card = Array.Find(AccountManager.Instance.myCards, x => x.cardId.CompareTo(args[0]) == 0);
            if (card != null && card.cardCount == 4) {
                handler.isDone = true;
                return;
            }

            NoneIngameSceneEventHandler.Instance.AddListener(
                NoneIngameSceneEventHandler.EVENT_TYPE.API_INVENTORIES_UPDATED,
                OnCardCreated
            );
        }

        private void OnCardCreated(Enum Event_Type, Component Sender, object Param) {
            var card = Array.Find(AccountManager.Instance.myCards, x => x.cardId.CompareTo(args[0]) == 0);
            if (card != null && card.cardCount == 4) {
                handler.isDone = true;
                NoneIngameSceneEventHandler.Instance.RemoveListener(
                    NoneIngameSceneEventHandler.EVENT_TYPE.API_INVENTORIES_UPDATED,
                    OnCardCreated
                );
            }
        }

        void OnDestroy() {
            NoneIngameSceneEventHandler.Instance.RemoveListener(
                NoneIngameSceneEventHandler.EVENT_TYPE.API_INVENTORIES_UPDATED,
                OnCardCreated
            );
        }
    }

    public class Wait_Create_Card_Exit : MenuExecute {
        public override void Execute() {
            var backButton = MenuCardInfo.cardInfoWindow.transform.parent.Find("BackButton");
            BlockerController.blocker.SetBlocker(backButton.gameObject);
            StartCoroutine(Proceed());
        }

        IEnumerator Proceed() {
            yield return new WaitUntil(() => !MenuCardInfo.cardInfoWindow.gameObject.activeSelf);
            BlockerController.blocker.gameObject.SetActive(false);
            handler.isDone = true;
        }

        private void OnDestroy() {
            StopAllCoroutines();
        }
    }

    /// <summary>
    /// 도감 화면 뒤로가기 버튼 클릭 대기
    /// </summary>
    public class Wait_DictionaryScene_Exit : MenuExecute {
        IDisposable clickStream;

        public override void Execute() {
            Button backBtn = CardDictionaryManager.cardDictionaryManager.transform.Find("UIbar/ExitBtn").GetComponent<Button>();
            BlockerController.blocker.SetBlocker(backBtn.gameObject);
            clickStream = backBtn.OnClickAsObservable().Subscribe(_ => CheckButton());
        }

        private void CheckButton() {
            if(clickStream != null) clickStream.Dispose();
            BlockerController.blocker.gameObject.SetActive(false);
            handler.isDone = true;
        }

        private void OnDestroy() {
            if (clickStream != null) clickStream.Dispose();
        }
    }

    /// <summary>
    /// 부대편집 진입 대기
    /// </summary>
    public class Wait_DeckEdit_Enter : MenuExecute {
        IDisposable clickStream;
        IDisposable clickEditBtnStream;

        string camp = string.Empty;
        Transform targetDeckObject = null;
        Button targetBtn = null;

        public override void Execute() {
            camp = args[0];
            int targetSibilingIndex = camp == "human" ? 1 : 2;
            var content = GetComponent<MenuTutorialManager>().deckEditWindow.transform.Find("DeckListParent/DeckList");
            
            foreach(Transform child in content) {
                DeckHandler deckHandler = child.GetComponent<DeckHandler>();
                if (child.GetSiblingIndex() == targetSibilingIndex) {
                    targetBtn = child.GetChild(0).Find("HeroImg").GetComponent<Button>();
                    targetDeckObject = child;
                }
            }
            if(targetBtn == null) {
                handler.isDone = true;
                return;
            }

            ShowHandInHeroImg();
            clickStream = targetBtn.OnClickAsObservable().Subscribe(_ => CheckButton());
        }

        private async void ShowHandInHeroImg() {
            await Task.Delay(300);
            BlockerController.blocker.SetBlocker(targetBtn.gameObject);
        }

        private void CheckButton() {
            clickStream.Dispose();
            BlockerController.blocker.gameObject.SetActive(false);

            //handler.isDone = true;
            StartCoroutine(Proceed());
        }

        IEnumerator Proceed() {
            yield return Wait(0.5f);

            Button editBtn = targetDeckObject.GetChild(0).Find("Buttons/EditBtn").GetComponent<Button>();
            clickEditBtnStream = editBtn.OnClickAsObservable().Subscribe(x => OnClickEditButton());

            BlockerController.blocker.SetBlocker(editBtn.gameObject);
        }

        private void OnClickEditButton() {
            Logger.Log("OnClickEditButton");

            if (clickStream != null) clickStream.Dispose();
            if (clickEditBtnStream != null) clickEditBtnStream.Dispose();

            BlockerController.blocker.gameObject.SetActive(false);
            handler.isDone = true;
        }

        //애니메이션 대기
        IEnumerator Wait(float sec) {
            yield return new WaitForSeconds(sec);
        }

        private void OnDestroy() {
            if (clickStream != null) clickStream.Dispose();
            if (clickEditBtnStream != null) clickEditBtnStream.Dispose();
        }
    }

    /// <summary>
    /// 부대 편집 대기
    /// </summary>
    public class Wait_DeckEditFinished : MenuExecute {
        GameObject deckEditCanvas;
        Transform cardBookArea;
        IDisposable clickStream;
        IDisposable observerMouseButtonDown, observerMouseButtonUp;

        bool isInitDimmedScale = false;
        Transform deckEditDimmed;

        public override void Execute() {
            string addTargetId = args[0];
            string removeTargetId = args[1];

            deckEditCanvas = GetComponent<MenuTutorialManager>().deckEditCanvas;

            FindRemoveTargetCard(removeTargetId);

            var scrollRects = deckEditCanvas.GetComponentsInChildren<ScrollRect>();
            observerMouseButtonDown = Observable
                .EveryUpdate()
                .Where(x => Input.GetMouseButtonDown(0))
                .Subscribe(x => {
                    foreach(ScrollRect sr in scrollRects) {
                        sr.enabled = false;
                    }
                    EditCardButtonHandler.canDrag = false;
                });

            observerMouseButtonUp = Observable
                .EveryUpdate()
                .Where(x => Input.GetMouseButtonUp(0))
                .Subscribe(x => {
                    foreach (ScrollRect sr in scrollRects) {
                        sr.enabled = true;
                    }
                    EditCardButtonHandler.canDrag = true;
                    var buttons = deckEditCanvas.transform.Find("InnerCanvas/HandDeckArea/CardButtons");
                    if(buttons != null) buttons.gameObject.SetActive(true);
                });
        }

        private async void FindAddTargetCard(string id) {
            await Task.Delay(300);

            cardBookArea = deckEditCanvas.transform.Find("InnerCanvas/CardBookParent/CardBookHeader/CardBookArea/CardBook");
            var editCardHandlers = cardBookArea.GetComponentsInChildren<EditCardHandler>();

            var editCardHandler = editCardHandlers.ToList().Find(x => x.cardID == id);
            //if(editCardHandler == null) {
            //    handler.isDone = true;
            //    return;
            //}

            deckEditDimmed.position = editCardHandler.transform.position;
            deckEditDimmed.localScale = new Vector3(0.8f, 0.6f, 1.0f);

            clickStream = editCardHandler.GetComponent<Button>().OnClickAsObservable().Subscribe(x => {
                WaitAddCard();

                if (!isInitDimmedScale) {
                    deckEditDimmed.localScale = Vector3.one;
                    deckEditDimmed.position = cardBookArea.parent.Find("CardButtons/Image").position;
                    isInitDimmedScale = true;
                }
            });

            BlockerController.blocker.SetBlocker(editCardHandler.gameObject);
        }

        
        private async void FindRemoveTargetCard(string id) {
            await Task.Delay(300);

            Transform handDeckArea = deckEditCanvas.transform.Find("InnerCanvas/HandDeckArea/SettedDeck");
            var editCardHandlers = handDeckArea.GetComponentsInChildren<EditCardHandler>();

            var editCardHandler = editCardHandlers.ToList().Find(x => x.cardID == id);
            if (editCardHandler == null) {
                handler.isDone = true;
                return;
            }

            deckEditDimmed = MenuMask.Instance.transform.Find("DeckEditDimmed");
            deckEditDimmed.gameObject.SetActive(true);
            deckEditDimmed.position = editCardHandler.transform.position;
            deckEditDimmed.localScale = new Vector3(0.8f, 0.6f, 1.0f);

            BlockerController.blocker.SetBlocker(editCardHandler.gameObject);

            clickStream = editCardHandler.GetComponent<Button>().OnClickAsObservable().Subscribe(x => {
                WaitRemoveCard();

                if (!isInitDimmedScale) {
                    deckEditDimmed.localScale = Vector3.one;
                    deckEditDimmed.position = handDeckArea.parent.Find("CardButtons/Image").position;
                    isInitDimmedScale = true;
                }
            });
        }

        EditCardButtonHandler buttonHandler;
        private async void WaitRemoveCard() {
            await Task.Delay(100);

            var exceptButton = GetComponent<MenuTutorialManager>().deckEditCanvas.transform.Find("InnerCanvas/HandDeckArea/CardButtons/Image/ExceptCard");
            buttonHandler = exceptButton.parent.parent.GetComponent<EditCardButtonHandler>();
            buttonHandler.cardExcepedAction += CardRemoved;

            BlockerController.blocker.SetBlocker(exceptButton.gameObject);
        }

        private void CardRemoved() {
            EditCardHandler cardHandler = buttonHandler
                .transform
                .GetChild(0)
                .Find("CardImage")
                .GetComponent<EditCardHandler>();

            int cardNum = cardHandler.HAVENUM;
            Logger.Log("CardNum : " + cardNum);
            if(cardNum == 0) {
                OffButtonPanel();
                isInitDimmedScale = false;

                buttonHandler.cardExcepedAction -= CardRemoved;
                FindAddTargetCard(args[0]);
            }
        }

        private async void OffButtonPanel() {
            await Task.Delay(100);
            buttonHandler.gameObject.SetActive(false);
        }

        private async void WaitAddCard() {
            await Task.Delay(300);

            var addButton = cardBookArea.parent.Find("CardButtons/Image/AddCard");
            buttonHandler = addButton.parent.parent.GetComponent<EditCardButtonHandler>();
            buttonHandler.cardAdded += CardAdded;

            BlockerController.blocker.SetBlocker(addButton.gameObject);
        }

        private void CardAdded() {
            EditCardHandler cardHandler = buttonHandler
                .transform
                .GetChild(0)
                .Find("CardImage")
                .GetComponent<EditCardHandler>();

            int cardNum = cardHandler.HAVENUM;
            Logger.Log("CardNum : " + cardNum);
            if (cardNum == 0) {
                Logger.Log("Wait_DeckEditFinished");
                buttonHandler.cardExcepedAction -= CardAdded;

                if (observerMouseButtonDown != null) observerMouseButtonDown.Dispose();
                if (observerMouseButtonUp != null) observerMouseButtonUp.Dispose();

                var scrollRects = deckEditCanvas.GetComponentsInChildren<ScrollRect>();
                foreach(ScrollRect rect in scrollRects) {
                    rect.enabled = true;
                }

                handler.isDone = true;

                deckEditDimmed.gameObject.SetActive(false);
                BlockerController.blocker.gameObject.SetActive(false);
            }
        }

        private void OnDestroy() {
            if(observerMouseButtonDown != null) observerMouseButtonDown.Dispose();
            if(observerMouseButtonUp != null) observerMouseButtonUp.Dispose();

            EditCardButtonHandler.canDrag = true;
        }
    }

    public class ForceToMainPage : MenuExecute {
        public override void Execute() {
            string pageName = args[0];
            var scrollSnap = GetComponent<MenuTutorialManager>().scrollSnap;
            Transform content = scrollSnap.transform.Find("Content");
            foreach(Transform window in content) {
                if(window.name == pageName) {
                    scrollSnap.GoToScreen(window.GetSiblingIndex());
                    break;
                }
            }

            handler.isDone = true;
        }
    }

    public class RequestTutorialPreSettings : MenuExecute {
        public override void Execute() {
            AccountManager.Instance.RequestTutorialPreSettings();
            handler.isDone = true;
        }
    }

    /// <summary>
    /// 강제가 아닌 클릭 리스너 등록
    /// </summary>
    public class ForceDailyQuestWithoutLeaguePlay : MenuExecute {
        IDisposable clickStream;
        public override void Execute() {
            Button button = MenuMask.Instance.GetMenuObject("hud_back_button").GetComponent<Button>();
            clickStream = button.OnClickAsObservable().Subscribe(_ => OnClick());
        }

        private void OnClick() {
            GetComponent<MenuTutorialManager>().menuSceneController.CheckDailyQuest(true);
            handler.isDone = true;
        }

        private void OnDestroy() {
            if (clickStream != null) clickStream.Dispose();
        }
    }

    public class OffTutoInCardInfo : MenuExecute {
        public override void Execute() {
            MenuCardInfo.onTuto = false;
            handler.isDone = true;
        }
    }
}