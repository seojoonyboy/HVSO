using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;
using System.Linq;
using System;
using UnityEngine;
using SocketFormat;

public partial class CardSelect : MonoBehaviour {

    protected dataModules.Target[] targets;
    protected MagicDragHandler magic;
    protected PlaceMonster monster;
    protected Transform highlight;

    protected bool isSelect;
    protected GameObject skillTarget = null;

    protected void SetUnitorMagic() {
        magic = GetComponent<MagicDragHandler>();
        monster = GetComponent<PlaceMonster>();
        if(magic != null) {
            targets = magic.cardData.targets;
            highlight = magic.highlightedSlot;
        }
        else targets = monster.unit.targets;
    }

    protected async Task CheckSelect(bool isEndCardPlay = true) {
        if(!isEndCardPlay) {
            Filter(false);
            await GetSelect(isEndCardPlay);
            isSelect = true;
            await Task.Delay(1);
            removeSelectUI();
            return;
        }
        int length = monster == null ? 2 : 1;
        if(targets.Length < length || Filter()) {
            Debug.Log("Can't Select");
            isSelect = false;
            await Task.Delay(1);
            removeSelectUI();
            return;
        }
        await GetSelect();
        isSelect = true;
        removeSelectUI();
    }

    private async Task GetSelect(bool isEndCardPlay = true) {
        Debug.Log("Select On");
        if(monster == null) transform.localScale = Vector3.zero;
        PlayMangement.instance.LockTurnOver();
        while(CheckTurnisOver()) { await Task.Delay(1); }
        while (skillTarget == null)  {
            if (CheckTurnisOver() == true) { isSelect = false; removeSelectUI(); break; }
            else {
                if (PlayMangement.instance.stopSelect == false) SetSelect();
            }
            await Task.Delay(1);
        }
        Debug.Log(skillTarget);
        Debug.Log("Select Done");
    }

    private bool CheckTurnisOver() {
        TurnState state = PlayMangement.instance.socketHandler.gameState.turn;
        bool isOktoSelect = (state.turnState.CompareTo("play") != 0) != (state.turnName.CompareTo("shieldTurn") == 0);
        return isOktoSelect;
    }

