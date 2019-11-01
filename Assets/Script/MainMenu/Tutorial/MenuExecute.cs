using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;
using Spine.Unity;
using BestHTTP;
using dataModules;

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
            Logger.Log("Glow Target : " + target);
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

            float offsetY = 400f;
            if (args.Count == 4) {
                switch (args[3]) {
                    case "top":
                        CharacterImage.localPosition = new Vector3(originPos.x, originPos.y + offsetY, prevCharacterImagePos.z);
                        MainText.localPosition = new Vector3(originPos.x, originPos.y + offsetY, prevMainTextPos.z);
                        NameObject.localPosition = new Vector3(originNameObjectPos.x, originNameObjectPos.y + offsetY, prevNameObjectPos.z);

                        if (transform.Find("MainMenuGlowCanvas").gameObject.activeSelf) {
                            menuMask.menuTalkPanel.transform.parent.GetComponent<Canvas>().sortingOrder = 86;
                        }
                        break;
                }
            }
            else {
                CharacterImage.localPosition = originPos;
                MainText.localPosition = originPos;
                NameObject.localPosition = originNameObjectPos;
            }

            if (args[2] != "both") {
                bool isPlayer = args[2] != "enemy";

                menuMask.menuTalkPanel.transform.Find("CharacterImage/Player").gameObject.SetActive(isPlayer);
                menuMask.menuTalkPanel.transform.Find("CharacterImage/Enemy").gameObject.SetActive(!isPlayer);
                menuMask.menuTalkPanel.transform.Find("NameObject/PlayerName").gameObject.SetActive(isPlayer);
                menuMask.menuTalkPanel.transform.Find("NameObject/EnemyName").gameObject.SetActive(!isPlayer);
                if (isPlayer) {
                    Logger.Log(args[0]);
                    menuMask.menuTalkPanel.transform.Find("CharacterImage/Player").GetComponent<Image>().sprite = AccountManager.Instance.resource.ScenarioUnitResurce[args[0]].sprite;
                    menuMask.menuTalkPanel.transform.Find("NameObject/PlayerName").GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.resource.ScenarioUnitResurce[args[0]].name;
                }
                else {
                    menuMask.menuTalkPanel.transform.Find("CharacterImage/Enemy").GetComponent<Image>().sprite = AccountManager.Instance.resource.ScenarioUnitResurce[args[0]].sprite;
                    menuMask.menuTalkPanel.transform.Find("NameObject/EnemyName").GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.resource.ScenarioUnitResurce[args[0]].name;
                }
            }
            else {
                menuMask.menuTalkPanel.transform.Find("CharacterImage/Player").gameObject.SetActive(true);
                menuMask.menuTalkPanel.transform.Find("CharacterImage/Enemy").gameObject.SetActive(true);
                menuMask.menuTalkPanel.transform.Find("NameObject/PlayerName").gameObject.SetActive(true);
                menuMask.menuTalkPanel.transform.Find("NameObject/EnemyName").gameObject.SetActive(true);

                menuMask.menuTalkPanel.transform.Find("CharacterImage/Player").GetComponent<Image>().sprite = AccountManager.Instance.resource.ScenarioUnitResurce["ac10005"].sprite;
                menuMask.menuTalkPanel.transform.Find("NameObject/PlayerName").GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.resource.ScenarioUnitResurce["ac10012"].name;
                menuMask.menuTalkPanel.transform.Find("CharacterImage/Enemy").GetComponent<Image>().sprite = AccountManager.Instance.resource.ScenarioUnitResurce["ac10012"].sprite;
                menuMask.menuTalkPanel.transform.Find("NameObject/EnemyName").GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.resource.ScenarioUnitResurce["ac10005"].name;
                menuMask.menuTalkPanel.GetComponent<TextTyping>().StartTyping(args[1], handler);
            }
            menuMask.menuTalkPanel.GetComponent<TextTyping>().StartTyping(args[1], handler);
#if UNITY_EDITOR
            menuMask.menuTalkPanel.transform.Find("StopTypingTrigger").gameObject.SetActive(true);
