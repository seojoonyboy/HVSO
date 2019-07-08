using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;

namespace SkillModules {
    public partial class Ability {
        public SkillHandler skillHandler;
        public object[] args;

        public virtual void Execute(object data) { Logger.Log("Please Define Excecute Func"); }

        protected void ShowFormatErrorLog(string additionalMsg = null) {
            Logger.LogError(additionalMsg + "에게 잘못된 인자를 전달하였습니다.");
        }
    }

    public class gain : Ability {
        public gain() : base(){ }

        public override void Execute(object data) {
            if (data.GetType().IsArray) {
                try {
                    object[] tmp = (object[])data;
                    List<GameObject> targets = (List<GameObject>)tmp[0];
                    GainArgs args = (GainArgs)tmp[1];

                    AddBuff(ref targets, ref args);
                }
                catch(FormatException ex) {
                    ShowFormatErrorLog("gain");
                }
                
            }
            else {
                ShowFormatErrorLog("gain");
            }
            skillHandler.isDone = true;
        }

        private void AddBuff(ref List<GameObject> targets, ref GainArgs args) {
            foreach (GameObject target in targets) {
                target.GetComponent<PlaceMonster>().RequestChangeStat(args.atk, args.hp);

                if(args.atk > 0) {
                    EffectSystem.Instance.ShowEffect(EffectSystem.EffectType.BUFF, target.transform.position);
                    EffectSystem.Instance.ContinueEffect(EffectSystem.EffectType.CONTINUE_BUFF, target.transform);
                }
                else if(args.hp > 0) {
                    EffectSystem.Instance.ShowEffect(EffectSystem.EffectType.BUFF, target.transform.position);
                    EffectSystem.Instance.ContinueEffect(EffectSystem.EffectType.CONTINUE_BUFF, target.transform);
                }
                else {
                    EffectSystem.Instance.DisableEffect(target.transform);
                }

            }
            skillHandler.isDone = true;
        }
    }

    public class give_attribute : Ability {
        public give_attribute() : base() { }

        public override void Execute(object data) {
            if (data.GetType().IsArray) {
                try {
                    object[] tmp = (object[])data;
                    List<GameObject> targets = (List<GameObject>)tmp[0];
                    string attrName = (string)tmp[1];

                    AddAttr(ref targets, attrName);
                }
                catch(FormatException ex) {
                    ShowFormatErrorLog("give_attribute");
                }
            }
            else {
                ShowFormatErrorLog("give_attribute");
            }
            skillHandler.isDone = true;
        }

        private void AddAttr(ref List<GameObject> targets, string attrName) {
            foreach(GameObject target in targets) {
                string attr = string.Format("SkillModules.{0}", attrName);
                var newComp = target.AddComponent(System.Type.GetType(attr));
                if(newComp == null) {
                    Logger.LogError(attrName + "컴포넌트를 찾을 수 없습니다.");
                }
            }
        }
    }

    public class set_skill_target : Ability {
        public set_skill_target() : base() { }

        public override void Execute(object data) {
            try {
                if (data == null) {
                    skillHandler.isDone = true;
                    return;
                }

                GameObject target = (GameObject)data;
                SetSkillTarget(ref target);
            }
            catch(FormatException ex) {
                ShowFormatErrorLog("set_skill_target");
            }
            skillHandler.isDone = true;
        }

        private void SetSkillTarget(ref GameObject target) {
            skillHandler.skillTarget = target;
        }
    }

    public class supply : Ability {
        public supply() : base() { }

        public override void Execute(object data) {
            int drawNum = 0;
            int.TryParse((string)args[0], out drawNum);
            int itemId = skillHandler.myObject.GetComponent<MagicDragHandler>().itemID;
            if(skillHandler.isPlayer)
                PlayMangement.instance.SocketHandler.DrawNewCards(drawNum, itemId);
            else
                PlayMangement.instance.StartCoroutine(PlayMangement.instance.EnemyMagicCardDraw(drawNum));

            skillHandler.isDone = true;
        }
    }

    public class hook : Ability {
        public hook() : base() { }

