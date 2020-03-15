using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using SocketFormat;

public class ActiveCard {

    public class CardPlayArgs {
        
    }

    public delegate void SkillAction();
    public SkillAction skillAction;

    public void Activate(string cardId, object args, DequeueCallback callback) {
        MethodInfo theMethod = this.GetType().GetMethod(cardId);
        object[] parameter = new object[]{args, callback};
        if(theMethod == null) {
            Logger.Log(cardId + "해당 카드는 아직 준비가 안되어있습니다.");
            callback();
            return;
        }
        theMethod.Invoke(this, parameter);
    }

    async void AfterAction(float time = 0f ,DequeueCallback callback = null) {
        await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(time));
        skillAction?.Invoke();
        skillAction = null;
        callback();
    }


    //축복
    public void ac10006(object args, DequeueCallback callback) {
        
        callback();
    }

    //긴급 보급
    public void ac10007(object args, DequeueCallback callback) {

        callback();
    }

    //재배치
    public void ac10015(object args, DequeueCallback callback) {
        callback();
    }

    //피의 분노
    public void ac10016(object args, DequeueCallback callback) {
        MagicArgs magicArgs = dataModules.JsonReader.Read<MagicArgs>(args.ToString());
        SkillInformation info = magicArgs.skillInfo;
        FieldUnitsObserver observer = PlayMangement.instance.UnitsObserver;
        PlaceMonster attacker = observer.GetUnitToItemID(info.attacker).GetComponent<PlaceMonster>();
        attacker.instanceAttack = true;
        List<GameObject> affected = observer.GetAfftecdList(attacker.unit.ishuman, info.affected);
        skillAction = delegate () { attacker.GetTarget(affected); };
        AfterAction(attacker.totalAtkTime, callback);
    }

    //전쟁의 외침
    public void ac10017(object args, DequeueCallback callback) {

        callback();
    }

    //투석 공격
    public void ac10021(object args, DequeueCallback callback) {
        MagicArgs magicArgs = dataModules.JsonReader.Read<MagicArgs>(args.ToString());
        SkillInformation info = magicArgs.skillInfo;
        AfterAction(EffectSystem.Instance.GetAnimationTime(EffectSystem.EffectType.TREBUCHET), callback);
    }

    //한파
    public void ac10022(object args, DequeueCallback callback) {

        callback();
    }
}

