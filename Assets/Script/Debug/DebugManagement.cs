using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugManagement : PlayMangement
{    
    public GameObject unitDeadObject;
    public Dropdown idDropDown;
    public Dropdown enemyIDDropDown;

    public GameObject enemySummonPanel;
    public GameObject UnitCard;
    public GameObject MagicCard;

    public GameObject debugUnit;

    public ResourceManager resource;

    private void Awake() {
        instance = this;
        Dictionary<string, CardData> cardData = transform.GetComponent<DebugData>().cardData;
        List<string> keyList = new List<string>(cardData.Keys);
        idDropDown.AddOptions(keyList);
        enemyIDDropDown.AddOptions(keyList);
        player = player.GetComponent<DebugPlayer>();
        enemyPlayer = enemyPlayer.GetComponent<DebugPlayer>();
    }

    private void Start() {
        SetCamera();
    }   

    public void DrawCard() {
        if (isGame == false) return;
        bool race = player.isHuman;
        string text = idDropDown.options[idDropDown.value].text;
        CardData cardData = DebugData.Instance.cardData[text];

        player.GetComponent<DebugPlayer>().debugcdpm.AddCard(null, cardData);
    }

    public void StartDebugBattle() {
        StartCoroutine("battleCoroutine");
    }

    public void EnemySummonMonster() {
        InputField xField = enemySummonPanel.transform.Find("x").GetComponent<InputField>();
        InputField yField = enemySummonPanel.transform.Find("y").GetComponent<InputField>();

        int x = int.Parse(xField.text);
        int y = int.Parse(yField.text);

        if (x < 0 || x > 4)
            x = 0;

        if (y < 0 || y > 1)
            y = 0;

        string id = enemyIDDropDown.options[enemyIDDropDown.value].text;

        CardData cardData = DebugData.Instance.cardData[id];
        GameObject skeleton;
        skeleton = resource.cardSkeleton[id];

        GameObject monster = Instantiate(debugUnit);

        monster.transform.SetParent(enemyPlayer.transform.GetChild(y).GetChild(x));
        monster.transform.position = enemyPlayer.transform.GetChild(y).GetChild(x).position;
        GameObject monsterSkeleton = Instantiate(skeleton, monster.transform);

        /*
        DebugUnitSpine debugUnitSpine = monsterSkeleton.AddComponent<DebugUnitSpine>();
        UnitSpine spine = monsterSkeleton.GetComponent<UnitSpine>();

        debugUnitSpine.idleAnimationName = spine.idleAnimationName;
        debugUnitSpine.generalAttackName = spine.generalAttackName;
        debugUnitSpine.rangeUpAttackName = spine.rangeUpAttackName;
        debugUnitSpine.rangeDownAttackName = spine.rangeDownAttackName;
        debugUnitSpine.appearAnimationName = spine.appearAnimationName;
        debugUnitSpine.attackAnimationName = spine.attackAnimationName;
        debugUnitSpine.hitAnimationName = spine.hitAnimationName;
        debugUnitSpine.attackEventName = spine.attackEventName;
        debugUnitSpine.arrow = spine.arrow;
        Destroy(spine);
        */

        monsterSkeleton.name = "skeleton";
        
        monster.GetComponent<DebugUnit>().unit.HP = (int)cardData.hp;
        monster.GetComponent<DebugUnit>().unit.currentHP = (int)cardData.hp;
        monster.GetComponent<DebugUnit>().unit.originalAttack = (int)cardData.attack;
        monster.GetComponent<DebugUnit>().unit.attack = (int)cardData.attack;
        monster.GetComponent<DebugUnit>().unit.name = cardData.name;
        monster.GetComponent<DebugUnit>().unit.type = cardData.type;
        monster.GetComponent<DebugUnit>().unit.attackRange = cardData.attackRange;
        monster.GetComponent<DebugUnit>().unit.cost = cardData.cost;
        monster.GetComponent<DebugUnit>().unit.rarelity = cardData.rarelity;
        monster.GetComponent<DebugUnit>().unit.id = cardData.cardId;

        if (cardData.category_2 != "") {
            monster.GetComponent<DebugUnit>().unit.cardCategories = new string[2];
            monster.GetComponent<DebugUnit>().unit.cardCategories[0] = cardData.category_1;
            monster.GetComponent<DebugUnit>().unit.cardCategories[1] = cardData.category_2;
        }
        else {
            monster.GetComponent<DebugUnit>().unit.cardCategories = new string[1];
            monster.GetComponent<DebugUnit>().unit.cardCategories[0] = cardData.category_1;
        }

        if (cardData.attackTypes.Length > 0) {
            monster.GetComponent<DebugUnit>().unit.attackType = new string[cardData.attackTypes.Length];
            monster.GetComponent<DebugUnit>().unit.attackType = cardData.attackTypes;

        }

        // foreach (dataModules.Skill skill in cardData.skills) {
        //     foreach (var effect in skill.effects) {
        //         var newComp = monster.AddComponent(System.Type.GetType("SkillModules.UnitAbility_" + effect.method));
        //         if (newComp == null) {
        //             Debug.LogError(effect.method + "에 해당하는 컴포넌트를 찾을 수 없습니다.");
        //         }
        //         else {
        //             //((Ability)newComp).InitData(skill, true);
        //         }
        //     }
        // }


        monster.GetComponent<DebugUnit>().Init(cardData);
        monster.GetComponent<DebugUnit>().SpawnUnit();

        EnemyUnitsObserver.UnitAdded(monster, x, y);

    }

    IEnumerator battleCoroutine() {
        int line = 0;
        dragable = false;
        yield return new WaitForSeconds(1.1f);
        while (line < 5) {
            yield return battleLine(line);
            if (isGame == false) break;
            line++;
        }
        yield return new WaitForSeconds(1f);
        yield return new WaitForSeconds(2.0f);
        StopCoroutine("battleCoroutine");
        dragable = true;
    }

    IEnumerator battleLine(int line) {
        backGround.transform.GetChild(line).Find("BattleLineEffect").gameObject.SetActive(true);
        backGround.transform.GetChild(line).Find("BattleLineEffect").GetComponent<SpriteRenderer>().color = new Color(1, 98.0f / 255.0f, 31.0f / 255.0f, 0.6f);
        if (player.isHuman == false) {
            yield return battleUnit(player.backLine, line);
            yield return battleUnit(player.frontLine, line);
            yield return battleUnit(enemyPlayer.backLine, line);
            yield return battleUnit(enemyPlayer.frontLine, line);
        }
        else {
            yield return battleUnit(enemyPlayer.backLine, line);
            yield return battleUnit(enemyPlayer.frontLine, line);
            yield return battleUnit(player.backLine, line);
            yield return battleUnit(player.frontLine, line);

        }

        if (player.backLine.transform.GetChild(line).childCount != 0) {
            player.backLine.transform.GetChild(line).GetChild(0).GetComponent<DebugUnit>().CheckHP();
            player.backLine.transform.GetChild(line).GetChild(0).GetComponent<DebugUnit>().CheckDebuff();
        }
        if (player.frontLine.transform.GetChild(line).childCount != 0) {
            player.frontLine.transform.GetChild(line).GetChild(0).GetComponent<DebugUnit>().CheckHP();
            player.frontLine.transform.GetChild(line).GetChild(0).GetComponent<DebugUnit>().CheckDebuff();
        }
        if (enemyPlayer.backLine.transform.GetChild(line).childCount != 0) {
            enemyPlayer.backLine.transform.GetChild(line).GetChild(0).GetComponent<DebugUnit>().CheckHP();
            enemyPlayer.backLine.transform.GetChild(line).GetChild(0).GetComponent<DebugUnit>().CheckDebuff();
        }
        if (enemyPlayer.frontLine.transform.GetChild(line).childCount != 0) {
            enemyPlayer.frontLine.transform.GetChild(line).GetChild(0).GetComponent<DebugUnit>().CheckHP();
            enemyPlayer.backLine.transform.GetChild(line).GetChild(0).GetComponent<DebugUnit>().CheckDebuff();
        }
        backGround.transform.GetChild(line).Find("BattleLineEffect").GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.6f);
        backGround.transform.GetChild(line).Find("BattleLineEffect").gameObject.SetActive(false);
        EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.LINE_BATTLE_FINISHED, this);
    }

    IEnumerator battleUnit(GameObject lineObject, int line) {
        if (!isGame) yield break;
        if (lineObject.transform.GetChild(line).childCount != 0) {
            DebugUnit placeMonster = lineObject.transform.GetChild(line).GetChild(0).GetComponent<DebugUnit>();
            while (placeMonster.atkCount < placeMonster.maxAtkCount) {

                if (placeMonster.unit.attackType.Length > 0 && placeMonster.unit.attackType[0] == "double" && placeMonster.atkCount > 0 && placeMonster.unit.currentHP <= 0)
                    break;

                if (placeMonster.unit.attack <= 0)
                    break;
                placeMonster.GetTarget();


                yield return new WaitForSeconds(1.1f + placeMonster.atkTime);
            }
            placeMonster.atkCount = 0;
        }
        yield return null;
    }
}
