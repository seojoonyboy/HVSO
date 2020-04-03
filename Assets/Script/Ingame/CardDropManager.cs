using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Spine;
using Spine.Unity;

public partial class CardDropManager : Singleton<CardDropManager> {
    protected Transform[] slotLine;
    public Transform[][] unitLine;
    public Transform[][] enemyUnitLine;
    public Transform playerHero, enemyHero;
    public void SetUnitDropPos() {
        slotLine = new Transform[5];
        Transform mapSlotLines = PlayMangement.instance.backGround.transform;
        for (int i = 0; i < 5; i++) {
            slotLine[i] = mapSlotLines.GetChild(i);
        }
        unitLine = new Transform[5][];
        enemyUnitLine = new Transform[5][];
        for (int i = 0; i < 5; i++) {
            unitLine[i] = new Transform[2];
            enemyUnitLine[i] = new Transform[2];
        }
        Transform unitSlotLines = PlayMangement.instance.player.transform;
        for (int i = 0; i < 5; i++) {
            for (int j = 0; j < 2; j++) {
                unitLine[i][j] = unitSlotLines.GetChild(j).GetChild(i);
                enemyUnitLine[i][j] = unitSlotLines.parent.GetChild(1).GetChild(j).GetChild(i);
            }
        }
        playerHero = PlayMangement.instance.player.transform;
        enemyHero = PlayMangement.instance.enemyPlayer.transform;
    }
}

