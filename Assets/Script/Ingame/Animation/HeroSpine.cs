using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Spine;


public class HeroSpine : MonoBehaviour
{
    [SpineAnimation]
    public string idleAnimationName;
    [SpineAnimation]
    public string attackAnimationName;
    [SpineAnimation]
    public string hitAnimationName;


    [SpineAnimation]
    public string dummyAnimation;

    protected SkeletonAnimation skeletonAnimation;
    protected Spine.AnimationState spineAnimationState;
    protected Skeleton skeleton;

    protected string currentAnimationName;

    private void Awake() {
        Init();
    }

    public virtual void Init() {
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        spineAnimationState = skeletonAnimation.AnimationState;
        spineAnimationState.Event += AnimationEvent;
        skeleton = skeletonAnimation.Skeleton;

    }

    public void AnimationEvent(TrackEntry entry, Spine.Event e) {

    }

    public virtual void Idle(TrackEntry trackEntry = null) {
        spineAnimationState.SetAnimation(0, idleAnimationName, true);
        currentAnimationName = idleAnimationName;
    }

    public virtual void Hit() {
        TrackEntry entry;
        entry = spineAnimationState.SetAnimation(0, hitAnimationName, false);
        currentAnimationName = hitAnimationName;
        entry.Complete += Idle;
    }

    public virtual void Attack() {
        TrackEntry entry;
        entry = spineAnimationState.SetAnimation(0, attackAnimationName, false);
        currentAnimationName = attackAnimationName;
        entry.Complete += Idle;
    }





}
