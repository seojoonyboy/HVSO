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
    protected Skeleton skeleton;

    protected string currentAnimationName;

    private void Start() {
        Init();
    }

    public virtual void Init() {
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        skeletonAnimation.AnimationState.Event += AnimationEvent;
        skeleton = skeletonAnimation.Skeleton;

    }

    public void AnimationEvent(TrackEntry entry, Spine.Event e) {

    }

    public virtual void Idle(TrackEntry trackEntry = null) {
        skeletonAnimation.timeScale = 0.6f;
        skeletonAnimation.AnimationState.SetAnimation(0, idleAnimationName, true);
        currentAnimationName = idleAnimationName;
    }

    public virtual void Hit() {
        TrackEntry entry;
        skeletonAnimation.timeScale = 1.3f;
        entry = skeletonAnimation.AnimationState.SetAnimation(0, hitAnimationName, false);
        currentAnimationName = hitAnimationName;
        entry.Complete += Idle;
    }

    public virtual void Attack() {
        TrackEntry entry;
        entry = skeletonAnimation.AnimationState.SetAnimation(0, attackAnimationName, false);
        currentAnimationName = attackAnimationName;
        entry.Complete += Idle;
    }





}
