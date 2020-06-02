using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomUISlider : MonoBehaviour {
    [SerializeField] private Slider _slider;
    [SerializeField] private TextMeshProUGUI currentValueLabel, maxValueLabel;
    
    public delegate void SliderIsInitialized();
    public delegate void SliderProgressFinished();
    private SliderIsInitialized _sliderIsInitialized;
    private SliderProgressFinished _finished;

    private void Start() {
        //Test();
    }

    public void Test() {
        __Case1Test();
    }

    private void __Case1Test() {
        List<ProceedSet> proceedSets = new List<ProceedSet>();
        proceedSets.Add(new ProceedSet(0, 100, 20, 100));
        proceedSets.Add(new ProceedSet(0, 10, 20, 100));
        StartProceed(proceedSets);
    }
    
    public void StartProceed(object data, SliderIsInitialized callback1 = null, SliderProgressFinished callback2 = null) {
        StartCoroutine(__MainProceed(data, callback1, callback2));
    }

    IEnumerator __MainProceed(object data, SliderIsInitialized callback1 = null, SliderProgressFinished callback2 = null) {
        if (data.GetType() == typeof(List<ProceedSet>)) {
            yield return __ProceedSetProgess((List<ProceedSet>)data, callback1, callback2);
        }
        else if (data.GetType() == typeof(List<MMR_proceedSet>)) {
            
        }
    }

    IEnumerator __ProceedSetProgess(List<ProceedSet> sets, SliderIsInitialized callback = null, SliderProgressFinished callback2 = null) {
        foreach (var set in sets) {
            _slider.maxValue = set.maxVal;
            maxValueLabel.text = _slider.maxValue.ToString();
            
            _slider.value = set.from;
            
            float perAmount = 1;
            while (_slider.value < set.to) {
                _slider.value += 1;
                currentValueLabel.text = _slider.value.ToString();
                
                yield return new WaitForSeconds(1 / (set.speed * 5));
            }
            callback?.Invoke();
        }
        callback2?.Invoke();
        yield return null;
    }

    public class ProceedSet {
        public int maxVal;
        public int from;
        public int to;
        public float speed;        //1 ~ 100 상대적 속도

        public ProceedSet(int from, int to, float speed, int maxVal) {
            this.maxVal = maxVal;
            this.from = from;
            this.to = to;
            if (speed <= 0) this.speed = 1;
            else if (speed > 100) this.speed = 100;
            else this.speed = speed;
        }
    }

    public class MMR_proceedSet : ProceedSet {
        public int minVal;
        public int maxVal;

        public MMR_proceedSet(int from, int to, float speed, int minVal, int maxVal) : base(from, to, speed, maxVal) {
            this.maxVal = maxVal;
        }
    }
}