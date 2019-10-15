using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuExecuteHandler : MonoBehaviour {
    MenuTutorialManager tutorialManager;
    void Start() {
        tutorialManager = GetComponent<MenuTutorialManager>();
    }

    public virtual void Initialize() {
        
    }
}

public class ToOrcStoryExecuteHandler : MenuExecuteHandler {
    
}

public class ToAIBattleExecuteHandler : MenuExecuteHandler {

}

public class ToBoxOpenExecuteHandler : MenuExecuteHandler {

}
