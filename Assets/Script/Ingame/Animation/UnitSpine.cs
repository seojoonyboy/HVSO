using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Spine;
using UniRx;
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
    [SpineAnimation]
    public string deClockAnimationName;


    [SpineEvent]
    public string attackEventName;

    protected string currentAnimationName;


    
    


    protected SkeletonAnimation skeletonAnimation;
    protected Spine.AnimationState spineAnimationState;
    protected Skeleton skeleton;

    protected SkeletonAnimation arrowAnimation;
    protected Spine.AnimationState arrowState;
    [SpineAnimation]
    public string arrowAnimationName;

    public UnitAttackAction attackAction;
    public UnityAction takeMagicCallback;
    
    public GameObject arrow;
    public GameObject hidingObject;
    protected HideUnit hideUnit;

    public string attackRamge = "";
    public string rarelity = "";
    private UISfxSound appearSound;

    public bool teleportMove;
    public bool isGlow = false;

    public Color spineColor = new Color(1f, 1f, 1f);
    System.IDisposable DecreaseGlowRX, IncreaseGlowRX;

    [HideInInspector]
    public Transform headbone;
    [HideInInspector]
    public Transform bodybone;
    [HideInInspector]
    public Transform rootbone;
    
    public SkeletonAnimation GetSkeleton {
        get { return skeletonAnimation; }
    }

    
    public float atkDuration {
        get { return skeletonAnimation.Skeleton.Data.FindAnimation(attackAnimationName).Duration; }
    }

    public float appearDuration {
        get { return skeletonAnimation.Skeleton.Data.FindAnimation(appearAnimationName).Duration;}
    }


    private void Awake() {
        
        Init();
    }

    private void Start() {
        //SetUpGlow();
        //ActiveGlow();
        SetAppearSound();
    }

    private void SetAppearSound() {
        if (SoundManager.Instance != null) {
            switch (rarelity) {
                case "common":
                    appearSound = UISfxSound.CARD_USE_NORMAL;
                    break;
                case "uncommon":
                    appearSound = UISfxSound.CARD_USE_NORMAL;
                    break;
                case "rare":
                    appearSound = UISfxSound.CARD_USE_RARE;
                    break;
                case "superrare":
                    appearSound = UISfxSound.CARD_USE_SUPERRARE;
                    break;
                case "legend":
                    appearSound = UISfxSound.CARD_USE_LEGEND;
                    break;
                default:
                    appearSound = UISfxSound.CARD_USE_NORMAL;
                    break;
            }
        }
    }

    public virtual void SetImmediateObject() {
        arrow = transform.parent.Find("arrow").gameObject;
        attackRamge = (transform.parent != null && transform.parent.GetComponent<PlaceMonster>() != null) ? transform.parent.GetComponent<PlaceMonster>().unit.attackRange : "";
        arrowAnimation = arrow.GetComponent<SkeletonAnimation>() ? arrow.GetComponent<SkeletonAnimation>() : null;
        arrowState = arrow.GetComponent<SkeletonAnimation>() ? arrow.GetComponent<SkeletonAnimation>().AnimationState : null;
        arrowAnimationName = arrowAnimation ? arrowAnimation.AnimationName : "";
        if(arrowState != null)
            arrowState.Event += delegate (TrackEntry temp, Spine.Event arrowEvent) {
                if (arrowEvent.Data.Name == "ATTACK") {
                    attackAction?.Invoke();
                    attackAction -= attackAction;
                }
        };
    }



    public virtual void Init() {
        skeletonAnimation = GetComponent<SkeletonAnimation>();
        spineAnimationState = skeletonAnimation.AnimationState;
        skeleton = skeletonAnimation.Skeleton;
        spineAnimationState.Event += AnimationEvent;

        headbone = transform.Find("effect_head");
        bodybone = transform.Find("effect_body");
        rootbone = transform.Find("effect_root");
        //skeleton.SetToSetupPose();

        
        

        if (arrow != null && transform.parent.GetComponent<PlaceMonster>().isPlayer == true) {
            if (rangeUpAttackName != "")
                attackAnimationName = rangeUpAttackName;
            else
                attackAnimationName = generalAttackName;
        }
        else if (arrow != null && transform.parent.GetComponent<PlaceMonster>().isPlayer == false) {
            if (rangeDownAttackName != "")
                attackAnimationName = rangeDownAttackName;
            else
                attackAnimationName = generalAttackName;
        }

        if (string.IsNullOrEmpty(deClockAnimationName))
            deClockAnimationName = appearAnimationName;

    }

    public virtual void Appear() {
        //skeletonAnimation.skeleton.SetSlotsToSetupPose();
        skeletonAnimation.Initialize(false);
        skeletonAnimation.Update(0);
        //spineAnimationState.Data.DefaultMix = 1;
        //spineAnimationState.ClearTrack(0);
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

    public virtual void Declocking() {
        TrackEntry entry;
        entry = spineAnimationState.SetAnimation(0, deClockAnimationName, false);
        currentAnimationName = deClockAnimationName;
        entry.Complete += Idle;
    }


    public virtual void MagicHit() {
        TrackEntry entry;
        entry = spineAnimationState.SetAnimation(0, hitAnimationName, false);
        currentAnimationName = hitAnimationName;
        //entry.Complete += TakeMagicEvent;
        entry.Complete += Idle;
    }
    



    public virtual void AnimationEvent(TrackEntry entry, Spine.Event e) {
        if(e.Data.Name == attackEventName) {
            if (attackRamge == "immediate") {
                TrackEntry arrowEntry;
                arrow.SetActive(true);
                arrowEntry = arrowState.SetAnimation(0, arrowAnimationName, false);
                entry.Complete += delegate (TrackEntry temp) { arrow.SetActive(false); arrow.transform.position = transform.position; };
            }
            else {
                attackAction?.Invoke();
                attackAction -= attackAction;
            }
        }
        

        if(e.Data.Name == "APPEAR") {
            EffectSystem.Instance.ShowEffect(EffectSystem.EffectType.APPEAR, transform.position);
            if(SoundManager.Instance != null)
                SoundManager.Instance.PlaySound(appearSound);
        }
    }

    public virtual void TakeMagicEvent(TrackEntry entry) {
        //if (takeMagicCallback != null) takeMagicCallback();
    }


    public void SetUpGlow() {
        bool increase = false;
        float speed = 0.01f;
        
        Observable.EveryUpdate().Where(_ => spineColor.r >= 1f).Subscribe(_ => increase = false).AddTo(this);
        Observable.EveryUpdate().Where(_ => spineColor.r <= 0.5f).Subscribe(_ => increase = true).AddTo(this);
        

        DecreaseGlowRX = Observable.EveryUpdate().Where(_ => isGlow == true).Where(_=> increase == false)
            .Select(_ => spineColor.r -= speed).Select(_ => spineColor.g -= speed).Select(_ => spineColor.b -= speed)
            .Subscribe(_ => skeleton.SetColor(spineColor)).AddTo(this);

        IncreaseGlowRX = Observable.EveryUpdate().Where(_ => isGlow == true).Where(_=> increase == true)
            .Select(_ => spineColor.r += speed).Select(_ => spineColor.g += speed).Select(_ => spineColor.b += speed)
            .Subscribe(_ => skeleton.SetColor(spineColor)).AddTo(this);

        
    }

    public void ActiveGlow() {
        isGlow = true;
    }

    public void DeactiveGlow() {
        isGlow = false;
        spineColor = Color.white;
        skeleton.SetColor(spineColor);
    }
    


}
