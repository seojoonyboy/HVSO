using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class CardDropManager : Singleton<CardDropManager> {
    protected Transform[] slotLine;
    protected Transform[][] unitLine;
    protected Transform[][] enemyUnitLine;

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
            unitLine[i][0] = unitSlotLines.GetChild(0).GetChild(i);
            unitLine[i][1] = unitSlotLines.GetChild(1).GetChild(i);
            enemyUnitLine[i][0] = unitSlotLines.parent.GetChild(1).GetChild(0).GetChild(i);
            enemyUnitLine[i][1] = unitSlotLines.parent.GetChild(1).GetChild(1).GetChild(i);
        }
    }
}

/// <summary>
/// 유닛 처리
/// </summary>
public partial class CardDropManager { 
    public void ShowDropableSlot(CardData card) {
        if (card.attributes.Length == 0) {
            for (int i = 0; i < 5; i++) {
                if (slotLine[i].GetComponent<Terrain>().terrain == PlayMangement.LineState.forest) continue;
                if (unitLine[i][0].childCount == 0) {
                    slotLine[i].GetChild(0).gameObject.SetActive(true);
                }
            }
            return;
        }

        bool forrestAble = false;
        bool chainAble = false;
        for (int i = 0; i < card.attributes.Length; i++) {
            if (card.attributes[i] == "footslog") forrestAble = true;
            else if (card.attributes[i] == "chain") chainAble = true;
        }

        for (int i = 0; i < 5; i++) {
            if (slotLine[i].GetComponent<Terrain>().terrain == PlayMangement.LineState.forest && !forrestAble) continue;
            if (!chainAble) {
                if (unitLine[i][0].childCount == 0) {
                    slotLine[i].GetChild(0).gameObject.SetActive(true);
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
            slotLine[i].GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 155.0f / 255.0f);
            slotLine[i].GetChild(1).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 155.0f / 255.0f);
            slotLine[i].GetChild(2).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 155.0f / 255.0f);
            slotLine[i].GetChild(0).gameObject.SetActive(false);
            slotLine[i].GetChild(1).gameObject.SetActive(false);
            slotLine[i].GetChild(2).gameObject.SetActive(false);
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
                //ani.SetTrigger("1_to_2");
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
                break;
            default:
                break;
        }

        GameObject placedMonster = Instantiate(cardHandler.unit, unitLine[lineNum][frontOrBack]);
        placedMonster.transform.position = unitLine[lineNum][frontOrBack].position;
        placedMonster.GetComponent<PlaceMonster>().isPlayer = true;

        placedMonster.GetComponent<PlaceMonster>().unit.name = cardHandler.cardData.name;
        placedMonster.GetComponent<PlaceMonster>().unit.HP = (int)cardHandler.cardData.hp;
        placedMonster.GetComponent<PlaceMonster>().unit.currentHP = (int)cardHandler.cardData.hp;
        placedMonster.GetComponent<PlaceMonster>().unit.attack = (int)cardHandler.cardData.attack;
        placedMonster.GetComponent<PlaceMonster>().unit.type = cardHandler.cardData.type;
        placedMonster.GetComponent<PlaceMonster>().unit.attackRange = cardHandler.cardData.attackRange;
        placedMonster.GetComponent<PlaceMonster>().unit.cost = cardHandler.cardData.cost;
        placedMonster.GetComponent<PlaceMonster>().unit.rarelity = cardHandler.cardData.rarelity;

        if(cardHandler.cardData.category_2 != "") {
            placedMonster.GetComponent<PlaceMonster>().unit.cardCategories = new string[2];
            placedMonster.GetComponent<PlaceMonster>().unit.cardCategories[0] = cardHandler.cardData.category_1;
            placedMonster.GetComponent<PlaceMonster>().unit.cardCategories[1] = cardHandler.cardData.category_2;
        }
        else {
            placedMonster.GetComponent<PlaceMonster>().unit.cardCategories = new string[1];
            placedMonster.GetComponent<PlaceMonster>().unit.cardCategories[0] = cardHandler.cardData.category_1;
        }


        GameObject skeleton = Instantiate(cardHandler.skeleton, placedMonster.transform);
        skeleton.name = "skeleton";
        skeleton.transform.localScale = new Vector3(-1, 1, 1);
        placedMonster.name = placedMonster.GetComponent<PlaceMonster>().unit.name;

        placedMonster.GetComponent<PlaceMonster>().Init();
        placedMonster.GetComponent<PlaceMonster>().SpawnUnit();
        //GetComponent<Image>().enabled = false;
        PlayMangement.instance.player.isPicking.Value = false;
        PlayMangement.instance.player.resource.Value -= cardHandler.cardData.cost;
        PlayMangement.instance.player.ActivePlayer();

        GameObject.Find("Player").transform.GetChild(0).GetComponent<PlayerController>().cdpm.DestroyCard(cardIndex);

        PlayMangement.instance.PlayerUnitsObserver.UnitAdded(placedMonster, lineNum, frontOrBack);
        return placedMonster;
    }
}

/// <summary>
/// 마법 처리
/// </summary>
public partial class CardDropManager {
    //public override void ShowDropableSlot(List<string> skills = null) {
    //}
}

    public enum FieldType {
    FOOTSLOG,
    HILL
}