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
    [SpineAnimation]
    public string previewAnimationName;

    [SpineAnimation]
    public string rangeUpAttackName;
    [SpineAnimation]
    public string rangeDownAttackName;
    [SpineAnimation]
    public string generalAttackName;

    


    [SpineEvent]
    public string attackEventName;

    protected string currentAnimationName;


    
    


    protected SkeletonAnimation skeletonAnimation;
    protected Spine.AnimationState spineAnimationState;
    protected Skeleton skeleton;
    
    public UnityAction attackCallback;

    public bool isPreview = false;
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

        if(isPreview == true) {
            previewAnimationName = idleAnimationName;
            Preview();
            return;
        }


        if(arrow != null && transform.parent.GetComponent<PlaceMonster>().isPlayer == true) {
            if (rangeUpAttackName != "")
                attackAnimationName = rangeUpAttackName;
            else
                attackAnimationName = generalAttackName;
        }
        else if(arrow != null && transform.parent.GetComponent<PlaceMonster>().isPlayer == false) {
            if (rangeDownAttackName != "")
                attackAnimationName = rangeDownAttackName;
            else
                attackAnimationName = generalAttackName;
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

    public virtual void Preview() {
        spineAnimationState.SetAnimation(0, idleAnimationName, true);
        currentAnimationName = previewAnimationName;
    }


    public void AnimationEvent(TrackEntry entry, Spine.Event e) {
        if(e.Data.Name == attackEventName) {
            if (attackCallback != null) attackCallback();
        }

        if(e.Data.Name == "APPEAR") {
            GameObject effect = Instantiate(PlayMangement.instance.effectManager.appearEffect, transform);
            effect.transform.position = transform.position;
            Destroy(effect.gameObject, effect.GetComponent<ParticleSystem>().main.duration - 0.2f);
        }


    }



}
