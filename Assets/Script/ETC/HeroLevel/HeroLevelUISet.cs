using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fbl_UIModule {
    public class HeroLevelUISet : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI heroName;
        [SerializeField] private Text heroLevel;
        [SerializeField] private CustomUISlider _customUiSlider;

        public void ProceedGauge(List<CustomUISlider.ProceedSet> proceedSets, CustomUISlider.SliderIsInitialized levelUpCallback = null, CustomUISlider.SliderProgressFinished allFinishedCallback = null) {
            _customUiSlider.StartProceed(proceedSets, levelUpCallback, allFinishedCallback);
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
}