        public override void Execute(object data) {
            if (data.GetType().IsArray) {
                object[] tmp = (object[])data;
                GameObject target = (GameObject)tmp[0];
                HookArgs args = (HookArgs)tmp[1];
                bool isPlayer = (bool)tmp[2];

                MoveUnit(ref target, ref args, isPlayer);
            }
            else {
                ShowFormatErrorLog("hook");
            }
            skillHandler.isDone = true;
        }

        private void MoveUnit(ref GameObject target, ref HookArgs args, bool isPlayer) {
            FieldUnitsObserver observer;
            if (isPlayer) {
                observer = PlayMangement.instance.EnemyUnitsObserver;
            }
            else {
                observer = PlayMangement.instance.PlayerUnitsObserver;
            }
            observer.UnitChangePosition(target, args.col, args.row);
        }
    }

    public class quick : Ability {
        public quick() : base() { }

        public override void Execute(object data) {
            try {
                GameObject target = (GameObject)data;
                if(target.GetComponent<stun>() == null) {
                    target.GetComponent<PlaceMonster>().Invoke("InstanceAttack", 0.5f);
                }
                else {
                    Logger.Log("Stun이 걸려있어 공격을 할 수 없습니다!");
                }
            }
            catch(FormatException ex) {
                ShowFormatErrorLog("quick");
            }
            skillHandler.isDone = true;
            skillHandler.finallyDone = false;
            waitDone();
        }
        

        private async void waitDone() {
            await Task.Delay(2500);
            skillHandler.finallyDone = true;
        }
    }

    public class clear_skill_target : Ability {
        public clear_skill_target() : base() { }

        public override void Execute(object data) {
            try {
                skillHandler.skillTarget = null;
            }
            catch(FormatException ex) {
                ShowFormatErrorLog("clear_skill_target");
            }
            skillHandler.isDone = true;
        }
    }

    public class skill_target_move : Ability {
        public skill_target_move() : base() { }

        public override void Execute(object data) {
            if (data.GetType().IsArray) {
                object[] tmp = (object[])data;
                GameObject target = (GameObject)skillHandler.skillTarget;

                GameObject slotToMove = (GameObject)tmp[0];
                SkillTargetArgs args = new SkillTargetArgs();
                
                args.col = slotToMove.transform.GetSiblingIndex();
                args.row = 0;
                //SkillTargetArgs args = (SkillTargetArgs)tmp[1];
                bool isPlayer = (bool)tmp[2];

                MoveUnit(ref target, ref args, isPlayer);
            }
            else {
                ShowFormatErrorLog("skill_target_move");
            }
        }

        private void MoveUnit(ref GameObject target, ref SkillTargetArgs args, bool isPlayer) {
            FieldUnitsObserver observer;
            if (isPlayer) {
                observer = PlayMangement.instance.PlayerUnitsObserver;
            }
            else {
                observer = PlayMangement.instance.EnemyUnitsObserver;
            }
            observer.UnitChangePosition(target, args.col, args.row);
            WaitDone();
        }

        private async void WaitDone() {
            await System.Threading.Tasks.Task.Delay(1500);
            skillHandler.isDone = true;
        }
    }

    public class self_move : Ability {
        public self_move() : base() { }

        public override void Execute(object data) {
            if (data.GetType().IsArray) {
                object[] tmp = (object[])data;
                SelfMoveArgs args = (SelfMoveArgs)tmp[0];
                bool isPlayer = (bool)tmp[1];

                Logger.Log(args.col);
                Logger.Log(args.row);

                MoveUnit(ref args, isPlayer);
            }
            else {
                ShowFormatErrorLog("self_move");
            }
        }

        private void MoveUnit(ref SelfMoveArgs args, bool isPlayer) {
            FieldUnitsObserver observer;
            if (isPlayer) {
                observer = PlayMangement.instance.PlayerUnitsObserver;
            }
            else {
                observer = PlayMangement.instance.EnemyUnitsObserver;
            }
            observer.UnitChangePosition(skillHandler.myObject, args.col, args.row);
            skillHandler.finallyDone = false;
            WaitDone();
        }

