using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using Spine;
using Sirenix.OdinInspector;
using System;


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

    public GameObject DamageGroup;


    public SpriteRenderer worldFade;

    public GameObject cardDragEffect;

    public float GetAnimationTime(EffectType type) {
        float time = effectObject[type].GetComponent<SkeletonAnimation>().skeleton.Data.FindAnimation("animation").Duration;
        return time;
    }


    private void Awake() {
        Instance = this;
    }

    private void Start() {
        GameObject backEffect = Instantiate(backgroundEffect);
        backEffect.transform.position = PlayMangement.instance.backGround.transform.Find("ParticlePosition").position;
        PlayMangement.instance.EventHandler.AddListener(IngameEventHandler.EVENT_TYPE.END_TURN_BTN_CLICKED, EndTurnDisableMask);
    }

    public void SetCutsceneSkin() {
        SkeletonGraphic humanAnime, orcAnime;
        GameObject humanCut, orcCut;

        string humanID = PlayMangement.instance.socketHandler.gameState.players.human.hero.id;
        string orcID = PlayMangement.instance.socketHandler.gameState.players.orc.hero.id;

        humanAnime = cutSceneCanvas.transform.Find("Human").gameObject.GetComponent<SkeletonGraphic>();
        orcAnime = cutSceneCanvas.transform.Find("Orc").gameObject.GetComponent<SkeletonGraphic>();

        Skin humanSkin = humanAnime.SkeletonData.FindSkin(humanID);
        Skin orcSkin = orcAnime.SkeletonData.FindSkin(orcID);


        humanCut = cutSceneCanvas.transform.Find("Human").gameObject;
        humanAnime.Skeleton.Skin = humanSkin;
        humanAnime.Skeleton.SetSlotsToSetupPose();
        humanAnime.AnimationState.Apply(humanAnime.Skeleton);
        humanAnime.LateUpdate();


        orcCut = cutSceneCanvas.transform.Find("Orc").gameObject;
        orcAnime.Skeleton.Skin = orcSkin;
        orcAnime.Skeleton.SetSlotsToSetupPose();
        orcAnime.AnimationState.Apply(humanAnime.Skeleton);
        orcAnime.LateUpdate();
    }


    public void ShowEffect(EffectType type, Vector3 pos) {
        if (effectObject.ContainsKey(type) == false || effectObject[type] == null) return;
        GameObject effect = GetReadyObject(effectObject[type]);
        effect.transform.position = pos;
        effect.name = effectObject[type].gameObject.name;
        effect.SetActive(true);
        SkeletonAnimation effectAnimation = effect.GetComponent<SkeletonAnimation>();
        effectAnimation.Initialize(true);

        TrackEntry entry;
        Spine.AnimationState.TrackEntryDelegate trackAction = delegate (TrackEntry e) { SetReadyObject(effect); Debug.Log("오브젝트 원위치"); };

        effectAnimation.Update(0);
        entry = effectAnimation.AnimationState.SetAnimation(0, "animation", false);
        entry.Complete += trackAction;


        //effectAnimation.AnimationState.Complete += trackAction;
        //effectAnimation.AnimationState.Complete += delegate (TrackEntry e) { effectAnimation.AnimationState.Complete -= trackAction; };

        //Destroy(effect, effectAnimation.skeleton.Data.FindAnimation("animation").Duration - 0.1f);
        //return effectAnimation.skeleton.Data.FindAnimation("animation").Duration - 0.1f;
    }

    public void ShowEffectOnEvent(EffectType type, Vector3 pos, ActionDelegate callback, bool main = false, Transform playerTransform = null, ActionDelegate afterAction = null) {
        if (effectObject.ContainsKey(type) == false || effectObject[type] == null) return;
        GameObject effect = GetReadyObject(effectObject[type]);
        effect.transform.position = pos;
        effect.SetActive(true);
        effect.name = effectObject[type].gameObject.name;
        SkeletonAnimation effectAnimation = effect.GetComponent<SkeletonAnimation>();
        TrackEntry entry = new TrackEntry();

        Spine.AnimationState.TrackEntryEventDelegate trackEvent = delegate (TrackEntry eve, Spine.Event e) {
            if (e.Data.Name == "ATTACK") {
                callback();
            }
            if (e.Data.Name == "APPEAR") {
                callback();
            }
        };
        Spine.AnimationState.TrackEntryDelegate trackAction = delegate (TrackEntry e) { SetReadyObject(effect); afterAction?.Invoke(); };


        effectAnimation.Initialize(true);
        effectAnimation.Update(0);
        if (playerTransform != null && playerTransform.gameObject.GetComponent<PlayerController>().isPlayer == false)
            effect.GetComponent<MeshRenderer>().sortingOrder = 8;

        if (effectAnimation.skeleton.Data.FindAnimation("animation") != null)
            entry = effectAnimation.AnimationState.SetAnimation(0, "animation", false);
        else {
            if (main)
                entry = effectAnimation.AnimationState.SetAnimation(0, "animation" + "_main", false);
            else
                entry = effectAnimation.AnimationState.SetAnimation(0, "animation" + "_sub", false);
        }
        entry.Event += trackEvent;
        entry.Complete += trackAction;
    }

    //animation 이름이 animation이 아닌 마법 스파인 이펙트(호랑말코같은)를 위한 함수
    public void ShowEffectOnEvent(EffectType type, Vector3 pos, string amimName, ActionDelegate afterAction = null) {
        if (effectObject.ContainsKey(type) == false || effectObject[type] == null) return;
        GameObject effect = GetReadyObject(effectObject[type]);
        effect.transform.position = pos;
        effect.SetActive(true);
        effect.name = effectObject[type].gameObject.name;
        SkeletonAnimation effectAnimation = effect.GetComponent<SkeletonAnimation>();

        TrackEntry entry;
        Spine.AnimationState.TrackEntryDelegate trackAction = delegate (TrackEntry e) { SetReadyObject(effect); Debug.Log("스파인 지속 테스트"); afterAction?.Invoke(); };

        effectAnimation.Initialize(true);
        effectAnimation.Update(0);

        entry = effectAnimation.AnimationState.SetAnimation(0, amimName, false);
        entry.Complete += trackAction;



        //effectAnimation.AnimationState.Complete += trackAction;
        //effectAnimation.AnimationState.Complete += delegate (TrackEntry e) { effectAnimation.AnimationState.Complete -= trackAction; };
    }

    public void ShowEffectAfterCall(EffectType type, Transform targetTransform, ActionDelegate callBack) {
        if (effectObject.ContainsKey(type) == false || effectObject[type] == null) return;
        GameObject effect = GetReadyObject(effectObject[type]);
        effect.transform.SetParent(targetTransform);
        effect.transform.position = targetTransform.position;
        effect.name = effectObject[type].gameObject.name;
        effect.SetActive(true);
        SkeletonAnimation effectAnimation = effect.GetComponent<SkeletonAnimation>();

        TrackEntry entry;
        Spine.AnimationState.TrackEntryDelegate trackAction = delegate (TrackEntry e) { callBack(); Debug.Log("스파인 지속 테스트"); SetReadyObject(effect); };

        string animationName = effectAnimation.AnimationName;
        effectAnimation.Initialize(true);
        effectAnimation.Update(0);
        entry = effectAnimation.AnimationState.SetAnimation(0, animationName, false);
        entry.Complete += trackAction;
    }

    public void SetUpToolLine(string cardID, int line , ActionDelegate action = null ,DequeueCallback callback = null) {

        Transform toolGroup = PlayMangement.instance.backGround.transform.Find("Tool");
        Transform targetLineForm = toolGroup.GetChild(line);
        GameObject targetTool = AccountManager.Instance.resource.toolObject[cardID];
        if(targetTool == null) { Debug.Log("툴 오브젝트 없는데여"); callback(); return; }


        ActionDelegate mainAction;

        mainAction = delegate () {
            GameObject lineObject = Instantiate(targetTool, targetLineForm);
            lineObject.transform.localPosition = Vector3.zero;
            lineObject.name = cardID;

            SkeletonAnimation effectAnimation = lineObject.GetComponent<SkeletonAnimation>();
            effectAnimation.Initialize(true);
            effectAnimation.Update(0);



            TrackEntry entry;
            Spine.AnimationState.TrackEntryDelegate trackAction = delegate (TrackEntry e) { action(); callback(); Debug.Log("어디 한번 계속 실행되나 보자."); };
            entry = effectAnimation.AnimationState.SetAnimation(0, "appear", false);
            entry.Complete += trackAction; 
            effectAnimation.AnimationState.AddAnimation(0, "idle", true, 0f);


            //Spine.AnimationState.TrackEntryDelegate trackAction = delegate (TrackEntry e) { effectAnimation.AnimationState.SetAnimation(0, "idle", true);  };
            //effectAnimation.AnimationState.Complete += trackAction;
            //effectAnimation.AnimationState.Complete += delegate (TrackEntry e) { effectAnimation.AnimationState.Complete -= trackAction; };
        };



        if(targetLineForm.childCount > 0) {
            Transform oldTool = targetLineForm.GetChild(0);

            if(oldTool.gameObject.name == cardID) {
                Debug.Log("동일한 툴 카드. 오브젝트 재갱신은 불필요");
                action?.Invoke();
                callback();
                return;
            }
            SkeletonAnimation oldAnimation = oldTool.gameObject.GetComponent<SkeletonAnimation>();
            oldAnimation.AnimationState.SetAnimation(0, "disappear", false);
            oldAnimation.AnimationState.Complete += delegate (TrackEntry e) { Destroy(oldTool.gameObject); mainAction(); };
        }
        else {
            mainAction();
        }
    }




    public GameObject ContinueEffect(EffectType type, Transform pos, Transform bonePos = null) {
        if (effectObject.ContainsKey(type) == false || effectObject[type] == null) return null;
        if (pos.Find(effectObject[type].gameObject.name) != null) return null;
        GameObject effect = GetReadyObject(effectObject[type]);
        effect.transform.SetParent(pos);
        effect.name = effectObject[type].gameObject.name;

        effect.transform.position = (bonePos == null) ? pos.position : bonePos.position;


        effect.SetActive(true);
        SkeletonAnimation effectAnimation = effect.GetComponent<SkeletonAnimation>();
        effectAnimation.AnimationState.SetAnimation(0, "animation", true);
        return effect;
    }

    public bool CheckActiveEffect(EffectType type, Transform pos) {
        foreach(Transform child in pos) {
            if (pos.gameObject.name == effectObject[type].gameObject.name)
                return true;
        }
        return false;
    }




    public void DisableEffect(EffectType type, Transform pos) {
        if (pos.childCount <= 0) return;
        Transform effect = pos.Find(effectObject[type].gameObject.name);
        if (effect != null)
            SetReadyObject(effect.gameObject);
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

    public IEnumerator HeroCutScene(string heroID) {
        SkeletonGraphic cutsceneAnimation;
        GameObject cutsceneObject;

        //if (isHuman == true) {
        //    cutsceneObject = cutSceneCanvas.transform.Find("Human").gameObject;
        //    cutsceneObject.SetActive(true);
        //    cutsceneAnimation = cutSceneCanvas.transform.Find("Human").gameObject.GetComponent<SkeletonGraphic>();
        //}
        //else {
        //    cutsceneObject = cutSceneCanvas.transform.Find("Orc").gameObject;
        //    cutsceneObject.SetActive(true);
        //    cutsceneAnimation = cutSceneCanvas.transform.Find("Orc").gameObject.GetComponent<SkeletonGraphic>();
        //}
        //cutsceneAnimation.AnimationState.ClearTrack(0);
        //cutsceneAnimation.AnimationState.Data.DefaultMix = 1;


        cutsceneObject = GetCutsceneObject(heroID);
        if (cutsceneObject == null) yield break;
        cutsceneAnimation = cutsceneObject.GetComponent<SkeletonGraphic>();
        cutsceneAnimation.Skeleton.SetSlotsToSetupPose();
        cutsceneAnimation.Initialize(true);
        cutsceneAnimation.Update(0);
        cutsceneAnimation.AnimationState.SetAnimation(0, "animation", false);
        yield return new WaitForSeconds(cutsceneAnimation.Skeleton.Data.FindAnimation("animation").Duration);
        cutsceneObject.SetActive(false);
    }

    private GameObject GetCutsceneObject(string heroID) {
        GameObject cutsceneObject;

        foreach (Transform child in cutSceneCanvas.transform) {
            if (child.name == heroID) {
                cutsceneObject = child.gameObject;
                cutsceneObject.SetActive(true);
                return cutsceneObject;
            }
        }
        return null;
    }
    
    public void EnemyHeroDim(bool maskingHero = false) {
        //GameObject backGroundTill = PlayMangement.instance.backGroundTillObject;
        //backGroundTill.SetActive(true);        
        //worldFade.transform.gameObject.SetActive(true);
        //worldFade.sortingOrder = (maskField == true) ? 48 : 46;
        //worldFade.color = new Color(0, 0, 0, 0.6f);

        GameObject backGroundTill = PlayMangement.instance.backGroundTillObject;
        GameObject enemyBackground = backGroundTill.transform.Find("upBackGround").gameObject;

        backGroundTill.SetActive(true);
        enemyBackground.GetComponent<SpriteRenderer>().sortingOrder = (maskingHero == true) ? 7 : 5;

        Color backGroundColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        backGroundColor.a = (maskingHero == true) ? 0.7f : 1f;
        enemyBackground.GetComponent<SpriteRenderer>().color = backGroundColor;
    }

    public void EnemyHeroHideDim() {
        GameObject backGroundTill = PlayMangement.instance.backGroundTillObject;
        backGroundTill.SetActive(false);
    }


    


    public void MaskLine(int lineNum, bool active) {
        GameObject lineMask = PlayMangement.instance.lineMaskObject;
        lineMask.SetActive(true);
        GameObject line = lineMask.transform.GetChild(lineNum).gameObject;
        line.SetActive(active);
        line.GetComponent<SpriteRenderer>().color = new Color(0.28f, 0.28f, 0.28f, 0.6f);        

        int sorting = 53;
        line.GetComponent<SpriteRenderer>().sortingOrder = (active == true) ? sorting : 11;
    }

    public void HideMaskLine() {
        GameObject lineMask = PlayMangement.instance.lineMaskObject;
        foreach (Transform child in lineMask.transform)
            MaskLine(child.GetSiblingIndex(), false);
        lineMask.SetActive(false);

    }

    public bool GetLineMaskEnable(int lineNum) {
        return PlayMangement.instance.lineMaskObject.transform.GetChild(lineNum).gameObject.activeSelf;
}



    public void CheckEveryLineMask(int lineNum = -1) {
        for (int i = 0; i < 5; i++)
            MaskLine(i, (i != lineNum) ? true : false);      
    }

    

    public void CheckEveryLineMask(PlaceMonster unit, bool heroDim = true) {
        for (int i = 0; i < 5; i++)
            MaskLine(i, true);

        EnemyHeroDim(heroDim);
        unit.OverMask();
    }

    public void ShowSlotWithDim() {
        for (int i = 0; i < 5; i++)
            MaskLine(i, true);

        EnemyHeroDim(true);
    }




    public void HideEveryDim() {
        HideMaskLine();
        EnemyHeroHideDim();
    }


    private void EndTurnDisableMask(Enum event_type, Component Sender, object Param) {
        HideEveryDim();
    }


    public void CameraZoomIn(Transform target, float size, float time) {

        Vector3 pos = new Vector3(target.position.x, target.position.y, -10);

        float targetSize = size;
        
        iTween.ValueTo(Camera.main.gameObject, iTween.Hash("from", Camera.main.orthographicSize, "to", targetSize, "onUpdate", "UpdateSize", "time", time/2));
        iTween.MoveTo(Camera.main.gameObject, iTween.Hash("position", pos, "time", time));
    }

    public void CameraZoomOut(float time) {

        Vector3 pos = new Vector3(0, 0, -10);

        iTween.ValueTo(Camera.main.gameObject, iTween.Hash("from", Camera.main.orthographicSize, "to", 9.6f, "onupdate", "UpdateSize", "time", time/2));
        iTween.MoveTo(Camera.main.gameObject, iTween.Hash("position", pos, "time", time));
    }

    public void UpdateSize(float num) {
        Camera.main.orthographicSize = num;
    }



    //public IEnumerator CameraZoomIn(Transform target,float size,float time) {
    //    float speed = 0;
    //    Camera cam = Camera.main;

    //    speed = (cam.orthographicSize - size) / time * 10f;

    //    while(cam.orthographicSize > size) {
    //        yield return new WaitForSeconds(0.1f);
    //        cam.orthographicSize -= speed;
    //    }
    //    yield return null;
    //}

    //public IEnumerator CameraZoomOut()

    
    public void ShowDamageText(Vector3 pos, int num) {
        GameObject textObject = DamageGroup.transform.GetChild(0).gameObject;
        textObject.transform.position = pos;
        textObject.transform.SetAsLastSibling();

        string temp = (num > 0) ? "+" : "";
        temp += num.ToString();
   
        Text valText = textObject.transform.GetChild(0).gameObject.GetComponent<Text>();
        valText.color = (num > 0) ? Color.green : Color.red;
        valText.text = temp;

        textObject.GetComponent<UnityEngine.Animation>().Play();
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
        if (PlayMangement.instance != null)
            PlayMangement.instance.EventHandler.RemoveListener(IngameEventHandler.EVENT_TYPE.END_TURN_BTN_CLICKED, EndTurnDisableMask);
        Instance = null;
    }

    public enum EffectType {
        APPEAR,
        BUFF,
        DEBUFF,
        HIT_LOW,
        HIT_MIDDLE,
        HIT_HIGH,
        EXPLOSION,
        ANGRY,
        DEAD,
        TREBUCHET,
        PORTAL,
        CONTINUE_BUFF,
        CONTINUE_DEBUFF,
        GETBACK,
        STUN,
        POISON_GET,
        HERO_DEAD,
        HERO_SHIELD,
        DARK_THORN,
        OVERHIT,
        CHAIN_LIGHTNING,
        NO_DAMAGE,
        FIRE_WAVE,
        DISTINCTION,            //종의 멸망 이펙트
        MAGIC_OVERWHELMED,      //마력폭주
        OVER_POWERED,           //과부하
        IGNORANCE,               //무지함
        DETECT
    }
}
