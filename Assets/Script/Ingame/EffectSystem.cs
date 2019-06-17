using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
public class EffectSystem : MonoBehaviour
{
    public static EffectSystem Instance { get; private set; }
    public Dictionary<EffectType, GameObject> effectObject;
    public SpineEffectManager spineEffectManager;

    private void Awake() {
        Instance = this;
        effectObject = new Dictionary<EffectType, GameObject>();
        effectObject[EffectType.APPEAR] = spineEffectManager.appearEffect;
        effectObject[EffectType.BUFF] = spineEffectManager.buffEffect;
        effectObject[EffectType.HIT_LOW] = spineEffectManager.lowAttackEffect;
        effectObject[EffectType.HIT_MIDDLE] = spineEffectManager.middileAttackEffect;
        effectObject[EffectType.HIT_HIGH] = spineEffectManager.highAttackEffect;
        effectObject[EffectType.EXPLOSION] = spineEffectManager.explosionEffect;
    }

    public void ShowEffect(EffectType type, Vector3 pos) {
        if (effectObject.ContainsKey(type) == false || effectObject[type] == null) return;
        GameObject effect = Instantiate(effectObject[type], pos, Quaternion.identity);
        SkeletonAnimation effectAnimation = effect.GetComponent<SkeletonAnimation>();
        effectAnimation.AnimationState.SetAnimation(0, "animation", false);
        Destroy(effect, effectAnimation.skeleton.Data.FindAnimation("animation").Duration - 0.1f);        
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
        DEAD
    }

}