    protected void SetSelect() {
        if (Input.GetMouseButtonDown(0)) {
            Transform selectedTarget = null;
            PlayMangement.dragable = false;
            dataModules.Target target = null;
            if(targets.Length == 0) {
                target = new dataModules.Target();
                target.method = "place";
            }
            else target = magic == null ? targets[0] : targets[1];


            switch (target.method) {
                case "place":
                    selectedTarget = GetClickedAreaSlot();
                    break;

                case "unit":
                    string layer = "PlayerUnit";
                    if (target.filter[0] == "enemy") {
                        layer = "EnemyUnit";
                    }
                    selectedTarget = GetClickedAreaUnit(layer);
                    break;

                case "unit_hero":
                    break;

                default:
                    Debug.Log("타게팅 나도 몰겄다 ㅇㅇ");
                    break;
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
        EffectSystem.Instance.HideEveryDim();
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

    public bool Filter(bool isEndCardPlay = true) {
        Debug.Log("Filtering....1");
        if (monster != null)
            EffectSystem.Instance.CheckEveryLineMask(monster);
        dataModules.Target target = !isEndCardPlay ? null : magic == null ? targets[0] : targets[1];
        Debug.Log("Filtering....2");
        switch (target != null ? target.filter[0] : "my") {
            case "my":
                if(target != null ? target.method.CompareTo("place") == 0 : true) {
                    if (CanSelect("place", true)) {
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
                else if (target.method.CompareTo("unit") == 0) {
                    if (CanSelect("unit", true)) {
                        PlayMangement.instance.OnBlockPanel("대상을 정해 주세요.");
                        //잠복중인 유닛은 타겟에서 제외
                        var units = PlayMangement.instance.UnitsObserver
                            .GetAllFieldUnits(PlayMangement.instance.player.isHuman);
                        
                        //units.RemoveAll(x=>x.GetComponent<PlaceMonster>())
                        //placeMonster에 Granted가 없음. 요걸로 잠복중인지 찾을려고 일단 주석처리
                        if(monster != null) units.RemoveAll(unit => unit.GetComponent<PlaceMonster>().itemId.CompareTo(monster.itemId) == 0); //자기 자신이 포함되어있다면 제외
                        // if(monster != null && monster.unit.id.CompareTo("ac10008")==0) {
                        //     List<GameObject> lineUnits = PlayMangement.instance.UnitsObserver.GetAllFieldUnits(monster.x, false);
                        //     units.RemoveAll(x => units.Exists(y=>x.GetComponent<PlaceMonster>().itemId.CompareTo(y.GetComponent<PlaceMonster>().y) == 0));
                        // }
                        if(monster != null && monster.unit.id.CompareTo("ac10032")==0) {
                            units.RemoveAll(x=>!x.GetComponent<PlaceMonster>().unit.cardCategories.Contains("mage"));
                        }
                        else if(monster != null && monster.unit.id.CompareTo("ac10040")==0) {
                            units.RemoveAll(x=>!x.GetComponent<PlaceMonster>().unit.cardCategories.Contains("monster"));
                        }
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
                    if (CanSelect("unit", false)) {
                        PlayMangement.instance.OnBlockPanel("대상을 정해 주세요.");
                        var units = PlayMangement.instance.UnitsObserver.GetAllFieldUnits(!PlayMangement.instance.player.isHuman);

                        //잠복중인 유닛은 타겟에서 제외
                        for (int i = 0; i < units.Count; i++) {
                            var placeMonster = units[i].GetComponent<PlaceMonster>();
                            if (placeMonster.GetComponent<ambush>() != null) {
                                units.Remove(units[i]);
                                //Logger.Log("잠복 유닛 감지됨");
                            }

                            if (Array.Exists(placeMonster.granted, grant => grant.name == "protect"))
                                units.Remove(units[i]);
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

    protected bool CanSelect(string arg, bool isMy) {
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
                        //유닛카드인 경우
                        if (monster != null) {
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
                var units = observer.GetAllFieldUnits(isHuman == isMy);
                Debug.Log(units.Count);
                if(monster != null) units.Remove(monster.gameObject); //자기 자신 제거
                //잠복중인 유닛은 타겟에서 제외
                for(int i=0; i<units.Count; i++) {
                    var placeMonster = units[i].GetComponent<PlaceMonster>();
                    if (placeMonster.GetComponent<ambush>() != null) {
                        units.Remove(units[i]);
                    }
                }
                if(monster != null) {
                    if(monster.unit.id.CompareTo("ac10008")==0) {
                        if(observer.GetAllFieldUnits(monster.x, false).Count > 0) {
                            Debug.Log("result");
                            result = false;
                            break;
                        }
                    }
                    else if(monster.unit.id.CompareTo("ac10032")==0) {
                        Debug.Log(units.Count);
                        for(int i=0; i<units.Count; i++) {
                            var categories = units[i].GetComponent<PlaceMonster>().unit.cardCategories.ToList();
                            if(categories.Exists(x=>x.CompareTo("mage")==0)) {
                                Debug.Log("found it");
                                result = true;
                                return result;
                            }
                        }
                        Debug.Log("can't find it");
                        result = false;
                        break;
                    }
                    else if(monster.unit.id.CompareTo("ac10040")==0) {
                        for(int i=0; i<units.Count; i++) {
                            var categories = units[i].GetComponent<PlaceMonster>().unit.cardCategories.ToList();
                            if(categories.Exists(x=>x.CompareTo("monster")==0)) {
                                result = true;
                                return result;
                            }
                        }
                        result = false;
                        break;
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

    public void EnemyNeedSelect() {
        SetUnitorMagic();
        //1. 메시지 올 때까지 기다리기
        PlayMangement.instance.OnBlockPanel("상대가 위치를 지정중입니다.");
        TintUnit(true);
    }

    public IEnumerator enemyUnitSelect(int position) {
        //턴 넘김으로 인한 경우 (현재 오크 잠복꾼) 위치 이동만 존재합니다.
        //yield return new WaitUntil(PlayMangement.instance.passOrc);
        //2. 밝혀줘야할 select 부분 찾기
        FieldUnitsObserver.Pos movePos = new FieldUnitsObserver.Pos(position, 0);
        Terrain[] terrains = GameObject.Find("BackGround").GetComponentsInChildren<Terrain>();
        Transform terrainSlot = null;
        
        foreach(Terrain x in terrains) {
            if(movePos.col == x.transform.GetSiblingIndex()) {
                terrainSlot = x.transform.Find("EnemyBackSlot");
                break;
            }
        }

        //3. unitSlot에서 특정 부분 밝혀주기 아마 baseSlot 맞겠지
        terrainSlot.gameObject.SetActive(true);
        terrainSlot.GetChild(0).gameObject.SetActive(true);
        terrainSlot.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 0.6f);

        //4.  1초뒤에 끄기
        yield return new WaitForSeconds(1f);
        terrainSlot.gameObject.SetActive(false);
        terrainSlot.GetChild(0).gameObject.SetActive(false);
        terrainSlot.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.6f);
        
        //5. 실제 effect 실행하러 보내기
        Transform selectedTarget = PlayMangement.instance.UnitsObserver
            .GetSlot(movePos, !PlayMangement.instance.player.isHuman)
            .transform;
        skillTarget = selectedTarget.gameObject;
        removeSelectUI();
        Destroy(this);
    }

    public IEnumerator EnemyTurnSelect() {
        SetUnitorMagic();
        BattleConnector server = PlayMangement.instance.socketHandler;
        TintUnit(true);
        Transform selectedTarget = null;
        //1. 사용한 카드 찾기
        string itemId = monster != null ? 
            monster.itemId : 
            magic.itemID;
        var play_list = server.gameState.playHistory.ToList();
        SocketFormat.PlayHistory played_card = play_list.Find(x => x.cardItem.itemId.CompareTo(itemId) == 0);
        
        //왠만하면은 2번째 targets가 select인 경우들인것 같다. (유닛 소환 직후 또는 마법카드 사용)
        if(played_card.targets.Length != 2) yield break;
        //2. 카드의 정보 확인 하기
        switch(played_card.targets[1].method) {
        case "place" :
            int line = int.Parse(played_card.targets[1].args[0]);
            bool targetCampHuman = played_card.targets[1].args[1].CompareTo("human")==0;
            //지정된 타겟이 아군인지 적군인지 판단용
            bool isTargetPlayer = PlayMangement.instance.player.isHuman == targetCampHuman;
            Terrain[] terrains = GameObject.Find("BackGround").GetComponentsInChildren<Terrain>();
            Transform terrainSlot = null;
        
            foreach(Terrain x in terrains) {
                if(line == x.transform.GetSiblingIndex()) {
                    if(isTargetPlayer) terrainSlot = x.transform.GetChild(0);
                    else terrainSlot = x.transform.Find("EnemyBackSlot");
                    break;
                }
            }

            //3. unitSlot에서 특정 부분 밝혀주기 아마 baseSlot 맞겠지
            terrainSlot.gameObject.SetActive(true);
            terrainSlot.GetChild(0).gameObject.SetActive(true);
            terrainSlot.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 0.6f);

            //4.  1초뒤에 끄기
            yield return new WaitForSeconds(1f);
            terrainSlot.gameObject.SetActive(false);
            terrainSlot.GetChild(0).gameObject.SetActive(false);
            terrainSlot.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.6f);
            
            FieldUnitsObserver observer = PlayMangement.instance.UnitsObserver;
                selectedTarget = observer
                    .GetSlot(new FieldUnitsObserver.Pos(line, 0), targetCampHuman)
                    .transform;
        break;
        case "unit" :
            //검색
            string targetItemId = played_card.targets[1].args[0];
            var camp = played_card.targets[1].args[1];
            var list = PlayMangement.instance.UnitsObserver
                    .GetAllFieldUnits(camp.CompareTo("human") == 0);
            //list.AddRange(PlayMangement.instance.EnemyUnitsObserver.GetAllFieldUnits());
            GameObject target = list.Find(x => x.GetComponent<PlaceMonster>().itemId.CompareTo(targetItemId) == 0);
            GameObject highlightUI = target.transform.Find("ClickableUI").gameObject;
            //3. unitSlot에서 특정 부분 밝혀주기
            highlightUI.SetActive(true);
            if(highlightUI.GetComponent<SpriteRenderer>() != null)
                highlightUI.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 0.6f);
            //4.  1.5초뒤에 끄기
            yield return new WaitForSeconds(1.5f);
            if(highlightUI.GetComponent<SpriteRenderer>() != null)
                highlightUI.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.6f);
            highlightUI.SetActive(false);
            selectedTarget = target.transform;
        break;
        default :
        yield break;
        }
        if(selectedTarget == null) {
            yield break;
        }
        skillTarget = selectedTarget.gameObject;
        
        PlayMangement.instance.OffBlockPanel();
        PlayMangement.instance.UnlockTurnOver();
        TintUnit(false);
        Destroy(this);
    }

    private void TintUnit(bool onOff) { //눌러진 상대
        PlaceMonster caster;
        caster = monster;
        if(caster == null) {
            caster = GetDropAreaUnit();
        }
        if(caster != null) caster.TintAnimation(onOff);
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


    protected Transform GetDropAreaHero() {

        return null;
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

    protected PlaceMonster GetDropAreaUnit() {
        PlaceMonster unit;
        if(highlight != null)
            unit = highlight.GetComponentInParent<PlaceMonster>();
        else
            unit = skillTarget.GetComponent<PlaceMonster>();
        return unit;
    }
    
    protected int GetDropAreaLine() {
        return highlight
            .GetComponentInParent<Terrain>()
            .transform.GetSiblingIndex();
    }
}