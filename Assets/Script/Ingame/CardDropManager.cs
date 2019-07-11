using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public void ShowDropableSlot(CardData card) {
        for (int i = 0; i < 5; i++) {
            if (card.attributes.Length == 0) {
                if (slotLine[i].GetComponent<Terrain>().terrain == PlayMangement.LineState.forest) continue;
                if (unitLine[i][0].childCount == 0) {
                    slotLine[i].GetChild(0).gameObject.SetActive(true);
                }
                else {
                    string[] attribute = unitLine[i][0].GetChild(0).GetComponent<PlaceMonster>().unit.attributes;
                    for (int j = 0; j < attribute.Length; j++) {
                        if (attribute[j] == "chain") {
                            unitLine[i][0].GetChild(0).position = new Vector3(unitLine[i][0].position.x, unitLine[i][0].GetChild(0).position.y + 0.5f, 0);
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

                if (slotLine[i].GetComponent<Terrain>().terrain == PlayMangement.LineState.forest && !forrestAble) continue;
                if (!chainAble) {
                    if (unitLine[i][0].childCount == 0) {
                        slotLine[i].GetChild(0).gameObject.SetActive(true);
                    }
                    else {
                        string[] attribute = unitLine[i][0].GetChild(0).GetComponent<PlaceMonster>().unit.attributes;
                        for (int j = 0; j < attribute.Length; j++) {
                            if (attribute[j] == "chain") {
                                unitLine[i][0].GetChild(0).position = new Vector3(unitLine[i][0].position.x, unitLine[i][0].GetChild(0).position.y + 0.5f, 0);
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
                            unitLine[i][0].GetChild(0).position = new Vector3(unitLine[i][0].position.x, unitLine[i][0].GetChild(0).position.y + 0.5f, 0);
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
                if (slotLine[i].GetComponent<Terrain>().terrain == PlayMangement.LineState.forest) continue;
                if (unitLine[i][0].childCount == 0) {
                    slotLine[i].GetChild(0).gameObject.SetActive(true);
                    if(isSkill)
                        slotLine[i].GetChild(0).GetChild(0).gameObject.SetActive(true);
                }
                else {
                    string[] attribute = unitLine[i][0].GetChild(0).GetComponent<PlaceMonster>().unit.attributes;
                    for (int j = 0; j < attribute.Length; j++) {
                        if (attribute[j] == "chain") {
                            unitLine[i][0].GetChild(0).position = new Vector3(unitLine[i][0].position.x, unitLine[i][0].GetChild(0).position.y + 0.5f, 0);
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

                if (slotLine[i].GetComponent<Terrain>().terrain == PlayMangement.LineState.forest && !forrestAble) continue;
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
                                unitLine[i][0].GetChild(0).position = new Vector3(unitLine[i][0].position.x, unitLine[i][0].GetChild(0).position.y + 0.5f, 0);
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
                            unitLine[i][0].GetChild(0).position = new Vector3(unitLine[i][0].position.x, unitLine[i][0].GetChild(0).position.y + 0.5f, 0);
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
            if (unitLine[i][0].childCount > 0)
                unitLine[i][0].GetChild(0).position = new Vector3(unitLine[i][0].position.x, unitLine[i][0].position.y, 0);
            if (unitLine[i][1].childCount > 0)
                unitLine[i][1].GetChild(0).position = new Vector3(unitLine[i][0].position.x, unitLine[i][1].position.y, 0);
            for (int j = 0; j < 3; j++) {
                slotLine[i].GetChild(j).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.6f);
                slotLine[i].GetChild(j).gameObject.SetActive(false);
                slotLine[i].GetChild(j).GetChild(0).gameObject.SetActive(false);
            }
        }
    }

    public void HighLightSlot(Transform target, bool highlighted) {
        if (target == null) return;
        int index = target.GetSiblingIndex();
        int lineNum = target.parent.GetSiblingIndex();
        GameObject fightEffect = slotLine[lineNum].GetChild(3).gameObject;
        Animator ani = fightEffect.GetComponent<Animator>();
        if (highlighted) {
            target.GetComponent<SpriteRenderer>().color = new Color(0.639f, 0.925f, 0.105f, 0.6f);
            if (index > 0) {
                if (index == 1) unitLine[lineNum][0].GetChild(0).position = unitLine[lineNum][0].position;
                else unitLine[lineNum][0].GetChild(0).position = unitLine[lineNum][1].position;
            }
            if (enemyUnitLine[lineNum][1].childCount > 0) {
                ani.SetTrigger("1_to_2");
            }
            else if (enemyUnitLine[lineNum][0].childCount > 0) {
                if (unitLine[lineNum][0].childCount > 0)
                    ani.SetTrigger("2_to_1");
                else
                    ani.SetTrigger("1_to_1");
            }
            else {
                ani.SetTrigger("1_to_hero");
            }

        }
        else {
            target.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.6f);
            if (index > 0) {
                unitLine[lineNum][0].GetChild(0).position = new Vector3(unitLine[lineNum][0].position.x, unitLine[lineNum][0].position.y + 0.5f, 0);
            }
            ani.SetTrigger("to_idle");
        }
    }

    public void HighLightMagicSlot(Transform target, bool highlighted) {
        if (target == null) return;
        if (target.name != "AllMagicTrigger") {
            if (target.name == "BattleLineEffect") {
                if (highlighted)
                    target.GetComponent<SpriteRenderer>().color = new Color(0.639f, 0.925f, 0.105f, 0.6f);
                else
                    target.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.6f);
            }
            else {
                if (highlighted)
                    target.parent.Find("ClickableUI").GetComponent<SpriteRenderer>().color = new Color(0.639f, 0.925f, 0.105f, 0.6f);
                else
                    target.parent.Find("ClickableUI").GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.6f);
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
    public void ShowMagicalSlot(string[] target, SkillModules.SkillHandler.DragFilter dragFiltering) {
        if (target == null) return;
        magicArgs = target[0];
        magicTarget = target[1];
        if (magicArgs == "my")
            ActivateTarget(unitLine, magicTarget, dragFiltering);
        else
            ActivateTarget(enemyUnitLine, magicTarget, dragFiltering);
    }

    private void ActivateTarget(Transform[][] units, string group, SkillModules.SkillHandler.DragFilter dragFiltering) {
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
                    if (units[i][0].childCount > 0) {
                        if(units[i][0].GetChild(0).GetComponent<ambush>() == null)
                            slotLine[i].Find("BattleLineEffect").gameObject.SetActive(true);
                    }
                    if (units[i][1].childCount > 0) {
                        slotLine[i].Find("BattleLineEffect").gameObject.SetActive(true);
                    }
                }
                break;
            default:
            case "all":
                slotLine[2].Find("AllMagicTrigger").gameObject.SetActive(true);
                break;
        }
    }

    public void HideMagicSlot() {
        if (magicArgs == null) return;
        if (magicArgs == "my")
            DeactivateTarget(unitLine, magicTarget);
        else
            DeactivateTarget(enemyUnitLine, magicTarget);
        magicArgs = magicTarget = null;
    }

    private void DeactivateTarget(Transform[][] units, string group) {
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
                    if (units[i][0].childCount > 0 || units[i][1].childCount > 0)
                        slotLine[i].Find("BattleLineEffect").gameObject.SetActive(false);
                }
                break;
            default:
            case "all":
                slotLine[2].Find("AllMagicTrigger").gameObject.SetActive(false);
                break;
        }
    }
}

public enum FieldType {
    FOOTSLOG,
    HILL
}