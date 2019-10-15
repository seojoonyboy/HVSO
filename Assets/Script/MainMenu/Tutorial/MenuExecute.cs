using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;

public class MenuExecute : MonoBehaviour {
    public MenuExecuteHandler handler;
    public List<string> args;

    public virtual void Initialize(List<string> args) {
        this.args = args;
        handler = GetComponent<MenuExecuteHandler>();
    }

    public virtual void Execute() { }
}

public class Wait_MainMenu_Click : MenuExecute {
    public Wait_MainMenu_Click() : base() { }

    IDisposable clickStream;

    //args[0] screen 또는 Dictionary 키값, args[1] 예비
    public override void Execute() {
        GameObject target;

        if (args[0] == "screen")
            target = null;
        else if (args.Count > 1)
            target = MenuMask.Instance.GetMenuObject(args[0], args[1]);       
        else
            target = MenuMask.Instance.GetMenuObject(args[0]);

        Button button = (target != null) ? target.GetComponent<Button>() : null;
        clickStream = (button != null) ? button.OnClickAsObservable().Subscribe(_=>CheckButton()) : Observable.EveryUpdate().Where(_ => Input.GetMouseButtonDown(0)).Subscribe(_ => CheckClick(target));
    }

    private void CheckClick(GameObject target) {
        if(target == null) {
            clickStream.Dispose();
            handler.isDone = true;
        }

    }

    private void CheckButton() {
        clickStream.Dispose();
        handler.isDone = true;
    }
}

public class Menu_NPC_Talk : MenuExecute {
    public Menu_NPC_Talk() : base() { }

    //args[0] 유닛 id, args[1] 대사내용, args[2] my,enemy
    public override void Execute() {
        MenuMask menuMask = MenuMask.Instance;
        MenuMask.Instance.gameObject.SetActive(true);



        
        bool isPlayer = true;
        if (args[2] == "enemy")
            isPlayer = false;

        menuMask.menuTalkPanel.transform.Find("CharacterImage/Player").gameObject.SetActive(isPlayer);
        menuMask.menuTalkPanel.transform.Find("CharacterImage/Enemy").gameObject.SetActive(!isPlayer);
        menuMask.menuTalkPanel.transform.Find("NameObject/PlayerName").gameObject.SetActive(isPlayer);
        menuMask.menuTalkPanel.transform.Find("NameObject/EnemyName").gameObject.SetActive(!isPlayer);
        if (isPlayer) {
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


