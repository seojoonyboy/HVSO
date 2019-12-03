using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;

public class MenuMask : SerializedMonoBehaviour
{
    public static MenuMask Instance;
    public Dictionary<string, GameObject> menuObject;
    public GameObject maskPanel;
    public GameObject dimmedPanel;
    public MenuTutorialManager menuTutorialManager;

    public RectTransform topMask;
    public RectTransform leftMask;
    public RectTransform rightMask;
    public RectTransform bottonMask;
    

    //사각형 모서리 길이 / 2
    public float rectWidthRadius = 0;
    public float rectHeightRadius = 0;

    public GameObject menuTalkPanel;
    public Dictionary<GameObject, Transform> dimmedObjInfos;

    private void Awake() {
        Instance = this;
    }

    private void OnDestroy() {
        Instance = null;
    }   

    // Start is called before the first frame update
    void Start()
    {
        CheckMaskObject();
        Vector3[] corner = new Vector3[4];
        topMask.GetWorldCorners(corner);
        rectWidthRadius = topMask.transform.position.x - corner[0].x;
        rectHeightRadius = rectWidthRadius;

        //Invoke("test", 1.0f);
    }

    void test() {
        ScopeMenuObject(menuObject["storyButton"]);
    }

    public void ScopeMenuObject(GameObject menuObject) {
        if (menuObject == null) return;
        ActiveMask();

        RectTransform menuTransform = menuObject.GetComponent<RectTransform>();
        if (menuTransform == null) return;
        Vector3 menuObjectPos = menuTransform.position;
        Vector3[] menuObjectCorner = new Vector3[4];
        menuTransform.GetWorldCorners(menuObjectCorner);

        float menuHalfHeight = menuObjectCorner[1].y - menuObjectPos.y;
        float menuHalfWidth = menuObjectPos.x - menuObjectCorner[1].x;

        Vector3 top = new Vector3(0, menuObjectPos.y + menuHalfHeight + rectHeightRadius, 0);
        Vector3 left = new Vector3(menuObjectPos.x - menuHalfWidth - rectWidthRadius, 0, 0);
        Vector3 right = new Vector3(menuObjectPos.x + menuHalfWidth + rectWidthRadius, 0, 0);
        Vector3 bottom = new Vector3(0, menuObjectPos.y - menuHalfHeight - rectHeightRadius, 0);
        topMask.position = top;
        leftMask.position = left;
        rightMask.position = right;
        bottonMask.position = bottom;
    }

    private void ActiveMask() {
        maskPanel.SetActive(true);
    }

    private void DeactiveMask() {
        maskPanel.SetActive(false);
    }

    public void BlockScreen() {
        ActiveMask();
        ZeroMaskPos();
    }

    public void BlockWithTransparent() {
        ActiveMask();
        ZeroMaskPos();

        topMask.GetComponent<Image>().color = new Color(1, 1, 1, 0.004f);
        leftMask.GetComponent<Image>().color = new Color(1, 1, 1, 0.004f);
        rightMask.GetComponent<Image>().color = new Color(1, 1, 1, 0.004f);
        bottonMask.GetComponent<Image>().color = new Color(1, 1, 1, 0.004f);
    }

    public void ResetTransparentMask() {
        topMask.GetComponent<Image>().color = new Color(1, 1, 1, 0.004f);
        leftMask.GetComponent<Image>().color = new Color(1, 1, 1, 0.004f);
        rightMask.GetComponent<Image>().color = new Color(1, 1, 1, 0.004f);
        bottonMask.GetComponent<Image>().color = new Color(1, 1, 1, 0.004f);
    }

    public void UnBlockScreen() {
        DeactiveMask();
    }

    public void HideText() {
        menuTalkPanel.SetActive(false);
    }


    private void ZeroMaskPos() {
        topMask.transform.position = Vector3.zero;
        leftMask.transform.position = Vector3.zero;
        rightMask.transform.position = Vector3.zero;
        bottonMask.transform.position = Vector3.zero;
    }


    private void CheckMaskObject() {
        if (topMask == null || leftMask == null || rightMask == null || bottonMask == null) {
            topMask = maskPanel.transform.Find("TopMask").GetComponent<RectTransform>();
            leftMask = maskPanel.transform.Find("LeftMask").GetComponent<RectTransform>();
            rightMask = maskPanel.transform.Find("RightMask").GetComponent<RectTransform>();
            bottonMask = maskPanel.transform.Find("BottomMask").GetComponent<RectTransform>();
        }
    }


    public GameObject GetMenuObject(string main, string sub = null) {

        GameObject maskObject = menuObject[main].gameObject;

        if (maskObject == null) return null;
        return sub == null ? maskObject : null;
    }

    public void OnDimmed(Transform origin, GameObject target) {
        dimmedPanel.SetActive(true);

        if (dimmedObjInfos == null) dimmedObjInfos = new Dictionary<GameObject, Transform>();
        dimmedObjInfos[target] = origin;
        target.transform.SetParent(dimmedPanel.transform);
    }

    public void OffDimmed(GameObject target) {
        dimmedPanel.SetActive(false);

        if (dimmedObjInfos.ContainsKey(target)) {
            target.transform.SetParent(dimmedObjInfos[target]);
            dimmedObjInfos.Remove(target);
        }
        else {
            Logger.LogError(target + "의 Origin 정보를 찾을 수 없습니다.");
        }
    }

    public void OffDimmed(GameObject target, int sibilingIndex) {
        dimmedPanel.SetActive(false);

        if (dimmedObjInfos.ContainsKey(target)) {
            target.transform.SetParent(dimmedObjInfos[target]);
            target.transform.SetSiblingIndex(sibilingIndex);
            dimmedObjInfos.Remove(target);
        }
        else {
            Logger.LogError(target + "의 Origin 정보를 찾을 수 없습니다.");
        }
    }

    public void OffDimmed(GameObject targetObject, string name) {
        int index = -1;
        switch (name) {
            case "orc_story_tutorial_1":
                index = 0;
                Transform tf = menuTutorialManager.scenarioManager.orc.stageContent.transform;
                foreach(Transform child in tf) {
                    if (child.name == name) Destroy(child.gameObject);
                }
                break;
        }

        if(index == -1) {
            OffDimmed(targetObject);
        }
        else {
            OffDimmed(targetObject, index);
        }
    }
}
