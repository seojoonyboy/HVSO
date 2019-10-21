using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;
using Spine.Unity;

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

            if (button != null)
                ShowTouchIcon(button.transform.gameObject);

            clickStream = (button != null) ? button.OnClickAsObservable().Subscribe(_ => CheckButton(button)) : Observable.EveryUpdate().Where(_ => Input.GetMouseButtonDown(0)).Subscribe(_ => CheckClick(target));
        }

        private void CheckClick(GameObject target) {
            if (target == null) {
                clickStream.Dispose();
                handler.isDone = true;
            }
        }

        private void CheckButton(Button button) {
            HideTouchIcon(button);
            clickStream.Dispose();
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

    public class Menu_NPC_Talk : MenuExecute {
        public Menu_NPC_Talk() : base() { }

        //args[0] 유닛 id, args[1] 대사내용, args[2] my,enemy
        public override void Execute() {
            MenuMask menuMask = MenuMask.Instance;
            MenuMask.Instance.gameObject.SetActive(true);
            menuMask.menuTalkPanel.SetActive(true);

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
            menuMask.menuTalkPanel.GetComponent<TextTyping>().StartTyping(args[1], handler);
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
            string target = args[0];
            switch (target) {
                case "orc":
                    var content = scenarioManager.orc.stageContent;
                    Button deckButton = content.transform.GetChild(0).GetComponent<Button>();
                    deckButton.onClick.Invoke();
                    scenarioManager.selectedDeck = new object[] { true, AccountManager.Instance.orcDecks[0] };
                    scenarioManager.deckContent.transform.GetChild(0).gameObject.SetActive(false);
                    break;
            }

            handler.isDone = true;
        }
    }

    /// <summary>
    /// 오크 튜토리얼 진행시 덱 리스트를 안보이게 해야함
    /// </summary>
    public class HideDeckList : MenuExecute {
        public override void Execute() {
            Transform deck =  handler
                .GetComponent<MenuTutorialManager>()
                .scenarioManager
                .stageCanvas
                .transform
                .Find("HUD/StagePanel/Scroll View")
                .GetChild(0);
            var images = deck.GetComponentsInChildren<Image>();
            var texts = deck.GetComponentsInChildren<Text>();
            var textmeshes = deck.GetComponentsInChildren<TMPro.TextMeshProUGUI>();
            foreach(Image image in images) {
                image.enabled = false;
            }
            foreach(Text text in texts) {
                text.enabled = false;
            }
            foreach(TMPro.TextMeshProUGUI text in textmeshes) {
                text.enabled = false;
            }

            Transform stagePanel = handler
                .GetComponent<MenuTutorialManager>()
                .scenarioManager
                .stageCanvas
                .transform
                .Find("HUD/StagePanel");

            stagePanel
                .Find("TextGroup/DeckScript")
                .gameObject.SetActive(false);

            stagePanel
                .Find("TextGroup/DeckCount")
                .gameObject.SetActive(false);

            stagePanel
                .Find("DeckBack")
                .gameObject
                .SetActive(false);

            handler.isDone = true;
        }
    }

    /// <summary>
    /// 오크 튜토리얼 진행시 덱 리스트를 안보이게 한거 초기화
    /// </summary>
    public class ResetDeckList : MenuExecute {
        public override void Execute() {
            Transform deck = handler
                .GetComponent<MenuTutorialManager>()
                .scenarioManager
                .stageCanvas
                .transform
                .Find("HUD/StagePanel/Scroll View")
                .GetChild(0);
            var images = deck.GetComponentsInChildren<Image>();
            var texts = deck.GetComponentsInChildren<Text>();
            var textmeshes = deck.GetComponentsInChildren<TMPro.TextMeshProUGUI>();
            foreach (Image image in images) {
                image.enabled = true;
            }
            foreach (Text text in texts) {
                text.enabled = true;
            }
            foreach (TMPro.TextMeshProUGUI text in textmeshes) {
                text.enabled = true;
            }


            //////////작업중
            Transform stagePanel = handler
                .GetComponent<MenuTutorialManager>()
                .scenarioManager
                .stageCanvas
                .transform
                .Find("HUD/StagePanel");

            stagePanel
                .Find("TextGroup/DeckScript")
                .gameObject.SetActive(true);

            stagePanel
                .Find("TextGroup/DeckCount")
                .gameObject.SetActive(true);

            stagePanel
                .Find("DeckBack")
                .gameObject
                .SetActive(true);

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

    public class RequestReward : MenuExecute {
        public override void Execute() {
            //TODO : 보상 Request 및 Callback이 오면 isDone 변경
            handler.isDone = true;
        }
    }

    public class Wait_DeckSelect : MenuExecute {
        public override void Execute() {
            var deckListPanel = GetComponent<MenuTutorialManager>().BattleReadydeckListPanel;
            var content = deckListPanel.transform.Find("Viewport/Content");
            foreach (Transform deckObject in content) {
                Button btn = deckObject.GetComponent<Button>();
                btn.onClick.AddListener(
                    () => {
                        Onclick();
                    });
            }
        }

        public void Onclick() {
            PlayerPrefs.SetString("SelectedBattleType", "solo");
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
}