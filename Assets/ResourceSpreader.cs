using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using UnityEngine.Events;

public class ResourceSpreader : MonoBehaviour {
    [SerializeField] Transform startObj;
    [SerializeField] Transform targetObj;
    [SerializeField] Sprite resourceImage;

    [SerializeField] Transform poolParent, content;
    [SerializeField] ScrollRect scrollRect;
    public bool blockScrollWhileSpreading;

    const int MAX_NUM = 20;
    private float randomX = 250, randomY = 250;
    private UnityAction spreadDoneCallback;

    public void SetRandomRange(float x, float y) {
        randomX = x;
        randomY = y;
    }

    public void StartSpread(int amount, Transform[] targets = null, UnityAction callback = null) {
        spreadDoneCallback = callback;
        if(targets == null) StartCoroutine(SpreadResource(amount));
        else {
            if(targets.Length == 2) {
                startObj = targets[0];
                targetObj = targets[1];

                StartCoroutine(SpreadResource(amount));
            }
        }
    }

    IEnumerator SpreadResource(int amount) {
        if (blockScrollWhileSpreading && scrollRect != null) scrollRect.enabled = false;

        for (int i = 0; i < amount; i++) {
            GameObject obj = GetObject();
            obj.SetActive(true);
            Vector2 start = obj.GetComponent<SpreadResourceController>().startRandomPos;
            iTween.MoveTo(obj, iTween.Hash(
                    "x", start.x, 
                    "y", start.y, 
                    "time", 0.6f,
                    "oncomplete", "MoveToTarget",
                    "oncompletetarget", gameObject,
                    "oncompleteparams", obj
            ));

            yield return new WaitForSeconds(0.02f);
        }
        spreadDoneCallback?.Invoke();
    }

    void MoveToTarget(GameObject obj) {
        Vector2 end = obj.GetComponent<SpreadResourceController>().targetPos;
        iTween.MoveTo(obj, iTween.Hash(
                    "x", end.x,
                    "y", end.y,
                    "time", 0.2f,
                    "oncomplete", "EffectFinished",
                    "oncompletetarget", gameObject,
                    "oncompleteparams", obj
        ));
    }

    void EffectFinished(GameObject obj) {
        ReturnObject(obj);
    }

    /// <summary>
    /// Pool에서 가져오기
    /// </summary>
    /// <returns>가져오는 Object</returns>
    public GameObject GetObject() {
        GameObject obj = null;
        if (poolParent.childCount > 1) {
            obj = poolParent.GetChild(0).gameObject;
        }
        else {
            obj = Instantiate(poolParent.GetChild(0).gameObject);
        }

        obj.transform.SetParent(content);
        
        obj.transform.localScale = Vector3.one;
        Vector2 imgSize = new Vector2(resourceImage.rect.width, resourceImage.rect.height);
        obj.GetComponent<RectTransform>().sizeDelta = imgSize;
        obj.GetComponent<Image>().sprite = resourceImage;
        obj.GetComponent<Image>().preserveAspect = true;
        obj.SetActive(true);
        obj.transform.position = startObj.position;

        float x = Random.Range(-randomX, randomX);
        float y = Random.Range(-randomY, randomY);
        Vector2 startRandomPos = new Vector2(startObj.position.x + x, startObj.position.y + y);

        obj.GetComponent<SpreadResourceController>()
            .SetPositions(
                startObj.position,
                startRandomPos,
                targetObj.position
            );

        return obj;
    }

    /// <summary>
    /// 반납
    /// </summary>
    public void ReturnObject(GameObject obj) {
        obj.SetActive(false);
        if (poolParent.childCount > MAX_NUM) {
            Destroy(obj);
        }
        else {
            obj.transform.SetParent(poolParent);
            obj.transform.localPosition = Vector3.zero;
        }

        //Animation 진행이 끝난 상태
        if(content.childCount == 0) {
            if (blockScrollWhileSpreading && scrollRect != null) scrollRect.enabled = true; 
        }
    }

    public void TriggerTestButton() {
        StartSpread(10);
    }
}
