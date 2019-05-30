using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkillModules {
    public class stun : Attribute {
        int turn;
        public int Turn {
            get {
                return turn;
            }
            set {
                turn = value;
                if(Turn <= 0) {
                    Destroy(GetComponent<stun>());
                }
            }
        }

        public override void Accumulate() {
            Turn++;
        }

        public override void Subtraction() {
            Turn--;
        }

        public override void Init() {
            Turn = 1;
        }
    }
}
