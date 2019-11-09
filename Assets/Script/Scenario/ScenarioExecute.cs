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
        scenarioMask.ShowText(args[0]);
        if (args.Count > 1) {
            scenarioMask.SetPosText(args[1]);
        }
        else
            scenarioMask.SetPosText();
        handler.isDone = true;
    }
}

public class Hide_message : ScenarioExecute {
    public Hide_message() : base() { }

    public override void Execute() {
        scenarioMask.HideText();
        handler.isDone = true;
    }
}

//args[0] npc id 또는 이름
//args[1] 텍스트 내용
//args[2] isPlayer


public class NPC_Print_message : ScenarioExecute {
    public NPC_Print_message() : base() { }

    public override void Execute() {
        scenarioMask.talkingText.SetActive(true);
        bool isPlayer = false;
        if (args[2] == "true")
            isPlayer = true;


        scenarioMask.talkingText.transform.position = scenarioMask.textDown.transform.position;
        if (args.Count > 3 && args[3] == "Top")
            scenarioMask.talkingText.transform.position = scenarioMask.textUP.transform.position;

        scenarioMask.talkingText.transform.Find("Panel").gameObject.SetActive(true);
        if (args.Count > 3 && args[3] == "maskOff")
            scenarioMask.talkingText.transform.Find("Panel").gameObject.SetActive(false);

        
        scenarioMask.talkingText.transform.Find("CharacterImage/Player").gameObject.SetActive(isPlayer);
        scenarioMask.talkingText.transform.Find("CharacterImage/Enemy").gameObject.SetActive(!isPlayer);
        scenarioMask.talkingText.transform.Find("NameObject/PlayerName").gameObject.SetActive(isPlayer);
        scenarioMask.talkingText.transform.Find("NameObject/EnemyName").gameObject.SetActive(!isPlayer);
        
        if (isPlayer) {
            scenarioMask.talkingText.transform.Find("CharacterImage/Player").GetComponent<Image>().sprite = AccountManager.Instance.resource.ScenarioUnitResurce[args[0]].sprite;
            scenarioMask.talkingText.transform.Find("NameObject/PlayerName").GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.resource.ScenarioUnitResurce[args[0]].name;
        }
        else {
            scenarioMask.talkingText.transform.Find("CharacterImage/Enemy").GetComponent<Image>().sprite = AccountManager.Instance.resource.ScenarioUnitResurce[args[0]].sprite;
            scenarioMask.talkingText.transform.Find("NameObject/EnemyName").GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.resource.ScenarioUnitResurce[args[0]].name;
        }
        scenarioMask.talkingText.GetComponent<TextTyping>().StartTyping(args[1], handler);

#if UNITY_EDITOR
        scenarioMask.talkingText.transform.Find("StopTypingTrigger").gameObject.SetActive(true);
#endif
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


        scenarioMask.SetHighlightImage(target);
        

        if (args[0] == "hand_card")
            Glowing(args[1]);
        else
            scenarioMask.GetMaskHighlight(target);

        handler.isDone = true;
        Logger.Log("Highlight");
    }

    private void Glowing(string ID) {
        scenarioMask.CardDeckGlow(ID);
    }
}


public class Till_On : ScenarioExecute {
    public Till_On() : base() { }

    public override void Execute() {
        scenarioMask.TillOn();
        handler.isDone = true;
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

public class Hero_Wait_Damage : ScenarioExecute {
    public Hero_Wait_Damage() : base() { }

    int waitCount = -1;
    int count = 0;

    public override void Execute() {
        waitCount = int.Parse(args[0]);
        PlayMangement.instance.EventHandler.AddListener(IngameEventHandler.EVENT_TYPE.HERO_UNDER_ATTACK, CheckCount);
    }

    private void CheckCount(Enum event_type, Component Sender, object Param) {
        bool isplayer = (bool)Param;

        if (args[1] == "player" && isplayer == true)
            count++;
        else if (args[1] == "enemy" && isplayer == false)
            count++;

        if(count == waitCount) {
            PlayMangement.instance.EventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.HERO_UNDER_ATTACK, CheckCount);
            handler.isDone = true;
        }
    }

}


public class Wait_click : ScenarioExecute {
    public Wait_click() : base() { }

    IDisposable clickstream;