        private async void WaitDone() {
            await System.Threading.Tasks.Task.Delay(1500);
            skillHandler.isDone = true;
            await System.Threading.Tasks.Task.Delay(500);
            skillHandler.finallyDone = true;
        }
    }

    public class clear_skill : Ability {
        public clear_skill() : base() { }

        public override void Execute(object data) {
            skillHandler.RemoveTriggerEvent();
            skillHandler.isDone = true;
        }
    }

    public class blast_enemy : Ability {
        public blast_enemy() : base() { }

        public override void Execute(object data) {
            if (data.GetType().IsArray) {
                object[] tmp = (object[])data;
                bool isPlayer = (bool)tmp[0];
                List<GameObject> targets = (List<GameObject>)tmp[1];
                int amount = (int)tmp[2];
                skillHandler.finallyDone = false;
                BlastEnemy(isPlayer, targets, amount);
            }
            else {
                ShowFormatErrorLog("blast_enemy");
            }
            skillHandler.isDone = true;
        }

        private void BlastEnemy(bool isPlayer, List<GameObject> targets, int amount) {
            foreach(GameObject target in targets) {
                target.GetComponent<PlaceMonster>().RequestChangeStat(0, -amount);
                EffectSystem.Instance.ShowEffect(EffectSystem.EffectType.EXPLOSION, target.transform.position);
                WaitEffect(target, amount);
            }
        }

        private async void WaitEffect(GameObject target, int amount) {
            await System.Threading.Tasks.Task.Delay(1500);
            target.GetComponent<PlaceMonster>().CheckHP();
            skillHandler.finallyDone = true;
        }
    }

    public class random_blast_enemy : Ability {
        public random_blast_enemy() : base() { }

        public override void Execute(object data) {
            // if (data.GetType().IsArray) {
            //     object[] tmp = (object[])data;
            //     bool isPlayer = (bool)tmp[0];
            //     List<GameObject> targets = (List<GameObject>)tmp[1];
            //     int num = (int)tmp[2];
            //     int amount = (int)tmp[3];

            //     var selectedItems = new List<GameObject>();
            //     for (int i=0; i<num; i++) {
            //         var selectedItem = PickItem(ref targets);
            //         if(selectedItem != null) selectedItems.Add(selectedItem);
            //     }

            //     BlastEnemy(isPlayer, selectedItems, amount);
            // }
            // else {
            //     ShowFormatErrorLog("blast_enemy");
            // }
            skillHandler.finallyDone = false;
            WaitDone();
            skillHandler.isDone = true;
        }

        private async void WaitDone() {
            int itemId;
            PlayMangement playMangement = PlayMangement.instance;
            SocketFormat.GameState state = playMangement.socketHandler.gameState;
            FieldUnitsObserver observer = skillHandler.isPlayer ? playMangement.EnemyUnitsObserver : playMangement.PlayerUnitsObserver;

            if(skillHandler.myObject.GetComponent<PlaceMonster>() != null) 
                itemId = skillHandler.myObject.GetComponent<PlaceMonster>().itemId;
            else 
                itemId = skillHandler.myObject.GetComponent<MagicDragHandler>().itemID;
            while(true) {
                await Task.Delay(20);
                state = playMangement.socketHandler.gameState;
                if(state.SearchUseItem(itemId)) break;
            }
        
            List<SocketFormat.Unit> socketList = state.map.allMonster;
            List<GameObject> enemyList = observer.GetAllFieldUnits();
            List<GameObject> targets = new List<GameObject>();
            string cardId = (string)args[0];
            int amount = (int)args[1];
            
            foreach(GameObject enemy in enemyList) {
                PlaceMonster monData = enemy.GetComponent<PlaceMonster>();
                bool found = false;
                foreach(SocketFormat.Unit serverData in socketList) {
                    //클라에 있는 유닛이랑 서버에 있는 유닛이 일치할 때
                    if(serverData.itemId == monData.itemId) {
                        found = true;
                        //체력이 일치 하지 않을 떄
                        if(serverData.currentHp != monData.unit.currentHP) {
                            targets.Add(enemy);
                        }
                        break;
                    }
                }
                //클라에 있는 유닛이 서버에 없을 때
                if(!found) {
                    targets.Add(enemy);
                }
            }
            BlastEnemy(skillHandler.isPlayer, targets, amount);
        }