#endif
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

    public class Darken_Menu_NPC_Talk : MenuExecute {
        public Darken_Menu_NPC_Talk() : base() { }

        //대화중 캐릭터 어둡게
        public override void Execute() {
            bool isPlayer = args[0] != "enemy";

            MenuMask menuMask = MenuMask.Instance;
            MenuMask.Instance.gameObject.SetActive(true);

            if (isPlayer) {
                menuMask.menuTalkPanel.transform.Find("CharacterImage/Player").GetComponent<Image>().color = new Color32(0, 0, 0, 255);
            }
            else {
                menuMask.menuTalkPanel.transform.Find("CharacterImage/Enemy").GetComponent<Image>().color = new Color32(0, 0, 0, 255);
            }
            handler.isDone = true;
        }
    }

    public class Brighten_Menu_NPC_Talk : MenuExecute {
        public Brighten_Menu_NPC_Talk() : base() { }

        //대화중 캐릭터 밝게
        public override void Execute() {
            bool isPlayer = args[0] != "enemy";

            MenuMask menuMask = MenuMask.Instance;
            MenuMask.Instance.gameObject.SetActive(true);

            if (isPlayer) {
                menuMask.menuTalkPanel.transform.Find("CharacterImage/Player").GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            }
            else {
                menuMask.menuTalkPanel.transform.Find("CharacterImage/Enemy").GetComponent<Image>().color = new Color32(255, 255, 255, 255);
            }
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
            Logger.Log(objectName);

            var targetObject = menuMask.GetMenuObject(objectName);

            menuMask.UnBlockScreen();
            switch (args[1]) {
                case "on":
                    menuMask.OnDimmed(targetObject.transform.parent, targetObject);
                    break;
                case "off":
                    menuMask.OffDimmed(targetObject);
                    break;
            }
            handler.isDone = true;
        }
    }

    public class Disable_Button : MenuExecute {
        public override void Execute() {
            var menuMask = MenuMask.Instance;

            string objectName = args[0];
            var targetObject = menuMask.GetMenuObject(objectName);

            var button = targetObject.GetComponent<Button>();
            if (button != null) button.enabled = false;

            handler.isDone = true;
        }
    }

    public class Enable_Button : MenuExecute {
        public override void Execute() {
            var menuMask = MenuMask.Instance;

            string objectName = args[0];
            var targetObject = menuMask.GetMenuObject(objectName);
            var button = targetObject.GetComponent<Button>();
            if (button != null) button.enabled = true;

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
            GetComponent<MenuTutorialManager>().ActiveTutorialStoryReadyCanvas(args[0]);

            if (args[0] == "human") {
                ScenarioGameManagment.chapterData = scenarioManager.human_chapterDatas[0];
                ScenarioGameManagment.challengeDatas = scenarioManager.human_challengeDatas[0].challenges;
            }
            else if(args[0] == "orc") {
                ScenarioGameManagment.chapterData = scenarioManager.orc_chapterDatas[0];
                ScenarioGameManagment.challengeDatas = scenarioManager.orc_challengeDatas[0].challenges;
            }

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

            NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_DECKS_UPDATED, (type, sender, parm) => {
                HTTPResponse res = (HTTPResponse)parm;
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
            });
            GetComponent<MenuTutorialManager>().ActiveRewardPanel();

            SkeletonGraphic skeletonGraphic = GetComponent<MenuTutorialManager>().rewardPanel.transform.Find("Anim").GetComponent<SkeletonGraphic>();

            skeletonGraphic.Skeleton.SetSkin(args[0]);
            skeletonGraphic.Skeleton.SetSlotsToSetupPose();
            skeletonGraphic.AnimationState.Apply(skeletonGraphic.Skeleton);

            yield return new WaitForSeconds(2.0f);

            clickStream = Observable.EveryUpdate()
                .Where(_ => Input.GetMouseButtonDown(0))
                .Subscribe(_ => CheckClick(target));

            AccountManager.Instance.RequestUserInfo();
            AccountManager.Instance.RequestMyDecks();
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
    }

    public class Wait_DeckSelect : MenuExecute {
        public override void Execute() {
            var deckListPanel = GetComponent<MenuTutorialManager>().BattleReadydeckListPanel;
            var content = deckListPanel.transform.Find("Viewport/Content");

            deckListPanel.transform.Find("Description").gameObject.SetActive(true);

            foreach (Transform deckObject in content) {
                if (deckObject.gameObject.activeSelf) {
                    Button btn = deckObject.GetComponent<Button>();
                    btn.onClick.AddListener(
                        () => {
                            Onclick();
                        });
                    GameObject handUI = btn.transform.Find("HandUI").gameObject;
                    handUI.SetActive(true);
                    SkeletonGraphic skeletonGraphic = handUI.GetComponent<SkeletonGraphic>();
                    skeletonGraphic.Initialize(true);
                    skeletonGraphic.Update(0);
                    skeletonGraphic.Skeleton.SetSlotsToSetupPose();
                }
            }
        }

        public void Onclick() {
            var deckListPanel = GetComponent<MenuTutorialManager>().BattleReadydeckListPanel;
            var content = deckListPanel.transform.Find("Viewport/Content");
            foreach (Transform deckObject in content) {
                if (deckObject.gameObject.activeSelf) {
                    GameObject handUI = deckObject.Find("HandUI").gameObject;
                    handUI.SetActive(false);
                }
            }
            handler.isDone = true;
        }
    }

    public class ChangePVPBattleBtnFunction : MenuExecute {
        public override void Execute() {
            Button btn = MenuMask.Instance.menuObject["ai_battle_start_button"].GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => { StartCoroutine(Wait()); });
            
            handler.isDone = true;

            IEnumerator Wait() {
                PlayerPrefs.SetString("SelectedBattleType", "tutorial");
                PlayerPrefs.SetString("PrevTutorial", "AI_Tutorial");

                GetComponent<MenuTutorialManager>()
                    .BattleReadydeckListPanel
                    .transform
                    .Find("Description")
                    .gameObject
                    .SetActive(false);

                yield return new WaitForSeconds(1.0f);
                FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.CONNECT_MATCHING_SCENE);
            }
        }
    }

    public class ResetPVPBattleBtnFunction : MenuExecute {
        public override void Execute() {
            Button btn = MenuMask.Instance.menuObject["ai_battle_start_button"].GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => {
                GetComponent<MenuTutorialManager>()
                    .BattleReadydeckListPanel
                    .GetComponent<DeckListHandlerInBattleReady>()
                    .parentController
                    .OnStartButton();
            });
            
            handler.isDone = true;
        }
    }

    public class Block_HUD : MenuExecute {
        public override void Execute() {
            var hud = GetComponent<MenuTutorialManager>().HUDCanvas;
            var blockPanel = hud.transform.Find("MenuHUD/MenuHeader/BlockPanel");
            blockPanel.gameObject.SetActive(true);
            
            handler.isDone = true;
        }
    }

    public class Unblock_HUD : MenuExecute {
        public override void Execute() {
            var hud = GetComponent<MenuTutorialManager>().HUDCanvas;
            var blockPanel = hud.transform.Find("MenuHUD/MenuHeader/BlockPanel");
            blockPanel.gameObject.SetActive(false);
            
            handler.isDone = true;
        }
    }

    public class ShowHandUI : MenuExecute {
        public override void Execute() {
            GameObject target = MenuMask.Instance.GetMenuObject(args[0]);

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
            string camp = args[0];
            string stageNum = args[1];

            PlayerPrefs.SetString("SelectedRace", camp);
            PlayerPrefs.SetString("SelectedBattleType", "story");
            PlayerPrefs.SetString("StageNum", stageNum);

            handler.isDone = true;
            FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.CONNECT_MATCHING_SCENE);
        }
    }

    public class BoxOpenProcess : MenuExecute {
        public override void Execute() {
            var userData = AccountManager.Instance.userData;
            var menuTutorialManager = GetComponent<MenuTutorialManager>();

            if (userData.supplyBox > 0) {
                menuTutorialManager.BoxRewardPanel.transform.Find("ExitButton").GetComponent<Button>().onClick.AddListener(onclick);
                menuTutorialManager.ActiveRewardBoxCanvas();
            }
            else {
                Logger.LogError("박스가 없습니다!");
                handler.isDone = true;
            }
        }

        private void onclick() {
            GetComponent<MenuTutorialManager>()
                .BoxRewardPanel
                .transform
                .Find("ExitButton")
                .GetComponent<Button>()
                .onClick
                .RemoveListener(onclick);
            handler.isDone = true;
        }

        public class Response {
            public string supplyBox;
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
            Logger.Log("MainMenuButtonGlow : " + args[0]);
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
            Logger.Log("모든 튜토리얼 끝!");
            MenuMask.Instance.UnBlockScreen();

            GetComponent<MenuTutorialManager>().EndTutorial();
            GetComponent<MenuTutorialManager>().enabled = false;
            
            handler.isDone = true;
        }
    }

    public class StartMainMenuBtnTutorial : MenuExecute {
        public override void Execute() {
            GetComponent<MenuTutorialManager>().StartTutorial(MenuTutorialManager.TutorialType.MAIN_BUTTON_DESC);
            handler.isDone = true;
        }
    }

    public class ActiveImageTutorial : MenuExecute {
        public override void Execute() {
            GetComponent<MenuTutorialManager>().OnMenuDescPanel(2);
            PlayerPrefs.SetInt("IsFirstMainMenu", 0);
            handler.isDone = true;
        }
    }
}