    public override void Execute() {
        GameObject target;

        if (args[0] == "screen") {
            target = null;
        }
        else if (args.Count > 2)
            target = scenarioMask.GetMaskingObject(args[0], args[1], args[2]);
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
            clickstream.Dispose();
            scenarioMask.HideText();


            if (args.Count > 1 && args[1] == "off") {
                scenarioMask.StopEveryHighlight();
                scenarioMask.HideText();
                
            }
            scenarioMask.TillOff();
            handler.isDone = true;
        }
        else {
            UnityEngine.EventSystems.PointerEventData clickEvent = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
            if (clickEvent.pointerPress.gameObject.name == target.name) {
                clickstream.Dispose();
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

public class StopHighlight : ScenarioExecute {
    public StopHighlight() : base() { }

    public override void Execute() {
        scenarioMask.StopEveryHighlight();
        GameObject handicon = scenarioMask.GetMaskingObject("turn_handicon");
        handicon.SetActive(false);
        handler.isDone = true;
    }

}

//단순히 유닛 소환 기달림 args[0] unitID
public class Wait_summon : ScenarioExecute {
    public Wait_summon() : base() { }

    public override void Execute() {
        PlayMangement.instance.EventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.UNIT_SUMMONED, CheckSummon);
        PlayMangement.instance.EventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.UNIT_DROP_FAIL, Glowing);
        Glow();
        PlayMangement.instance.EventHandler.AddListener(IngameEventHandler.EVENT_TYPE.UNIT_SUMMONED, CheckSummon);
        PlayMangement.instance.EventHandler.AddListener(IngameEventHandler.EVENT_TYPE.UNIT_DROP_FAIL, Glowing);
    }
    

    private void CheckSummon(Enum event_type, Component Sender, object Param) {
        string unitID = (string)Param;


        if (unitID == args[0]) {
            PlayMangement.instance.EventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.UNIT_SUMMONED, CheckSummon);
            PlayMangement.instance.EventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.UNIT_DROP_FAIL, Glowing);
            handler.isDone = true;
        }
    }

    private void Glow() {
        scenarioMask.CardDeckGlow(args[0]);
    }


    private void Glowing(Enum event_type, Component Sender, object Param) {
        scenarioMask.CardDeckGlow(args[0]);
    }

}

public class Disable_Deck_card : ScenarioExecute {
    public Disable_Deck_card() : base() { }

    //args[0] 카드 아이디
    //특정 카드 이외의 드래그 제외
    public override void Execute() {
        GameObject cardHand = scenarioMask.targetObject["hand_card"];
        
        foreach(Transform slot in cardHand.transform) {
            if (slot.childCount < 1)
                continue;

            CardHandler card = slot.GetChild(0).gameObject.GetComponent<CardHandler>();

            if (card.cardID != args[0])
                slot.GetChild(0).gameObject.GetComponent<CardHandler>().enabled = false;
            else {
                slot.GetChild(0).gameObject.GetComponent<CardHandler>().enabled = true;
            }
        }
        
        handler.isDone = true;
    }

    private void Glowing(string ID) {
        scenarioMask.CardDeckGlow(ID);
    }

}

public class Enable_Deck_card : ScenarioExecute {
    public Enable_Deck_card() : base() { }

    public override void Execute() {
        GameObject cardHand = scenarioMask.targetObject["hand_card"];

        foreach(Transform slot in cardHand.transform) {
            if (slot.childCount < 1)
                continue;
            slot.GetChild(0).gameObject.GetComponent<CardHandler>().enabled = true;
        }
        handler.isDone = true;
    }

}



//args[0] 얼마나? 
public class Wait_Multiple_Summon : ScenarioExecute {
    public Wait_Multiple_Summon() : base() { }

    private int summonCount = 0;
    private int clearCount = -1;
    private string[] numParse;
    private int[] line;

    public override void Execute() {
        if (args.Count > 1) {
            numParse = args[1].Split('-');
            line = new int[numParse.Length];

            for(int i =0; i<numParse.Length; i++) {
                line[i] = int.Parse(numParse[i]);
            }
            scenarioGameManagment.forcedSummonAt = line[0];
        }
        scenarioMask.CardDeckGlow();
        int.TryParse(args[0], out clearCount);
        PlayMangement.instance.EventHandler.AddListener(IngameEventHandler.EVENT_TYPE.UNIT_SUMMONED, CheckSummon);
        PlayMangement.instance.EventHandler.AddListener(IngameEventHandler.EVENT_TYPE.UNIT_DROP_FAIL, HighLightOn);
    }

