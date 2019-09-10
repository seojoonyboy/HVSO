using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScenarioExecute : MonoBehaviour {
    public ScenarioExecuteHandler handler;
    public List<string> args;

    public virtual void Initialize(List<string> args) {
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
        target = (args.Count > 1) ? ScenarioMask.Instance.GetMaskingObject(args[0], args[1]) : ScenarioMask.Instance.GetMaskingObject(args[0]);
        ScenarioMask.Instance.GetMaskHighlight(target);
        handler.isDone = true;
        Logger.Log("Highlight");
    }

}

public class Wait_until : ScenarioExecute {
    public Wait_until() : base() { }

    public override void Execute() {
        var parms = args;
        float sec = 0;
        float.TryParse(parms[0], out sec);
        StartCoroutine(WaitSec(sec));
    }

    IEnumerator WaitSec(float sec = 0) {
        Logger.Log("WaitSec");
        yield return new WaitForSeconds(sec);
        handler.isDone = true;
    }
}

public class Wait_click : ScenarioExecute {
    public Wait_click() : base() { }

    public override void Execute() {
        StartCoroutine(WaitClick());
        Logger.Log("Wait_click");
    }

    IEnumerator WaitClick() {
        GameObject target;

        if (args[0] == "screen")
            target = null;
        else if (args.Count > 1)
            target = ScenarioMask.Instance.GetMaskingObject(args[0], args[1]);
        else
            target = ScenarioMask.Instance.GetMaskingObject(args[0]);

        Button button = target.GetComponent<Button>();
        bool buttonClick = false;
        if(button != null) {
            button.onClick.AddListener(delegate () { buttonClick = true; });
        }


        while (handler.isDone == false) {
            
            if(Input.GetMouseButton(0) == true) {

                if (target == null)
                    handler.isDone = true;
                else {
                    if (button != null) {
                        handler.isDone = (buttonClick == true) ? true : false;
                    }
                    else {
                        UnityEngine.EventSystems.PointerEventData clickEvent = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
                        if (clickEvent.pointerPress.gameObject.name == target.name)
                            handler.isDone = true;
                        else
                            handler.isDone = false;
                    }              
                }
            }
        }


        yield return null;
    }
}

public class wait_summon : ScenarioExecute {
    public wait_summon() : base() { }

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


public class Multiple_highlight : ScenarioExecute {
    public Multiple_highlight() : base() { }

    public override void Execute() {
        Highlighting(ScenarioMask.Instance.GetMaskingObject(args[0]));
    }

    public void Highlighting(GameObject target) {

    }

}

public class Wait_Drag : ScenarioExecute {
    public Wait_Drag() : base() { }

    public override void Execute() {
        StartCoroutine(CheckDrag());
    }

    IEnumerator CheckDrag() {
        while(handler.isDone == false) {
            UnityEngine.EventSystems.PointerEventData clickEvent = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);

            CardHandler card = clickEvent.pointerDrag.gameObject.GetComponent<CardHandler>();

            if (card.cardID == args[1])
                handler.isDone = true;
        }
        yield return null;
    }

}

public class Activate_Shield : ScenarioExecute {
    public Activate_Shield() : base() { }

    public override void Execute() {
        PlayMangement.instance.player.ActiveShield();
    }
}



public class Fill_shield_gage : ScenarioExecute {
    public Fill_shield_gage() : base() { }

    public override void Execute() {
        PlayMangement.instance.player.ChangeShieldStack(0,8);
        PlayMangement.instance.player.FullShieldStack(8);
    }
}


public class Summon_Force : ScenarioExecute {
    public Summon_Force() : base() { }

    public override void Execute() {
        HighlightLine();
    }

    public void HighlightLine() {
        GameObject targetLine;
        targetLine = ScenarioMask.Instance.GetMaskingObject(args[0], args[1]);
        ScenarioMask.Instance.GetMaskHighlight(targetLine);
    }
}

public class End_tutorial : ScenarioExecute {
    public End_tutorial() : base() { }

    public override void Execute() {
        ScenarioGameManagment.scenarioInstance.isTutorial = false;
    }

}

