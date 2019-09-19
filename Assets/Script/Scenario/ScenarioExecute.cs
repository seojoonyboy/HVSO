using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class ScenarioExecute : MonoBehaviour {
    public ScenarioExecuteHandler handler;
    public List<string> args;
    public string chapterNum;

    public ScenarioMask scenarioMask;
    protected ScenarioGameManagment scenarioGameManagment;

    public virtual void Initialize(List<string> args) {
        scenarioMask = ScenarioMask.Instance;
        scenarioGameManagment = ScenarioGameManagment.scenarioInstance;
        this.args = args;
        handler = GetComponent<ScenarioExecuteHandler>();
    }

    public virtual void Execute() { }
}

public class Print_message : ScenarioExecute {
    public Print_message() : base() { }

    public override void Execute() {
        handler.isDone = true;
        Logger.Log("Print_message");
    }
}

public class Highlight : ScenarioExecute {
    public Highlight() :base() { }

    public override void Execute() {
        GameObject target;
        if (args.Count > 1) 
            target = (args.Count > 2) ? scenarioMask.GetMaskingObject(args[0], args[1], args[2]) : scenarioMask.GetMaskingObject(args[0], args[1]);
        else
            target = scenarioMask.GetMaskingObject(args[0]);


        ScenarioMask.Instance.SetHighlightImage(target);
        ScenarioMask.Instance.GetMaskHighlight(target);
        handler.isDone = true;
        Logger.Log("Highlight");
    }

}

public class Wait_until : ScenarioExecute {
    public Wait_until() : base() { }

    public override void Execute() {
        scenarioMask.MaskScreen();
        var parms = args;
        float sec = 0;
        float.TryParse(parms[0], out sec);
        StartCoroutine(WaitSec(sec));
    }

    IEnumerator WaitSec(float sec = 0) {
        Logger.Log("WaitSec");
        yield return new WaitForSeconds(sec);
        scenarioMask.OffMaskScreen();
        handler.isDone = true;
    }
}

public class Wait_click : ScenarioExecute {
    public Wait_click() : base() { }

    IDisposable clickstream;

    public override void Execute() {
        GameObject target;

        if (args[0] == "screen")
            target = null;
        else if (args.Count > 1)
            target = scenarioMask.GetMaskingObject(args[0], args[1]);
        else
            target = scenarioMask.GetMaskingObject(args[0]);


        Button button = (target != null) ? target.GetComponent<Button>() : null;


        clickstream =  (button != null) ? button.OnClickAsObservable().Subscribe(_=>CheckButton())  : Observable.EveryUpdate().Where(_ => Input.GetMouseButtonDown(0)).Subscribe(_ => CheckClick(target));
        //Observable.EveryUpdate().Where(_ => handler.isDone == true).Subscribe(_ => { clickstream.Dispose(); Debug.Log("테스트!"); });
        


        Logger.Log("Wait_click");
    }

    public void CheckClick(GameObject target) {       
        if (target == null) {
            ScenarioMask.Instance.StopEveryHighlight();
            clickstream.Dispose();
            handler.isDone = true;
        }
        else {
            UnityEngine.EventSystems.PointerEventData clickEvent = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
            if (clickEvent.pointerPress.gameObject.name == target.name) {
                clickstream.Dispose();
                scenarioMask.StopEveryHighlight();
                handler.isDone = true;
            }
            else
                handler.isDone = false;
        }
    }

    public void CheckButton() {
        clickstream.Dispose();
        scenarioMask.StopEveryHighlight();
        handler.isDone = true;
    }


}

public class Wait_summon : ScenarioExecute {
    public Wait_summon() : base() { }

    public override void Execute() {
        PlayMangement.instance.EventHandler.AddListener(IngameEventHandler.EVENT_TYPE.UNIT_SUMMONED, CheckSummon);
    }
    

    private void CheckSummon(Enum event_type, Component Sender, object Param) {
        string unitID = (string)Param;


        if (unitID == args[1]) {
            PlayMangement.instance.EventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.UNIT_SUMMONED, CheckSummon);
            handler.isDone = true;
        }
    }
}

public class Wait_drop : ScenarioExecute {
    public Wait_drop() : base() { }

    public override void Execute() {
        PlayMangement.instance.EventHandler.AddListener(IngameEventHandler.EVENT_TYPE.MAGIC_USED, CheckMagicUse);
    }

