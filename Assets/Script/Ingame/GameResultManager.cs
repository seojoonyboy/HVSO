using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine;
using Spine.Unity;

public class GameResultManager : MonoBehaviour {
    public GameObject SocketDisconnectedUI;
    [SerializeField] Transform BgCanvas;

    private float lv;
    private float exp;
    private float lvExp;
    private float nextLvExp;
    private int getExp = 0;
    private int crystal = 0;
    private int supply = 0;
    private int additionalSupply = 0;
    private int supplyBox = 0;
    private bool isHuman;
    private string result;

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
        this.isHuman = isHuman;
        this.result = result;
        PlayerPrefs.DeleteKey("ReconnectData");
        gameObject.SetActive(true);
        transform.Find("FirstWindow").gameObject.SetActive(true);
        BgCanvas.gameObject.SetActive(true);
        GameObject heroSpine = transform.Find("FirstWindow/HeroSpine/" + PlayMangement.instance.player.heroID).gameObject;
        heroSpine.SetActive(true);
        iTween.ScaleTo(heroSpine, iTween.Hash("scale", Vector3.one, "islocal", true, "time", 0.3f));
        getExp = PlayMangement.instance.socketHandler.result.reward.heroExp;
        supply = PlayMangement.instance.socketHandler.result.reward.supply;
        additionalSupply = PlayMangement.instance.socketHandler.result.reward.additionalSupply;

        SkeletonGraphic backSpine;
        SkeletonGraphic frontSpine;
        switch (result) {
            case "win": {
                    BgCanvas.Find("Particle/First").gameObject.SetActive(true);
                    heroSpine.GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "IDLE", true);
                    backSpine = transform.Find("FirstWindow/BackSpine/WinningBack").GetComponent<SkeletonGraphic>();
                    frontSpine = transform.Find("FirstWindow/FrontSpine/WinningFront").GetComponent<SkeletonGraphic>();
                    transform.Find("SecondWindow/BackSpine/WinningBack").gameObject.SetActive(true);
                    transform.Find("SecondWindow/FrontSpine/WinningFront").gameObject.SetActive(true);
                }
                break;
            case "lose": {
                    heroSpine.GetComponent<SkeletonGraphic>().Initialize(true);
                    heroSpine.GetComponent<SkeletonGraphic>().Update(0);
                    heroSpine.GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "DEAD", false);
                    backSpine = transform.Find("FirstWindow/BackSpine/LosingBack").GetComponent<SkeletonGraphic>();
                    frontSpine = transform.Find("FirstWindow/FrontSpine/LosingFront").GetComponent<SkeletonGraphic>();
                    transform.Find("SecondWindow/BackSpine/LosingBack").gameObject.SetActive(true);
                    transform.Find("SecondWindow/FrontSpine/LosingFront").gameObject.SetActive(true);
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

    public void OpenSecondWindow() {
        transform.Find("FirstWindow").gameObject.SetActive(false);
        transform.Find("SecondWindow").gameObject.SetActive(true);
        BgCanvas.Find("Particle/First").gameObject.SetActive(false);
        if (isHuman) 
            BgCanvas.Find("BackgroundImg/human").gameObject.SetActive(true);
        else 
            BgCanvas.Find("BackgroundImg/orc").gameObject.SetActive(true);
        if (result == "win")
            BgCanvas.Find("Particle/Second").gameObject.SetActive(true);
        transform.Find("SecondWindow/Buttons/FindNewGame").GetComponent<Button>().interactable = false;
        transform.Find("SecondWindow/Buttons/BattleReady").GetComponent<Button>().interactable = false;
        StartCoroutine(SetRewards());
    }

    IEnumerator SetRewards() {
        Transform rewards = transform.Find("SecondWindow/ResourceRewards");
        yield return new WaitForSeconds(0.1f);
        Image slider = transform.Find("SecondWindow/PlayerExp/ExpSlider/SliderValue").GetComponent<Image>();
        slider.fillAmount = exp / lvExp;
        Transform playerExp = transform.Find("SecondWindow/PlayerExp");
        playerExp.Find("LevelIcon/Value").GetComponent<Text>().text = AccountManager.Instance.userResource.lv.ToString();
        playerExp.Find("UserName").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.userData.nickName;
        playerExp.Find("ExpSlider/ExpValue").GetComponent<TMPro.TextMeshProUGUI>().text = ((int)exp).ToString();
        playerExp.Find("ExpSlider/ExpMaxValue").GetComponent<TMPro.TextMeshProUGUI>().text = "/" + ((int)lvExp).ToString();
        iTween.ScaleTo(transform.Find("SecondWindow/PlayerExp").gameObject, iTween.Hash("scale", Vector3.one, "islocal", true, "time", 0.5f));
        yield return new WaitForSeconds(0.1f);
        iTween.ScaleTo(transform.Find("SecondWindow/PlayerMmr").gameObject, iTween.Hash("scale", Vector3.one, "islocal", true, "time", 0.5f));
        yield return new WaitForSeconds(0.1f);
        iTween.ScaleTo(transform.Find("SecondWindow/PlayerSupply").gameObject, iTween.Hash("scale", Vector3.one, "islocal", true, "time", 0.5f));
        transform.Find("SecondWindow/PlayerSupply/SupplyText/Value").GetComponent<TMPro.TextMeshProUGUI>().text = (supply + additionalSupply).ToString();
        transform.Find("SecondWindow/PlayerSupply/ExtraSupply/Value").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.userResource.additionalPreSupply.ToString();
        yield return new WaitForSeconds(0.1f);
        iTween.ScaleTo(transform.Find("SecondWindow/Buttons").gameObject, iTween.Hash("scale", Vector3.one, "islocal", true, "time", 0.5f));
        yield return new WaitForSeconds(0.1f);
        if (getExp > 0) StartCoroutine(GetUserExp(slider));
        //if (supply > 0) {
        //    rewards.GetChild(0).gameObject.SetActive(true);
        //    rewards.GetChild(0).Find("Text/Value").GetComponent<TMPro.TextMeshProUGUI>().text = supply.ToString();
        //    Transform additionalSupTxt = rewards.GetChild(0).Find("Text/Additional");
        //    if (additionalSupply > 0) {
        //        additionalSupTxt.gameObject.SetActive(true);
        //        additionalSupTxt.GetComponent<TMPro.TextMeshProUGUI>().text = "+" + additionalSupply.ToString();
        //    }
        //    else
        //        additionalSupTxt.gameObject.SetActive(false);
        //    yield return new WaitForSeconds(0.1f);
        //    iTween.ScaleTo(rewards.GetChild(0).gameObject, iTween.Hash("scale", Vector3.one, "islocal", true, "time", 0.5f));
        //}
    }

    IEnumerator GetUserExp(Image slider) {
        float gain = getExp;
        TMPro.TextMeshProUGUI expValueText = transform.Find("SecondWindow/PlayerExp/ExpSlider/ExpValue").GetComponent<TMPro.TextMeshProUGUI>();
        TMPro.TextMeshProUGUI lvUpValueText = transform.Find("SecondWindow/PlayerExp/ExpSlider/ExpMaxValue").GetComponent<TMPro.TextMeshProUGUI>();
        while (gain > 0) {
            exp += 1;
            gain -= 1;
            slider.fillAmount = exp / lvExp;
            expValueText.text = ((int)exp).ToString();
            lvUpValueText.text = " / " + ((int)lvExp).ToString();
            if (exp == (int)lvExp) {
                lv++;
                //transform.Find("RankGage/LevelImage/LevelText").GetComponent<TMPro.TextMeshProUGUI>().text = lv.ToString();
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