/// <summary>
/// 유닛 처리
/// </summary>
public partial class CardDropManager {
    /// <summary>
    /// 튜토리얼용 강제 소환위치 지정
    /// </summary>
    /// <param name="line">Args는 1부터 시작</param>
    public void ForcedShowDropableSlot(int line, string args = null) {
        //Logger.Log("ForcedShowDropableSlot");
        for(int i=0; i<5; i++) {
            if(i == line - 1) {
                if (args == null || args == "slot") {
                    if (unitLine[i][0].childCount == 0) {
                        slotLine[i].GetChild(0).gameObject.SetActive(true);
                        slotLine[i].GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1f);
                    }
                    else {
                        slotLine[i].GetChild(1).gameObject.SetActive(true);
                        slotLine[i].GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1f);
                    }

                }
                else if (args == "line") {
                    slotLine[i].Find("BattleLineEffect").gameObject.SetActive(true);
                }

            }
        }
    }
    /// <summary>
    /// ForcedShowDropableSlot과는 비슷한데, 이건 1~3번째 라인 형식으로 처리
    /// </summary>
    public void ForcedLimitLineSlot(int line, string args = null) {
        for (int i = 0; i < 5; i++) {
            if (args == null || args == "slot") {
                if (unitLine[i][0].childCount == 0) {
                    slotLine[i].GetChild(0).gameObject.SetActive(true);
                    slotLine[i].GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1f);
                }
            }
            else if (args == "line") {
                slotLine[i].Find("BattleLineEffect").gameObject.SetActive(true);
            }

            if (i == line - 1)
                break;
        }
    }
    public void ForcedShowDropableSlot(int[] line, string args = null) {
        //Logger.Log("ForcedShowDropableSlot");
        int temp = 0;
        for (int i = 0; i < 5; i++) {
            if (temp >= line.Length)
                break;

            if (i == line[temp] - 1) {
                if (args == null || args == "slot") {
                    if (unitLine[i][0].childCount == 0) {
                        slotLine[i].GetChild(0).gameObject.SetActive(true);
                        slotLine[i].GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1f);
                    }
                }
                else if (args == "line") {
                    slotLine[i].Find("BattleLineEffect").gameObject.SetActive(true);
                }

                if (temp < line.Length)
                    temp++;
            }
        }
    }




    public void ShowDropableSlot(dataModules.CollectionCard card) {
        if(ScenarioGameManagment.scenarioInstance != null && ScenarioGameManagment.scenarioInstance.isTutorial) {            
            int targetline = ScenarioGameManagment.scenarioInstance.forcedSummonAt;
            int limitLine = ScenarioGameManagment.scenarioInstance.forcedLine;
            int multiLine = ScenarioGameManagment.scenarioInstance.multipleforceLine[0];

            if(multiLine != -1) {
                ForcedShowDropableSlot(ScenarioGameManagment.scenarioInstance.multipleforceLine);
                return;
            }
            if (targetline != -1) {
                ForcedShowDropableSlot(targetline);
                return;
            }
            if(limitLine != -1) {
                ForcedLimitLineSlot(limitLine);
                return;
            }
        }
        for (int i = 0; i < 5; i++) {
            if (card.attributes.Length == 0) {
                // if (slotLine[i].GetComponent<Terrain>().terrain == PlayMangement.LineState.forest) continue;
                if (unitLine[i][0].childCount == 0) {
                    slotLine[i].GetChild(0).gameObject.SetActive(true);
                }
                else {
                    dataModules.Attr[] attribute = unitLine[i][0].GetChild(0).GetComponent<PlaceMonster>().unit.attributes;
                    for (int j = 0; j < attribute.Length; j++) {
                        if (attribute[j].name == "combo") {
                            if(unitLine[i][1].childCount != 0) continue;
                            unitLine[i][0].GetChild(0).position = new Vector3(unitLine[i][0].position.x, unitLine[i][0].GetChild(0).position.y + 1.5f, 0);
                            unitLine[i][0].GetChild(0).Find("InfoWindowTrigger").gameObject.SetActive(false);
                            unitLine[i][0].GetChild(0).Find("MagicTargetTrigger").gameObject.SetActive(false);
                            slotLine[i].GetChild(1).gameObject.SetActive(true);
                            slotLine[i].GetChild(2).gameObject.SetActive(true);
                        }
                    }
                }
            }
            else {

                bool forrestAble = false;
                bool chainAble = false;
                for (int j = 0; j < card.attributes.Length; j++) {
                    if (card.attributes[j].name == "footslog") forrestAble = true;
                    else if (card.attributes[j].name == "combo") chainAble = true;
                }

                // if (slotLine[i].GetComponent<Terrain>().terrain == PlayMangement.LineState.forest && !forrestAble) continue;
                // if (!forrestAble) continue;
                if (!chainAble) {
                    if (unitLine[i][0].childCount == 0) {
                        slotLine[i].GetChild(0).gameObject.SetActive(true);
                    }
                    else if(unitLine[i][1].childCount == 0) {
                        dataModules.Attr[] attribute = unitLine[i][0].GetChild(0).GetComponent<PlaceMonster>().unit.attributes;
                        for (int j = 0; j < attribute.Length; j++) {
                            if (attribute[j].name == "combo") {
                                unitLine[i][0].GetChild(0).position = new Vector3(unitLine[i][0].position.x, unitLine[i][0].GetChild(0).position.y + 1.5f, 0);
                                unitLine[i][0].GetChild(0).Find("InfoWindowTrigger").gameObject.SetActive(false);
                                unitLine[i][0].GetChild(0).Find("MagicTargetTrigger").gameObject.SetActive(false);
                                slotLine[i].GetChild(1).gameObject.SetActive(true);
                                slotLine[i].GetChild(2).gameObject.SetActive(true);
                            }
                        }
                    }
                }
                else {
                    if (unitLine[i][0].childCount == 0) {
                        slotLine[i].GetChild(0).gameObject.SetActive(true);
                    }
                    else {
                        if (unitLine[i][1].childCount == 0) {
                            unitLine[i][0].GetChild(0).position = new Vector3(unitLine[i][0].position.x, unitLine[i][0].GetChild(0).position.y + 1.5f, 0);
                            unitLine[i][0].GetChild(0).Find("InfoWindowTrigger").gameObject.SetActive(false);
                            unitLine[i][0].GetChild(0).Find("MagicTargetTrigger").gameObject.SetActive(false);
                            slotLine[i].GetChild(1).gameObject.SetActive(true);
                            slotLine[i].GetChild(2).gameObject.SetActive(true);
                        }
                    }
                }
            }
        }
    }
    public void ShowScopeSlot(bool isHuman) {
        for(int i= 0; i< 5; i++) {
            if (ScenarioGameManagment.scenarioInstance.forcedSummonAt - 1 == i) {
                if (PlayMangement.instance.UnitsObserver.IsUnitExist(new FieldUnitsObserver.Pos(i,0),isHuman)== false)
                    slotLine[i].GetChild(0).gameObject.SetActive(true);
                else
                    slotLine[i].GetChild(1).gameObject.SetActive(true);
            }

        }
    }


    public void ShowDropableSlot(dataModules.Attr[] attributes, bool isSkill = false) {
        for (int i = 0; i < 5; i++) {
            if (attributes.Length != 0) {
                // if (slotLine[i].GetComponent<Terrain>().terrain == PlayMangement.LineState.forest) continue;
                if (unitLine[i][0].childCount == 0) {
                    slotLine[i].GetChild(0).gameObject.SetActive(true);
                    if(isSkill)
                        slotLine[i].GetChild(0).GetChild(0).gameObject.SetActive(true);
                }
                else {
                    dataModules.Attr[] attribute = unitLine[i][0].GetChild(0).GetComponent<PlaceMonster>().unit.attributes;
                    for (int j = 0; j < attribute.Length; j++) {
                        if (attribute[j].name == "combo") {
                            unitLine[i][0].GetChild(0).position = new Vector3(unitLine[i][0].position.x, unitLine[i][0].GetChild(0).position.y + 1.5f, 0);
                            unitLine[i][0].GetChild(0).Find("InfoWindowTrigger").gameObject.SetActive(false);
                            unitLine[i][0].GetChild(0).Find("MagicTargetTrigger").gameObject.SetActive(false);
                            slotLine[i].GetChild(1).gameObject.SetActive(true);
                            slotLine[i].GetChild(2).gameObject.SetActive(true);
                            if (isSkill) {
                                slotLine[i].GetChild(1).GetChild(0).gameObject.SetActive(true);
                                slotLine[i].GetChild(2).GetChild(0).gameObject.SetActive(true);
                            }
                        }
                    }
                }
            }
            else {

                bool forrestAble = false;
                bool chainAble = false;
                for (int j = 0; j < attributes.Length; j++) {
                    if (attributes[j].name == "footslog") forrestAble = true;
                    else if (attributes[j].name == "combo") chainAble = true;
                }

                // if (slotLine[i].GetComponent<Terrain>().terrain == PlayMangement.LineState.forest && !forrestAble) continue;
                //if (!forrestAble) continue;
                if (!chainAble) {
                    if (unitLine[i][0].childCount == 0) {
                        slotLine[i].GetChild(0).gameObject.SetActive(true);
                        if (isSkill)
                            slotLine[i].GetChild(0).GetChild(0).gameObject.SetActive(true);
                    }
                    else {
                        dataModules.Attr[] attribute = unitLine[i][0].GetChild(0).GetComponent<PlaceMonster>().unit.attributes;
                        for (int j = 0; j < attribute.Length; j++) {
                            if (attribute[j].name == "combo") {
                                unitLine[i][0].GetChild(0).position = new Vector3(unitLine[i][0].position.x, unitLine[i][0].GetChild(0).position.y + 1.5f, 0);
                                unitLine[i][0].GetChild(0).Find("InfoWindowTrigger").gameObject.SetActive(false);
                                unitLine[i][0].GetChild(0).Find("MagicTargetTrigger").gameObject.SetActive(false);
                                slotLine[i].GetChild(1).gameObject.SetActive(true);
                                slotLine[i].GetChild(2).gameObject.SetActive(true);
                                if (isSkill) {
                                    slotLine[i].GetChild(1).GetChild(0).gameObject.SetActive(true);
                                    slotLine[i].GetChild(2).GetChild(0).gameObject.SetActive(true);
                                }
                            }
                        }
                    }
                }
                else {
                    if (unitLine[i][0].childCount == 0) {
                        slotLine[i].GetChild(0).gameObject.SetActive(true);
                            if (isSkill)
                                slotLine[i].GetChild(0).GetChild(0).gameObject.SetActive(true);
                        }
                    else {
                        if (unitLine[i][1].childCount == 0) {
                            unitLine[i][0].GetChild(0).position = new Vector3(unitLine[i][0].position.x, unitLine[i][0].GetChild(0).position.y + 1.5f, 0);
                            unitLine[i][0].GetChild(0).Find("InfoWindowTrigger").gameObject.SetActive(false);
                            unitLine[i][0].GetChild(0).Find("MagicTargetTrigger").gameObject.SetActive(false);
                            slotLine[i].GetChild(1).gameObject.SetActive(true);
                            slotLine[i].GetChild(2).gameObject.SetActive(true);
                            if (isSkill) {
                                slotLine[i].GetChild(1).GetChild(0).gameObject.SetActive(true);
                                slotLine[i].GetChild(2).GetChild(0).gameObject.SetActive(true);
                            }
                        }
                    }
                }
            }
        }
    }

    public void ShowDropableSlot(FieldType type) {
        switch (type) {
            case FieldType.FOOTSLOG:
                slotLine[4].GetChild(0).gameObject.SetActive(true);
                break;
            case FieldType.HILL:
                slotLine[0].GetChild(0).gameObject.SetActive(true);
                break;
        }
    }

    public void HideDropableSlot() {
        for (int i = 0; i < 5; i++) {
            if (unitLine[i][0].childCount > 0) {
                unitLine[i][0].GetChild(0).position = new Vector3(unitLine[i][0].position.x, unitLine[i][0].position.y, 0);
                unitLine[i][0].GetChild(0).Find("InfoWindowTrigger").gameObject.SetActive(true);
            }
            if (unitLine[i][1].childCount > 0) {
                unitLine[i][1].GetChild(0).position = new Vector3(unitLine[i][0].position.x, unitLine[i][1].position.y, 0);
                unitLine[i][1].GetChild(0).Find("InfoWindowTrigger").gameObject.SetActive(true);
            }
            for (int j = 0; j < 3; j++) {
                slotLine[i].GetChild(j).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1f);
                slotLine[i].GetChild(j).gameObject.SetActive(false);
                slotLine[i].GetChild(j).GetChild(0).gameObject.SetActive(false);
            }
            slotLine[i].Find("FightGuide").gameObject.SetActive(false);
        }
    }



    public void HighLightSlot(Transform target, bool highlighted) {
        if (target == null) return;
        int index = target.GetSiblingIndex();
        int lineNum = target.parent.GetSiblingIndex();
        GameObject fightEffect = slotLine[lineNum].GetChild(3).gameObject;
        if (highlighted) {
            target.GetComponent<SpriteRenderer>().color = new Color(0.639f, 0.925f, 0.105f, 1f);
            if (index > 0) {
                if (index == 1) unitLine[lineNum][0].GetChild(0).position = unitLine[lineNum][0].position;
                else unitLine[lineNum][0].GetChild(0).position = unitLine[lineNum][1].position;
            }
            if (enemyUnitLine[lineNum][0].childCount == 0 && enemyUnitLine[lineNum][1].childCount == 0)
                PlayMangement.instance.enemyPlayer.transform.Find("FightSpine").gameObject.SetActive(true);
            else {
                if (enemyUnitLine[lineNum][1].childCount > 0) {
                    enemyUnitLine[lineNum][1].GetChild(0).Find("FightSpine").gameObject.SetActive(true);
                }
                if (enemyUnitLine[lineNum][0].childCount > 0) {
                    enemyUnitLine[lineNum][0].GetChild(0).Find("FightSpine").gameObject.SetActive(true);
                }
            }
            target.parent.Find("FightGuide").gameObject.SetActive(true);
            SkeletonAnimation spineAni = target.parent.Find("FightGuide/Straight").GetComponent<SkeletonAnimation>();
            spineAni.Initialize(true);
            spineAni.Update(0);
            spineAni.AnimationName = "STRAIGHT";
        }
        else {
            target.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1f);
            if(index > 0)
                unitLine[lineNum][0].GetChild(0).position = new Vector3(unitLine[lineNum][0].position.x, unitLine[lineNum][0].position.y, 0);
            if (enemyUnitLine[lineNum][0].childCount == 0 && enemyUnitLine[lineNum][1].childCount == 0)
                PlayMangement.instance.enemyPlayer.transform.Find("FightSpine").gameObject.SetActive(false);
            else {
                if (enemyUnitLine[lineNum][0].childCount > 0)
                    enemyUnitLine[lineNum][0].GetChild(0).Find("FightSpine").gameObject.SetActive(false);
                if (enemyUnitLine[lineNum][1].childCount > 0)
                    enemyUnitLine[lineNum][1].GetChild(0).Find("FightSpine").gameObject.SetActive(false);
            }
            target.parent.Find("FightGuide").gameObject.SetActive(false);
        }
    }

    public void HighLightMagicSlot(Transform target, bool highlighted) {
        if (target == null) return;
        if (target.name != "AllMagicTrigger") {
            if (target.name == "BattleLineEffect") {
                if (highlighted)
                    target.GetComponent<SpriteRenderer>().color = new Color(0.639f, 0.925f, 0.105f, 1f);
                else
                    target.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1f);
            }
            else {
                if (highlighted)
                    target.parent.Find("ClickableUI").GetComponent<SkeletonAnimation>().Skeleton.SetSkin("green");
                else
                    target.parent.Find("ClickableUI").GetComponent<SkeletonAnimation>().Skeleton.SetSkin("WHITE");
            }
        }
        else {
            if (highlighted) {
                for (int i = 0; i < 5; i++)
                    slotLine[i].Find("BattleLineEffect").GetComponent<SpriteRenderer>().color = new Color(0.639f, 0.925f, 0.105f, 1f);
            }
            else {
                for (int i = 0; i < 5; i++)
                    slotLine[i].Find("BattleLineEffect").GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1f);
            }

        }
    }

    public GameObject DropUnit(GameObject card, Transform target) {
        if (target == null || target.childCount > 1) return null;
        card.GetComponent<CardHandler>().CARDUSED = true;
        HighLightSlot(target, false);
        HideDropableSlot();
        int cardIndex = card.transform.parent.GetSiblingIndex();
        CardHandler cardHandler = card.GetComponent<CardHandler>();
        int frontOrBack = 0; //뒤에 배치시 0, 앞에 배치시 1, 기본 뒤에 배치
        int lineNum = target.parent.GetSiblingIndex();
        switch (target.GetSiblingIndex()) {
            case 0:
                break;
            case 1:
                frontOrBack = 1;
                break;
            case 2:
                unitLine[lineNum][0].GetChild(0).SetParent(unitLine[lineNum][1]);
                unitLine[lineNum][1].GetChild(0).position = unitLine[lineNum][1].position;
                unitLine[lineNum][1].GetChild(0).GetComponent<PlaceMonster>().unitLocation = unitLine[lineNum][1].position;
                PlayMangement.instance.UnitsObserver
                    .UnitChangePosition(
                        unitLine[lineNum][1]
                        .GetChild(0)
                        .gameObject, 
                     new FieldUnitsObserver.Pos(lineNum, 1), 
                     PlayMangement.instance.player.isHuman
                );
                break;
            default:
                break;
        }

        GameObject placedMonster = PlayMangement.instance.SummonUnit(true, cardHandler.cardID, lineNum, frontOrBack, cardHandler.itemID, cardIndex, unitLine);
        return placedMonster;
    }
}

