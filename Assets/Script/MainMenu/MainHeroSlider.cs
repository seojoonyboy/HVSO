using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainHeroSlider : MonoBehaviour
{
    bool onPlay = false;

    private void Awake() {
        NoneIngameSceneEventHandler.Instance.AddListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_MAINHEROES_UPDATED, SetHeroInfos);
    }

    private void OnDestroy() {
        NoneIngameSceneEventHandler.Instance.RemoveListener(NoneIngameSceneEventHandler.EVENT_TYPE.API_MAINHEROES_UPDATED, SetHeroInfos);
    }    

    void SetHeroInfos(Enum Event_Type, Component Sender, object Param) {
        if(AccountManager.Instance.mainHeroes != null) {
            onPlay = false;
            for (int i = 0; i < AccountManager.Instance.mainHeroes.Length; i++) {
                SetHeroObject(AccountManager.Instance.mainHeroes[i]);
            }
        }
        if (!onPlay) 
            StartCoroutine(PlaySlider());
    }

    void SetHeroObject(dataModules.HeroInventory heroData) {
        Transform heroObject = transform.Find("HeroPool/" + heroData.heroId);
        Transform lvUI = heroObject.Find("HeroLvUISet");
        bool haveHero = heroData.howToGet == null;
        heroObject.Find("HowToGet").gameObject.SetActive(!haveHero);
        heroObject.Find("TalkBox").gameObject.SetActive(false);
        heroObject.Find("TalkBox").GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = heroData.mainDialogues[0];
        lvUI.Find("HeroLv").gameObject.SetActive(haveHero);
        lvUI.Find("HeroPiece").gameObject.SetActive(!haveHero);
        lvUI.Find("CustomUISlider/Labels").gameObject.SetActive(!haveHero);
        lvUI.Find("CustomUISlider/Percentage").gameObject.SetActive(haveHero);
        lvUI.Find("HeroName").GetComponent<TMPro.TextMeshProUGUI>().text = heroData.name;
        if (haveHero) {
            heroObject.Find("HeroSpine").GetComponent<SkeletonGraphic>().color = Color.white;
            lvUI.Find("HeroLv/Text").GetComponent<Text>().text = heroData.lv.ToString();
            double expRate = heroData.exp / heroData.nextExp;
            lvUI.Find("CustomUISlider/Percentage").GetComponent<TMPro.TextMeshProUGUI>().text = Math.Truncate(expRate * 100).ToString() + "%";
            lvUI.Find("CustomUISlider/Slider").GetComponent<Slider>().value = (float)expRate;
        }
        else {
            heroObject.Find("HeroSpine").GetComponent<SkeletonGraphic>().color = new Color(0.235f, 0.235f, 0.235f);
            heroObject.Find("HowToGet").GetComponent<TMPro.TextMeshProUGUI>().text = heroData.howToGet;
            lvUI.Find("HeroPiece").GetComponent<Image>().sprite = AccountManager.Instance.resource.heroPortraite[heroData.heroId + "_piece"];
            if (AccountManager.Instance.myHeroInventories.ContainsKey(heroData.heroId)) {
                dataModules.HeroInventory myHero = AccountManager.Instance.myHeroInventories[heroData.heroId];
                lvUI.Find("CustomUISlider/Labels/CurrentValueLabel").GetComponent<TMPro.TextMeshProUGUI>().text = myHero.piece.ToString();
                lvUI.Find("CustomUISlider/Labels/MaxValueLabel").GetComponent<TMPro.TextMeshProUGUI>().text = myHero.nextTier.piece.ToString();
                lvUI.Find("CustomUISlider/Slider").GetComponent<Slider>().value = heroData.piece / heroData.nextTier.piece;
            }
            else {
                lvUI.Find("CustomUISlider/Labels/CurrentValueLabel").GetComponent<TMPro.TextMeshProUGUI>().text = "0";
                lvUI.Find("CustomUISlider/Labels/MaxValueLabel").GetComponent<TMPro.TextMeshProUGUI>().text = "30";
                lvUI.Find("CustomUISlider/Slider").GetComponent<Slider>().value = 0;
            }
            
        }
    }

    IEnumerator PlaySlider() {
        onPlay = true;
        int count = 1;
        int dialogCount = 0;
        Transform targetParent = transform.Find("Mask");
        Transform obj1 = transform.Find("HeroPool/" + AccountManager.Instance.mainHeroes[0].heroId);
        Transform obj2 = transform.Find("HeroPool/" + AccountManager.Instance.mainHeroes[1].heroId);
        obj1.SetParent(targetParent.GetChild(0));
        obj1.localPosition = Vector3.zero;
        obj1.Find("OpenHeroInfoButton").GetComponent<Button>().onClick.RemoveAllListeners();
        obj1.Find("OpenHeroInfoButton").GetComponent<Button>().onClick.AddListener(() => OpenHeroInfo(AccountManager.Instance.mainHeroes[0].heroId));
        obj1.Find("TalkBox").gameObject.SetActive(true);
        obj2.SetParent(targetParent.GetChild(1));
        obj2.localPosition = Vector3.zero;
        obj2.Find("OpenHeroInfoButton").GetComponent<Button>().onClick.RemoveAllListeners();
        obj2.Find("OpenHeroInfoButton").GetComponent<Button>().onClick.AddListener(() => OpenHeroInfo(AccountManager.Instance.mainHeroes[1].heroId));
        obj2.Find("TalkBox").gameObject.SetActive(false);
        while (onPlay) {
            yield return new WaitForSeconds(5.5f);
            targetParent.GetChild(0).GetChild(0).Find("TalkBox").gameObject.SetActive(false);
            iTween.MoveTo(targetParent.GetChild(0).gameObject, iTween.Hash("x", -950, "islocal", true, "time", 0.7f));
            iTween.MoveTo(targetParent.GetChild(1).gameObject, iTween.Hash("x", 0, "islocal", true, "time", 0.7f));
            yield return new WaitForSeconds(1.0f);
            targetParent.GetChild(1).GetChild(0).Find("TalkBox").gameObject.SetActive(true);
            targetParent.GetChild(0).GetChild(0).SetParent(transform.Find("HeroPool"));
            targetParent.GetChild(0).SetAsLastSibling();
            targetParent.GetChild(1).localPosition = new Vector3(950, 0, 0);
            count++;
            if (count == AccountManager.Instance.mainHeroes.Length) {
                count = 0;
                dialogCount++;
                if (dialogCount == 3) dialogCount = 0;
            }
            string nextId = AccountManager.Instance.mainHeroes[count].heroId;
            Transform nextHero = transform.Find("HeroPool/" + nextId);
            nextHero.Find("TalkBox").GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.mainHeroes[count].mainDialogues[dialogCount];
            nextHero.Find("OpenHeroInfoButton").GetComponent<Button>().onClick.RemoveAllListeners();
            nextHero.Find("OpenHeroInfoButton").GetComponent<Button>().onClick.AddListener(() => OpenHeroInfo(nextId));
            nextHero.SetParent(targetParent.GetChild(1));
            nextHero.localPosition = Vector3.zero;
        }
    }

    public void OpenHeroInfo(string heroId) {
        MenuHeroInfo.heroInfoWindow.SetHeroInfoWindow(heroId);
        MenuHeroInfo.heroInfoWindow.transform.parent.gameObject.SetActive(true);
        MenuHeroInfo.heroInfoWindow.gameObject.SetActive(true);
    }
}
