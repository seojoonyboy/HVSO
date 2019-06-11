using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SkillModules;

public class gain : Ability, IEffectStrategy {
    private object target;
    private Args args;

    public void Execute() {

    }

    public void SetArgs(object args) {
        this.args = (Args)args;
    }

    public void SetTarget(object target) {
        this.target = target;
    }

    struct Args {
        int atk;
        int hp;
    }
}
