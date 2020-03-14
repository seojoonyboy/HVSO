using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using SocketFormat;

public class ActiveCard {

    public class CardPlayArgs {
        
    }

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

        callback();
    }

    //전쟁의 외침
    public void ac10017(object args, DequeueCallback callback) {

        callback();
    }

    //투석 공격
    public void ac10021(object args, DequeueCallback callback) {

        callback();
    }

    //한파
    public void ac10022(object args, DequeueCallback callback) {

        callback();
    }
}