    private void CheckMagicUse(Enum event_type, Component Sender, object Param) {
        string magicID = (string)Param;

        if(magicID == args[1]) {
            PlayMangement.instance.EventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.MAGIC_USED, CheckMagicUse);
            handler.isDone = true;
        }      
    }
}



public class Multiple_Highlight : ScenarioExecute {
    public Multiple_Highlight() : base() { }

    public override void Execute() {

        string[] Parse;
        GameObject target;

        for (int i = 0; i< args.Count; i++) {
            Parse = args[i].Split('-');

            if (Parse.Length > 1) {
                target = scenarioMask.GetMaskingObject(Parse[0], Parse[1]);
            }
            else
                target = scenarioMask.GetMaskingObject(args[i]);

            scenarioMask.SetHighlightImage(target);
        }

        //Highlighting(ScenarioMask.Instance.GetMaskingObject(args[0]));
        //for(int i = 0; i < args.Count; i++) {
        //    GameObject target = ScenarioMask.Instance.GetMaskingObject(args[i]);
        //    ScenarioMask.Instance.SetHighlightImage(target);
        //}
        handler.isDone = true;
    }

}

public class Wait_Drag : ScenarioExecute {
    public Wait_Drag() : base() { }

    public override void Execute() {
        PlayMangement.instance.EventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.BEGIN_CARD_PLAY, CheckDrag);
        PlayMangement.instance.EventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.UNIT_DROP_FAIL, RollBack);

        PlayMangement.instance.EventHandler.AddListener(IngameEventHandler.EVENT_TYPE.BEGIN_CARD_PLAY, CheckDrag);
        PlayMangement.instance.EventHandler.AddListener(IngameEventHandler.EVENT_TYPE.UNIT_DROP_FAIL, RollBack);
    }

    private void CheckDrag(Enum event_type, Component Sender, object Param) {
        object[] parms = (object[])Param;
        if(parms.Length > 1) {
            GameObject cardObject = (GameObject)parms[1];
            CardHandler card = cardObject.GetComponent<CardHandler>();

            if(card != null) {
                if(card.cardData.cardId == args[1]) {
                    scenarioMask.StopEveryHighlight();
                    handler.isDone = true;
                }
            }
        } 
    }

    private void RollBack(Enum event_type, Component Sender, object Param) {
        if(args.Count >= 3 && args[2].Contains("rollback")) {
            Logger.Log("Rollback!");

            string[] parsed = args[2].Split('_');
            int index = -1;
            int.TryParse(parsed[1], out index);

            if(index != -1) {
                scenarioMask.StopEveryHighlight();
                handler.RollBack(1);
            }
        }
    }
}

public class Activate_shield : ScenarioExecute {
    public Activate_shield() : base() { }

    public override void Execute() {
        PlayMangement.instance.player.ActiveShield();
        handler.isDone = true;
    }
}



public class Fill_shield_gage : ScenarioExecute {
    public Fill_shield_gage() : base() { }

    public override void Execute() {
        StartCoroutine(Fill_gage());
    }

    IEnumerator Fill_gage() {
        yield return new WaitUntil(() => PlayMangement.instance.player.shieldStack.Value >= 1);
        PlayMangement.instance.player.FullShieldStack(PlayMangement.instance.player.shieldStack.Value);
        PlayMangement.instance.player.shieldStack.Value = 8;
        handler.isDone = true;
    }
}

public class End_tutorial : ScenarioExecute {
    public End_tutorial() : base() { }

    public override void Execute() {
        ScenarioGameManagment.scenarioInstance.isTutorial = false;
    }
}

public class Battle_turn : ScenarioExecute {
    public Battle_turn() : base() { }

    int Line = -1;

    public override void Execute() {
        switch (args[0]) {
            case "stop":
                int.TryParse(args[1], out Line);
                scenarioGameManagment.StopBattle(Line);
                break;
            case "proceed":
                scenarioGameManagment.BattleResume();
                break;
        }

        handler.isDone = true;
    }
}


public class Select_Skill_Force : ScenarioExecute {
    public Select_Skill_Force() : base() { }

    public override void Execute() {

        
    }

    public void HighlightLine() {
        GameObject targetLine;
        targetLine = scenarioMask.GetMaskingObject(args[0], args[1]);
        scenarioMask.GetMaskHighlight(targetLine);
    }
}

public class Enable_EndTurn : ScenarioExecute {
    public Enable_EndTurn() : base() { }

