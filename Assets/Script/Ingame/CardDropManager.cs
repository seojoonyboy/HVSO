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
                slotLine[i].GetChild(j).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 155.0f / 255.0f);
                slotLine[i].GetChild(j).gameObject.SetActive(false);
            }
        }
    }

    public void BeginCheckLines() {
        var lines = PlayMangement.instance.player.dropableLines;
        foreach (Transform line in lines) {
            line.Find("BattleLineEffect").gameObject.SetActive(true);
            line.Find("BattleLineEffect").GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    public void HideDropableLine() {
        var lines = PlayMangement.instance.player.dropableLines;
        foreach (Transform line in lines) {
            line.Find("BattleLineEffect").gameObject.SetActive(false);
            line.Find("BattleLineEffect").GetComponent<SpriteRenderer>().enabled = true;
        }
    }

    public void HighLightSlot(Transform target, bool highlighted) {
        if (target == null) return;
        int index = target.GetSiblingIndex();
        int lineNum = target.parent.GetSiblingIndex();
        GameObject fightEffect = slotLine[lineNum].GetChild(3).gameObject;
        Animator ani = fightEffect.GetComponent<Animator>();
        if (highlighted) {
            target.GetComponent<SpriteRenderer>().color = new Color(163.0f / 255.0f, 236.0f / 255.0f, 27.0f / 255.0f, 155.0f / 255.0f);
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
            target.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 155.0f / 255.0f);
            if (index > 0) {
                unitLine[lineNum][0].GetChild(0).position = new Vector3(unitLine[lineNum][0].position.x, unitLine[lineNum][0].position.y + 0.5f, 0);
            }
            ani.SetTrigger("to_idle");
        }
    }

    public void HighLightMagicSlot(Transform target, bool highlighted) {
        if (target == null) return;
        if (target.name != "AllMagicTrigger") {
            if(highlighted)
                target.parent.Find("ClickableUI").GetComponent<SpriteRenderer>().color = new Color(163.0f / 255.0f, 236.0f / 255.0f, 27.0f / 255.0f, 155.0f / 255.0f);
            else
                target.parent.Find("ClickableUI").GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 155.0f / 255.0f);
        }
    }

    public GameObject DropUnit(GameObject card, Transform target) {
        if (target == null || target.childCount > 0) return null;
        HighLightSlot(target, false);
        HideDropableSlot();
        int cardIndex = 0;
        if (card.transform.parent.parent.name == "CardSlot_1")
            cardIndex = card.transform.parent.GetSiblingIndex();
        else {
            Transform slot1 = card.transform.parent.parent.parent.GetChild(0);
            for (int i = 0; i < 5; i++) {
                if (slot1.GetChild(i).gameObject.activeSelf)
                    cardIndex++;
            }
            cardIndex += card.transform.parent.GetSiblingIndex();
        }
        CardHandler cardHandler = card.GetComponent<CardHandler>();
        int frontOrBack = 0; //뒤에 배치시 0, 앞에 배치시 1, 기본 뒤에 배치
        string posMessage = "front";
        int lineNum = target.parent.GetSiblingIndex();
        switch (target.GetSiblingIndex()) {
            case 0:
                posMessage = "front";
                break;
            case 1:
                posMessage = "front";
                frontOrBack = 1;
                break;
            case 2:
                posMessage = "near";
                unitLine[lineNum][0].GetChild(0).SetParent(unitLine[lineNum][1]);
                unitLine[lineNum][1].GetChild(0).position = unitLine[lineNum][1].position;
                unitLine[lineNum][1].GetChild(0).GetComponent<PlaceMonster>().unitLocation = unitLine[lineNum][1].position;
                break;
            default:
                break;
        }
        string[] args = {cardHandler.itemID.ToString(),
                        "place",
                        lineNum.ToString(),
                        PlayMangement.instance.player.isHuman ? "human" : "orc",
                        posMessage};
        PlayMangement.instance.socketHandler.UseCard(args);

        GameObject placedMonster = Instantiate(cardHandler.unit, unitLine[lineNum][frontOrBack]);
        placedMonster.transform.position = unitLine[lineNum][frontOrBack].position;
        placedMonster.GetComponent<PlaceMonster>().isPlayer = true;

        placedMonster.GetComponent<PlaceMonster>().itemId = (int)cardHandler.itemID;
        placedMonster.GetComponent<PlaceMonster>().unit.name = cardHandler.cardData.name;
        placedMonster.GetComponent<PlaceMonster>().unit.HP = (int)cardHandler.cardData.hp;
        placedMonster.GetComponent<PlaceMonster>().unit.currentHP = (int)cardHandler.cardData.hp;
        placedMonster.GetComponent<PlaceMonster>().unit.attack = (int)cardHandler.cardData.attack;
        placedMonster.GetComponent<PlaceMonster>().unit.type = cardHandler.cardData.type;
        placedMonster.GetComponent<PlaceMonster>().unit.attackRange = cardHandler.cardData.attackRange;
        placedMonster.GetComponent<PlaceMonster>().unit.cost = cardHandler.cardData.cost;
        placedMonster.GetComponent<PlaceMonster>().unit.rarelity = cardHandler.cardData.rarelity;
        placedMonster.GetComponent<PlaceMonster>().unit.id = cardHandler.cardData.cardId;
        placedMonster.GetComponent<PlaceMonster>().unit.attributes = cardHandler.cardData.attributes;

        if (cardHandler.cardData.category_2 != "") {
            placedMonster.GetComponent<PlaceMonster>().unit.cardCategories = new string[2];
            placedMonster.GetComponent<PlaceMonster>().unit.cardCategories[0] = cardHandler.cardData.category_1;
            placedMonster.GetComponent<PlaceMonster>().unit.cardCategories[1] = cardHandler.cardData.category_2;
        }
        else {
            placedMonster.GetComponent<PlaceMonster>().unit.cardCategories = new string[1];
            placedMonster.GetComponent<PlaceMonster>().unit.cardCategories[0] = cardHandler.cardData.category_1;
        }

        if (cardHandler.cardData.attackTypes.Length > 0) {
            placedMonster.GetComponent<PlaceMonster>().unit.attackType = new string[cardHandler.cardData.attackTypes.Length];
            placedMonster.GetComponent<PlaceMonster>().unit.attackType = cardHandler.cardData.attackTypes;

        }

        GameObject skeleton = Instantiate(cardHandler.skeleton, placedMonster.transform);
        skeleton.name = "skeleton";
        skeleton.transform.localScale = new Vector3(-1, 1, 1);
        placedMonster.name = placedMonster.GetComponent<PlaceMonster>().unit.name;

        placedMonster.GetComponent<PlaceMonster>().Init(cardHandler.cardData);
        placedMonster.GetComponent<PlaceMonster>().SpawnUnit();
        PlayMangement.instance.player.isPicking.Value = false;
        PlayMangement.instance.player.resource.Value -= cardHandler.cardData.cost;
        if (PlayMangement.instance.player.isHuman)
            PlayMangement.instance.player.ActivePlayer();
        else
            PlayMangement.instance.player.ActiveOrcTurn();
        GameObject.Find("Player").transform.GetChild(0).GetComponent<PlayerController>().cdpm.DestroyCard(cardIndex);
        PlayMangement.instance.PlayerUnitsObserver.RefreshFields(unitLine);

        return placedMonster;
    }
}

