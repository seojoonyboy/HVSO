using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Spine;
using UnityEngine.Events;

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
    [SpineEvent]
    public string attackEventName;

    protected string currentAnimationName;

    private Transform hitEffect;

    protected SkeletonAnimation skeletonAnimation;
    protected Spine.AnimationState spineAnimationState;
    protected Skeleton skeleton;
    
    public UnityAction attackCallback;

    public GameObject arrow;
    
    public float atkDuration {
        get { return skeletonAnimation.Skeleton.Data.FindAnimation(attackAnimationName).Duration; }
    }


    private void Awake() {
        Init();
    }


    public virtual void Init() {
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        spineAnimationState = skeletonAnimation.AnimationState;
        spineAnimationState.Event += AnimationEvent;
        skeleton = skeletonAnimation.Skeleton;

        if(arrow != null && transform.parent.GetComponent<PlaceMonster>().isPlayer == true) {
            attackAnimationName = skeletonAnimation.Skeleton.Data.FindAnimation("ATTACK_UP").Name;
        }
        else if(arrow != null && transform.parent.GetComponent<PlaceMonster>().isPlayer == false) {
            attackAnimationName = skeletonAnimation.Skeleton.Data.FindAnimation("ATTACK_DOWN").Name;
        }
        
    }

    public virtual void Appear() {
        TrackEntry entry;
        entry = spineAnimationState.SetAnimation(0, appearAnimationName, false);
        currentAnimationName = appearAnimationName;
        entry.Complete += Idle;        
    }

    public virtual void Idle(TrackEntry trackEntry = null) {
        spineAnimationState.SetAnimation(0, idleAnimationName, true);
        currentAnimationName = idleAnimationName;
    }

    public virtual void Attack() {
        TrackEntry entry;
        entry = spineAnimationState.SetAnimation(0, attackAnimationName, false);        
        currentAnimationName = attackAnimationName;
        entry.Complete += Idle;
    }


    public virtual void Hit() {
        TrackEntry entry;
        entry = spineAnimationState.SetAnimation(0, hitAnimationName, false);
        currentAnimationName = hitAnimationName;
        entry.Complete += Idle;
    }

    public void AnimationEvent(TrackEntry entry, Spine.Event e) {
        if(e.Data.Name == attackEventName) {
            if (attackCallback != null) attackCallback();
        }
    }



}
