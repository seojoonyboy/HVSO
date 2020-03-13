using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using UnityEngine;
using SocketFormat;

public partial class CardUseSendSocket : MonoBehaviour {

    private dataModules.Target[] targets;
    private MagicDragHandler magic;
    private PlaceMonster monster;
    private Transform highlight;
    public bool isSendMessageDone = false;

    public async void Init() {
        magic = GetComponent<MagicDragHandler>();
        monster = GetComponent<PlaceMonster>();
        if(magic != null) {
            targets = magic.cardData.targets;
            highlight = magic.highlightedSlot;
        }
        else targets = monster.unit.targets;
        await CheckSelect();
        Debug.Log("sending Socket");
        SendSocket();
        DestroyMyCard();
        PlayMangement.instance.UnlockTurnOver();
        Destroy(this);
    }

    private void DestroyMyCard() {
        if(magic != null) {
            if(!magic.heroCardActivate) {
                int cardIndex = transform.parent.GetSiblingIndex();
                PlayMangement.instance.player.cdpm.DestroyCard(cardIndex);
                if(PlayMangement.instance.player.isHuman)
                    PlayMangement.instance.player.ActivePlayer();
                else
                    PlayMangement.instance.player.ActiveOrcTurn();
            }
            else {
                PlayMangement.instance.player.cdpm.DestroyUsedHeroCard(transform);
            }
            magic.CARDUSED = true;
            magic.heroCardActivate = false;
        }
    }

    public void SendSocket() {
        BattleConnector connector = PlayMangement.instance.socketHandler;
        MessageFormat format = MessageForm(true);
        connector.UseCard(format);
    }

    private MessageFormat MessageForm(bool isEndCardPlay) {
        MessageFormat format = new MessageFormat();
        List<Arguments> targets = new List<Arguments>();

        //마법 사용
        if(magic != null) {
            format.itemId = magic.itemID;
            targets.Add(ArgumentForm(this.targets[0], false, isEndCardPlay));
        }
        //유닛 소환
        else if(isEndCardPlay) {
            format.itemId = monster.itemId;
            targets.Add(UnitArgument());
        }
        else {
            format.itemId = monster.itemId;
        }
        //Select 스킬 있을 시
        //Playing scope 있는 Select일 때
        if(skillTarget != null) {
            targets.Add(ArgumentForm(magic == null ? this.targets[0] : this.targets[1], true, isEndCardPlay));
        }
        
        format.targets = targets.ToArray();
        return format;
    }

    private Arguments UnitArgument() {
        PlayMangement manage = PlayMangement.instance;
        
        string camp = manage.player.isHuman ? "human" : "orc";
        var observer = manage.UnitsObserver;
        int line = observer.GetMyPos(gameObject).col;
        var posObject = observer.GetAllFieldUnits(line, manage.player.isHuman);
        string placed = posObject.Count == 1 ? "front" : posObject[0] == gameObject ? "rear" : "front";
        return new Arguments("place", new string[]{line.ToString(), camp, placed});
    }