    private void CheckSummon(Enum event_type, Component Sender, object Param) {

        summonCount++;       
        

        if (line != null && line.Length > 0 && summonCount < line.Length) {
            scenarioGameManagment.forcedSummonAt = line[summonCount];
            Invoke("Glowing", 0.1f);
        }
        if (summonCount == clearCount) {
            PlayMangement.instance.EventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.UNIT_SUMMONED, CheckSummon);
            PlayMangement.instance.EventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.UNIT_DROP_FAIL, HighLightOn);
            scenarioGameManagment.forcedSummonAt = -1;
            scenarioMask.OffDeckCardGlow();
            handler.isDone = true;
        }
    }

    private void Glowing() {
        scenarioMask.CardDeckGlow();
    }

    private void HighLightOn(Enum event_type, Component Sender, object Param) {
        scenarioMask.CardDeckGlow();
    }
}

// args[0] x,y x는 어디 어디 배치할껀가?
public class Wait_Multiple_Summon_ScopeLine : ScenarioExecute {
    public Wait_Multiple_Summon_ScopeLine() : base() { }

    private int summonCount = 0;
    private int clearCount = -1;
    private string[] stringNum;
    private int[] line;

    
    public override void Execute() {
        stringNum = args[0].Split(',');
        line = new int[stringNum.Length];

        for(int i = 0; i< stringNum.Length; i++) {
            line[i] = int.Parse(stringNum[i]);
        }
        scenarioGameManagment.multipleforceLine = line;
        clearCount = stringNum.Length;

        Glowing();
        PlayMangement.instance.EventHandler.AddListener(IngameEventHandler.EVENT_TYPE.UNIT_SUMMONED, CheckSummon);
        PlayMangement.instance.EventHandler.AddListener(IngameEventHandler.EVENT_TYPE.UNIT_DROP_FAIL, HighLightOn);
    }

    private void CheckSummon(Enum event_type, Component Sender, object Param) {
        summonCount++;
        
        if (summonCount == clearCount) {
            PlayMangement.instance.EventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.UNIT_SUMMONED, CheckSummon);
            PlayMangement.instance.EventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.UNIT_DROP_FAIL, HighLightOn);
            scenarioGameManagment.multipleforceLine[0] = -1;
            scenarioGameManagment.multipleforceLine[1] = -1;
            handler.isDone = true;
        }

        if (summonCount < clearCount)
            Invoke("Glowing", 0.3f);


    }

    private void Glowing() {
        if (args.Count > 1)
            scenarioMask.CardDeckGlow(args[1]);
        else
            scenarioMask.CardDeckGlow();
    }


    private void HighLightOn(Enum event_type, Component Sender, object Param) {
        if (args.Count > 1)
            scenarioMask.CardDeckGlow(args[1]);
        else
            scenarioMask.CardDeckGlow();
    }
}


//args[0] 얼마나 소환할지? args[1] 1~x 라인중 x까지 라인 활성화
public class Wait_Multiple_Summon_linelimit : ScenarioExecute {
    public Wait_Multiple_Summon_linelimit() : base() { }

    private int summonCount = 0;
    private int clearCount = -1;
    private int line;

    public override void Execute() {
        if (args.Count > 1) {
            line = int.Parse(args[1]);            
            scenarioGameManagment.forcedLine = line;
        }

        scenarioMask.CardDeckGlow("ac10018");
        int.TryParse(args[0], out clearCount);
        PlayMangement.instance.EventHandler.AddListener(IngameEventHandler.EVENT_TYPE.UNIT_SUMMONED, CheckSummon);
        PlayMangement.instance.EventHandler.AddListener(IngameEventHandler.EVENT_TYPE.UNIT_DROP_FAIL, HighLightOn);
    }

    private void CheckSummon(Enum event_type, Component Sender, object Param) {

        summonCount++;

        Invoke("Glowing", 0.1f);
        if (summonCount == clearCount) {
            PlayMangement.instance.EventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.UNIT_SUMMONED, CheckSummon);
            PlayMangement.instance.EventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.UNIT_DROP_FAIL, HighLightOn);
            scenarioGameManagment.forcedLine = -1;
            handler.isDone = true;

            scenarioMask.OffDeckCardGlow();
        }
    }
    
    private void Glowing() {
        scenarioMask.CardDeckGlow("ac10018");
    }


    private void HighLightOn(Enum event_type, Component Sender, object Param) {
        scenarioMask.CardDeckGlow("ac10018");
    }
}




// 마법사용 대기
public class Wait_drop : ScenarioExecute {
    public Wait_drop() : base() { }

