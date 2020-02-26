using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceSpreader : MonoBehaviour {
    [SerializeField] Transform startObj;
    [SerializeField] Transform targetObj;
    [SerializeField] Sprite resourceImage;

    [SerializeField] Transform poolParent, content;

    const int MAX_NUM = 20;

    public void StartSpread(int amount, Transform[] targets = null) {
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

        float x = Random.Range(-250, 250);
        float y = Random.Range(-250, 250);
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
    }

    public void TriggerTestButton() {
        StartSpread(10);
    }
}