    private Arguments ArgumentForm(dataModules.Target target, bool isSelect, bool isEndCardPlay) {
        Arguments arguments = new Arguments();
        arguments.method = target.method;
        PlayerController player = PlayMangement.instance.player;
        bool isPlayerHuman = player.isHuman;
        bool isOrc;
        List<string> args = new List<string>();

        //타겟이 unit, hero인 경우
        if (arguments.method.Contains("unit")){
            if (arguments.method.Contains("hero")) {
                //unit인지 hero인지 구분
                string unitItemId;
                PlaceMonster monster;
                //select 스킬인 경우
                if (isSelect) {
                    monster = ((GameObject)skillTarget).GetComponent<PlaceMonster>();
                    //타겟이 영웅?
                    if(monster == null) {
                        if (((GameObject)skillTarget).GetComponentInParent<PlayerController>() != null) {
                            isOrc = (target.filter[0].CompareTo("my") == 0) != isPlayerHuman;
                            arguments.method = "hero";
                            args.Add(isOrc ? "orc" : "human");
                        }
                    }
                    //타겟이 유닛
                    else {
                        monster = GetDropAreaUnit();
                        unitItemId = monster.itemId;
                        arguments.method = "unit";
                        args.Add(unitItemId.ToString());
                        isOrc = monster.isPlayer != isPlayerHuman;
                        args.Add(isOrc ? "orc" : "human");
                    }
                }
                //select 스킬이 아닌경우
                else {
                    monster = highlight.GetComponentInParent<PlaceMonster>();
                    //타겟이 영웅?
                    if (monster == null) {
                        if (highlight.GetComponentInParent<PlayerController>() != null) {
                            isOrc = (Array.Exists(target.filter, x=>x.CompareTo("my") == 0)) != isPlayerHuman;
                            arguments.method = "hero";
                            args.Add(isOrc ? "orc" : "human");
                        }
                    }
                    //타겟이 유닛
                    else {
                        monster = GetDropAreaUnit();
                        unitItemId = monster.itemId;
                        args.Add(unitItemId.ToString());
                        arguments.method = "unit";
                        isOrc = monster.isPlayer != isPlayerHuman;
                        args.Add(isOrc ? "orc" : "human");
                    }
                }
            }
            else {
                string unitItemId;
                PlaceMonster monster;
                if (isSelect) monster = ((GameObject)skillTarget).GetComponent<PlaceMonster>();
                else monster = GetDropAreaUnit();
                unitItemId = monster.itemId;
                args.Add(unitItemId);
                isOrc = monster.isPlayer != isPlayerHuman;
                args.Add(isOrc ? "orc" : "human");
            }
        }
        else {
            if (arguments.method.Contains("all")) {
                isOrc = (Array.Exists(target.filter, x=>x.CompareTo("my") == 0)) != isPlayerHuman;
                args.Add(isOrc ? "orc" : "human");
            }

            else if (arguments.method.Contains("line")) {
                if (isSelect) args.Add(((GameObject)skillTarget).GetComponent<PlaceMonster>().x.ToString());
                else args.Add(GetDropAreaLine().ToString());
            }

            else if (arguments.method.Contains("place")) {
                int line = ((GameObject)skillTarget).transform.GetSiblingIndex();
                args.Add(line.ToString());
                if (isEndCardPlay) {
                    isOrc = ((GameObject)skillTarget).GetComponent<PlaceMonster>().isPlayer != isPlayerHuman;
                }
                else
                    isOrc = monster.isPlayer != isPlayerHuman;
                args.Add(isOrc ? "orc" : "human");
            }
        }
        arguments.args = args.ToArray();
        return arguments;
    }

    private PlaceMonster GetDropAreaUnit() {
        PlaceMonster unit;
        if(highlight != null)
            unit = highlight.GetComponentInParent<PlaceMonster>();
        else
            unit = ((List<GameObject>)skillTarget)[0].GetComponent<PlaceMonster>();
        return unit;
    }
    
    private int GetDropAreaLine() {
        return highlight
            .GetComponentInParent<Terrain>()
            .transform.GetSiblingIndex();
    }
}


public partial class CardUseSendSocket : MonoBehaviour {

    private bool isSelect;
    private object skillTarget = null;

     private async Task CheckSelect() {
        int length = monster == null ? 2 : 1;
        if(targets.Length < length || Filter()) {
            Debug.Log("Can't Select");
            isSelect = false;
            return;
        }
        await GetSelect();
        isSelect = true;
        return;
    }

    private async Task GetSelect() {
        Debug.Log("Select On");
        PlayMangement.instance.OnBlockPanel("대상을 정해 주세요.");
        if(monster == null) transform.localScale = Vector3.zero;
        PlayMangement.instance.LockTurnOver();
        //TODO : 선택할 것들의 UI세팅
        while(skillTarget == null)  {
            Debug.Log("selecting");
            if(CheckTurnisOver()) {isSelect = false; break;}
            SetSelect();
            await Task.Delay(1);
        }
        Debug.Log(skillTarget);
        Debug.Log("Select Done");
        removeSelectUI();
    }

