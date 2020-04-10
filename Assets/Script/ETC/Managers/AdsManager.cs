using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Haegin;
using HaeginGame;


public class AdsManager : Singleton<AdsManager>
{
    IronSourcePlacement placement = null;
    public void Init() {
        IronSourceEvents.onRewardedVideoAdOpenedEvent += RewardedVideoAdOpenedEvent;
        IronSourceEvents.onRewardedVideoAdClosedEvent += RewardedVideoAdClosedEvent;
        IronSourceEvents.onRewardedVideoAvailabilityChangedEvent += RewardedVideoAvailabilityChangedEvent;
        IronSourceEvents.onRewardedVideoAdStartedEvent += RewardedVideoAdStartedEvent;
        IronSourceEvents.onRewardedVideoAdEndedEvent += RewardedVideoAdEndedEvent;
        IronSourceEvents.onRewardedVideoAdRewardedEvent += RewardedVideoAdRewardedEvent;
        IronSourceEvents.onRewardedVideoAdShowFailedEvent += RewardedVideoAdShowFailedEvent;

#if MDEBUG
        IronSource.Agent.setAdaptersDebug(true);
#endif
        IronSource.Agent.setUserId(Account.GetAccountId()); //Haegin's Id
#if UNITY_ANDROID
        IronSource.Agent.init("abe0d80d");
#elif UNITY_IOS
        IronSource.Agent.init("abe064fd");
#endif
        IronSource.Agent.validateIntegration();
        IronSource.Agent.shouldTrackNetworkState(true);
        CheckInfo();
    }

    void CheckInfo() {
        DebugPlacement(IronSource.Agent.getPlacementInfo("main"));
        DebugPlacement(IronSource.Agent.getPlacementInfo("shop"));
    }

    void DebugPlacement(IronSourcePlacement placement) {
        if(placement != null) {
            string rewardName = placement.getRewardName();
            int rewardAmount = placement.getRewardAmount();
            Debug.Log("보상 이름 : " + rewardName + "\n보상 규모 : " + rewardAmount);
        }
        else Debug.Log("해당 이름이 존재 하지 않습니다.");
    }

    void OnApplicationPause(bool isPaused) {
        IronSource.Agent.onApplicationPause(isPaused);
    }


    void RewardedVideoAdOpenedEvent() {
        #if MDEBUG
        Debug.Log("RewardedVideoAdOpenedEvent");
        #endif
    }

    void RewardedVideoAdClosedEvent() {
        #if MDEBUG
        Debug.Log("RewardedVideoAdClosedEvent");
        #endif
        if(placement != null)
            AccountManager.Instance.RequestMainAdReward(placement, ()=>placement = null);
    }

    void RewardedVideoAvailabilityChangedEvent(bool available) {
        #if MDEBUG
        Debug.Log("RewardedVideoAvailabilityChangedEvent : " + available);
        #endif
        bool rewardedVideoAvailability = available;
    }

    void RewardedVideoAdStartedEvent() {
        #if MDEBUG
        Debug.Log("RewardedVideoAdStartedEvent");
        #endif
    }

    void RewardedVideoAdEndedEvent() {
        #if MDEBUG
        Debug.Log("RewardedVideoAdEndedEvent");
        #endif
    }

    void RewardedVideoAdRewardedEvent(IronSourcePlacement placement) {
        #if MDEBUG
        Debug.Log("RewardedVideoAdRewardedEvent");
        if(placement != null) {
            string rewardName = placement.getRewardName();
            int rewardAmount = placement.getRewardAmount();
            Debug.Log("보상 이름 : " + rewardName + "\n보상 규모 : " + rewardAmount);
        }
        this.placement = placement;
#endif
    }

    void RewardedVideoAdShowFailedEvent(IronSourceError error) {
        #if MDEBUG
        Debug.Log("RewardedVideoAdShowFailedEvent : " + error.getDescription());
        #endif
    }

    public void ShowRewardedBtn(string placementName) {
#if UNITY_EDITOR
        string rewardName = placementName.CompareTo("main") == 0 ? "presupply" : "gift";
        int rewardAmount = placementName.CompareTo("main") == 0 ? 40 : 1;
        AccountManager.Instance.RequestMainAdReward(new IronSourcePlacement(placementName, rewardName, rewardAmount));
#endif
        if (IronSource.Agent.isRewardedVideoAvailable()) IronSource.Agent.showRewardedVideo(placementName);
        else {
            
            Modal.instantiate(AccountManager.Instance.GetComponent<Fbl_Translator>().GetLocalizedText("UIPopup", "ui_popup_failedloadad"), Modal.Type.CHECK);
        }
    }
}