/// <summary>
/// 마법 처리
/// </summary>
public partial class CardDropManager {

    //protected string methods;
    //protected string[] filter;
    protected dataModules.Target[] targetData;
    //protected string magicArgs;
    //protected string magicTarget;

    public void ShowMagicalSlot(dataModules.Target[] targets) {
        if (targets == null) return;

        targetData = targets;

        int forceLine = PlayMangement.instance.forcedSummonAt > -1 ? ScenarioGameManagment.scenarioInstance.forcedSummonAt : -1;
        ActivateTarget(targets, forceLine);

        //if (ScenarioGameManagment.scenarioInstance != null && ScenarioGameManagment.scenarioInstance.isTutorial) {           
        //    int targetline = ScenarioGameManagment.scenarioInstance.forcedSummonAt - 1;
        //    PlayMangement.instance.backGroundTillObject.SetActive(true);

        //    if (Array.Exists(methods, x => x == "line")) {
        //        DeactivateTarget(unitLine, "unit");
        //        DeactivateTarget(enemyUnitLine, "unit");
        //        for (int i = 0; i < 5; i++) {
        //            if (i == targetline) {
        //                EffectSystem.Instance.MaskLine(i, false);
        //                slotLine[i].Find("BattleLineEffect").gameObject.SetActive(true);
        //                slotLine[i].Find("BattleLineEffect").gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
        //            }
        //            else {
        //                EffectSystem.Instance.MaskLine(i, true);

        //            }                        
        //        }
        //        return;
        //    }
        //    if (Array.Exists(methods, x => x == "unit")) {
        //        PlayMangement.instance.backGroundTillObject.SetActive(true);
        //        for (int i = 0; i < 5; i++) {
        //            if (Array.Exists(filter, x => Array.Exists(x, y => y == "enemy"))) {
        //                enemyUnitLine[i][0].GetChild(0).Find("ClickableUI").gameObject.SetActive(true);
        //                enemyUnitLine[i][0].GetChild(0).Find("MagicTargetTrigger").gameObject.SetActive(true);
        //            }                    
        //            else {
        //                if (i == targetline) {
        //                    unitLine[i][0].GetChild(0).Find("ClickableUI").gameObject.SetActive(true);
        //                    unitLine[i][0].GetChild(0).Find("MagicTargetTrigger").gameObject.SetActive(true);
        //                }
        //            }
        //        }
        //        return;
        //    }            
        //}


        //if (Array.Exists(methods, x=> x == "my"))
        //    ActivateTarget(unitLine, filter, dragFiltering, conditionChecker);
        //else if(Array.Exists(methods, x => x == "all")) {
        //    ActivateTarget(unitLine, filter, dragFiltering, conditionChecker);
        //    ActivateTarget(enemyUnitLine, filter, dragFiltering, conditionChecker);
        //}
        //else
        //    ActivateTarget(enemyUnitLine, filter, dragFiltering, conditionChecker);

        //if (Array.Exists(methods, x => x == "tool"))
        //    ActivateTarget(unitLine, filter, dragFiltering, conditionChecker, methods);
    }