    private bool CheckTurnisOver() {return PlayMangement.instance.socketHandler.gameState.turn.turnState.CompareTo("play") != 0;}

    protected void SetSelect() {
        if (Input.GetMouseButtonDown(0)) {
            Transform selectedTarget = null;
            PlayMangement.dragable = false;
            dataModules.Target target = magic == null ? targets[0] : targets[1];

            if (target.method == "place") {
                selectedTarget = GetClickedAreaSlot();
            }
            else if(target.method == "unit") {
                string layer = "PlayerUnit";
                if(target.filter[0] == "enemy") {
                    layer = "EnemyUnit";
                }
                selectedTarget = GetClickedAreaUnit(layer);
            }

            if (selectedTarget != null) {
                if (target.method  == "place") {
                    int col = selectedTarget.parent.GetSiblingIndex();
                    int row = 0;

                    var observer = PlayMangement.instance.UnitsObserver;
                    selectedTarget = observer.GetSlot(new FieldUnitsObserver.Pos(col, row), true);
                }

                if(target.method  == "unit") {
                    PlaceMonster targetMonster = selectedTarget.gameObject.GetComponentInParent<PlaceMonster>();
                    if(!targetMonster.transform.Find("ClickableUI").gameObject.activeSelf) return;
                    selectedTarget = targetMonster.transform;
                }
                
                skillTarget = selectedTarget.gameObject;
                
                TintUnit(false);
            }
        }
    }

    private void removeSelectUI() {
        PlayMangement.instance.OffBlockPanel();
        CardDropManager.Instance.HideDropableSlot();
        PlayMangement.instance.infoOn = false;
        PlayMangement.dragable = true;
        PlayMangement.instance.UnlockTurnOver();
        TintUnit(false);

        bool isHuman = PlayMangement.instance.player.isHuman;
        
        var units = PlayMangement.instance.UnitsObserver.GetAllFieldUnits(!isHuman);
        foreach (GameObject unit in units) {
            unit
                .transform
                .Find("ClickableUI")
                .gameObject
                .SetActive(false);

            unit.transform.Find("MagicTargetTrigger").gameObject.SetActive(false);
        }
        units = PlayMangement.instance.UnitsObserver.GetAllFieldUnits(isHuman);
        foreach (GameObject unit in units) {
            unit
                .transform
                .Find("ClickableUI")
                .gameObject
                .SetActive(false);

            unit.transform.Find("MagicTargetTrigger").gameObject.SetActive(false);
        }
    }