/// <summary>
/// 마법 처리
/// </summary>
public partial class CardDropManager {
    public void ShowMagicalSlot(string target) {
        if (target == null) return;
        switch (target) {
            case "my":
                for (int i = 0; i < 5; i++) {
                    for (int j = 0; j < 2; j++) {
                        if (unitLine[i][j].childCount > 0) {
                            unitLine[i][j].GetChild(0).Find("ClickableUI").gameObject.SetActive(true);
                            unitLine[i][j].GetChild(0).Find("MagicTargetTrigger").gameObject.SetActive(true);
                        }
                    }
                }
                break;
            case "enemy":
                for (int i = 0; i < 5; i++) {
                    for (int j = 0; j < 2; j++) {
                        if (enemyUnitLine[i][j].childCount > 0) {
                            enemyUnitLine[i][j].GetChild(0).Find("ClickableUI").gameObject.SetActive(true);
                            enemyUnitLine[i][j].GetChild(0).Find("MagicTargetTrigger").gameObject.SetActive(true);
                        }
                    }
                }
                break;
            case "line":
                for (int i = 0; i < 5; i++) {
                    if (enemyUnitLine[i][0].childCount > 0 || enemyUnitLine[i][1].childCount > 0)
                        slotLine[i].Find("BattleLineEffect").gameObject.SetActive(true);
                }
                break;
            case "all":
                slotLine[2].Find("AllMagicTrigger").gameObject.SetActive(true);
                break;
        }
    }
    /// <summary>
    /// 일정 공격력 이상의 적만 타겟팅
    /// </summary>
    /// <param name="target"></param>
    /// <param name="enemyAttack"></param> 
    public void ShowMagicalSlot(string target, int enemyAttack) {
        for (int i = 0; i < 5; i++) {
            for (int j = 0; j < 2; j++) {
                if (enemyUnitLine[i][j].childCount > 0) {
                    if (enemyUnitLine[i][j].GetChild(0).GetComponent<PlaceMonster>().unit.attack >= enemyAttack) {
                        enemyUnitLine[i][j].GetChild(0).Find("ClickableUI").gameObject.SetActive(true);
                        enemyUnitLine[i][j].GetChild(0).Find("MagicTargetTrigger").gameObject.SetActive(true);
                    }
                }
            }
        }
    }


    public void HideMagicSlot(CardData card) {
        if (card.skills == null) return;
        string target = card.skills[0].targets[0].args[0];
        switch (target) {
            case "my":
                for (int i = 0; i < 5; i++) {
                    for (int j = 0; j < 2; j++) {
                        if (unitLine[i][j].childCount > 0) {
                            unitLine[i][j].GetChild(0).Find("ClickableUI").gameObject.SetActive(false);
                            unitLine[i][j].GetChild(0).Find("MagicTargetTrigger").gameObject.SetActive(false);
                        }
                    }
                }
                break;
            case "enemy":
                int attackLimit = 0;
                if (card.skills[0].activate.conditions.Length > 0)
                    attackLimit = int.Parse(card.skills[0].activate.conditions[0].args[0]);
                for (int i = 0; i < 5; i++) {
                    for (int j = 0; j < 2; j++) {
                        if (enemyUnitLine[i][j].childCount > 0) {
                            if (enemyUnitLine[i][j].GetChild(0).GetComponent<PlaceMonster>().unit.attack >= attackLimit) {
                                enemyUnitLine[i][j].GetChild(0).Find("ClickableUI").gameObject.SetActive(false);
                                enemyUnitLine[i][j].GetChild(0).Find("MagicTargetTrigger").gameObject.SetActive(false);
                            }
                        }
                    }
                }
                break;
            case "line":
                for (int i = 0; i < 5; i++) {
                    if (enemyUnitLine[i][0].childCount > 0 || enemyUnitLine[i][1].childCount > 0)
                        slotLine[i].Find("BattleLineEffect").gameObject.SetActive(false);
                }
                break;
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