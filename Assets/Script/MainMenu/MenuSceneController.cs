using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSceneController : MonoBehaviour {

    /// <summary>
    /// 싱글플레이 미션 버튼 클릭 (식물, 좀비)
    /// </summary>
    public void OnMissionClicked() {
        SceneManager.Instance.LoadScene(SceneManager.Scene.MISSION_SELECT_SCENE);
    }

    /// <summary>
    /// PVP대전 버튼 클릭
    /// </summary>
    public void OnPVPClicked() {
        SceneManager.Instance.LoadScene(SceneManager.Scene.LOADING_SCENE);

        SoundManager.Instance.PlaySound(SoundType.FIRST_TURN);
    }
}
