using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class MenuMask : SerializedMonoBehaviour
{
    public static MenuMask Instance;
    public Dictionary<string, GameObject> menuObject;
    public GameObject maskPanel;


    public RectTransform topMask;
    public RectTransform leftMask;
    public RectTransform rightMask;
    public RectTransform bottonMask;
    

    //사각형 모서리 길이 / 2
    public float rectWidthRadius = 0;
    public float rectHeightRadius = 0;

    public GameObject menuTalkPanel;


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

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ScopeMenuObject(GameObject menuObject) {
        if (menuObject == null) return;
        ActiveMask();

        RectTransform menuTransform = menuObject.GetComponent<RectTransform>();
        if(menuTransform != null) {
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

        if(maskObject != null) {

            if(sub == null) {
                return maskObject;
            }
            else {


                return null;
            }
        }
        return null;

    }
}
