using System.Collections;
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

public class Fill_shield_gage : ScenarioExecute {
    public Fill_shield_gage() : base() { }

    public override void Execute() {
    }
}


public class Summon_Force : ScenarioExecute {
    public Summon_Force() : base() { }

    public override void Execute() {
    }
}