    public override void Execute() {
        Glowing(args[1]);
        PlayMangement.instance.EventHandler.AddListener(IngameEventHandler.EVENT_TYPE.MAGIC_USED, CheckMagicUse);
        PlayMangement.instance.EventHandler.AddListener(IngameEventHandler.EVENT_TYPE.UNIT_DROP_FAIL, GlowTrigger);
    }

    private void CheckMagicUse(Enum event_type, Component Sender, object Param) {
        string magicID = (string)Param;

        if(magicID == args[1]) {
            PlayMangement.instance.EventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.MAGIC_USED, CheckMagicUse);
            PlayMangement.instance.EventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.UNIT_DROP_FAIL, GlowTrigger);
            handler.isDone = true;
        }      
    }

    private void Glowing(string id) {
        scenarioMask.CardDeckGlow(id);
    }

    private void GlowTrigger(Enum event_type, Component Sender, object Param) {
        scenarioMask.CardDeckGlow(args[1]);
    }
}

public class Wait_AnyMagic_Use : ScenarioExecute {
    public override void Execute() {
        //Glowing(args[1]);
        PlayMangement.instance.EventHandler.AddListener(IngameEventHandler.EVENT_TYPE.MAGIC_USED, CheckMagicUse);
    }

    private void CheckMagicUse(Enum event_type, Component Sender, object Param) {
        string magicID = (string)Param;

        PlayMangement.instance.EventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.MAGIC_USED, CheckMagicUse);
        handler.isDone = true;
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
                if(card.cardData.id == args[1]) {
                    scenarioMask.StopEveryHighlight();
                    handler.isDone = true;
                }
            }
        } 
    }

    private void OnDestroy() {
        PlayMangement.instance.EventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.BEGIN_CARD_PLAY, CheckDrag);
        PlayMangement.instance.EventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.UNIT_DROP_FAIL, RollBack);
    }

    private void RollBack(Enum event_type, Component Sender, object Param) {
        if(args.Count >= 3 && args[2].Contains("rollback")) {
            Logger.Log("Rollback!");

            string[] parsed = args[2].Split('_');
            int index = -1;
            int.TryParse(parsed[1], out index);

            if(index != -1) {
                scenarioMask.StopEveryHighlight();
                handler.RollBack(index);
            }
            else {
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
        scenarioMask.UnmaskHeroGuide();
        scenarioMask.HideText();
        ScenarioGameManagment.scenarioInstance.isTutorial = false;
        ScenarioGameManagment.scenarioInstance.socketHandler.TutorialEnd();
    }
}

//배틀턴 args[0] stop or proceed, args[1] 몇라인?
public class Battle_turn : ScenarioExecute {
    public Battle_turn() : base() { }

    int Line = -1;

    public override void Execute() {
        switch (args[0]) {
            case "stop":
                if (args.Count > 1) {
                    int.TryParse(args[1], out Line);
                    scenarioGameManagment.StopBattle(Line);
                }
                else
                    scenarioGameManagment.stopBattle = true;
                break;
            case "proceed":
                scenarioGameManagment.BattleResume();
                scenarioGameManagment.stopBattle = false;
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

//턴버튼 무력화
public class Enable_EndTurn : ScenarioExecute {
    public Enable_EndTurn() : base() { }

    public override void Execute() {
        GameObject endTurn = scenarioMask.GetMaskingObject("button", "endTurn");
        GameObject handicon = scenarioMask.GetMaskingObject("turn_handicon");
        endTurn.GetComponent<Button>().enabled = true;
        handicon.SetActive(true);
        
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
                    
                    targets.RemoveAll(x => x.GetComponent<MagicDragHandler>().cardData.id == args[2]);
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

public class Stop_orc_magic : ScenarioExecute {
    public Stop_orc_magic() : base() { }

    public override void Execute() {
        scenarioGameManagment.stopEnemySpell = true;
        handler.isDone = true;
    }

}

public class Proceed_orc_magic : ScenarioExecute {
    public Proceed_orc_magic() : base() { }
    public override void Execute() {
        scenarioGameManagment.stopEnemySpell = false;
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
                try {
                    int.TryParse(args[1], out detail);
                    _type = Type.MAGIC;

                    if (detail == 5) {
                        if (scenarioGameManagment.UnitsObserver.IsUnitExist(new FieldUnitsObserver.Pos(4, 0), PlayMangement.instance.player.isHuman) || scenarioGameManagment.UnitsObserver.IsUnitExist(new FieldUnitsObserver.Pos(4, 1), PlayMangement.instance.player.isHuman)) {
                            detail = 4;
                        }
                    }
                }
                catch (ArgumentException) {
                    Logger.Log("Unit 소환에 대한 세부 정보 없음");
                }
                break;
        }

        if(_type == Type.NONE || detail == -1) {
            handler.isDone = true;
            return;
        }


        if (args.Count > 2)
            scenarioGameManagment.targetArgs = args[2];

        scenarioGameManagment.forcedSummonAt = detail;
        handler.isDone = true;
    }

    internal enum Type {
        UNIT,
        MAGIC,
        NONE
    }
}

public class Reinforement_Unit : ScenarioExecute {
    public Reinforement_Unit() : base() { }

    public override void Execute() {
        string front = "ac10001";
        string back = "ac10002";

        for (int i = 0; i < 5; i++) {
            
            scenarioGameManagment.SummonUnit(scenarioGameManagment.player.isPlayer, back, i, 0);
            scenarioGameManagment.SummonUnit(scenarioGameManagment.player.isPlayer, front, i, 1);
        }
        handler.isDone = true;
    }


}


//배틀 끝나고 다음턴
public class Stop_Next_Turn : ScenarioExecute {
    public Stop_Next_Turn() : base() { }

    public override void Execute() {
        scenarioGameManagment.stopNextTurn = true;
        handler.isDone = true;
    }
}

public class Proceed_Next_Turn : ScenarioExecute {
    public Proceed_Next_Turn() : base() { }

    public override void Execute() {
        scenarioGameManagment.stopNextTurn = false;
        handler.isDone = true;
    }

}

public class Set_Victory : ScenarioExecute {
    public Set_Victory() : base() { }

    public override void Execute() {

        handler.isDone = true;
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
                if (args.Count > 1) {
                    switch (args[1]) {
                        case "begin":
                            PlayMangement.instance.beginStopTurn = true;
                            break;
                        case "after":
                            PlayMangement.instance.afterStopTurn = true;
                            break;
                        default:
                            PlayMangement.instance.beginStopTurn = true;
                            break;
                    }
                }
                else
                    PlayMangement.instance.beginStopTurn = true;
                break;
            case "proceed":
                PlayMangement.instance.stopTurn = false;
                PlayMangement.instance.beginStopTurn = false;
                PlayMangement.instance.afterStopTurn = false;
                break;
            default:
                break;
        }
        handler.isDone = true;
    }
}
//사각형으로 된 오브젝트들 setactive = false
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
//전투 종료를 기달림
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
//미사용 함수 폐기예정
public class Block_Turn_Btn : ScenarioExecute {
    public Block_Turn_Btn() : base() { }

    public override void Execute() {
        scenarioMask.BlockButton();
        handler.isDone = true;
    }

}
//미사용 함수 폐기예정
public class Unblock_Turn_Btn : ScenarioExecute {
    public Unblock_Turn_Btn() : base() { }

    public override void Execute() {
        scenarioMask.UnblockButton();
        handler.isDone = true;
    }
}
//몇번째 라인 전투 종료?
public class Wait_End_Line_Battle : ScenarioExecute {
    public Wait_End_Line_Battle() : base() { }

    IDisposable playerHit;

    public override void Execute() {
        PlayMangement.instance.EventHandler.AddListener(IngameEventHandler.EVENT_TYPE.LINE_BATTLE_FINISHED, CheckEnd);
        //int hp = PlayMangement.instance.player.HP.Value;
        //playerHit = PlayMangement.instance.player.HP.Where(x => x < hp).Subscribe(_ => CheckEnd());
    }

    private void CheckEnd(Enum event_type, Component Sender, object Param) {
        //playerHit.Dispose();
        int line = (int)Param;
        int targetLine = int.Parse(args[0]) - 1;


        if (line == targetLine)
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
        scenarioMask.DisableMask();
        handler.isDone = true;
    }

}

public class Wait_shield_active : ScenarioExecute {
    public Wait_shield_active() : base() { }

    bool isPlayer;

    public override void Execute() {
        PlayMangement.instance.EventHandler.AddListener(IngameEventHandler.EVENT_TYPE.HERO_SHIELD_ACTIVE, CheckActive);
    }

    protected void CheckActive(Enum event_type, Component Sender, object Param) {        

        if (args[0] == "player")
            isPlayer = true;
        else
            isPlayer = false;

        bool senderPlayer = (bool)Param;

        if (senderPlayer == isPlayer) {
            PlayMangement.instance.EventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.HERO_SHIELD_ACTIVE, CheckActive);
            handler.isDone = true;
        }
    }
}

public class ChallengeStart : ScenarioExecute {
    public ChallengeStart() : base() { }

    public override void Execute() {
        ChallengerHandler challengerHandler = PlayMangement.instance.GetComponent<ChallengerHandler>();

        challengerHandler.AddListener(args[0]);
        handler.isDone = true;
    }
}

public class ChallengeEnd : ScenarioExecute {
    public ChallengeEnd() : base() { }

    public override void Execute() {
        ChallengerHandler challengerHandler = PlayMangement.instance.GetComponent<ChallengerHandler>();

        challengerHandler.RemoveListener(args[0]);
        handler.isDone = true;
    }
}
// 턴과 턴사이, 배틀 종료 아님!
public class Stop_Invoke_NextTurn : ScenarioExecute {
    public Stop_Invoke_NextTurn() : base() {  }

    public override void Execute() {
        PlayMangement.instance.gameObject.GetComponent<TurnMachine>().turnStop = true;
        
        handler.isDone = true;
    }
}

public class Proceed_Invoke_NextTurn : ScenarioExecute {
    public Proceed_Invoke_NextTurn() : base() { }

    public override void Execute() {
        PlayMangement.instance.gameObject.GetComponent<TurnMachine>().turnStop = false;
        GameObject handicon = scenarioMask.GetMaskingObject("turn_handicon");
        handicon.SetActive(false);
        handler.isDone = true;
    }
}
//영웅카드 뽑을상황에 잠시 뽑지말도록 스탑
public class Wait_DrawHero : ScenarioExecute {
    public Wait_DrawHero() : base() { }

    public override void Execute() {
        PlayMangement.instance.waitDraw = true;
        handler.isDone = true;
    }
}

public class Proceed_DrawHero : ScenarioExecute {
    public Proceed_DrawHero() : base() { }

    public override void Execute() {
        PlayMangement.instance.waitDraw = false;
        handler.isDone = true;
    }
}

public class Fadeout_Enemy : ScenarioExecute {
    public Fadeout_Enemy() : base() { }

    public override void Execute() {
        StartCoroutine(ScenarioGameManagment.scenarioInstance.OpponentRanAway());
        handler.isDone = true;
    }
}

public class Wait_UnitPos_Change : ScenarioExecute {
    public Wait_UnitPos_Change() : base() { }

    public override void Execute() {
        PlayMangement.instance.EventHandler.AddListener(IngameEventHandler.EVENT_TYPE.FIELD_CHANGED, CheckChange);        
    }

    private void CheckChange(Enum event_type, Component Sender, object Param) {
        PlayMangement.instance.EventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.FIELD_CHANGED, CheckChange);
        handler.isDone = true;
    }
}

public class Result_battle : ScenarioExecute {
    public Result_battle() : base() { }

    public override void Execute() {

        if (args[0] == "stop")
            PlayMangement.instance.waitShowResult = true;
        else if (args[0] == "proceed")
            PlayMangement.instance.waitShowResult = false;


        handler.isDone = true;
    }
}

public class Wait_Enemy_hero_Dead : ScenarioExecute {
    public Wait_Enemy_hero_Dead() : base() { }

    IDisposable enemyDead;

    public override void Execute() {
        enemyDead = scenarioGameManagment.enemyPlayer.HP.Where(x => x <= 0).Subscribe(_ => CheckExecute());
    }

    private void CheckExecute() {
        enemyDead.Dispose();
        PlayMangement.instance.stopBattle = true;
        PlayMangement.instance.stopTurn = true;
        PlayMangement.instance.beginStopTurn = true;
        handler.isDone = true;
    }
}


//첫 카드배분 스탑 or 배분
public class First_Card : ScenarioExecute {
    public First_Card() : base() { }

    public override void Execute() {
        if (args[0] == "stop")
            PlayMangement.instance.stopFirstCard = true;
        else
            PlayMangement.instance.stopFirstCard = false;
        handler.isDone = true;
    }

}

/// <summary>
/// 적 유닛이 소환해야할 경우의 class
/// </summary>
public class Wait_Enemy_Summon : ScenarioExecute {
    public Wait_Enemy_Summon() : base() { }
    
    private int count = 0;
    private int goalCount = -1;

    public override void Execute() {
        goalCount = int.Parse(args[0]);
        PlayMangement.instance.EventHandler.AddListener(IngameEventHandler.EVENT_TYPE.ENEMY_SUMMON_UNIT, CheckSummon);
    }

    private void CheckSummon(Enum event_type, Component Sender, object Param) {
        count++;

        if(count >= goalCount) {
            PlayMangement.instance.EventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.ENEMY_SUMMON_UNIT, CheckSummon);
            handler.isDone = true;
        }

    }
}

public class Highlight_Unit : ScenarioExecute {
    public Highlight_Unit() : base() { }

    public override void Execute() {
        string cardId = args[0];
        List<GameObject> list = PlayMangement.instance.UnitsObserver.GetAllFieldUnits(false);
        GameObject target = list.Find((unit) => unit.GetComponent<PlaceMonster>().unit.id.CompareTo(cardId)== 0);
        scenarioMask.InfoTouchON(target.transform.position);
        handler.isDone = true;
    }
}


/// <summary>
/// 특정 유닛의 정보창을 띄워야 할 스크립트 args[0] 유닛 아이디
/// </summary>
public class Wait_Info_Window : ScenarioExecute {
    public Wait_Info_Window() : base() { }

    public override void Execute() {
        PlayMangement.instance.EventHandler.AddListener(IngameEventHandler.EVENT_TYPE.OPEN_INFO_WINDOW, CheckOpen);
    }

    private void CheckOpen(Enum event_type, Component Sender, object Param) {
        PlaceMonster placeMonster = (PlaceMonster)Param;
        string unitID = placeMonster.unit.id;

        if(unitID == args[0]) {
            PlayMangement.instance.EventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.OPEN_INFO_WINDOW, CheckOpen);
            handler.isDone = true;
        }
    }
}