        private void BlastEnemy(bool isPlayer, List<GameObject> targets, int amount) {
            foreach (GameObject target in targets) {
                target.GetComponent<PlaceMonster>().RequestChangeStat(0, -amount);
                EffectSystem.Instance.ShowEffect(EffectSystem.EffectType.EXPLOSION, target.transform.position);
                WaitEffect(target, amount);
            }
        }

        private async void WaitEffect(GameObject target, int amount) {
            await System.Threading.Tasks.Task.Delay(1500);
            target.GetComponent<PlaceMonster>().CheckHP();
            skillHandler.finallyDone = true;
        }
    }

    public class r_return : Ability {
        public r_return() : base() { }

        FieldUnitsObserver playerObserver, enemyObserver;

        public override void Execute(object data) {
            if (data.GetType().IsArray) {
                try {
                    object[] tmp = (object[])data;
                    bool isPlayer = (bool)tmp[0];

                    if (isPlayer) {
                        playerObserver = PlayMangement.instance.PlayerUnitsObserver;
                        enemyObserver = PlayMangement.instance.EnemyUnitsObserver;
                    }
                    else {
                        playerObserver = PlayMangement.instance.EnemyUnitsObserver;
                        enemyObserver = PlayMangement.instance.PlayerUnitsObserver;
                    }
                    skillHandler.finallyDone = false;
                    ReturnUnit(isPlayer);
                }
                catch (Exception ex) {
                    if (ex is ArgumentException || ex is FormatException) {
                        ShowFormatErrorLog("r_return");
                    }
                }
            }
            else {
                ShowFormatErrorLog("r_return");
            }
            skillHandler.isDone = true;
        }

        private async void ReturnUnit(bool isPlayer) {
            PlayMangement playMangement = PlayMangement.instance;
            SocketFormat.GameState state = playMangement.socketHandler.gameState;
            if(isPlayer) {
                int itemId;

                if(skillHandler.myObject.GetComponent<PlaceMonster>() != null) 
                    itemId = skillHandler.myObject.GetComponent<PlaceMonster>().itemId;
                else 
                    itemId = skillHandler.myObject.GetComponent<MagicDragHandler>().itemID;
                while(true) {
                    await Task.Delay(20);
                    state = playMangement.socketHandler.gameState;
                    if(state.SearchUseItem(itemId)) break;
                }
            }
            else {
                state = playMangement.socketHandler.r_returnState;
            }
            var units = enemyObserver.GetAllFieldUnits();
            
            List<SocketFormat.Unit> socketList = state.map.allMonster;

            foreach(GameObject selectedUnit in units) {
                bool found = false;
                PlaceMonster mondata = selectedUnit.GetComponent<PlaceMonster>();
                foreach(SocketFormat.Unit unit in socketList) {
                    if(unit.itemId.CompareTo(mondata.itemId) == 0) {
                        found = true;
                        break;
                    }
                }
                //server에서 사라진 유닛을 찾는다.
                if(!found) {
                    var selectedUnitPos = enemyObserver.GetMyPos(selectedUnit);
                    UnityEngine.Object.Destroy(selectedUnit);
                    enemyObserver.UnitRemoved(selectedUnitPos.col, selectedUnitPos.row);

                    //내 유닛이 사라진 경우
                    if (mondata.isPlayer) {
                        MakeMyUnitToCard(mondata);
                    }
                    //적 유닛이 사라진 경우
                    else {
                        MakeEnemyUnitToCard();
                    }
                }
            }
            skillHandler.finallyDone = true;
        }

