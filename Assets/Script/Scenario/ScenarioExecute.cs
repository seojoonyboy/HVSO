using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioExecute : MonoBehaviour {
    protected ScenarioExecuteHandler handler;
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
        handler.isDone = true;
        Logger.Log("Wait_click");
    }
}