public class Wait_Close_Info : ScenarioExecute {
    public Wait_Close_Info() : base() { }

    public override void Execute() {
        scenarioMask.DisableMask();
        scenarioMask.InfoTouchON();
        PlayMangement.instance.EventHandler.AddListener(IngameEventHandler.EVENT_TYPE.CLOSE_INFO_WINDOW, CheckClose);
    }
    private void CheckClose(Enum event_type, Component Sender, object Param) {
        PlayMangement.instance.EventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.CLOSE_INFO_WINDOW, CheckClose);
        scenarioMask.InfoTouchOFF();
        handler.isDone = true;
    }

}


public class Focus_Skill_Icon : ScenarioExecute {
    public Focus_Skill_Icon() : base() { }

    public override void Execute() {
        scenarioMask.FocusSkillIcon();
        PlayMangement.instance.EventHandler.AddListener(IngameEventHandler.EVENT_TYPE.CLICK_SKILL_ICON, CheckClick);
    }

    private void CheckClick(Enum event_type, Component Sender, object Param) {
        PlayMangement.instance.EventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.CLICK_SKILL_ICON, CheckClick);
        handler.isDone = true;
    }

}

public class Blur_Skill_Icon : ScenarioExecute {
    public Blur_Skill_Icon() : base() { }

