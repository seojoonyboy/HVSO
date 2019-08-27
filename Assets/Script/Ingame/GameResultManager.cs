using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;

public class GameResultManager : MonoBehaviour
{
    public GameObject SocketDisconnectedUI;

    

    public void OnReturnBtn() {
        if (transform.GetChild(0).gameObject.activeSelf) {
            transform.GetChild(0).gameObject.SetActive(false);
            transform.GetChild(1).gameObject.SetActive(true);
        }
        else if (transform.GetChild(1).gameObject.activeSelf) {
            FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.MAIN_SCENE);
        }
    }

    public void SocketErrorUIOpen(bool friendOut) {
        SocketDisconnectedUI.SetActive(true);
        if (friendOut)
            SocketDisconnectedUI.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "상대방이 게임을 \n 종료했습니다.";
    }

    public void OnMoveSceneBtn() {
        FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.MAIN_SCENE);
    }

    public void SetResultWindow(string result, string race) {
        Transform baseWindow = transform.GetChild(0);
        Transform resourceWindow = transform.GetChild(1);
        baseWindow.gameObject.SetActive(true);
        switch (result) {
            case "win":
                if (race == "human") {
                    baseWindow.Find("ResultCharacter/ResultHuman").gameObject.SetActive(true);
                    resourceWindow.Find("ResourceResultRibon/HumanRibon").gameObject.SetActive(true);
                }
                else {
                    baseWindow.Find("ResultCharacter/ResultOrc").gameObject.SetActive(true);
                    resourceWindow.Find("ResourceResultRibon/OrcRibon").gameObject.SetActive(true);
                }
                baseWindow.Find("ShineEffect/WinShineEffect").gameObject.SetActive(true);
                resourceWindow.Find("ShineEffect/WinShineEffect").gameObject.SetActive(true);
                baseWindow.Find("ResultCharacter/ResultText/WinText").gameObject.SetActive(true);
                resourceWindow.Find("ResultText/WinText").gameObject.SetActive(true);
                break;
            case "lose":
                if (race == "human") {
                    baseWindow.Find("ResultCharacter/ResultHuman").gameObject.SetActive(true);
                    baseWindow.Find("ResultCharacter/ResultHuman/ResultHumanHero").gameObject.GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "DEAD", false);
                }
                else {
                    baseWindow.Find("ResultCharacter/ResultOrc").gameObject.SetActive(true);
                    baseWindow.Find("ResultCharacter/ResultOrc/ResultOrcHero").gameObject.GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "DEAD", false);
                }
                baseWindow.Find("ShineEffect/LoseShineEffect").gameObject.SetActive(true);
                resourceWindow.Find("ShineEffect/LoseShineEffect").gameObject.SetActive(true);
                baseWindow.Find("ResultCharacter/LoseRibon").gameObject.SetActive(true);
                resourceWindow.Find("LoseRibon").gameObject.SetActive(true);
                baseWindow.Find("ResultCharacter/ResultText/LoseText").gameObject.SetActive(true);
                resourceWindow.Find("ResultText/LoseText").gameObject.SetActive(true);
                break;
            default:
                break;
        }
        resourceWindow.gameObject.SetActive(false);
    }
}
