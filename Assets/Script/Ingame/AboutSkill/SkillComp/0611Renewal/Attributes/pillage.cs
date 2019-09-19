using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkillModules {
    public class pillage : MonoBehaviour {
        // Start is called before the first frame update
        void Start() {
            gameObject.GetComponent<PlaceMonster>().AddAttackProperty("pillage");
        }
    }
}