    private void ActivateTarget(dataModules.Target[] targets, int forcedLine) {
        Transform[][] units = { };

        //PlayMangement.instance.backGroundTillObject.SetActive(true);

        string targetMethod = targets[0].method;
        string filter;


        if (targets[0].filter != null && targets[0].filter.Length > 0) {
            if (targets[0].filter[0] == "")
                filter = null;
            else
                filter = targets[0].filter[0];
        }
        else
            filter = null;



        if (filter != null) {
            if (filter == "my")
                units = unitLine;
            else if (filter == "enemy")
                units = enemyUnitLine;
            else {
            }
        }
        else
            units = null;

        if (targetMethod == "unit") {
            EffectSystem.Instance.ShowSlotWithDim();

            for(int i = 0; i< 5; i++) {
                if (forcedLine > -1 && i != forcedLine) continue;
                for (int j = 0; j< 2; j++) {
                    if (units[i][j].childCount > 0) {
                        if (units[i][j].GetChild(0).GetComponent<ambush>() == null) {
                            units[i][j].GetChild(0).Find("ClickableUI").gameObject.SetActive(true);
                            units[i][j].GetChild(0).Find("MagicTargetTrigger").gameObject.SetActive(true);
                        }
                    }
                }
            }
        }
        else if(targetMethod == "unit_hero") {
            for (int i = 0; i < 5; i++) {
                if (forcedLine > -1 && i != forcedLine) continue;
                for (int j = 0; j < 2; j++) {
                    if (units[i][j].childCount > 0) {
                        if (units[i][j].GetChild(0).GetComponent<ambush>() == null) {
                            units[i][j].GetChild(0).Find("ClickableUI").gameObject.SetActive(true);
                            units[i][j].GetChild(0).Find("MagicTargetTrigger").gameObject.SetActive(true);
                        }
                    }
                }
            }
            if (filter == "my")
                PlayMangement.instance.player.heroTargeting = true;
            else
                PlayMangement.instance.enemyPlayer.heroTargeting = true;
        }
        else if(targetMethod == "hero") {

            if (filter == "my")
                PlayMangement.instance.player.heroTargeting = true;
            else
                PlayMangement.instance.enemyPlayer.heroTargeting = true;
        }
        else if(targetMethod == "line") {
            PlayMangement.instance.backGroundTillObject.SetActive(true);
            DeactivateTarget(targetData, "unit");
            EffectSystem.Instance.EnemyHeroDim(true);

            if (units != null) {
                for (int i = 0; i < 5; i++) {
                    if (forcedLine > -1 && i != forcedLine) {
                        EffectSystem.Instance.MaskLine(i, true);
                        continue;
                    }
                    if ((units[i][0].childCount <= 0 && units[i][0].childCount <= 0)) {
                        EffectSystem.Instance.MaskLine(i, true);
                        continue;
                    }
                    if (units[i][0].childCount > 0) {
                        if (units[i][0].GetChild(0).GetComponent<ambush>() == null) {
                            slotLine[i].Find("BattleLineEffect").gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
                            slotLine[i].Find("BattleLineEffect").gameObject.SetActive(true);
                            EffectSystem.Instance.MaskLine(i, false);
                        }

                    }
                }
            }
            else {
                for (int i = 0; i < 5; i++) {
                    slotLine[i].Find("BattleLineEffect").gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
                    slotLine[i].Find("BattleLineEffect").gameObject.SetActive(true);
                    EffectSystem.Instance.MaskLine(i, false);
                }
            }
        }
        else {
            if (CheckConditionToUse("all")) {
                slotLine[2].Find("AllMagicTrigger").gameObject.SetActive(true);
                for (int i = 0; i < 5; i++) {
                    slotLine[i].Find("BattleLineEffect").gameObject.SetActive(true);
                    slotLine[i].Find("BattleLineEffect").gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
                    slotLine[i].Find("BattleLineEffect").GetComponent<BoxCollider2D>().enabled = false;
               
                }
            }
            EffectSystem.Instance.EnemyHeroDim(true);
        }     
    }