    public override void Execute() {
        GameObject endTurn = scenarioMask.GetMaskingObject("button", "endTurn");
        endTurn.GetComponent<Button>().enabled = true;

        handler.isDone = true;
    }
}

public class Disable_EndTurn : ScenarioExecute {
    public Disable_EndTurn() : base() { }

    public override void Execute() {
        GameObject endTurn = scenarioMask.GetMaskingObject("button", "endTurn");
        endTurn.GetComponent<Button>().enabled = false;

        handler.isDone = true;
    }
}

public class Disable_to_hand_herocard : ScenarioExecute {
    public Disable_to_hand_herocard() : base() { }

    public override void Execute() {
        scenarioGameManagment.canHeroCardToHand = false;

        handler.isDone = true;
    }
}

public class Enable_to_hand_herocard : ScenarioExecute {
    public Enable_to_hand_herocard() : base() { }

    public override void Execute() {
        scenarioGameManagment.canHeroCardToHand = true;

        handler.isDone = true;
    }
}

public class Disable_drag : ScenarioExecute {
    public Disable_drag() : base() { }

    public override void Execute() {
        string _target = args[0];
        List<GameObject> targets = new List<GameObject>();
        switch (_target) {
            case "shieldCard":
                string subArgs = args[1];
                if(subArgs == "exclusive") {
                    Transform showCardPos = scenarioGameManagment.showCardPos;

                    foreach(Transform child in showCardPos.Find("Left").transform) {
                        if (child.GetComponent<MagicDragHandler>()) {
                            targets.Add(child.gameObject);
                        }
                    }
                    foreach(Transform child in showCardPos.Find("Right").transform) {
                        if (child.GetComponent<MagicDragHandler>()) {
                            targets.Add(child.gameObject);
                        }
                    }
                    
                    targets.RemoveAll(x => x.GetComponent<MagicDragHandler>().cardData.cardId == args[2]);
                    foreach(var card in targets) {
                        card.GetComponent<MagicDragHandler>().enabled = false;
                    }
                }
                break;
        }
        handler.isDone = true;
    }

    public class Enable_drag : ScenarioExecute {
        public Enable_drag() : base() { }

        public override void Execute() {
            string _target = args[0];
            switch (_target) {
                case "shieldCards":
                    Transform showCardPos = scenarioGameManagment.showCardPos;

                    foreach (Transform child in showCardPos.Find("Left").transform) {
                        if (child.GetComponent<MagicDragHandler>()) {
                            child.GetComponent<MagicDragHandler>().enabled = true;
                        }
                    }
                    foreach (Transform child in showCardPos.Find("Right").transform) {
                        if (child.GetComponent<MagicDragHandler>()) {
                            child.GetComponent<MagicDragHandler>().enabled = true;
                        }
                    }
                    break;
            }
        }
    }
}

public class Wait_orc_pre_turn : ScenarioExecute {
    public Wait_orc_pre_turn() : base() { }

    private void Awake() {
        ScenarioGameManagment.scenarioInstance.EventHandler.AddListener(IngameEventHandler.EVENT_TYPE.BEGIN_ORC_PRE_TURN, OnOrcPostTurn);
    }

    public override void Execute() {
        Logger.Log("Execute Wait_orc_pre_turn");
    }

    private void OnOrcPostTurn(Enum Event_Type, Component Sender, object Param) {
        ScenarioGameManagment.scenarioInstance.EventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.BEGIN_ORC_PRE_TURN, OnOrcPostTurn);
        handler.isDone = true;
    }
}

public class Stop_orc_summon : ScenarioExecute {
    public Stop_orc_summon() : base() { }

    public override void Execute() {
        scenarioGameManagment.stopEnemySummon = true;
        handler.isDone = true;
    }
}

public class Proceed_orc_summon : ScenarioExecute {
    public Proceed_orc_summon() : base() { }

    public override void Execute() {
        scenarioGameManagment.stopEnemySummon = false;
        handler.isDone = true;
    }
}

public class Force_drop_zone : ScenarioExecute {
    public Force_drop_zone() : base() { }

