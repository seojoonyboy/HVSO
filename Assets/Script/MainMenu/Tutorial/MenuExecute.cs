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
                    }
                    menuMask.OnDimmed(targetObject.transform.parent, targetObject);
                    break;
                case "off":
                    if (args.Count > 2) {
                        menuMask.OffDimmed(targetObject, objectName);
                    }
                    else {
                        menuMask.OffDimmed(targetObject);
                    }
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

            skeletonGraphic.Initialize(true);

            skeletonGraphic.Skeleton.SetSkin(args[0]);
            skeletonGraphic.Skeleton.SetSlotsToSetupPose();

            yield return new WaitForEndOfFrame();
            skeletonGraphic.transform.parent.Find("SubBackground").gameObject.SetActive(false);
            skeletonGraphic.AnimationState.SetAnimation(0, "sampledeck", false);

            yield return new WaitForSeconds(0.8f);

            skeletonGraphic.transform.Find("Header/Text").GetComponent<TMPro.TextMeshProUGUI>().text = "덱 획득";
            if(args[0] == "human") {
                skeletonGraphic.transform.Find("Description/Text").GetComponent<TMPro.TextMeshProUGUI>().text = "휴먼 기본 부대, '왕국 수비대' 획득!";
            }
            else {
                skeletonGraphic.transform.Find("Description/Text").GetComponent<TMPro.TextMeshProUGUI>().text = "오크 기본 부대, '주술사 부족' 획득!";
            }

            yield return new WaitForSeconds(1.0f);

            clickStream = Observable.EveryUpdate()
                .Where(_ => Input.GetMouseButtonDown(0))
                .Subscribe(_ => CheckClick(target));

            AccountManager.Instance.RequestUserInfo();
            AccountManager.Instance.RequestMyDecks();
            AccountManager.Instance.RequestInventories();
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

    public class DestroyLoadingModal : MenuExecute {
        public override void Execute() {
            var loadingModal = GetComponent<MenuTutorialManager>().menuSceneController.hideModal;
            loadingModal.SetActive(false);
            handler.isDone = true;
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

            skeletonGraphic.transform.Find("Header/Text").GetComponent<TMPro.TextMeshProUGUI>().text = "오크 스토리 해금";
            skeletonGraphic.transform.Find("Description/Text").GetComponent<TMPro.TextMeshProUGUI>().text = "오크 진영 튜토리얼이 개방 되었습니다!";

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

    public class UnlockOrcStory_2_Anim : MenuExecute {
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
            skeletonGraphic.AnimationState.SetAnimation(0, "story_reward2", false);

            skeletonGraphic.transform.Find("Header/Text").GetComponent<TMPro.TextMeshProUGUI>().text = "오크 스토리 해금";
            skeletonGraphic.transform.Find("Description/Text").GetComponent<TMPro.TextMeshProUGUI>().text = "오크 진영 튜토리얼이 개방 되었습니다!";

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

    public class UnlockHumanStory_2_Anim : MenuExecute {
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

            skeletonGraphic.Skeleton.SetSkin("human");
            skeletonGraphic.Skeleton.SetSlotsToSetupPose();

            yield return new WaitForEndOfFrame();
            skeletonGraphic.transform.parent.Find("SubBackground").gameObject.SetActive(false);
            skeletonGraphic.AnimationState.SetAnimation(0, "story_reward2", false);

            skeletonGraphic.transform.Find("Header/Text").GetComponent<TMPro.TextMeshProUGUI>().text = "휴먼 스토리 해금";
            skeletonGraphic.transform.Find("Description/Text").GetComponent<TMPro.TextMeshProUGUI>().text = "휴먼 진영 튜토리얼이 개방 되었습니다!";

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

            yield return new WaitForEndOfFrame();
            skeletonGraphic.transform.parent.Find("SubBackground").gameObject.SetActive(false);
            skeletonGraphic.AnimationState.SetAnimation(0, "story_reward1", false);

            skeletonGraphic.transform.Find("Header/Text").GetComponent<TMPro.TextMeshProUGUI>().text = "오크 튜토리얼 개방";
            skeletonGraphic.transform.Find("Description/Text").GetComponent<TMPro.TextMeshProUGUI>().text = "오크 진영 튜토리얼이 개방 되었습니다!";

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
    /// 메인화면에서 스토리 메뉴 전체 해금 표시
    /// </summary>
    public class UnlockStroyAnim : MenuExecute {
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

            skeletonGraphic.Skeleton.SetSkin("human");
            skeletonGraphic.Skeleton.SetSlotsToSetupPose();
            yield return new WaitForEndOfFrame();
            skeletonGraphic.transform.parent.Find("SubBackground").gameObject.SetActive(false);
            skeletonGraphic.AnimationState.SetAnimation(0, "story", false);

            skeletonGraphic.transform.Find("Header/Text").GetComponent<TMPro.TextMeshProUGUI>().text = "스토리 메뉴 해금";
            skeletonGraphic.transform.Find("Description/Text").GetComponent<TMPro.TextMeshProUGUI>().text = "스토리 메뉴가 오픈되었습니다!";

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

                PlayerPrefs.SetString("StoryUnlocked", "true");

                AccountManager.Instance.RequestUnlockInTutorial(1);
            }
        }
    }

    public class UnlockBattleAnim : MenuExecute {
        IDisposable clickStream;
        IEnumerator coroutine;

        public override void Execute() {
            coroutine = Proceed();
            StartCoroutine(coroutine);

            var loadingModal = GetComponent<MenuTutorialManager>().menuSceneController.hideModal;
            loadingModal.SetActive(true);
        }

        IEnumerator Proceed() {
            GameObject target = null;

            GetComponent<MenuTutorialManager>().ActiveRewardPanel();
            SkeletonGraphic skeletonGraphic = GetComponent<MenuTutorialManager>().rewardPanel.transform.Find("Anim").GetComponent<SkeletonGraphic>();

            skeletonGraphic.Initialize(true);

            skeletonGraphic.Skeleton.SetSkin("human");
            skeletonGraphic.Skeleton.SetSlotsToSetupPose();

            yield return new WaitForEndOfFrame();
            var loadingModal = GetComponent<MenuTutorialManager>().menuSceneController.hideModal;
            loadingModal.SetActive(false);

            skeletonGraphic.transform.parent.Find("SubBackground").gameObject.SetActive(false);
            skeletonGraphic.AnimationState.SetAnimation(0, "battle", false);

            skeletonGraphic.transform.Find("Header/Text").GetComponent<TMPro.TextMeshProUGUI>().text = "배틀 메뉴 해금";
            skeletonGraphic.transform.Find("Description/Text").GetComponent<TMPro.TextMeshProUGUI>().text = "배틀 메뉴가 오픈되었습니다!";

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

                AccountManager.Instance.RequestUnlockInTutorial(4);

                GetComponent<MenuTutorialManager>()
                    .battleMenuCanvas
                    .transform
                    .Find("InnerCanvas/MainPanel/LeagueButton")
                    .GetChild(0)
                    .GetChild(0)
                    .gameObject
                    .SetActive(false);
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
                PlayerPrefs.SetInt("TutorialBoxRecieved", 1);

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
            PlayerPrefs.SetInt("TutorialBoxRecieved", 1);
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
            MenuMask.Instance.UnBlockScreen();

            GetComponent<MenuTutorialManager>().EndTutorial();
            GetComponent<MenuTutorialManager>().enabled = false;
            
            handler.isDone = true;
        }
    }

    public class ActiveImageTutorial : MenuExecute {
        public override void Execute() {
            GameObject mainDescCanvas = GetComponent<MenuTutorialManager>().mainDescCanvas;
            foreach (Transform child in mainDescCanvas.transform) {
                child.GetComponent<BooleanIndex>().isOn = true;
            }
            GetComponent<MenuTutorialManager>().OnMainPageChanged();
            handler.isDone = true;
        }
    }

    public class DeactiveImageTutorial : MenuExecute {
        public override void Execute() {
            GameObject mainDescCanvas = GetComponent<MenuTutorialManager>().mainDescCanvas;
            foreach(Transform child in mainDescCanvas.transform) {
                child.GetComponent<BooleanIndex>().isOn = false;
            }
            handler.isDone = true;
        }
    }

    public class ForceToPage : MenuExecute {
        public override void Execute() {
            string pageName = args[0];

            switch (pageName) {
                case "StoryLobby":
                    GetComponent<MenuTutorialManager>().battleMenuCanvas.gameObject.SetActive(true);
                    GetComponent<MenuTutorialManager>().scenarioManager.gameObject.SetActive(true);
                    break;
            }

            handler.isDone = true;
        }
    }

    public class UnlockCardMenu : MenuExecute {
        IDisposable clickStream;
        IEnumerator coroutine;

        public override void Execute() {
            coroutine = Proceed();
            StartCoroutine(coroutine);

            var loadingModal = GetComponent<MenuTutorialManager>().menuSceneController.hideModal;
            loadingModal.SetActive(true);
        }

        IEnumerator Proceed() {
            GameObject target = null;

            GetComponent<MenuTutorialManager>().ActiveRewardPanel();
            SkeletonGraphic skeletonGraphic = GetComponent<MenuTutorialManager>().rewardPanel.transform.Find("Anim").GetComponent<SkeletonGraphic>();

            skeletonGraphic.Initialize(true);

            skeletonGraphic.Skeleton.SetSkin("human");
            skeletonGraphic.Skeleton.SetSlotsToSetupPose();

            yield return new WaitForEndOfFrame();
            var loadingModal = GetComponent<MenuTutorialManager>().menuSceneController.hideModal;
            loadingModal.SetActive(false);

            skeletonGraphic.transform.parent.Find("SubBackground").gameObject.SetActive(false);
            skeletonGraphic.AnimationState.SetAnimation(0, "NOANI", false);

            skeletonGraphic.transform.Find("Header/Text").GetComponent<TMPro.TextMeshProUGUI>().text = "카드 메뉴 해금";
            skeletonGraphic.transform.Find("Description/Text").GetComponent<TMPro.TextMeshProUGUI>().text = "카드 메뉴가 해금되었습니다!";

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

                AccountManager.Instance.RequestUnlockInTutorial(2);
                AccountManager.Instance.RequestQuestInfo();
            }
        }
    }

    public class RequestQuestInfo : MenuExecute {
        public override void Execute() {
            AccountManager.Instance.RequestQuestInfo();
            handler.isDone = true;
        }
    }
}