    public bool Filter() {
        Debug.Log("Filtering....1");
        if (monster != null)
            EffectSystem.Instance.CheckEveryLineMask(monster);
        dataModules.Target target = magic == null ? targets[0] : targets[1];
        Debug.Log("Filtering....2");
        switch (target.filter[0]) {
            case "my":
                if(target.method.CompareTo("place") == 0) {
                    if (CanSelect("place")) {
                        PlayMangement.instance.OnBlockPanel("위치를 지정해 주세요.");
                        EffectSystem.Instance.ShowSlotWithDim();
                        dataModules.Attr[] attributes; 
                        if(monster != null)
                            attributes = monster.unit.attributes;
                        else
                            attributes = GetDropAreaUnit().unit.attributes;

                        if (ScenarioGameManagment.scenarioInstance != null && ScenarioGameManagment.scenarioInstance.forcedSummonAt != -1)
                            CardDropManager.Instance.ShowScopeSlot(PlayMangement.instance.player.isHuman);
                        else
                            CardDropManager.Instance.ShowDropableSlot(attributes, true);
                        TintUnit(true);
                    }
                    else {
                        return true;
                    }
                }
                if (target.method.CompareTo("unit") == 0) {
                    if (CanSelect("unit")) {
                        PlayMangement.instance.OnBlockPanel("대상을 정해 주세요.");
                        //잠복중인 유닛은 타겟에서 제외
                        var units = PlayMangement.instance.UnitsObserver
                            .GetAllFieldUnits(PlayMangement.instance.player.isHuman);
                        
                        //units.RemoveAll(x=>x.GetComponent<PlaceMonster>())
                        //placeMonster에 Granted가 없음. 요걸로 잠복중인지 찾을려고 일단 주석처리
                        if(monster != null) units.RemoveAll(unit => unit.GetComponent<PlaceMonster>().itemId.CompareTo(monster.itemId) == 0); //자기 자신이 포함되어있다면 제외

                        foreach (GameObject unit in units) {
                            var ui = unit.transform.Find("ClickableUI").gameObject;
                            if (ui != null) {
                                ui.SetActive(true);
                                PlayMangement.instance.infoOn = true;
                            }
                            unit.transform.Find("MagicTargetTrigger").gameObject.SetActive(true);
                        }
                        TintUnit(true);
                    }
                    else {
                        return true;
                    }
                }
                break;
            case "enemy":
                if (target.method.CompareTo("unit") == 0) {
                    if (CanSelect("unit")) {
                        PlayMangement.instance.OnBlockPanel("대상을 정해 주세요.");
                        var units = PlayMangement.instance.UnitsObserver.GetAllFieldUnits(!PlayMangement.instance.player.isHuman);

                        //잠복중인 유닛은 타겟에서 제외
                        for (int i = 0; i < units.Count; i++) {
                            var placeMonster = units[i].GetComponent<PlaceMonster>();
                            if (placeMonster.GetComponent<ambush>() != null) {
                                units.Remove(units[i]);
                                //Logger.Log("잠복 유닛 감지됨");
                            }
                        }

                        if(units.Count == 0) {
                            return true;
                        }
                        Debug.Log("유닛 표시해주기");
                        foreach (GameObject unit in units) {
                            Debug.Log(unit);
                            var ui = unit.transform.Find("ClickableUI").gameObject;
                            if (ScenarioGameManagment.scenarioInstance != null && ScenarioGameManagment.scenarioInstance.forcedTargetAt != -1) {
                                if (unit.GetComponent<PlaceMonster>().x == ScenarioGameManagment.scenarioInstance.forcedTargetAt) {
                                    if (ui != null) {
                                        ui.SetActive(true);
                                        PlayMangement.instance.infoOn = true;
                                    }
                                    unit.transform.Find("MagicTargetTrigger").gameObject.SetActive(true);
                                }
                            }
                            else {
                                if (ui != null) {
                                    ui.SetActive(true);
                                    PlayMangement.instance.infoOn = true;
                                }
                                unit.transform.Find("MagicTargetTrigger").gameObject.SetActive(true);
                            }
                        }

                        TintUnit(true);
                    }
                    else {
                        return true;
                    }
                }
                break;
        }
        Debug.Log("Filtering....Done");
        return false;
    }

    protected bool CanSelect(string arg) {
        bool result = false;
        var observer = PlayMangement.instance.UnitsObserver;
        bool isHuman = PlayMangement.instance.player.isHuman;

        Transform slotParent = null;
        GameObject[,] slots = null;
        if (isHuman) {
            slots = observer.humanUnits;
            slotParent = PlayMangement.instance.player.transform;
        }
        else {
            slots = observer.orcUnits;
            slotParent = PlayMangement.instance.enemyPlayer.transform;
        }

        switch (arg) {
            case "place":
                for (int i=0; i<5; i++) {
                    //빈 공간인 경우
                    if(slots[i, 0] == null) {
                        var placeMonster = ((GameObject)this.skillTarget).GetComponent<PlaceMonster>();
                        //유닛카드인 경우
                        if (placeMonster != null) {
                            //숲 지형인 경우
                            //if (slotParent.transform.GetChild(0).GetChild(i).GetComponent<Terrain>().terrain == PlayMangement.LineState.forest) {
                            //    //유닛이 숲 지형에 갈 수 있는 경우
                            //    if (placeMonster.unit.attributes.ToList().Contains("footslog")) {
                            //        result = true;
                            //    }
                            //}
                            ////숲 지형이 아닌 경우
                            //else {
                            //    result = true;
                            //}
                            result = true;
                        }
                        //마법카드인 경우
                        else {
                            var skillTarget = (GameObject)this.skillTarget;
                            placeMonster = skillTarget.GetComponent<PlaceMonster>();
                            //if (PlayMangement.instance.player.transform.GetChild(0).GetChild(i).GetComponent<Terrain>().terrain == PlayMangement.LineState.forest) {
                            //    //유닛이 숲 지형에 갈 수 있는 경우
                            //    if (placeMonster.unit.attributes.ToList().Contains("footslog")) {
                            //        result = true;
                            //    }
                            //}
                            ////숲 지형이 아닌 경우
                            //else {
                            //    result = true;
                            //}
                            result = true;
                        }
                    }
                }
                break;
            case "unit":
                var units = observer.GetAllFieldUnits(isHuman);

                //잠복중인 유닛은 타겟에서 제외
                for(int i=0; i<units.Count; i++) {
                    var placeMonster = units[i].GetComponent<PlaceMonster>();
                    if (placeMonster.GetComponent<ambush>() != null) {
                        units.Remove(units[i]);
                    }
                }

                if (units.Count != 0) {
                    //Logger.Log(observer.GetAllFieldUnits(isHuman).Count + "개의 적이 감지됨");
                    result = true;
                }
                break;
        }
        return result;
    }