    public override void Execute() {
        if(args.Count == 0) {
            Logger.Log("Args가 없어 skip");
            handler.isDone = true;
            return;
        }

        Type _type = Type.NONE;
        int detail = -1;
        switch (args[0]) {
            case "unit":
                try {
                    int.TryParse(args[1], out detail);
                    _type = Type.UNIT;
                }
                catch (ArgumentException) {
                    Logger.Log("Unit 소환에 대한 세부 정보 없음");
                }
                break;
            case "magic":
                _type = Type.MAGIC;
                break;
        }

        if(_type == Type.NONE || detail == -1) {
            handler.isDone = true;
            return;
        }

        if(_type == Type.UNIT) {
            scenarioGameManagment.forcedSummonAt = detail;
        }
        handler.isDone = true;
    }

    internal enum Type {
        UNIT,
        MAGIC,
        NONE
    }
}

public class Remove_forced_drop_zone : ScenarioExecute {
    public Remove_forced_drop_zone() : base() { }

    public override void Execute() {
        scenarioGameManagment.forcedSummonAt = -1;

        handler.isDone = true;
    }
}

public class Orc_post_turn : ScenarioExecute {
    public Orc_post_turn() : base() { }

    public override void Execute() {
        scenarioMask.StopEveryHighlight();

        switch (args[0]) {
            case "stop":
                PlayMangement.instance.stopTurn = true;
                break;
            case "proceed":
                PlayMangement.instance.stopTurn = false;
                break;
            default:
                break;
        }
        handler.isDone = true;
    }
}

public class OffHighlight : ScenarioExecute {
    public OffHighlight() : base() { }

    public override void Execute() {
        scenarioMask.DisableMask();
        handler.isDone = true;
    }
}

public class Enable_drag : ScenarioExecute {
    public Enable_drag() : base() { }

    public override void Execute() {
        handler.isDone = true;
    }
}

public class Stop_orc_turn : ScenarioExecute {
    public Stop_orc_turn() : base() { }

    public override void Execute() {

        PlayMangement.instance.stopTurn = true;
        handler.isDone = true;
    }

}

public class Proceed_orc_turn : ScenarioExecute {
    public Proceed_orc_turn() : base() { }

    public override void Execute() {
        PlayMangement.instance.stopTurn = false;
        handler.isDone = true;
    }


}

public class Wait_Battle_End : ScenarioExecute {
    public Wait_Battle_End() : base() { }

    public override void Execute() {
        PlayMangement.instance.EventHandler.AddListener(IngameEventHandler.EVENT_TYPE.END_BATTLE_TURN, CheckEnd);
    }

    private void CheckEnd(Enum event_type, Component Sender, object Param) {
        PlayMangement.instance.EventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.END_BATTLE_TURN, CheckEnd);
        handler.isDone = true;
    }
}

public class Block_Turn_Btn : ScenarioExecute {
    public Block_Turn_Btn() : base() { }

    public override void Execute() {
        scenarioMask.BlockButton();
        handler.isDone = true;
    }

}

public class Unblock_Turn_Btn : ScenarioExecute {
    public Unblock_Turn_Btn() : base() { }

    public override void Execute() {
        scenarioMask.UnblockButton();
        handler.isDone = true;
    }
}

public class Wait_Turn : ScenarioExecute {
    public Wait_Turn() : base() { }

    IngameEventHandler.EVENT_TYPE eventType;

    public override void Execute() {

        switch (args[0]) {
            case "ORC":
                eventType = IngameEventHandler.EVENT_TYPE.BEGIN_ORC_PRE_TURN;
                break;
            case "HUMAN":
                eventType = IngameEventHandler.EVENT_TYPE.BEGIN_HUMAN_TURN;
                break;
            case "SECRET":
                eventType = IngameEventHandler.EVENT_TYPE.BEGIN_ORC_POST_TURN;
                break;
            case "BATTLE":
                eventType = IngameEventHandler.EVENT_TYPE.BEGIN_BATTLE_TURN;
                break;

        }
        PlayMangement.instance.EventHandler.AddListener(eventType, CheckTurn);

    }

    protected void CheckTurn(Enum event_type, Component Sender, object Param) {
        PlayMangement.instance.EventHandler.RemoveListener(event_type, CheckTurn);
        handler.isDone = true;
    }
}

public class Block_Screen : ScenarioExecute {
    public Block_Screen() : base() { }

    public override void Execute() {
        scenarioMask.MaskScreen();
        handler.isDone = true;
    }
}

public class Unblock_Screen : ScenarioExecute {
    public Unblock_Screen() : base() { }

    public override void Execute() {
        scenarioMask.OffMaskScreen();
        handler.isDone = true;
    }

}

