using TMPro;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

public class ScenarioPlayer : PlayerController
{
    // Start is called before the first frame update
    void Start()
    {
        Init();
        CardDropManager.Instance.SetUnitDropPos();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Init() {
        costText = playerUI.transform.Find("PlayerResource").GetChild(0).Find("Text").GetComponent<Text>();
        HPText = playerUI.transform.Find("PlayerHealth/HealthText").GetComponent<Text>();
        HPGauge = playerUI.transform.Find("PlayerHealth/Helth&Shield/HpParent1/HpParent2/HpParent3/HpGage");
        //shieldGauge = playerUI.transform.Find("PlayerHealth/Helth&Shield/SheildGauge").GetComponent<Image>();
        if (isHuman) {
            playerUI.transform.Find("PlayerHealth/Flag/Human").gameObject.SetActive(true);
            sheildRemain = playerUI.transform.Find("PlayerHealth/HumanSheild");
        }
        else {
            playerUI.transform.Find("PlayerHealth/Flag/Orc").gameObject.SetActive(true);
            sheildRemain = playerUI.transform.Find("PlayerHealth/OrcSheild");
        }
        sheildRemain.gameObject.SetActive(true);
        if (isPlayer) {
            buttonParticle = playerUI.transform.Find("TurnUI/ResourceOut").gameObject;
            if (isHuman)
                buttonParticle.GetComponent<SkeletonGraphic>().color = new Color(0.552f, 0.866f, 1);
            else
                buttonParticle.GetComponent<SkeletonGraphic>().color = new Color(1, 0.556f, 0.556f);
            buttonParticle.SetActive(false);
        }
        else {
        }

        SetPlayerHero(isHuman);
        if (!isPlayer)
            transform.Find("FightSpine").localPosition = new Vector3(0, 3, 0);
        shieldGauge.Initialize(false);
        shieldGauge.Update(0);
        shieldGauge.Skeleton.SetSlotsToSetupPose();
        shieldGauge.AnimationState.SetAnimation(0, "0", false);
        SetShield();

        shieldCount = 3;
        Debug.Log(heroSpine);
    }

}