    private void TintUnit(bool onOff) {
        PlaceMonster caster;
        caster = monster;
        if(caster == null) {
            object skillTarget = this.skillTarget;
            if(skillTarget.GetType() ==  typeof(GameObject))
                caster = ((GameObject)skillTarget).GetComponent<PlaceMonster>();
            else if(skillTarget.GetType() == typeof(List<GameObject>))
                caster = ((List<GameObject>)skillTarget)[0].GetComponent<PlaceMonster>();
        }
        caster.TintAnimation(onOff);
    }

    // protected Transform GetDropAreaCharacter() {
    //     var highlightedSlot = GetComponent<CardHandler>().highlightedSlot;
    //     if (highlightedSlot.GetComponentInParent<PlaceMonster>() != null) {
    //         return GetDropAreaUnit();
    //     }
    //     else {
    //         return highlightedSlot.GetComponentInParent<PlayerController>().transform;
    //     }
    // }

    protected int GetDropLine() {
        Transform highlightedSlot = GetComponent<CardHandler>().highlightedSlot;
        int col = highlightedSlot.parent.GetSiblingIndex();

        //Logger.Log(col);

        return col;
    }

    protected Transform GetClickedAreaUnit() {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        LayerMask mask1 = (1 << LayerMask.NameToLayer("PlayerUnit"));
        LayerMask mask2 = (1 << LayerMask.NameToLayer("EnemyUnit"));
        LayerMask mask = mask1 | mask2;

        RaycastHit2D[] hits = Physics2D.RaycastAll(
            new Vector2(mousePos.x, mousePos.y),
            Vector2.zero,
            Mathf.Infinity,
            mask
        );

        if (hits != null) {
            foreach (RaycastHit2D hit in hits) {
                return hit.transform;
            }
        }
        return null;
    }

    protected Transform GetClickedAreaUnit(string layer) {
        layer = "MagicTarget";

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        LayerMask mask = (1 << LayerMask.NameToLayer(layer));

        RaycastHit2D[] hits = Physics2D.RaycastAll(
            new Vector2(mousePos.x, mousePos.y),
            Vector2.zero,
            Mathf.Infinity,
            mask
        );

        if (hits != null) {
            foreach (RaycastHit2D hit in hits) {
                return hit.transform;
            }
        }
        return null;
    }

    protected Transform GetClickedAreaSlot() {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        LayerMask mask = (1 << LayerMask.NameToLayer("UnitSlot"));

        RaycastHit2D[] hits = Physics2D.RaycastAll(
            new Vector2(mousePos.x, mousePos.y),
            Vector2.zero,
            Mathf.Infinity,
            mask
        );

        if (hits != null) {
            foreach (RaycastHit2D hit in hits) {
                return hit.transform;
            }
        }
        return null;
    }
}