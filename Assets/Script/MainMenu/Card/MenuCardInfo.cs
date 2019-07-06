using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuCardInfo : MonoBehaviour
{
    public void SetCardInfo(dataModules.CollectionCard data, bool isHuman) {
        if (!data.isHeroCard) {
            transform.Find("Frame").GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites["frame_" + data.rarelity];
            transform.Find("Name").GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites["name_" + data.rarelity];
            transform.Find("Name/Text").GetComponent<TMPro.TextMeshProUGUI>().text = data.name;
            transform.Find("Dialog").GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites["dialog_" + data.rarelity];
        }
        else {
            string race;
            if (isHuman)
                race = "human_";
            else
                race = "orc_";
            transform.Find("Frame").GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites["hero_frame_" + race + data.rarelity];
            transform.Find("Name").GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites["hero_name_" + race + data.rarelity];
            transform.Find("Dialog").GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites["hero_dialog_" + race + data.rarelity];
        }
        transform.Find("Name/Text").GetComponent<TMPro.TextMeshProUGUI>().text = data.name;
        if (AccountManager.Instance.resource.infoPortraite.ContainsKey(data.id)) transform.Find("Portrait").GetComponent<Image>().sprite = AccountManager.Instance.resource.infoPortraite[data.id];
        if (data.skills.Length != 0) {
            transform.Find("Dialog/Text").GetComponent<TMPro.TextMeshProUGUI>().text = data.skills[0].desc;
        }
        else
            transform.Find("Dialog/Text").GetComponent<TMPro.TextMeshProUGUI>().text = null;

        if (data.hp != null) {
            transform.Find("Health/Text").GetComponent<TMPro.TextMeshProUGUI>().text = data.hp.ToString();
            transform.Find("Health").gameObject.SetActive(true);
        }
        else
            transform.Find("Health").gameObject.SetActive(false);

        if (data.attack != null) {
            transform.Find("Attack/Text").GetComponent<TMPro.TextMeshProUGUI>().text = data.attack.ToString();
            transform.Find("Attack").gameObject.SetActive(true);
        }
        else
            transform.Find("Attack").gameObject.SetActive(false);

        transform.Find("Cost/Text").GetComponent<TMPro.TextMeshProUGUI>().text = data.cost.ToString();
        transform.Find("Class").GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites["class_" + data.cardClasses[0]];
        transform.Find("Class/Icon").GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites["class_icon_" + data.cardClasses[0]];
        transform.Find("Class/Icon").GetComponent<Image>().SetNativeSize();

        transform.Find("SkillIcon1").gameObject.SetActive(false);
        transform.Find("SkillIcon2").gameObject.SetActive(false);
        if (data.type == "unit") {
            if (data.attackTypes.Length != 0) {
                transform.Find("SkillIcon1").gameObject.SetActive(true);
                transform.Find("SkillIcon1").GetComponent<Image>().sprite = AccountManager.Instance.resource.skillIcons[data.attackTypes[0]];
            }
            if (data.attributes.Length != 0) {
                transform.Find("SkillIcon1").gameObject.SetActive(true);
                transform.Find("SkillIcon1").GetComponent<Image>().sprite = AccountManager.Instance.resource.skillIcons[data.attributes[0]];
            }
            if (data.attackTypes.Length != 0 && data.attributes.Length != 0) {
                transform.Find("SkillIcon2").gameObject.SetActive(true);
                transform.Find("SkillIcon2").GetComponent<Image>().sprite = AccountManager.Instance.resource.skillIcons[data.attackTypes[0]];
            }
        }
    }

    public void CloseInfo() {
        transform.parent.gameObject.SetActive(false);
    }
}