        private void MakeEnemyUnitToCard() {
            PlayMangement playMangement = PlayMangement.instance;

            GameObject enemyCard = UnityEngine.Object.Instantiate(playMangement.player.isHuman ? playMangement.enemyPlayer.back : playMangement.player.back);
            enemyCard.transform.SetParent(playMangement.enemyPlayer.playerUI.transform.Find("CardSlot").GetChild(playMangement.CountEnemyCard()));
            enemyCard.transform.localScale = new Vector3(1, 1, 1);
            enemyCard.transform.localPosition = new Vector3(0, 0, 0);
            enemyCard.SetActive(true);
        }

        private void MakeMyUnitToCard(PlaceMonster placeMonster) {
            PlayMangement playMangement = PlayMangement.instance;
            Transform cardStorage = playMangement.cardCircleManager.GetcardStorage();
            GameObject card = cardStorage.Find("UnitCards").GetChild(0).gameObject;

            //카드가 꽉 차 있는 경우 날라감.
            var id = placeMonster.unit.id;
            var itemId = placeMonster.itemId;

            card.GetComponent<CardHandler>().DrawCard(id, itemId);
            playMangement.cardCircleManager.AddCard(card);
        }
    }

    public class kill : Ability {
        public kill() : base() { }

        public override void Execute(object data) {
            FieldUnitsObserver observer;

            if (data.GetType().IsArray) {
                object[] tmp = (object[])data;
                GameObject target = (GameObject)tmp[0];
                bool isPlayer = (bool)tmp[1];

                if (isPlayer) {
                    observer = PlayMangement.instance.EnemyUnitsObserver;
                }
                else {
                    observer = PlayMangement.instance.PlayerUnitsObserver;
                }

                RemoveUnit(ref target);
            }
            else {
                ShowFormatErrorLog("kill");
            }
            skillHandler.isDone = true;
        }

        private void RemoveUnit(ref GameObject target) {
            EffectSystem.Instance.ShowEffect(EffectSystem.EffectType.EXPLOSION, target.transform.position);
            target.GetComponent<PlaceMonster>().InstanceKilled();
        }
    }

    public class give_attack_type : Ability {
        public give_attack_type() : base() { }

        public override void Execute(object data) {
            List<GameObject> targets = new List<GameObject>();

            if (data.GetType().IsArray) {
                try {
                    object[] tmp = (object[])data;
                    if(tmp[0].GetType() == typeof(List<GameObject>)){
                        targets.AddRange((List<GameObject>)tmp[0]);
                    }
                    else if(tmp[0].GetType() == typeof(GameObject)) {
                        targets.Add((GameObject)tmp[0]);
                    }

                    string attrName = (string)tmp[1];

                    GiveAttackType(ref targets, attrName);
                }
                catch (Exception ex) {
                    if (ex is ArgumentException || ex is FormatException) {
                        ShowFormatErrorLog("give_attack_type");
                    }
                }
            }
            else {
                ShowFormatErrorLog("give_attack_type");
            }
            skillHandler.isDone = true;
        }

        private void GiveAttackType(ref List<GameObject> targets, string attrName) {
            string attr = string.Format("SkillModules.{0}", attrName);
            foreach (GameObject target in targets) {

                var newComp = target.AddComponent(System.Type.GetType(attr));
                if(newComp == null) {
                    string logMsg = string.Format("{0} 컴포넌트가 정상적으로 부착되지 않았습니다.", attrName);
                    Logger.Log(logMsg);
                }
            }
        }
    }

    public class summon_random : Ability {
        public summon_random() : base() { }

        public override void Execute(object data) {   
            skillHandler.finallyDone = false;       
            WaitDone();
            skillHandler.isDone = true;
        }

