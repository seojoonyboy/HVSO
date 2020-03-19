using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using UnityEngine;
using SocketFormat;

public partial class CardUseSendSocket : CardSelect {

    public bool isSendMessageDone = false;

    public async void Init(bool isEndCardPlay = true) {
        SetUnitorMagic();
        if(magic != null) {
            targets = magic.cardData.targets;
            highlight = magic.highlightedSlot;
        }
        else targets = monster.unit.targets;
        await CheckSelect(isEndCardPlay);
        Debug.Log("sending Socket");
        if (isEndCardPlay) {
            SendSocket();
            DestroyMyCard();
        }
        else SendSkillActivate();
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

    public void SendSkillActivate() {
        BattleConnector connector = PlayMangement.instance.socketHandler;
        MessageFormat format = MessageForm(false);
        connector.UnitSkillActivate(format);
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
            if (isEndCardPlay)
                targets.Add(ArgumentForm(magic == null ? this.targets[0] : this.targets[1], true, isEndCardPlay));
            else {
                dataModules.Target target = new dataModules.Target {method = "place"};
                targets.Add(ArgumentForm(target, true, isEndCardPlay));
            }

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
                if (target.filter.Length != 0) {
                    isOrc = (target.filter[0].CompareTo("my") == 0) != isPlayerHuman;
                    args.Add(isOrc ? "orc" : "human");
                }
            }

            else if (arguments.method.Contains("place")) {
                int line = ((GameObject)skillTarget).transform.GetSiblingIndex();
                args.Add(line.ToString());
                if (isEndCardPlay) {
                    isOrc = GetDropAreaUnit().isPlayer != isPlayerHuman;
                }
                else
                    isOrc = monster.isPlayer != isPlayerHuman;
                args.Add(isOrc ? "orc" : "human");
                args.Add("front");
            }
        }
        arguments.args = args.ToArray();
        return arguments;
    }
}