    public override void Execute() {
        scenarioMask.BlurSkillIcon();
        handler.isDone = true;
    }
}

public class Wait_Field_Change : ScenarioExecute {
    public Wait_Field_Change() : base() { }

    public override void Execute() {
        PlayMangement.instance.EventHandler.AddListener(IngameEventHandler.EVENT_TYPE.FIELD_CHANGED, CheckClose);

    }
    private void CheckClose(Enum event_type, Component Sender, object Param) {
        PlayMangement.instance.EventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.FIELD_CHANGED, CheckClose);
        handler.isDone = true;
    }
}

public class Set_Unit : ScenarioExecute {
    public Set_Unit() : base() { }

    public override void Execute() {
        string[] parse = args[0].Split(',');
        int col = int.Parse(parse[0]);
        int row = int.Parse(parse[1]);

        if (AccountManager.Instance.allCardsDic[args[1]] != null) {
            GameObject unit = PlayMangement.instance.SummonUnit(true, args[1], col, row);
            unit.GetComponent<PlaceMonster>().itemId = PlayMangement.instance.socketHandler.gameState.map.lines[col].human[row].itemId;
        }
        handler.isDone = true;

    }

}

public class Set_Hero_Shield : ScenarioExecute {
    public Set_Hero_Shield() : base() { }

