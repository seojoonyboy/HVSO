using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PackageBannerSlider : MonoBehaviour
{
    [SerializeField] ShopManager shopManager;
    List<dataModules.Shop> itemList;
    public Dictionary<string, Transform> packageObjects;
    IEnumerator sliderOn;
    bool onPlay = false;

    private void OnDestroy() {
        StopCoroutine(sliderOn);
    }

    public void SetBanner() {
        if(onPlay)
            StopCoroutine(sliderOn);
        onPlay = false;
        sliderOn = PlaySlider();
        transform.GetChild(0).localPosition = Vector3.zero;
        transform.GetChild(1).localPosition = new Vector3(720, 0, 0);
        if(itemList != null)
            itemList.Clear();
        itemList = new List<dataModules.Shop>();
        foreach (dataModules.Shop item in AccountManager.Instance.shopItems) {
            if (item.category == "package" && item.enabled) itemList.Add(item);
        }
        string language = AccountManager.Instance.GetLanguageSetting();
        string id1 = Regex.Replace(itemList[0].id, @"\d", "");
        transform.GetChild(0).GetComponent<Image>().sprite = AccountManager.Instance.resource.packageImages["banner_" + id1 + "_" + language];
        transform.GetChild(0).GetComponent<Button>().onClick.RemoveAllListeners();
        transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => shopManager.GoToPackage(packageObjects[id1]));
        if (itemList.Count > 1) {
            string id2 = Regex.Replace(itemList[1].id, @"\d", "");
            transform.GetChild(1).GetComponent<Image>().sprite = AccountManager.Instance.resource.packageImages["banner_" + id2 + "_" + language];
            transform.GetChild(1).GetComponent<Button>().onClick.RemoveAllListeners();
            transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() => shopManager.GoToPackage(packageObjects[id2]));
            StartCoroutine(sliderOn);
        }
    }

    IEnumerator PlaySlider() {
        onPlay = true;
        int itemNum = itemList.Count;
        int count = 1;
        string language = AccountManager.Instance.GetLanguageSetting();
        while (onPlay) {
            yield return new WaitForSeconds(2.5f);
            iTween.MoveTo(transform.GetChild(0).gameObject, iTween.Hash("x", -720, "islocal", true, "time", 0.7f));
            iTween.MoveTo(transform.GetChild(1).gameObject, iTween.Hash("x", 0, "islocal", true, "time", 0.7f));
            yield return new WaitForSeconds(2.5f);
            transform.GetChild(0).SetAsLastSibling();
            transform.GetChild(1).localPosition = new Vector3(720, 0, 0);
            count++;
            if (count == itemNum) count = 0;
            string id = Regex.Replace(itemList[count].id, @"\d", "");
            transform.GetChild(1).GetComponent<Image>().sprite = AccountManager.Instance.resource.packageImages["banner_" + id + "_" + language];
            transform.GetChild(1).GetComponent<Button>().onClick.RemoveAllListeners();
            transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() => shopManager.GoToPackage(packageObjects[id]));
        }
    }
}
