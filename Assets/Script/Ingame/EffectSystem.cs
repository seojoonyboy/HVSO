using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using UnityEngine.Events;
using Spine;
using Sirenix.OdinInspector;

public class EffectSystem : SerializedMonoBehaviour {
    public delegate void ActionDelegate();

    public static EffectSystem Instance { get; private set; }
    public Dictionary<EffectType, GameObject> effectObject;
    public GameObject deadEffect;
    public GameObject backgroundEffect;

    public GameObject pollingGroup;
    public GameObject spareObject;
    public GameObject cutSceneCanvas;
    public GameObject fadeCanvas;
    public SpriteRenderer worldFade;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        GameObject backEffect = Instantiate(backgroundEffect);
        backEffect.transform.position = PlayMangement.instance.backGround.transform.Find("ParticlePosition").position;
    }


    public void ShowEffect(EffectType type, Vector3 pos) {
        if (effectObject.ContainsKey(type) == false || effectObject[type] == null) return;
        GameObject effect = GetReadyObject(effectObject[type]);
        effect.transform.position = pos;
        effect.name = effectObject[type].gameObject.name;
        effect.SetActive(true);
        SkeletonAnimation effectAnimation = effect.GetComponent<SkeletonAnimation>();
        effectAnimation.Initialize(true);
        effectAnimation.Update(0);
        effectAnimation.AnimationState.SetAnimation(0, "animation", false);
        effectAnimation.AnimationState.Complete += delegate (TrackEntry entry) { SetReadyObject(effect); };
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
        effectAnimation.Initialize(true);
        effectAnimation.Update(0);
        if (playerTransform != null && playerTransform.gameObject.GetComponent<PlayerController>().isPlayer == false)
            effect.GetComponent<MeshRenderer>().sortingOrder = 8;
        effectAnimation.AnimationState.SetAnimation(0, "animation", false);
        effectAnimation.AnimationState.Event += delegate (TrackEntry entry, Spine.Event e) {
            if (e.Data.Name == "ATTACK") {
                callback();
            }
            if (e.Data.Name == "APPEAR") {
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
        effectAnimation.Initialize(true);
        effectAnimation.Update(0);
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
        effectAnimation.AnimationState.SetAnimation(0, "animation", true);
    }

    public void DisableEffect(EffectType type, Transform pos) {
        if (pos.childCount <= 0) return;
        GameObject effect = pos.Find(effectObject[type].gameObject.name).gameObject;
        if (effect != null)
            SetReadyObject(effect);
    }

    public GameObject GetReadyObject(GameObject original) {
        GameObject effectObject;
        foreach (Transform child in pollingGroup.transform) {
            if (child.gameObject.activeSelf == false && child.gameObject.name == original.name) {
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

    public IEnumerator HeroCutScene(bool isHuman) {
        SkeletonGraphic cutsceneAnimation;
        GameObject cutsceneObject;
        //StartCoroutine(FadeOut(0f, 0.6f, 0.8f));
        if (isHuman == true) {
            cutsceneObject = cutSceneCanvas.transform.Find("Human").gameObject;
            cutsceneObject.SetActive(true);
            cutsceneAnimation = cutSceneCanvas.transform.Find("Human").gameObject.GetComponent<SkeletonGraphic>();
        }
        else {
            cutsceneObject = cutSceneCanvas.transform.Find("Orc").gameObject;
            cutsceneObject.SetActive(true);
            cutsceneAnimation = cutSceneCanvas.transform.Find("Orc").gameObject.GetComponent<SkeletonGraphic>();
        }
        //cutsceneAnimation.AnimationState.ClearTrack(0);
        //cutsceneAnimation.AnimationState.Data.DefaultMix = 1;
        cutsceneAnimation.Skeleton.SetSlotsToSetupPose();
        cutsceneAnimation.Initialize(true);
        cutsceneAnimation.Update(0);
        cutsceneAnimation.AnimationState.SetAnimation(0, "animation", false);
        yield return new WaitForSeconds(cutsceneAnimation.Skeleton.Data.FindAnimation("animation").Duration / 2);
        //yield return FadeIn(0.6f, 0, cutsceneAnimation.Skeleton.Data.FindAnimation("animation").Duration / 2);
        cutsceneObject.SetActive(false);
    }

    public void DecreaseFadeAlpha() {
        worldFade.transform.gameObject.SetActive(true);
        worldFade.sortingOrder = 16;
        worldFade.color = new Color(0, 0, 0, 0.6f);
    }

    public void IncreaseFadeAlpha() {
        worldFade.sortingOrder = 16;
        worldFade.color = new Color(0, 0, 0, 0.6f);
        worldFade.transform.gameObject.SetActive(false);
    }

    public IEnumerator FadeOut(float min, float max, float time) {
        float speed = 0;
        Image fadeObject = fadeCanvas.transform.Find("Fade").gameObject.GetComponent<Image>();
        fadeObject.color = new Color(0, 0, 0, min);
        fadeCanvas.SetActive(true);
        Color fadeColor = fadeObject.color;
        speed = (max - min) / time;
        speed /= 10;

        while (fadeObject.color.a < max) {
            yield return new WaitForSeconds(speed / 30f);
            fadeColor.a += speed;
            fadeObject.color = fadeColor;
        }

        yield return null;
    }

    public IEnumerator FadeIn(float max, float min, float time) {
        fadeCanvas.SetActive(true);
        float speed = 0;
        Image fadeObject = fadeCanvas.transform.Find("Fade").gameObject.GetComponent<Image>();
        fadeObject.color = new Color(0, 0, 0, max);
        Color fadeColor = fadeObject.color;
        speed = (max - min) / time;
        speed /= 10;

        while (fadeObject.color.a > min) {
            yield return new WaitForSeconds(speed / 30);
            fadeColor.a -= speed;
            fadeObject.color = fadeColor;
        }

        fadeCanvas.SetActive(false);
        yield return null;
    }

    public void IncreaseShieldFeedBack(GameObject shieldFeed, int amount) {
        SkeletonGraphic skeletonGraphic = shieldFeed.GetComponent<SkeletonGraphic>();
        shieldFeed.SetActive(true);
        //skeletonGraphic.AnimationState.Data.DefaultMix = 1;
        //skeletonGraphic.AnimationState.ClearTrack(0);
        skeletonGraphic.Initialize(true);
        skeletonGraphic.Update(0);        
        skeletonGraphic.Skeleton.SetSlotsToSetupPose();
        TrackEntry entry;
        entry = skeletonGraphic.AnimationState.SetAnimation(0, amount.ToString(), false);
        skeletonGraphic.AnimationState.Complete += delegate (TrackEntry e) { shieldFeed.SetActive(false); skeletonGraphic.AnimationState.ClearTrack(0); };
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
        POISON_GET,
    }

}
