using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine;
using Spine.Unity;

public class GameResultManager : MonoBehaviour {
    public GameObject SocketDisconnectedUI;

    private float lv;
    private float exp;
    private float lvExp;
    private float nextLvExp;
    private int getExp = 0;
    private int crystal = 0;
    private int supply = 0;
    private int additionalSupply = 0;
    private int supplyBox = 0;

    private void Awake() {
        lv = AccountManager.Instance.userResource.lv;
        exp = AccountManager.Instance.userResource.exp;
        lvExp = AccountManager.Instance.userResource.lvExp;
        nextLvExp = AccountManager.Instance.userResource.nextLvExp;
        gameObject.SetActive(false);
    }

    public void OnReturnBtn() {
        FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.MAIN_SCENE);
    }

    public void SocketErrorUIOpen(bool friendOut) {
        SocketDisconnectedUI.SetActive(true);
        if (friendOut)
            SocketDisconnectedUI.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "상대방이 게임을 \n 종료했습니다.";
    }

    public void OnMoveSceneBtn() {
        FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.MAIN_SCENE);
    }

    public void SetResultWindow(string result, bool isHuman) {
        gameObject.SetActive(true);
        GameObject heroSpine = transform.Find("HeroSpine/" + PlayMangement.instance.player.heroID).gameObject;
        heroSpine.SetActive(true);
        iTween.ScaleTo(heroSpine, iTween.Hash("scale", Vector3.one, "islocal", true, "time", 0.3f));
        getExp = PlayMangement.instance.socketHandler.result.reward.heroExp;
        supply = PlayMangement.instance.socketHandler.result.reward.supply;
        additionalSupply = PlayMangement.instance.socketHandler.result.reward.additionalSupply;
        StartCoroutine(SetRewards());
        SkeletonGraphic backSpine;
        SkeletonGraphic frontSpine;
        switch (result) {
            case "win": {
                    heroSpine.GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "IDLE", true);
                    backSpine = transform.Find("BackSpine/WinningBack").GetComponent<SkeletonGraphic>();
                    frontSpine = transform.Find("FrontSpine/WinningFront").GetComponent<SkeletonGraphic>();

                }
                break;
            case "lose": {
                    heroSpine.GetComponent<SkeletonGraphic>().Initialize(true);
                    heroSpine.GetComponent<SkeletonGraphic>().Update(0);
                    heroSpine.GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "DEAD", false);
                    backSpine = transform.Find("BackSpine/LosingBack").GetComponent<SkeletonGraphic>();
                    frontSpine = transform.Find("FrontSpine/LosingFront").GetComponent<SkeletonGraphic>();
                }
                break;
            default:
                backSpine = null;
                frontSpine = null;
                break;
        }
        backSpine.Initialize(true);
        backSpine.Update(0);
        frontSpine.Initialize(true);
        frontSpine.Update(0);
        backSpine.gameObject.SetActive(true);
        frontSpine.gameObject.SetActive(true);
        backSpine.AnimationState.SetAnimation(0, "01.start", false);
        backSpine.AnimationState.AddAnimation(1, "02.play", true, 0.8f);
        if (isHuman)
            frontSpine.Skeleton.SetSkin("human");
        else
            frontSpine.Skeleton.SetSkin("orc");
        frontSpine.AnimationState.SetAnimation(0, "01.start", false);
        frontSpine.AnimationState.AddAnimation(1, "02.play", true, 0.8f);
    }

    IEnumerator SetRewards() {
        Transform rewards = transform.Find("ResourceRewards");
        yield return new WaitForSeconds(0.1f);
        Image slider = transform.Find("RankGage/RankBar").GetComponent<Image>();
        slider.fillAmount = exp / lvExp;
        transform.Find("RankGage/LevelImage/LevelText").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.userResource.lv.ToString();
        transform.Find("RankGage/RankText/Value").GetComponent<TMPro.TextMeshProUGUI>().text = ((int)exp).ToString();
        transform.Find("RankGage/RankText/MaxValue").GetComponent<TMPro.TextMeshProUGUI>().text = " / " + ((int)lvExp).ToString();
        iTween.ScaleTo(transform.Find("RankGage").gameObject, iTween.Hash("scale", Vector3.one, "islocal", true, "time", 0.5f));
        if (getExp > 0) StartCoroutine(GetUserExp(slider));
        if (supply > 0) {
            rewards.GetChild(0).gameObject.SetActive(true);
            rewards.GetChild(0).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = supply.ToString();
            if (additionalSupply > 0)
                rewards.GetChild(0).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = supply.ToString() + "+" + additionalSupply.ToString();
            yield return new WaitForSeconds(0.1f);
            iTween.ScaleTo(rewards.GetChild(0).gameObject, iTween.Hash("scale", Vector3.one, "islocal", true, "time", 0.5f));
        }
    }

    IEnumerator GetUserExp(Image slider) {
        float gain = getExp;
        TMPro.TextMeshProUGUI expValueText = transform.Find("RankGage/RankText/Value").GetComponent<TMPro.TextMeshProUGUI>();
        TMPro.TextMeshProUGUI lvUpValueText = transform.Find("RankGage/RankText/MaxValue").GetComponent<TMPro.TextMeshProUGUI>();
        while (gain > 0) {
            exp += 1;
            gain -= 1;
            slider.fillAmount = exp / lvExp;
            expValueText.text = ((int)exp).ToString();
            lvUpValueText.text = " / " + ((int)lvExp).ToString();
            if (exp == (int)lvExp) {
                lv++;
                transform.Find("RankGage/LevelImage/LevelText").GetComponent<TMPro.TextMeshProUGUI>().text = lv.ToString();
                lvExp = nextLvExp;
                slider.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
                yield return new WaitForSeconds(0.2f);
                slider.transform.localScale = Vector3.one;
                yield return new WaitForSeconds(0.2f);
                slider.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
                yield return new WaitForSeconds(0.2f);
                slider.transform.localScale = Vector3.one;
                yield return new WaitForSeconds(0.3f);
                slider.fillAmount = 0;
                exp = 0;
                expValueText.text = ((int)exp).ToString();
                lvUpValueText.text = " / " + ((int)lvExp).ToString();
            }
            yield return new WaitForSeconds(0.01f);
        }
    }
}
