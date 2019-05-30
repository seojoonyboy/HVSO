using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkillModules {
    public class stun : Attribute {
        int turn;
        int Turn {
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

        public override void Init() {
            Turn = 1;
        }

        public void AddTurn() {
            Turn++;
        }

        public void ReduceTurn() {
            Turn--;
        }
    }
}
