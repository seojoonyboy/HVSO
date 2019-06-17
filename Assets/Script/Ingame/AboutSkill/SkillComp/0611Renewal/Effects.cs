using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkillModules {
    public partial class Ability {
        public SkillHandler skillHandler;

        public virtual void Execute(object data) { Debug.Log("Please Define Excecute Func"); }

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
            foreach(GameObject target in targets) {
                target.GetComponent<PlaceMonster>().RequestChangeStat(args.atk, args.hp);
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
                    Debug.LogError(attrName + "컴포넌트를 찾을 수 없습니다.");
                }
            }
        }
    }

    public class set_skill_target : Ability {
        public set_skill_target() : base() { }

        public override void Execute(object data) {
            try {
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
            try {
                int drawNum = (int)data;
                PlayMangement.instance.SocketHandler.DrawNewCards(drawNum);
            }
            catch(FormatException ex) {
                ShowFormatErrorLog("supply");
            }
            skillHandler.isDone = true;
        }
    }

    public class hook : Ability {
        public hook() : base() { }

        public override void Execute(object data) {
            if (data.GetType().IsArray) {
                try {
                    object[] tmp = (object[])data;
                    GameObject target = (GameObject)tmp[0];
                    HookArgs args = (HookArgs)tmp[1];
                    bool isPlayer = (bool)tmp[2];

                    MoveUnit(ref target, ref args, isPlayer);
                }
                catch (Exception ex) {
                    if(ex is FormatException || ex is ArgumentException ex2) {
                        ShowFormatErrorLog("hook");
                    }
                }
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
                target.GetComponent<PlaceMonster>().InstanceAttack();
            }
            catch(FormatException ex) {
                ShowFormatErrorLog("quick");
            }
            skillHandler.isDone = true;
        }
    }

    public class clear_skill_target : Ability {
        public clear_skill_target() : base() { }

        public override void Execute(object data) {
            try {
                GameObject target = (GameObject)data;
                target.GetComponent<SkillHandler>().skillTarget = null;
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
                try {
                    object[] tmp = (object[])data;
                    GameObject target = (GameObject)tmp[0];
                    SkillTargetArgs args = (SkillTargetArgs)tmp[1];
                    bool isPlayer = (bool)tmp[2];

                    MoveUnit(ref target, ref args, isPlayer);
                }
                catch (FormatException ex) {
                    ShowFormatErrorLog("skill_target_move");
                }
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
                try {
                    object[] tmp = (object[])data;
                    HookArgs args = (HookArgs)tmp[0];
                    bool isPlayer = (bool)tmp[1];

                    MoveUnit(ref args, isPlayer);
                }
                catch(Exception ex) {
                    if(ex is ArgumentException || ex is FormatException) {
                        ShowFormatErrorLog("self_move");
                    }
                }
            }
            else {
                ShowFormatErrorLog("self_move");
            }
        }

        private void MoveUnit(ref HookArgs args, bool isPlayer) {
            FieldUnitsObserver observer;
            if (isPlayer) {
                observer = PlayMangement.instance.PlayerUnitsObserver;
            }
            else {
                observer = PlayMangement.instance.EnemyUnitsObserver;
            }
            observer.UnitChangePosition(skillHandler.myObject, args.col, args.row);
            WaitDone();
        }

        private async void WaitDone() {
            await System.Threading.Tasks.Task.Delay(1500);
            skillHandler.isDone = true;
        }
    }

    public class blast_enemy : Ability {
        public blast_enemy() : base() { }

        public override void Execute(object data) {
            if (data.GetType().IsArray) {
                try {
                    object[] tmp = (object[])data;
                    bool isPlayer = (bool)tmp[0];
                    int col = (int)tmp[1];
                    int amount = (int)tmp[2];

                    BlastEnemy(isPlayer, col, amount);
                }
                catch (Exception ex) {
                    if (ex is ArgumentException || ex is FormatException) {
                        ShowFormatErrorLog("self_move");
                    }
                }
            }
            else {
                ShowFormatErrorLog("self_move");
            }
            skillHandler.isDone = true;
        }

        private void BlastEnemy(bool isPlayer, int col, int amount) {
            FieldUnitsObserver observer;
            if (isPlayer) {
                observer = PlayMangement.instance.EnemyUnitsObserver;
            }
            else {
                observer = PlayMangement.instance.PlayerUnitsObserver;
            }

            var units = observer.GetAllFieldUnits(col);
            foreach(GameObject unit in units) {
                unit.GetComponent<PlaceMonster>().RequestChangeStat(-amount, 0);
            }
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

        private void ReturnUnit(bool isPlayer) {
            if (IsEnemyExist(isPlayer)) {
                var units = enemyObserver.GetAllFieldUnits();

                var selectedUnit = SelectRandomItem(units);

                var selectedUnitPos = enemyObserver.GetMyPos(selectedUnit);
                UnityEngine.Object.Destroy(selectedUnit);
                enemyObserver.UnitRemoved(selectedUnitPos.col, selectedUnitPos.row);
                
                MakeEnemyUnitToCard();
            }
            else {
                Logger.Log("r_return 에서 적을 찾지 못했습니다.");
            }
        }

        private bool IsEnemyExist(bool isPlayer) {
            FieldUnitsObserver observer;
            if (isPlayer) {
                observer = enemyObserver;
            }
            else {
                observer = playerObserver;
            }
            var selectedUnits = observer.GetAllFieldUnits();
            return selectedUnits.Count != 0;
        }

        private GameObject SelectRandomItem(List<GameObject> pool) {
            System.Random random = new System.Random();
            int index = random.Next(pool.Count);
            var selectedUnit = pool[index];

            return selectedUnit;
        }

        private void MakeEnemyUnitToCard() {
            PlayMangement playMangement = PlayMangement.instance;

            GameObject enemyCard = UnityEngine.Object.Instantiate(playMangement.player.isHuman ? playMangement.enemyPlayer.back : playMangement.player.back);
            enemyCard.transform.SetParent(playMangement.enemyPlayer.playerUI.transform.Find("CardSlot").GetChild(playMangement.CountEnemyCard()));
            enemyCard.transform.localScale = new Vector3(1, 1, 1);
            enemyCard.transform.localPosition = new Vector3(0, 0, 0);
            enemyCard.SetActive(true);
        }
    }

    public class over_a_kill : Ability {
        public over_a_kill() : base() { }

        public override void Execute(object data) {
            FieldUnitsObserver observer;

            if (data.GetType().IsArray) {
                try {
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
                catch (Exception ex) {
                    if (ex is ArgumentException || ex is FormatException) {
                        ShowFormatErrorLog("over_a_kill");
                    }
                }
            }
            else {
                ShowFormatErrorLog("over_a_kill");
            }
            skillHandler.isDone = true;
        }

        private void RemoveUnit(ref GameObject target) {
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

    public struct GainArgs {
        public int atk;
        public int hp;
    }

    public struct HookArgs {
        public int col;
        public int row;
    }

    public struct SkillTargetArgs {
        public int col;
        public int row;
    }
}
