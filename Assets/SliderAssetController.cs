using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class SliderAssetController : MonoBehaviour
{
    public bool filled;
    public bool textOn;

    public delegate void Callback();
    private Callback callback = null;

    Transform target;
    TMPro.TextMeshProUGUI text;
    float startPos;
    float width;

    // Start is called before the first frame update
    void Awake() {
        target = transform.Find("Slider/Value");
        text = transform.Find("Slider/ValueText").GetComponent<TMPro.TextMeshProUGUI>();
        width = target.GetComponent<RectTransform>().rect.width;
        if (!filled) {
            target.GetComponent<Image>().type = Image.Type.Sliced;
            startPos = (width * -0.5f) * 3;
        }
        else {
            target.GetComponent<Image>().type = Image.Type.Filled;
        }
    }

    void Start() {
        //Test();
    }

    public void SetSliderAmount(int amount, int max) {
        if (!filled) {
            target.localPosition = new Vector3(startPos + (width * ((float)amount / (float)max)), 0, 0);
        }
        else {
            target.GetComponent<Image>().fillAmount = (float)amount / (float)max;
        }

        text.gameObject.SetActive(textOn);
        if (textOn) {
            text.text = amount.ToString() + "/" + max.ToString();
        }
        if (amount == max) callback?.Invoke();
    }

    public void SetCallBack(Callback call) {
        callback = call;
    }

    public async void Test() {
        await Task.Delay(3000);
        var result = SliceProceed(currentValue: 780, increaseAmnt: 1000, AccountManager.Instance.rankTable);
    }

    /// <summary>
    /// 게이지 진행 분할(MMR 등급)
    /// </summary>
    /// <param name="increaseAmnt">증가량</param>
    /// <param name="resultValue">최종결과값</param>
    /// <returns></returns>
    public List<ProceedSet> SliceProceed(int currentValue, int increaseAmnt, List<AccountManager.RankTableRow> table) {
        List<ProceedSet> sets = new List<ProceedSet>();
        int resultValue = currentValue + increaseAmnt;

        var startRow = table
            .Find(
                x =>
                    x.pointOverThen <= currentValue
                    && x.pointLessThen >= currentValue
            );

        var middleRows = table
            .FindAll(
                x =>
                    x.pointOverThen >= currentValue
                    && x.pointLessThen <= resultValue
            );

        var endRow = table
            .Find(
                x => 
                    x.pointOverThen < resultValue 
                    && x.pointLessThen > resultValue
            );

        List<AccountManager.RankTableRow> selectedRows = new List<AccountManager.RankTableRow>();
        selectedRows.Add(startRow);
        selectedRows.AddRange(middleRows);
        selectedRows.Add(endRow);
        selectedRows = selectedRows.OrderBy(x => x.pointOverThen).ToList();

        var first = selectedRows.First();
        var last = selectedRows.Last();

        foreach(AccountManager.RankTableRow row in selectedRows) {
            if(row == first) {
                sets.Add(
                    new ProceedSet(
                        currentValue,
                        row.pointLessThen,
                        row.pointOverThen,
                        row.pointLessThen
                    ));
            }
            else if(row == last) {
                sets.Add(
                    new ProceedSet(
                        0, 
                        resultValue,
                        row.pointOverThen, 
                        row.pointLessThen
                    ));
            }
            else {
                sets.Add(
                    new ProceedSet(
                        row.pointLessThen, 
                        row.pointOverThen, 
                        row.pointOverThen, 
                        row.pointLessThen
                    ));
            }
        }
        sets = sets.OrderBy(x => x.from).ToList();
        return sets;
    }

    public class ProceedSet {
        public int? minVal;
        public int? maxVal;
        
        public int? from;
        public int? to;

        public ProceedSet(int? from, int? to, int? minVal, int? maxVal) {
            this.from = from;
            this.to = to;

            this.minVal = minVal;
            this.maxVal = maxVal;
        }
    }
}

