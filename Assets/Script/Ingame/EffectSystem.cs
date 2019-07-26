using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using UnityEngine.Events;
using Spine;
using Sirenix.OdinInspector;

public class EffectSystem : SerializedMonoBehaviour
{
    public delegate void ActionDelegate();

    public static EffectSystem Instance { get; private set; }
    public Dictionary<EffectType, GameObject> effectObject;
    public GameObject deadEffect;

    private void Awake() {
        Instance = this;
    }

    public void ShowEffect(EffectType type, Vector3 pos) {
        if (effectObject.ContainsKey(type) == false || effectObject[type] == null) return;
        GameObject effect = Instantiate(effectObject[type], pos, Quaternion.identity);
        SkeletonAnimation effectAnimation = effect.GetComponent<SkeletonAnimation>();     
        effectAnimation.AnimationState.SetAnimation(0, "animation", false);
        Destroy(effect, effectAnimation.skeleton.Data.FindAnimation("animation").Duration - 0.1f);
        //return effectAnimation.skeleton.Data.FindAnimation("animation").Duration - 0.1f;
    }

    public void ShowEffectOnEvent(EffectType type, Vector3 pos, ActionDelegate callback, Transform playerTransform = null) {
        if (effectObject.ContainsKey(type) == false || effectObject[type] == null) return;
        GameObject effect = Instantiate(effectObject[type], pos, Quaternion.identity);
        SkeletonAnimation effectAnimation = effect.GetComponent<SkeletonAnimation>();
        if(playerTransform != null && playerTransform.gameObject.GetComponent<PlayerController>().isPlayer == false) 
            effect.GetComponent<MeshRenderer>().sortingOrder = 8;      
        effectAnimation.AnimationState.SetAnimation(0, "animation", false);
        effectAnimation.AnimationState.Event += delegate (TrackEntry entry, Spine.Event e) {
            if(e.Data.Name == "ATTACK") {
                callback();
            }

            if(e.Data.Name == "APPEAR") {
                callback();
            }

        };
        effectAnimation.AnimationState.End += delegate (TrackEntry entry) { Destroy(effect); };
    }
    

    public void ShowEffectAfterCall(EffectType type, Transform transform, ActionDelegate callBack) {
        if (effectObject.ContainsKey(type) == false || effectObject[type] == null) return;
        GameObject effect = Instantiate(effectObject[type], transform);
        SkeletonAnimation effectAnimation = effect.GetComponent<SkeletonAnimation>();
        effectAnimation.AnimationState.SetAnimation(0, "animation", false);        
        effectAnimation.AnimationState.Complete += delegate (TrackEntry entry) { callBack(); Destroy(effect); };
    }
    


    public void ContinueEffect(EffectType type, Transform pos) {
        if (effectObject.ContainsKey(type) == false || effectObject[type] == null) return;
        GameObject effect = Instantiate(effectObject[type], pos);
        effect.name = effectObject[type].gameObject.name;
        effect.transform.position = pos.position;
        SkeletonAnimation effectAnimation = effect.GetComponent<SkeletonAnimation>();
        effectAnimation.AnimationState.SetAnimation(0, "animation", true);
    }

    public void DisableEffect(EffectType type, Transform pos) {
        if (pos.childCount <= 0) return;
        GameObject effect = pos.Find(effectObject[type].gameObject.name).gameObject;
        if(effect != null) 
            Destroy(effect);
        
    }


    private void OnDestroy() {
        Instance = null;
    }

    public enum EffectType {
        APPEAR,
        BUFF,
        HIT_LOW,
        HIT_MIDDLE,
        HIT_HIGH,
        EXPLOSION,
        ANGRY,
        DEAD,
        TREBUCHET,
        PORTAL,
        CONTINUE_BUFF,
        GETBACK,
        STUN,
        POISON_GET
    }

}
