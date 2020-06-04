using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainHeroSlider : MonoBehaviour {
    void SetHeroInfos() {
        if(AccountManager.Instance.allHeroes != null) {
            foreach(dataModules.HeroInventory heroData in AccountManager.Instance.allHeroes) {
                Transform heroObject = transform.Find("HeroPool" + heroData.id);
                heroObject.Find("HeroLvUISet/HeroName").GetComponent<TMPro.TextMeshProUGUI>().text = heroData.name;
                if (AccountManager.Instance.myHeroInventories.ContainsKey(heroData.id)) {
                    dataModules.HeroInventory myHero = AccountManager.Instance.myHeroInventories[heroData.id];
                    heroObject.Find("HeroLvUISet/HeroLv/Text").GetComponent<Text>().text = myHero.lv.ToString();

                }
            }
        }
    }
}
