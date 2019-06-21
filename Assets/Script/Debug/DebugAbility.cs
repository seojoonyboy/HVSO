using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SkillModules;
using dataModules;
using System;

public class DebugAbility : Ability
{
    /*public override void InitData(dataModules.Skill data, bool isPlayer) {

        skillData = data;
        

        foreach (var condition in data.activate.conditions) {
            var newComp = gameObject.AddComponent(System.Type.GetType("SkillModules." + condition.method));

            if (newComp != null) {
                ((ConditionChecker)newComp).Init(data, condition, isPlayer);
            }
        }
    }*/
}
