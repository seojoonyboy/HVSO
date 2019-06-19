using UnityEngine;
using Spine.Unity;
using Spine;
using UnityEngine.Events;

public class DebugUnitSpine : UnitSpine
{

    private void Start() {
        Init();
    }


    public override void Init() {
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        spineAnimationState = skeletonAnimation.AnimationState;
        spineAnimationState.Event += AnimationEvent;
        skeleton = skeletonAnimation.Skeleton;
        

        




        if (arrow != null && transform.parent.GetComponent<DebugUnit>().isPlayer == true) {
            if (rangeUpAttackName != "")
                attackAnimationName = rangeUpAttackName;
            else
                attackAnimationName = generalAttackName;
        }
        else if (arrow != null && transform.parent.GetComponent<DebugUnit>().isPlayer == false) {
            if (rangeDownAttackName != "")
                attackAnimationName = rangeDownAttackName;
            else
                attackAnimationName = generalAttackName;
        }
    }

    public override void Appear() {
        TrackEntry entry;
        entry = spineAnimationState.SetAnimation(0, appearAnimationName, false);
        currentAnimationName = appearAnimationName;
        entry.Complete += Idle;
    }

    public override void Idle(TrackEntry trackEntry = null) {
        spineAnimationState.SetAnimation(0, idleAnimationName, true);
        currentAnimationName = idleAnimationName;
    }

    public override void Attack() {
        TrackEntry entry;
        entry = spineAnimationState.SetAnimation(0, attackAnimationName, false);
        currentAnimationName = attackAnimationName;
        entry.Complete += Idle;
    }


    public override void Hit() {
        TrackEntry entry;
        entry = spineAnimationState.SetAnimation(0, hitAnimationName, false);
        currentAnimationName = hitAnimationName;
        entry.Complete += Idle;
    }

    public override void Preview() {
        spineAnimationState.SetAnimation(0, idleAnimationName, true);
        currentAnimationName = previewAnimationName;
    }

    public override void MagicHit() {
        TrackEntry entry;
        entry = spineAnimationState.SetAnimation(0, hitAnimationName, false);
        currentAnimationName = hitAnimationName;
        entry.Complete += TakeMagicEvent;
        entry.Complete += Idle;
    }



    public override void AnimationEvent(TrackEntry entry, Spine.Event e) {
        if (e.Data.Name == attackEventName) {
            if (attackCallback != null) attackCallback();
        }

        if (e.Data.Name == "APPEAR") {
            GameObject effect = Instantiate(DebugManagement.Instance.spineEffectManager.appearEffect, transform);
            effect.transform.position = transform.position;
            effect.GetComponent<SkeletonAnimation>().AnimationState.SetAnimation(0, "animation", false);
            Destroy(effect.gameObject, effect.GetComponent<SkeletonAnimation>().skeleton.Data.FindAnimation("animation").Duration - 0.1f);
        }
    }

    public override void TakeMagicEvent(TrackEntry entry) {
        if (takeMagicCallback != null) takeMagicCallback();
    }


    public override void HideUnit() {
        skeletonAnimation.skeleton.A = 0.2f;
        DebugUnit placeMonster = transform.parent.GetComponent<DebugUnit>();
        if (placeMonster != null) {
            transform.parent.Find("HP").gameObject.SetActive(false);
            transform.parent.Find("ATK").gameObject.SetActive(false);
        }
    }

    public override void DetectUnit() {
        skeletonAnimation.skeleton.A = 1f;
        DebugUnit placeMonster = transform.parent.GetComponent<DebugUnit>();
        if (placeMonster != null) {
            transform.parent.Find("HP").gameObject.SetActive(true);
            transform.parent.Find("ATK").gameObject.SetActive(true);
        }
    }
}
