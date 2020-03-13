using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using UnityEngine;
using SocketFormat;

public class CardUseSendSocket : MonoBehaviour {

    private dataModules.Target[] targets;
    private MagicDragHandler magic;
    private PlaceMonster monster;
    private Transform highlight;
    public object skillTarget;
    public bool isSendMessageDone = false;

    public void Init() {
        magic = GetComponent<MagicDragHandler>();
        monster = GetComponent<PlaceMonster>();
        if(magic != null) {
            targets = magic.cardData.targets;
            highlight = magic.highlightedSlot;
        }
        else targets = monster.unit.targets;
        SendSocket();
        DestroyMyCard();
        Destroy(this);
    }
    // public void SendingMessage(bool after) {
    //     if(TargetSelectExist() != after) return;
    //     if(isPlayingCard()) SendSocket();
    //     if(isFieldCard()) SkillActivate();
    // }

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

    // private bool isPlayingCard() {
    //     if (!isPlayer) return false;
    //     if (targetData == null) return false;
    //     if (!targetData.GetType().IsArray) return false;
    //     if (myObject != (GameObject)(((object[])targetData)[1])) return false;
    //     if (!triggerList.Exists(x => IngameEventHandler.EVENT_TYPE.END_CARD_PLAY == x)) return false;
    //     return true;
    // }

    // private bool isFieldCard() {
    //     if (!isPlayer) return false;
    //     if (!TargetSelectExist()) return false;
    //     if (!triggerList.Exists(x => IngameEventHandler.EVENT_TYPE.BEGIN_ORC_POST_TURN == x)) return false;
    //     return true;
    // }

    // private void SkillActivate() {
    //     BattleConnector connector = PlayMangement.instance.socketHandler;
    //     MessageFormat format = MessageForm(false);
    //     if(format.targets.Count() == 0) return;
    //     connector.UnitSkillActivate(format);
    // }

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
        //Select 스킬 있을 시 // 임시 select 무효
        // Skill select = skills.ToList().Find(x => x.TargetSelectExist());
        // //Playing scope 있는 Select일 때
        // if(select != null && select.isPlayingSelect()) {
        //     List<GameObject> selectList = select.GetTargetFromSelect();
        //     if(selectList != null && selectList.Count > 0)
        //         targets.Add(ArgumentForm(select, true, isEndCardPlay));
        // }
        // else if(!isEndCardPlay) {
        //     List<GameObject> selectList = select.GetTargetFromSelect();
        //     if(selectList != null && selectList.Count > 0)
        //         targets.Add(ArgumentForm(select, true, isEndCardPlay));
        // }
        
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
        List<GameObject> selectList = null;//target.GetTargetFromSelect();//Select가 있을 경우 메시지를 조합하는가보다 근데 미리 Select가 완료 되어야하나보다.
        List<string> args = new List<string>();

        //타겟이 unit, hero인 경우
        if (arguments.method.Contains("unit")){
            if (arguments.method.Contains("hero")) {
                //unit인지 hero인지 구분
                string unitItemId;
                PlaceMonster monster;
                //select 스킬인 경우
                if (isSelect) {
                    // monster = selectList[0].GetComponent<PlaceMonster>();
                    // //타겟이 영웅?
                    // if(monster == null) {
                    //     if (selectList[0].GetComponentInParent<PlayerController>() != null) {
                    //         isOrc = (target.targetCamp().CompareTo("my") == 0) != isPlayerHuman;
                    //         arguments.method = "hero";
                    //         args.Add(isOrc ? "orc" : "human");
                    //     }
                    // }
                    // //타겟이 유닛
                    // else {
                    //     monster = GetDropAreaUnit();
                    //     unitItemId = monster.itemId;
                    //     arguments.method = "unit";
                    //     args.Add(unitItemId.ToString());
                    //     isOrc = monster.isPlayer != isPlayerHuman;
                    //     args.Add(isOrc ? "orc" : "human");
                    // }
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
                if (isSelect) monster = selectList[0].GetComponent<PlaceMonster>();
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
                if (isSelect) args.Add(selectList[0].GetComponent<PlaceMonster>().x.ToString());
                else args.Add(GetDropAreaLine().ToString());
            }

            else if (arguments.method.Contains("place")) {
                int line = selectList[0].transform.GetSiblingIndex();
                args.Add(line.ToString());
                if (isEndCardPlay) {
                    isOrc = (((List<GameObject>)skillTarget))[0].GetComponent<PlaceMonster>().isPlayer != isPlayerHuman;
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
