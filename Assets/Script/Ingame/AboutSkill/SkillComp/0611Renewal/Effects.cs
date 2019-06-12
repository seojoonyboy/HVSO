using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkillModules {
    public partial class Ability {
        public virtual void Execute() { Debug.Log("Please Define Excecute Func"); }
        public virtual void InitData(object target, object args) {
            Debug.Log("Please Define Init Data Func");
        }
    }

    public class gain : Ability{
        public List<GameObject> targets;
        public Args args;

        public override void InitData(object target, object args) {
            targets = new List<GameObject>();

            if(target.GetType() == typeof(GameObject)) {
                targets.Add((GameObject)target);
            }
            else if(target.GetType() == typeof(List<GameObject>)) {
                targets.AddRange((List<GameObject>)target);
            }
            else {
                Debug.LogError("Target을 잘못 전달하였습니다.");
                return;
            }

            this.args = (Args)args;
        }

        public override void Execute() {
            if(targets == null) {
                Debug.LogError("Ability에 Data가 정상적으로 전달되지 않았습니다.");
                return;
            }
            foreach(GameObject target in targets) {
                //target.GetComponent<PlaceMonster>().RequestChangeStat(args.atk, args.hp);
            }
        }

        public struct Args {
            public int atk;
            public int hp;
        }
    }

    public class give_attribute : Ability {
        public object target;
        public string attrName;

        public override void InitData(object target, object args) {

        }

        public override void Execute() {

        }
    }
}
