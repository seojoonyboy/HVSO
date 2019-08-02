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

    public GameObject pollingGroup;
    public GameObject spareObject;
    public GameObject activeGroup;

    private void Awake() {
        Instance = this;
    }

    public void ShowEffect(EffectType type, Vector3 pos) {
        if (effectObject.ContainsKey(type) == false || effectObject[type] == null) return;
        GameObject effect = GetReadyObject(effectObject[type]);
        effect.transform.position = pos;
        effect.name = effectObject[type].gameObject.name;
        effect.SetActive(true);
        SkeletonAnimation effectAnimation = effect.GetComponent<SkeletonAnimation>();
        effectAnimation.skeleton.SetToSetupPose();
        effectAnimation.AnimationState.SetAnimation(0, "animation", false);
        effectAnimation.AnimationState.Complete += delegate (TrackEntry entry) {SetReadyObject(effect); };
        //Destroy(effect, effectAnimation.skeleton.Data.FindAnimation("animation").Duration - 0.1f);
        //return effectAnimation.skeleton.Data.FindAnimation("animation").Duration - 0.1f;
    }

    public void ShowEffectOnEvent(EffectType type, Vector3 pos, ActionDelegate callback, Transform playerTransform = null) {
        if (effectObject.ContainsKey(type) == false || effectObject[type] == null) return;
        GameObject effect = GetReadyObject(effectObject[type]);
        effect.transform.position = pos;
        effect.SetActive(true);
        effect.name = effectObject[type].gameObject.name;
        SkeletonAnimation effectAnimation = effect.GetComponent<SkeletonAnimation>();
        if(playerTransform != null && playerTransform.gameObject.GetComponent<PlayerController>().isPlayer == false) 
            effect.GetComponent<MeshRenderer>().sortingOrder = 8;
        effectAnimation.skeleton.SetToSetupPose();
        effectAnimation.AnimationState.SetAnimation(0, "animation", false);
        effectAnimation.AnimationState.Event += delegate (TrackEntry entry, Spine.Event e) {
            if(e.Data.Name == "ATTACK") {
                callback();
            }

            if(e.Data.Name == "APPEAR") {
                callback();
            }

        };
        effectAnimation.AnimationState.Complete += delegate (TrackEntry entry) { SetReadyObject(effect); };
    }
    

    public void ShowEffectAfterCall(EffectType type, Transform targetTransform, ActionDelegate callBack) {
        if (effectObject.ContainsKey(type) == false || effectObject[type] == null) return;
        GameObject effect = GetReadyObject(effectObject[type]);
        effect.transform.SetParent(targetTransform);
        effect.transform.position = targetTransform.position;
        effect.name = effectObject[type].gameObject.name;
        effect.SetActive(true);        
        SkeletonAnimation effectAnimation = effect.GetComponent<SkeletonAnimation>();
        effectAnimation.skeleton.SetToSetupPose();
        effectAnimation.AnimationState.SetAnimation(0, "animation", false);        
        effectAnimation.AnimationState.Complete += delegate (TrackEntry entry) { callBack(); SetReadyObject(effect); };
    }
    


    public void ContinueEffect(EffectType type, Transform pos) {
        if (effectObject.ContainsKey(type) == false || effectObject[type] == null) return;
        GameObject effect = GetReadyObject(effectObject[type]);
        effect.transform.SetParent(pos);
        effect.name = effectObject[type].gameObject.name;
        effect.transform.position = pos.position;
        effect.SetActive(true);
        SkeletonAnimation effectAnimation = effect.GetComponent<SkeletonAnimation>();
        effectAnimation.skeleton.SetToSetupPose();
        effectAnimation.AnimationState.SetAnimation(0, "animation", true);
    }

    public void DisableEffect(EffectType type, Transform pos) {
        if (pos.childCount <= 0) return;
        GameObject effect = pos.Find(effectObject[type].gameObject.name).gameObject;
        if(effect != null)
            SetReadyObject(effect);        
    }

    public GameObject GetReadyObject(GameObject original) {
        GameObject effectObject;
        foreach(Transform child in pollingGroup.transform) {
            if(child.gameObject.activeSelf == false && child.gameObject.name == original.name) {
                effectObject = child.gameObject;
                //effectObject.GetComponent<SkeletonAnimation>().skeletonDataAsset = original.GetComponent<SkeletonAnimation>().skeletonDataAsset;
                //effectObject.GetComponent<SkeletonAnimation>().timeScale = original.GetComponent<SkeletonAnimation>().timeScale;
                effectObject.transform.localScale = original.transform.localScale;
                return effectObject;
            }
        }
        effectObject = Instantiate(original);
        effectObject.transform.localScale = original.transform.localScale;
        effectObject.name = original.name;
        return effectObject;
    }

    public void SetReadyObject(GameObject effectObject) {
        Transform targetTransform = effectObject.transform;
        targetTransform.SetParent(pollingGroup.transform);
        effectObject.SetActive(false);
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