    public override void Execute() {
        PlayMangement playMangement = PlayMangement.instance;
        bool isHuman = playMangement.player.isHuman;
        int socketMyShield = playMangement.socketHandler.gameState.players.myPlayer(isHuman).hero.shieldGauge;
        int socketEnemyShield = playMangement.socketHandler.gameState.players.myPlayer(!isHuman).hero.shieldGauge;
        if(socketMyShield > 0) {
            playMangement.player.shieldStack.Value = socketMyShield;
            PlayMangement.instance.player.FullShieldStack(socketMyShield);
        }
        if(socketEnemyShield > 0) {
            playMangement.enemyPlayer.shieldStack.Value = socketEnemyShield;
            PlayMangement.instance.enemyPlayer.FullShieldStack(socketEnemyShield);
        }

        handler.isDone = true;
    }
}



//제로베이스
public class SetUp_Protect_Unit : ScenarioExecute {
    public SetUp_Protect_Unit() : base() { }

    public override void Execute() {

        if (PlayMangement.instance.gameObject.GetComponent<victoryModule.ProtectObject>() != null) {
            victoryModule.ProtectObject protectObject = PlayMangement.instance.gameObject.GetComponent<victoryModule.ProtectObject>();

            string[] parse = args[0].Split(',');
            int col = int.Parse(parse[0]);
            int row = int.Parse(parse[1]);


            FieldUnitsObserver.Pos pos = new FieldUnitsObserver.Pos(col, row);

            PlaceMonster targetUnit = PlayMangement.instance.UnitsObserver.GetUnit(pos, true).gameObject.GetComponent<PlaceMonster>();


            if (protectObject != null && targetUnit != null)
                protectObject.SetTargetUnit(targetUnit);

        }
        handler.isDone = true;
    }
}

