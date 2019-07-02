using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class MenuSceneController : MonoBehaviour {
    [SerializeField] Transform fixedCanvas;
    [SerializeField] HorizontalScrollSnap windowScrollSnap;

    private void Start() {
        
    }

    /// <summary>
    /// PVP대전 버튼 클릭
    /// </summary>
    public void OnPVPClicked() {
        SceneManager.Instance.LoadScene(SceneManager.Scene.LOADING_SCENE);

        SoundManager.Instance.PlaySound(SoundType.FIRST_TURN);
    }

    public void ClickMenuButton(int pageNum) {
        windowScrollSnap.GoToScreen(pageNum);
    }
}
