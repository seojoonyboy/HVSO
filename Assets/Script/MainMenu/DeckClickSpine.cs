using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Spine;

public class DeckClickSpine : MonoBehaviour {
    [SpineAnimation]
    public string idleAnimationName;
    [SpineAnimation]
    public string clickAnimationName;

    SkeletonGraphic skeletonAnimation;
    Spine.AnimationState spineAnimationState;
    Skeleton skeleton;
    string currentAnimationName;

    void Awake() {
        Init();
    }

    void Init() {
        skeletonAnimation = GetComponent<SkeletonGraphic>();
        spineAnimationState = skeletonAnimation.AnimationState;
        spineAnimationState.Event += AnimationEvent;
        skeleton = skeletonAnimation.Skeleton;
    }

    public void Click() {
        TrackEntry entry;
        entry = spineAnimationState.SetAnimation(0, clickAnimationName, false);
        currentAnimationName = clickAnimationName;
        entry.Complete += Idle;
    }

    public void Idle(TrackEntry trackEntry = null) {
        //spineAnimationState.SetAnimation(0, idleAnimationName, true);
        currentAnimationName = idleAnimationName;
    }

    public void AnimationEvent(TrackEntry entry, Spine.Event e) {
        if (e.Data.Name == "CLICK") {

        }
    }
}