        private async void WaitDone() {
            int itemId;
            PlayMangement playMangement = PlayMangement.instance;
            SocketFormat.GameState state = playMangement.socketHandler.gameState;
            FieldUnitsObserver observer = skillHandler.isPlayer ? playMangement.PlayerUnitsObserver : playMangement.EnemyUnitsObserver;
            bool isPlayer = skillHandler.isPlayer ? true : false;
            string cardId = (string)args[0];

            if(skillHandler.myObject.GetComponent<PlaceMonster>() != null) 
                itemId = skillHandler.myObject.GetComponent<PlaceMonster>().itemId;
            else 
                itemId = skillHandler.myObject.GetComponent<MagicDragHandler>().itemID;
            while(true) {
                await Task.Delay(20);
                state = playMangement.socketHandler.gameState;
                if(state.SearchUseItem(itemId)) break;
            }
        
            List<SocketFormat.Unit> socketList = state.map.allMonster;
           
            foreach(SocketFormat.Unit serverUnit in socketList) {
                if(serverUnit.cardId.CompareTo(cardId) == 0) {
                    Pos pos = serverUnit.pos;
                    List<GameObject> list = observer.GetAllFieldUnits(pos.col);
                    //유닛이 존재하지 않으면 그곳에 생성
                    if(list.Count == 0) {
                        var summonedUnit = playMangement.SummonUnit(isPlayer, cardId, pos.col, pos.row, itemId);

                        if (isPlayer) observer.RefreshFields(CardDropManager.Instance.unitLine);
                        else observer.RefreshFields(CardDropManager.Instance.enemyUnitLine);
                    }
                }
            }
            skillHandler.finallyDone = true;
        }


    }


    public class heal : Ability {
        public heal() : base() { }

        public override void Execute(object data) {
            object[] tmp = (object[])data;
            bool isPlayer = (bool)tmp[0];
            int amount = (int)tmp[1];
            HealPlayer((isPlayer == true) ? true : false, amount);
            skillHandler.isDone = true;
        }

        private void HealPlayer(bool player, int amount) {
            PlayerController targetPlayer = (player == true) ? PlayMangement.instance.player : PlayMangement.instance.enemyPlayer;
            targetPlayer.HP.Value += amount;
            Logger.Log(targetPlayer.name + "에" + amount + "만큼 회복");
        }

    }


    //skill target filtering with terrain
    public class st_filter_terrain : Ability {
        public st_filter_terrain() : base() { }

        public override void Execute(object data) {
            object[] tmp = (object[])data;
            bool isPlayer = (bool)tmp[0];
            List<GameObject> targets = (List<GameObject>)tmp[1];

            var filteredList = new List<GameObject>();

            var terrain = (string)args[0];
            switch (terrain) {
                case "normal":
                    filteredList = targets.FindAll(x => x.GetComponentInParent<Terrain>().terrain == PlayMangement.LineState.flat);
                    break;
            }
            if (filteredList.Count == 0) Logger.Log(terrain + "지형 속성의 유닛이 존재하지 않습니다.");

            skillHandler.skillTarget = filteredList;
            skillHandler.isDone = true;
        }
    }

    //skill target filtering with category
    public class st_filter_ctg : Ability {
        //TODO
        public st_filter_ctg() : base() { }

        public override void Execute(object data) {
            object[] tmp = (object[])data;
            bool isPlayer = (bool)tmp[0];
            List<GameObject> targets = (List<GameObject>)tmp[1];

            var filteredList = new List<GameObject>();
            var category = (string)args[0];
            filteredList = targets
                .FindAll(
                    x => x.GetComponent<PlaceMonster>()
                    .unit
                    .cardCategories
                    .ToList()
                    .Contains(category)
                );
            skillHandler.skillTarget = filteredList;
            skillHandler.isDone = true;
        }
    }

    public class gain_resource : Ability {
        public gain_resource() : base() { }

        public override void Execute(object data) {
            //data 필요없음.
            int amount = 0;
            int.TryParse((string)args[0], out amount);

            Logger.Log("추가 자원 얻음");

            PlayMangement.instance.player.resource.Value += amount;
            //TODO : 자원 추가 효과 넣기
            skillHandler.isDone = true;
        }
    }

    public struct GainArgs {
        public int atk;
        public int hp;
    }

    public struct HookArgs {
        public int col;
        public int row;
    }

    public struct SkillTargetArgs {
        public SkillTargetArgs(Pos pos) {
            col = pos.col;
            row = pos.row;
        }
        
        public int col;
        public int row;
    }

    public struct SelfMoveArgs {
        public int col;
        public int row;
    }
}
