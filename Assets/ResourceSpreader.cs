using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceSpreader : MonoBehaviour {
    [SerializeField] Transform startObj;
    [SerializeField] Transform targetObj;
    [SerializeField] Sprite resourceImage;

    Transform startPos;
    Transform targetPos;

    private void Awake() {
        startPos = transform.Find("Start");
        targetPos = transform.Find("Target");
        startPos.SetParent(startObj);
        startPos.localPosition = Vector3.zero;
        startPos.SetParent(transform);
        targetPos.SetParent(targetObj);
        targetPos.localPosition = Vector3.zero;
        targetPos.SetParent(transform);

        Transform objectList = transform.Find("Objects");
        Vector2 imgSize = new Vector2(resourceImage.rect.width, resourceImage.rect.height);
        for (int i = 0; i < objectList.childCount; i++) {
            Transform obj = objectList.GetChild(i);
            float x = Random.Range(-250, 250);
            float y = Random.Range(-250, 250);
            Vector2 startRandomPos = new Vector2(startObj.localPosition.x + x, startObj.localPosition.y + y);
            obj.GetComponent<SpreadResourceController>().SetPositions(startPos.localPosition,startRandomPos, targetPos.localPosition);
            obj.GetComponent<RectTransform>().sizeDelta = imgSize;
            obj.GetComponent<Image>().sprite = resourceImage;
            obj.localPosition = startPos.localPosition;
        }
    }

    public void StartSpread(int amount) {
        StartCoroutine(SpreadResource(amount));
    }

    IEnumerator SpreadResource(int amount) {
        Transform objectList = transform.Find("Objects");
        if (amount > 80)
            amount = 80;
        for (int i = 0; i < amount * 5; i++) {
            GameObject obj = objectList.GetChild(i).gameObject;
            obj.SetActive(true);
            Vector2 start = obj.GetComponent<SpreadResourceController>().startRandomPos;
            iTween.MoveTo(obj, iTween.Hash("x", start.x, "y", start.y, "time", 1.0f, "islocal", true));
        }
        yield return new WaitForSeconds(0.2f);
        for (int i = 0; i < amount * 5; i++) {
            GameObject obj = objectList.GetChild(i).gameObject;
            iTween.MoveTo(obj, iTween.Hash("x", targetPos.localPosition.x, "y", targetPos.localPosition.y, "time", 0.6f, "islocal", true));
            yield return new WaitForSeconds(0.01f);
        }
    }
}