public class Wait_Match_End : ScenarioExecute {
    public Wait_Match_End() : base() { }

    IDisposable playerStatus, enemyStatus, objectStatus;

    public override void Execute() {
        ResetForceDropzone();
        ResetDisableCard();
        ResetGameDisease();
        scenarioGameManagment.isTutorial = false;

        playerStatus = PlayMangement.instance.player.HP.Where(x => x <= 0).Subscribe(_ => GameEnd()).AddTo(PlayMangement.instance.gameObject);
        enemyStatus = PlayMangement.instance.enemyPlayer.HP.Where(x => x <= 0).Subscribe(_ => GameEnd()).AddTo(PlayMangement.instance.gameObject); ;

        if(PlayMangement.instance.gameObject.GetComponent<victoryModule.ProtectObject>() != null) {
            PlaceMonster targetUnit = PlayMangement.instance.gameObject.GetComponent<victoryModule.ProtectObject>().targetUnit;
            objectStatus = Observable.EveryUpdate().Where(_ => targetUnit.unit.currentHP <= 0).Subscribe(_ => GameEnd()).AddTo(PlayMangement.instance.gameObject); ;
        }
    }

    private void GameEnd() {
        playerStatus.Dispose();
        enemyStatus.Dispose();
        if (objectStatus != null)
            objectStatus.Dispose();

        if(PlayMangement.instance.player.HP.Value <= 0) {

        }
        else if(PlayMangement.instance.enemyPlayer.HP.Value <= 0) {
            handler.isDone = true;
        }
    }


    private void ResetForceDropzone() {
        scenarioGameManagment.forcedSummonAt = -1;
        scenarioGameManagment.forcedLine = -1;
        scenarioGameManagment.multipleforceLine = new int[]{ -1,-1};

    }

    private void ResetDisableCard() {
        scenarioGameManagment.canHeroCardToHand = true;
        scenarioMask.DisableMask();
        GameObject cardHand = scenarioMask.targetObject["hand_card"];

        foreach (Transform slot in cardHand.transform) {
            if (slot.childCount < 1)
                continue;
            slot.GetChild(0).gameObject.GetComponent<CardHandler>().enabled = true;
        }
    }

    private void ResetGameDisease() {
        scenarioGameManagment.stopNextTurn = false;
        PlayMangement.instance.gameObject.GetComponent<TurnMachine>().turnStop = false;
        scenarioGameManagment.stopEnemySummon = false;
    }
}

public class Set_Tutorial_After_Battle : ScenarioExecute {
    public Set_Tutorial_After_Battle() : base() { }

    public override void Execute() {
        scenarioGameManagment.matchRule.SetCondition();
        handler.isDone = true;
    }

}



//Proceed_Invoke_NextTurn, Stop_Invoke_NextTurn (오크->휴먼->마법->배틀 턴중에 '버튼'을 눌렀을 경우)
//Stop_Next_Turn, Proceed_Next_Turn (전투 종료후, 다음턴)
//Wait_Turn 턴을 중단 시키는게 아니라, 어떤 턴이 올때까지 대기
//

