using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Spine;


public class UnitSpine : MonoBehaviour
{
    [SpineAnimation]
    public string idleAnimationName;
    [SpineAnimation]
    public string appearAnimationName;    
    [SpineAnimation]
    public string attackAnimationName;
    [SpineAnimation]
    public string hitAnimationName;

    protected string currentAnimationName;

    private Transform hitEffect;

    protected SkeletonAnimation skeletonAnimation;
    protected Spine.AnimationState spineAnimationState;
    protected Skeleton skeleton;


    private void Awake() {
        Init();
    }


    public virtual void Init() {
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        spineAnimationState = skeletonAnimation.AnimationState;
        skeleton = skeletonAnimation.Skeleton;
    }

    public virtual void Appear() {
        spineAnimationState.SetAnimation(0, appearAnimationName, false);
        currentAnimationName = appearAnimationName;
    }

    public virtual void Idle() {
        spineAnimationState.SetAnimation(0, idleAnimationName, true);
        currentAnimationName = idleAnimationName;
    }

    public virtual void Attack() {
        spineAnimationState.SetAnimation(0, attackAnimationName, false);
        currentAnimationName = attackAnimationName;
    }

    public virtual void Hit() {
        spineAnimationState.SetAnimation(0, hitAnimationName, false);
        currentAnimationName = hitAnimationName;
    }



}
