using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;

public class GameResultManager : MonoBehaviour
{
    public GameObject SocketDisconnectedUI;

    

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
        iTween.ScaleTo(heroSpine, iTween.Hash("scale", Vector3.one, "islocal", true, "time", 1f));
        SkeletonGraphic backSpine;
        SkeletonGraphic frontSpine;
        switch (result) {
            case "win": {
                    heroSpine.GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "ATTACK", false);
                    heroSpine.GetComponent<SkeletonGraphic>().AnimationState.AddAnimation(1, "IDLE", true, 1);
                    backSpine = transform.Find("BackSpine/WinningBack").GetComponent<SkeletonGraphic>();
                    frontSpine = transform.Find("FrontSpine/WinningFront").GetComponent<SkeletonGraphic>();
                    
                }
                break;
            case "lose": {
                    heroSpine.GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "DEAD", false);
                    backSpine = transform.Find("BackSpine/LoseingBack").GetComponent<SkeletonGraphic>();
                    frontSpine = transform.Find("FrontSpine/LoseingFront").GetComponent<SkeletonGraphic>();
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
        backSpine.AnimationState.AddAnimation(1, "02.play", true, 1);
        if (isHuman)
            frontSpine.Skeleton.SetSkin("human");
        else
            frontSpine.Skeleton.SetSkin("orc");
        frontSpine.AnimationState.SetAnimation(0, "01.start", false);
        frontSpine.AnimationState.AddAnimation(1, "02.play", true, 1);
    }
}
