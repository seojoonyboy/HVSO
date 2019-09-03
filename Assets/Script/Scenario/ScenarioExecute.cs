using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioExecute : MonoBehaviour {
    public ScenarioGameManagment ScenarioGame;

    public virtual void Execute(object data = null) { }

}

public class PRINT_MESSAGE : ScenarioExecute {
    public PRINT_MESSAGE() : base() { }

    public override void Execute(object data = null) {
        
    }
}

public class HIGHLIGHT : ScenarioExecute {
    public HIGHLIGHT() :base() { }

    public override void Execute(object data = null) {
        
    }

}

public class WAIT_UNTIL : ScenarioExecute {
    public WAIT_UNTIL() : base() { }

    public override void Execute(object data = null) {
        
    }

}

public class OFFHIGHLIGHT : ScenarioExecute {
    public OFFHIGHLIGHT() : base() {}

    public override void Execute(object data = null) {

    }
}

public class OFFMESSAGE : ScenarioExecute {
    public OFFMESSAGE() : base() {}

    public override void Execute(object data = null) {

    }
}

public class GOTO_NEXT : ScenarioExecute {
    public GOTO_NEXT() : base() { }

    public override void Execute(object data = null) {

    }
}

