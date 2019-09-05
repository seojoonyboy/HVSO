using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScenarioMask : MonoBehaviour
{
    public GameObject targetObject;

    public RectTransform topMask;
    public RectTransform leftMask;
    public RectTransform rightMask;
    public RectTransform bottonMask;

    public float rectWidthRadius = 0;
    public float rectHeightRadius = 0;

    public void GetMaskHighlight() {
        if (targetObject == null) return;

        RectTransform targetTransform = targetObject.GetComponent<RectTransform>();

        if (targetTransform != null) {
            Vector3 targetPos = targetTransform.position;
            Vector3[] targetRectPos = new Vector3[4];
            targetTransform.GetWorldCorners(targetRectPos);

            // Vector3 leftTop = new Vector3(targetPos.x - targetRectPos[1].x, targetRectPos[1].y - targetPos.y, 0);

            float halfHeight = targetRectPos[1].y - targetPos.y;
            float halfWidth = targetPos.x - targetRectPos[1].x;

            Vector3 top = new Vector3(0, targetPos.y + halfHeight + rectHeightRadius, 0);
            Vector3 left = new Vector3(targetPos.x - halfWidth - rectWidthRadius, 0, 0);
            Vector3 right = new Vector3(targetPos.x + halfWidth + rectWidthRadius, 0, 0);
            Vector3 bottom = new Vector3(0, targetPos.y - halfHeight - rectHeightRadius, 0);
            topMask.position = top;
            leftMask.position = left;
            rightMask.position = right;
            bottonMask.position = bottom;
        }
        
    }







    // Start is called before the first frame update
    void Start()
    {
        if (topMask == null)
            topMask = gameObject.transform.Find("TopMask").GetComponent<RectTransform>();

        if (leftMask == null)
            leftMask = gameObject.transform.Find("LeftMask").GetComponent<RectTransform>();

        if (rightMask == null)
            rightMask = gameObject.transform.Find("RightMask").GetComponent<RectTransform>();

        if (bottonMask == null)
            bottonMask = gameObject.transform.Find("BottomMask").GetComponent<RectTransform>();




        Vector3[] temp = new Vector3[4];
        topMask.GetWorldCorners(temp);


        rectWidthRadius = topMask.transform.position.x - temp[0].x;
        rectHeightRadius = rectWidthRadius;
        






    }

}
