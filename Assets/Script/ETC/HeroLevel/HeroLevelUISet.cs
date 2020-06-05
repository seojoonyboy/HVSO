using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fbl_UIModule {
    public class HeroLevelUISet : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI heroName;
        [SerializeField] private Text heroLevel;

        [SerializeField] private PiecUISet _piecUiSet;
        [SerializeField] private ExpUISet _expUiSet;

        private void SetUi(UIType type, int maxVal, int currentVal, string name = null, int lv = -1) {
            switch (type) {
                case UIType.EXP:
                    if(!_expUiSet.obj.activeSelf) _expUiSet.obj.SetActive(true);
                    if(_piecUiSet.obj.activeSelf) _piecUiSet.obj.SetActive(false);
                    
                    _expUiSet.Init(maxVal, currentVal, name, lv);
                    break;
                case UIType.PIECE:
                    if(!_piecUiSet.obj.activeSelf) _piecUiSet.obj.SetActive(true);
                    if(_expUiSet.obj.activeSelf) _expUiSet.obj.SetActive(false);
                    
                    _piecUiSet.Init(maxVal, currentVal, name, lv);
                    break;
            }
        }

        /// <summary>
        /// 조각 게이지 즉시 세팅
        /// </summary>
        /// <param name="maxVal"></param>
        /// <param name="CurrentVal"></param>
        public void InitPieceGage(int maxVal, int CurrentVal, string name, int lv, string heroId) {
            SetUi(UIType.PIECE, maxVal, CurrentVal, name, lv);

            var translator = AccountManager.Instance.GetComponent<Fbl_Translator>();
            string _heroName = string.IsNullOrEmpty(name)
                ? translator.GetLocalizedText("Hero", "hero_pc_" + heroId + "_name")
                : name;
            
            if (!string.IsNullOrEmpty(_heroName)) heroName.text = _heroName;
            
            if (lv != -1) heroLevel.text = lv.ToString();

            var _resourceManager = AccountManager.Instance.resource;
            heroId += "_piece";
            if(_resourceManager.heroPortraite.ContainsKey(heroId)) _piecUiSet.portrait.sprite = AccountManager.Instance.resource.heroPortraite[heroId];
        }
        
        /// <summary>
        /// 조각 게이지 증가 애니메이션
        /// </summary>
        /// <param name="data"></param>
        /// <param name="proceedSets">조각 게이지 구간 세트</param>
        /// <param name="levelUpCallback">레벨업 일어났을 때 callback</param>
        /// <param name="allFinishedCallback">모든 게이지 진행이 끝났을 때 callback</param>
        public void ProceedPieceGauge(List<CustomUISlider.PieceProceedSet> proceedSets, CustomUISlider.SliderIsInitialized levelUpCallback = null, CustomUISlider.SliderProgressFinished allFinishedCallback = null) {
            SetUi(UIType.PIECE, proceedSets[0].maxVal, proceedSets[0].from);
            
            _piecUiSet
                .uiSlider
                .StartProceed(proceedSets, levelUpCallback, allFinishedCallback);
        }

        /// <summary>
        /// 경험치 게이지 즉시 세팅
        /// </summary>
        /// <param name="maxVal"></param>
        /// <param name="CurrentVal"></param>
        public void InitExpGage(int maxVal, int CurrentVal, string name = null, int lv = -1, bool isPercentage = false) {
            _expUiSet.uiSlider.isPercentage = isPercentage;
            SetUi(UIType.EXP, maxVal, CurrentVal);

            if (!string.IsNullOrEmpty(name)) heroName.text = name;
            if (lv != -1) heroLevel.text = lv.ToString();
        }
        
        /// <summary>
        /// 단순 경험치 게이지 증가 애니메이션
        /// </summary>
        /// <param name="proceedSets">경험치 구간 세트</param>
        /// <param name="levelUpCallback">레벨업 일어났을 때 callback</param>
        /// <param name="allFinishedCallback">모든 게이지 진행이 끝났을 때 callback</param>
        public void ProceedExpGauge(List<CustomUISlider.ProceedSet> proceedSets, CustomUISlider.SliderIsInitialized levelUpCallback = null, CustomUISlider.SliderProgressFinished allFinishedCallback = null) {
            SetUi(UIType.EXP, proceedSets[0].maxVal, proceedSets[0].from);
            
            _expUiSet
                .uiSlider
                .StartProceed(proceedSets, levelUpCallback, allFinishedCallback);
        }

        public void LevelUp() {
            int newLevel = 0;
            int.TryParse(heroLevel.text, out newLevel);
            newLevel += 1;
            if (newLevel != 0) heroLevel.text = newLevel.ToString();
        }

        public void Init(string heroName, int origHeroLv) {
            this.heroName.text = heroName;
            heroLevel.text = origHeroLv.ToString();
        }
    }

    public enum UIType {
        EXP,
        PIECE
    }

    [Serializable]
    public class BaseUISet {
        public GameObject obj;

        public virtual void Init(int maxVal, int currentVal, string name = null, int lv = -1) { }
    }

    [Serializable]
    public class PiecUISet : BaseUISet {
        public CustomUISlider uiSlider;
        public TextMeshProUGUI label;    //percentage 표시
        public Image portrait;

        public override void Init(int maxVal, int currentVal, string name = null, int lv = -1) {
            uiSlider._slider.maxValue = maxVal;
            uiSlider._slider.value = currentVal;

            if (uiSlider.isPercentage) {
                uiSlider.transform.Find("PercentageLabel").gameObject.SetActive(true);
                uiSlider.percentageValueLabel.gameObject.SetActive(true);
                uiSlider.percentageValueLabel.text = (100 * currentVal / maxVal) + "%";
            }
            else {
                uiSlider.transform.Find("ValueLabels").gameObject.SetActive(true);
                uiSlider.currentValueLabel.gameObject.SetActive(true);
                uiSlider.maxValueLabel.gameObject.SetActive(true);
                
                uiSlider.currentValueLabel.text = currentVal.ToString();
                uiSlider.maxValueLabel.text = maxVal.ToString();
            }
        }
    }

    [Serializable]
    public class ExpUISet : BaseUISet {
        public CustomUISlider uiSlider;
        
        public override void Init(int maxVal, int currentVal, string name = null, int lv = -1) {
            uiSlider._slider.maxValue = maxVal;
            uiSlider._slider.value = currentVal;
            
            if (uiSlider.isPercentage) {
                uiSlider.transform.Find("PercentageLabel").gameObject.SetActive(true);
                uiSlider.percentageValueLabel.gameObject.SetActive(true);
                uiSlider.percentageValueLabel.text = (100 * currentVal / maxVal) + "%";
            }
            else {
                uiSlider.transform.Find("ValueLabels").gameObject.SetActive(true);
                uiSlider.currentValueLabel.gameObject.SetActive(true);
                uiSlider.maxValueLabel.gameObject.SetActive(true);
                
                uiSlider.currentValueLabel.text = currentVal.ToString();
                uiSlider.maxValueLabel.text = maxVal.ToString();
            }
        }
    }
}
