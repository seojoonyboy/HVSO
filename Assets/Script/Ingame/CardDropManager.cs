using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Spine;
using Spine.Unity;

public partial class CardDropManager : Singleton<CardDropManager> {
    protected Transform[] slotLine;
    public Transform[][] unitLine;
    public Transform[][] enemyUnitLine;

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
        Logger.Log("ForcedShowDropableSlot");
        for(int i=0; i<5; i++) {
            if(i == line - 1) {
                if (args == null || args == "slot") {
                    if (unitLine[i][0].childCount == 0) {
                        slotLine[i].GetChild(0).gameObject.SetActive(true);
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
                }
            }
            else if (args == "line") {
                slotLine[i].Find("BattleLineEffect").gameObject.SetActive(true);
            }

            if (i == line - 1)
                break;
        }


    }




    public void ShowDropableSlot(dataModules.CollectionCard card) {
        if(ScenarioGameManagment.scenarioInstance != null && ScenarioGameManagment.scenarioInstance.isTutorial) {


            if (card.skills.Length > 0) {
                try {
                    magicArgs = card.skills[0].target.args[0];
                    magicTarget = card.skills[0].target.args[1];
                }
                catch(System.IndexOutOfRangeException ex) {

                }
                
            }
            
            int targetline = ScenarioGameManagment.scenarioInstance.forcedSummonAt;
            int limitLine = ScenarioGameManagment.scenarioInstance.forcedLine;
            if(targetline != -1) {
                ForcedShowDropableSlot(targetline, magicTarget);
                return;
            }
            if(limitLine != -1) {
                ForcedLimitLineSlot(limitLine, magicTarget);
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
                    string[] attribute = unitLine[i][0].GetChild(0).GetComponent<PlaceMonster>().unit.attributes;
                    for (int j = 0; j < attribute.Length; j++) {
                        if (attribute[j] == "chain") {
                            if(unitLine[i][1].childCount != 0) continue;
                            unitLine[i][0].GetChild(0).position = new Vector3(unitLine[i][0].position.x, unitLine[i][0].GetChild(0).position.y + 1.5f, 0);
                            unitLine[i][0].GetChild(0).Find("InfoWindowTrigger").gameObject.SetActive(false);
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
                    if (card.attributes[j] == "footslog") forrestAble = true;
                    else if (card.attributes[j] == "chain") chainAble = true;
                }

                // if (slotLine[i].GetComponent<Terrain>().terrain == PlayMangement.LineState.forest && !forrestAble) continue;
                // if (!forrestAble) continue;
                if (!chainAble) {
                    if (unitLine[i][0].childCount == 0) {
                        slotLine[i].GetChild(0).gameObject.SetActive(true);
                    }
                    else if(unitLine[i][1].childCount == 0) {
                        string[] attribute = unitLine[i][0].GetChild(0).GetComponent<PlaceMonster>().unit.attributes;
                        for (int j = 0; j < attribute.Length; j++) {
                            if (attribute[j] == "chain") {
                                unitLine[i][0].GetChild(0).position = new Vector3(unitLine[i][0].position.x, unitLine[i][0].GetChild(0).position.y + 1.5f, 0);
                                unitLine[i][0].GetChild(0).Find("InfoWindowTrigger").gameObject.SetActive(false);
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
                            slotLine[i].GetChild(1).gameObject.SetActive(true);
                            slotLine[i].GetChild(2).gameObject.SetActive(true);
                        }
                    }
                }
            }
        }
    }

    public void ShowDropableSlot(string[] attributes, bool isSkill = false) {
        for (int i = 0; i < 5; i++) {
            if (attributes.Length == 0) {
                // if (slotLine[i].GetComponent<Terrain>().terrain == PlayMangement.LineState.forest) continue;
                if (unitLine[i][0].childCount == 0) {
                    slotLine[i].GetChild(0).gameObject.SetActive(true);
                    if(isSkill)
                        slotLine[i].GetChild(0).GetChild(0).gameObject.SetActive(true);
                }
                else {
                    string[] attribute = unitLine[i][0].GetChild(0).GetComponent<PlaceMonster>().unit.attributes;
                    for (int j = 0; j < attribute.Length; j++) {
                        if (attribute[j] == "chain") {
                            unitLine[i][0].GetChild(0).position = new Vector3(unitLine[i][0].position.x, unitLine[i][0].GetChild(0).position.y + 1.5f, 0);
                            unitLine[i][0].GetChild(0).Find("InfoWindowTrigger").gameObject.SetActive(false);
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
                    if (attributes[j] == "footslog") forrestAble = true;
                    else if (attributes[j] == "chain") chainAble = true;
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
                        string[] attribute = unitLine[i][0].GetChild(0).GetComponent<PlaceMonster>().unit.attributes;
                        for (int j = 0; j < attribute.Length; j++) {
                            if (attribute[j] == "chain") {
                                unitLine[i][0].GetChild(0).position = new Vector3(unitLine[i][0].position.x, unitLine[i][0].GetChild(0).position.y + 1.5f, 0);
                                unitLine[i][0].GetChild(0).Find("InfoWindowTrigger").gameObject.SetActive(false);
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
                unitLine[lineNum][0].GetChild(0).position = new Vector3(unitLine[lineNum][0].position.x, unitLine[lineNum][0].position.y + 1.5f, 0);
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

    protected string magicArgs;
    protected string magicTarget;
    public void ShowMagicalSlot(string[] target, SkillModules.SkillHandler.DragFilter dragFiltering, SkillModules.ConditionChecker conditionChecker = null) {
        if (target == null) return;
        magicArgs = target[0];
        magicTarget = target[1];
        if (magicArgs == "my")
            ActivateTarget(unitLine, magicTarget, dragFiltering: dragFiltering, conditionChecker);
        else if(magicArgs == "all") {
            ActivateTarget(unitLine, magicTarget, dragFiltering: dragFiltering, conditionChecker);
            ActivateTarget(enemyUnitLine, magicTarget, dragFiltering, conditionChecker);
        }
        else
            ActivateTarget(enemyUnitLine, magicTarget, dragFiltering, conditionChecker);

        if (magicArgs == "tool")
            ActivateTarget(unitLine, magicTarget, dragFiltering, conditionChecker, magicArgs);
    }

    private void ActivateTarget(Transform[][] units, string group, SkillModules.SkillHandler.DragFilter dragFiltering, SkillModules.ConditionChecker conditionChecker = null, string args = null) {
        switch (group) {
            case "unit":
                for (int i = 0; i < 5; i++) {
                    for (int j = 0; j < 2; j++) {
                        if(dragFiltering != null) {
                            if(units[i][j].childCount == 0) continue;
                            if(!dragFiltering(units[i][j].GetChild(0).gameObject)) continue;
                        }
                        if (units[i][j].childCount > 0 && units[i][j].GetChild(0).GetComponent<ambush>() == null) {
                            units[i][j].GetChild(0).Find("ClickableUI").gameObject.SetActive(true);
                            units[i][j].GetChild(0).Find("MagicTargetTrigger").gameObject.SetActive(true);
                        }
                    }
                }
                break;
            case "line":
                for (int i = 0; i < 5; i++) {
                    if (args == null) {
                        if (units[i][0].childCount > 0) {
                            if (units[i][0].GetChild(0).GetComponent<ambush>() == null)
                                slotLine[i].Find("BattleLineEffect").gameObject.SetActive(true);
                        }
                        if (units[i][1].childCount > 0) {
                            slotLine[i].Find("BattleLineEffect").gameObject.SetActive(true);
                        }
                    }
                    else {
                        slotLine[i].Find("BattleLineEffect").gameObject.SetActive(true);
                    }
                }
                break;
            default:
            case "all":
                if (CheckConditionToUse(conditionChecker, group)) {
                    slotLine[2].Find("AllMagicTrigger").gameObject.SetActive(true);
                    for (int i = 0; i < 5; i++) {
                        slotLine[i].Find("BattleLineEffect").gameObject.SetActive(true);
                        slotLine[i].Find("BattleLineEffect").GetComponent<BoxCollider2D>().enabled = false;
                    }
                }
                break;
        }
    }

    private bool CheckConditionToUse(SkillModules.ConditionChecker conditionChecker, string scope) {
        if (conditionChecker == null) return true;

        switch (scope) {
            case "all":
                PlayMangement playMangement = PlayMangement.instance;
                bool isPlayer = playMangement.player.isPlayer;

                bool isHuman;
                if (isPlayer) isHuman = playMangement.player.isHuman;
                else isHuman = playMangement.enemyPlayer.isHuman;

                var enemyUnits = PlayMangement.instance.UnitsObserver.GetAllFieldUnits(!isHuman);
                if(enemyUnits.Count == 0) return false;
                if(conditionChecker.args[0] == "enemy") {
                    if(conditionChecker.args[1] == "dmg_gte") {
                        int atk_std = 0;
                        int.TryParse(conditionChecker.args[2], out atk_std);

                        //IngameNotice.instance.SetNotice("타겟이 존재하지 않습니다.");
                        bool result = enemyUnits.Exists(x => x.GetComponent<PlaceMonster>().unit.attack >= atk_std);
                        return result;
                    }
                    else if(conditionChecker.args[1] == "without_attr") {
                        string attr = conditionChecker.args[2];
                        bool result = false;
                        enemyUnits.RemoveAll(x => x.GetComponent<PlaceMonster>().unit.attributes.ToList().Exists(y => y.CompareTo(attr) == 0));
                        result = enemyUnits.Count != 0;
                        return result;
                    }
                }
                break;
        }
        return true;
    }

    public void HideMagicSlot() {
        if (magicArgs == null) return;
        if (magicArgs == "my")
            DeactivateTarget(unitLine, magicTarget);
        else if(magicArgs == "all") {
            DeactivateTarget(unitLine, magicTarget);
            DeactivateTarget(enemyUnitLine, magicTarget);
        }
        else
            DeactivateTarget(enemyUnitLine, magicTarget);

        if (magicArgs == "tool")
            DeactivateTarget(unitLine, magicTarget, magicArgs);

        magicArgs = magicTarget = null;
    }

    private void DeactivateTarget(Transform[][] units, string group, string args = null) {
        switch (group) {
            case "unit":
                for (int i = 0; i < 5; i++) {
                    for (int j = 0; j < 2; j++) {
                        if (units[i][j].childCount > 0) {
                            units[i][j].GetChild(0).Find("ClickableUI").gameObject.SetActive(false);
                            units[i][j].GetChild(0).Find("MagicTargetTrigger").gameObject.SetActive(false);
                        }
                    }
                }
                break;
            case "line":
                for (int i = 0; i < 5; i++) {
                    if ((units[i][0].childCount > 0 || units[i][1].childCount > 0) && args == null)
                        slotLine[i].Find("BattleLineEffect").gameObject.SetActive(false);

                    if(args != null)
                        slotLine[i].Find("BattleLineEffect").gameObject.SetActive(false);

                }
                break;
            default:
            case "all":
                slotLine[2].Find("AllMagicTrigger").gameObject.SetActive(false);
                for (int i = 0; i < 5; i++) {
                    slotLine[i].Find("BattleLineEffect").gameObject.SetActive(false);
                    slotLine[i].Find("BattleLineEffect").GetComponent<BoxCollider2D>().enabled = true;
                }
                break;
        }
    }
}

public enum FieldType {
    FOOTSLOG,
    HILL
}