    //private void ActivateTarget(Transform[][] units, string[][] group, SkillModules.SkillHandler.DragFilter dragFiltering, SkillModules.ConditionChecker conditionChecker = null, string[] args = null) {
    //    if (group.Contains("unit")) {
    //        if (group.Contains("hero")) {
    //            if (magicArgs == "enemy") {
    //                enemyHero.Find("MagicTargetTrigger").gameObject.SetActive(true);
    //                enemyHero.Find("ClickableUI").gameObject.SetActive(true);
    //            }
    //            else {
    //                playerHero.Find("MagicTargetTrigger").gameObject.SetActive(true);
    //                playerHero.Find("ClickableUI").gameObject.SetActive(true);
    //            }
    //            EffectSystem.Instance.EnemyHeroDim();
    //        }
    //        else {
    //            EffectSystem.Instance.EnemyHeroDim(true);
    //        }

    //        EffectSystem.Instance.ShowSlotWithDim();
    //        for (int i = 0; i < 5; i++) {
    //            for (int j = 0; j < 2; j++) {
    //                if (dragFiltering != null) {
    //                    if (units[i][j].childCount == 0) continue;
    //                    if (!dragFiltering(units[i][j].GetChild(0).gameObject)) continue;
    //                }
    //                if (units[i][j].childCount > 0) {
    //                    if (units[i][j].GetChild(0).GetComponent<ambush>() == null) {
    //                        units[i][j].GetChild(0).Find("ClickableUI").gameObject.SetActive(true);
    //                        units[i][j].GetChild(0).Find("MagicTargetTrigger").gameObject.SetActive(true);
    //                    }
    //                    else if (conditionChecker != null) {
    //                        if (conditionChecker.GetType().Name.Contains("ambush")) {
    //                            units[i][j].GetChild(0).Find("ClickableUI").gameObject.SetActive(true);
    //                            units[i][j].GetChild(0).Find("MagicTargetTrigger").gameObject.SetActive(true);
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    else if (group.Contains("line")) {
    //        for (int i = 0; i < 5; i++) {
    //            if (args == null) {
    //                if (units[i][0].childCount > 0) {
    //                    if (units[i][0].GetChild(0).GetComponent<ambush>() == null) {
    //                        slotLine[i].Find("BattleLineEffect").gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
    //                        slotLine[i].Find("BattleLineEffect").gameObject.SetActive(true);
    //                        EffectSystem.Instance.MaskLine(i, false);
    //                    }
    //                }
    //                if (units[i][1].childCount > 0) {
    //                    slotLine[i].Find("BattleLineEffect").gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
    //                    slotLine[i].Find("BattleLineEffect").gameObject.SetActive(true);
    //                    EffectSystem.Instance.MaskLine(i, false);
    //                }
    //                if (units[i][0].childCount <= 0 && units[i][0].childCount <= 0) {
    //                    EffectSystem.Instance.MaskLine(i, true);
    //                }

    //            }
    //            else {
    //                slotLine[i].Find("BattleLineEffect").gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
    //                slotLine[i].Find("BattleLineEffect").gameObject.SetActive(true);
    //                EffectSystem.Instance.MaskLine(i, false);
    //            }
    //        }
    //    }

    //    else if (group.Contains("all")) {
    //        //PlayMangement.instance.backGroundTillObject.SetActive(true);
    //        EffectSystem.Instance.EnemyHeroDim(true);
    //        if (CheckConditionToUse(conditionChecker, group)) {
    //            slotLine[2].Find("AllMagicTrigger").gameObject.SetActive(true);
    //            for (int i = 0; i < 5; i++) {
    //                slotLine[i].Find("BattleLineEffect").gameObject.SetActive(true);
    //                slotLine[i].Find("BattleLineEffect").gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
    //                slotLine[i].Find("BattleLineEffect").GetComponent<BoxCollider2D>().enabled = false;
    //            }
    //        }
    //    }

    //    else {
    //        //Logger.LogError("undefined target" + group);
    //    }
    //}

    private bool CheckConditionToUse(string scope) {
        switch (scope) {
            case "all":
                PlayMangement playMangement = PlayMangement.instance;
                bool isPlayer = playMangement.player.isPlayer;

                bool isHuman;
                if (isPlayer) isHuman = playMangement.player.isHuman;
                else isHuman = playMangement.enemyPlayer.isHuman;

                var enemyUnits = PlayMangement.instance.UnitsObserver.GetAllFieldUnits(!isHuman);
                if(enemyUnits.Count == 0) return false;
                break;
        }
        return true;
    }

    public void HideMagicSlot() {
        if (targetData == null) return;
        //if (methods[0] == "my")
        //    DeactivateTarget(unitLine, magicTarget);
        //else if(methods[0] == "all") {
        //    DeactivateTarget(unitLine, magicTarget);
        //    DeactivateTarget(enemyUnitLine, magicTarget);
        //}
        //else
        //    DeactivateTarget(enemyUnitLine, magicTarget);

        //if (methods[0] == "tool")
        //    DeactivateTarget(unitLine, magicTarget, magicArgs);

        DeactivateTarget(targetData);
        EffectSystem.Instance.HideEveryDim();
        targetData = null;
    }

    private void DeactivateTarget(dataModules.Target[] target, string args = null) {
        Transform[][] units = { };
        string targetMethod = target[0].method;
        string filter;


        if (target[0].filter != null && target[0].filter.Length > 0) {
            if (target[0].filter[0] == "")
                filter = null;
            else
                filter = target[0].filter[0];
        }
        else
            filter = null;

        if (filter != null) {
            if (filter == "my")
                units = unitLine;
            else if (filter == "enemy")
                units = enemyUnitLine;
            else {
            }
        }
        else
            units = null;

        if (targetMethod == "unit") {
            EffectSystem.Instance.ShowSlotWithDim();
            for(int i = 0; i < 5; i++) {
                for (int j = 0; j < 2; j++) {
                    if (units[i][j].childCount > 0) {
                        units[i][j].GetChild(0).Find("ClickableUI").gameObject.SetActive(false);
                        units[i][j].GetChild(0).Find("MagicTargetTrigger").gameObject.SetActive(false);
                    }
                }
            }

        }
        else if (targetMethod == "unit_hero") {
            for (int i = 0; i < 5; i++) {
                for (int j = 0; j < 2; j++) {
                    if (units[i][j].childCount > 0) {
                        units[i][j].GetChild(0).Find("ClickableUI").gameObject.SetActive(false);
                        units[i][j].GetChild(0).Find("MagicTargetTrigger").gameObject.SetActive(false);
                    }
                }
            }
            PlayMangement.instance.enemyPlayer.heroTargeting = false;
        }
        else if (targetMethod == "hero") {
            PlayMangement.instance.enemyPlayer.heroTargeting = false;
            PlayMangement.instance.player.heroTargeting = true;
        }
        else if (targetMethod == "line") {

            if (units != null) {
                for (int i = 0; i < 5; i++) {
                    if ((units[i][0].childCount > 0 || units[i][1].childCount > 0) && args == null)
                        slotLine[i].Find("BattleLineEffect").gameObject.SetActive(false);
                    if (args != null)
                        slotLine[i].Find("BattleLineEffect").gameObject.SetActive(false);
                }
            }
            else {
                for(int i = 0; i<5; i++)
                    slotLine[i].Find("BattleLineEffect").gameObject.SetActive(false);
            }
        }
        else {
            slotLine[2].Find("AllMagicTrigger").gameObject.SetActive(false);
            for(int i=0; i<5; i++) {
                slotLine[i].Find("BattleLineEffect").gameObject.SetActive(false);
                slotLine[i].Find("BattleLineEffect").GetComponent<BoxCollider2D>().enabled = true;
            }
        }
    }


    //private void DeactivateTarget(Transform[][] units, string group, string args = null) {
    //    if (group.Contains("unit")) {
    //        if (group.Contains("hero")) {
    //            if (magicArgs == "enemy") {
    //                enemyHero.Find("MagicTargetTrigger").gameObject.SetActive(false);
    //                enemyHero.Find("ClickableUI").gameObject.SetActive(false);
    //            }
    //            else {
    //                playerHero.Find("MagicTargetTrigger").gameObject.SetActive(false);
    //                playerHero.Find("ClickableUI").gameObject.SetActive(false);
    //            }
    //        }

    //        for (int i = 0; i < 5; i++) {
    //            for (int j = 0; j < 2; j++) {
    //                if (units[i][j].childCount > 0) {
    //                    units[i][j].GetChild(0).Find("ClickableUI").gameObject.SetActive(false);
    //                    units[i][j].GetChild(0).Find("MagicTargetTrigger").gameObject.SetActive(false);
    //                }
    //            }
    //        }
    //    }

    //    else if (group.Contains("line")) {
    //        for (int i = 0; i < 5; i++) {
    //            if ((units[i][0].childCount > 0 || units[i][1].childCount > 0) && args == null)
    //                slotLine[i].Find("BattleLineEffect").gameObject.SetActive(false);

    //            if (args != null)
    //                slotLine[i].Find("BattleLineEffect").gameObject.SetActive(false);

    //        }
    //    }

    //    else if (group.Contains("all")) {
    //        slotLine[2].Find("AllMagicTrigger").gameObject.SetActive(false);
    //        for (int i = 0; i < 5; i++) {
    //            slotLine[i].Find("BattleLineEffect").gameObject.SetActive(false);
    //            slotLine[i].Find("BattleLineEffect").GetComponent<BoxCollider2D>().enabled = true;
    //        }
    //    }

    //    else {
    //        //Logger.LogError("undefined target" + group);
    //    }
    //}
}

public enum FieldType {
    FOOTSLOG,
    HILL
}