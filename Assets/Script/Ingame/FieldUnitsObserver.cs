using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldUnitsObserver : SerializedMonoBehaviour {
    [TableMatrix(SquareCells = true)] public GameObject[,] orcUnits = new GameObject[5, 2];
    [TableMatrix(SquareCells = true)] public GameObject[,] humanUnits = new GameObject[5, 2];

    public void Initialize() { }

    public void UnitAdded(GameObject target, Pos pos, bool isHuman) {
        if (isHuman) humanUnits[pos.col, pos.row] = target;
        else orcUnits[pos.col, pos.row] = target;


        //if (ScenarioGameManagment.scenarioInstance != null && ScenarioGameManagment.scenarioInstance.isTutorial == true && target.GetComponent<PlaceMonster>().isPlayer == true)
            //PlayMangement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.UNIT_SUMMONED, this, target.GetComponent<PlaceMonster>().unit.id);
    }

    public void UnitRemoved(Pos pos, bool isHuman) {
        if (isHuman) humanUnits[pos.col, pos.row] = null;
        else orcUnits[pos.col, pos.row] = null;
    }

    //TODO : 적이 호출한지, 내가 호출한지 구분해야함
    public virtual void UnitChangePosition(GameObject target, Pos pos, bool isPlayer, string cardID = "") {
        Logger.Log("Col : " + pos.col);
        Logger.Log("Row : " + pos.row);

        Pos prevPos = GetMyPos(target);
        bool isHuman = target.GetComponent<PlaceMonster>().unit.ishuman;
        //target.GetComponent<PlaceMonster>().unitSpine.gameObject.GetComponent<Spine.Unity.SkeletonAnimation>().enabled = false;
        //target.GetComponent<PlaceMonster>().unitSpine.gameObject.SetActive(false);

        if (isHuman) humanUnits[pos.col, pos.row] = target;
        else orcUnits[pos.col, pos.row] = target;

        Transform slotParent = null;
        if (isPlayer) slotParent = PlayMangement.instance.player.transform;
        else slotParent = PlayMangement.instance.enemyPlayer.transform;

        Transform parent = slotParent.GetChild(pos.row).GetChild(pos.col);
        Vector2 targetPos = parent.position;


        //if (PlayMangement.instance.magicHistroy == "") {
        //    iTween.MoveTo(
        //        target,
        //        new Vector2(targetPos.x, targetPos.y),
        //        1.0f
        //    );
        //}

        if (isHuman) humanUnits[prevPos.col, prevPos.row] = null;
        else orcUnits[prevPos.col, prevPos.row] = null;

        StartCoroutine(UnitChangeCoroutine(target, prevPos, pos, parent, cardID));
    }

    public bool IsUnitExist(Pos targetPos, bool isHuman) {
        bool check = false;
        if (isHuman) check = humanUnits[targetPos.col, targetPos.row] != null;
        else check = orcUnits[targetPos.col, targetPos.row] != null;

        return check;
    }

    public int CheckLineEmptyCount(int col, bool isHuman) {
        int count = 0;

        GameObject[,] units = null;
        if (isHuman) units = humanUnits;
        else units = orcUnits;
        for(int i = 0; i < 5; i++) {
            if (units[col, i] == null)
                count++;
        }
        return count;
    }

    /// <summary>
    /// 자리 이동
    /// </summary>
    /// <param name="target">대상</param>
    /// <param name="prevPos">이전 위치</param>
    /// <param name="row">새로운 위치 row</param>
    /// <param name="col">새로운 위치 col</param>
    /// <returns></returns>
    IEnumerator UnitChangeCoroutine(GameObject target, Pos prevPos, Pos newPos, Transform parent, string useCardID = "") {
        //yield return new WaitForSeconds(1.0f);
        yield return null;
        target.transform.SetParent(parent);
        //target.transform.localPosition = Vector3.zero;

        target.GetComponent<PlaceMonster>().ChangePosition(
            newPos.col,
            newPos.row,
            parent.position,
            useCardID
        );

        Logger.Log(string.Format("prev Pos Col : {0}",prevPos.col));
        Logger.Log(string.Format("prev Pos Row : {0}", prevPos.row));

        PlayMangement.instance.EventHandler.PostNotification(IngameEventHandler.EVENT_TYPE.FIELD_CHANGED, null, null);
    }

    /// <summary>
    /// 필드에 존재하는 모든 유닛(적 포함)
    /// </summary>
    /// <returns></returns>
    public List<GameObject> GetAllFieldUnits() {
        List<GameObject> result = new List<GameObject>();
        foreach(GameObject unit in humanUnits) {
            if (unit != null) result.Add(unit);
        }
        foreach(GameObject unit in orcUnits) {
            if (unit != null) result.Add(unit);
        }
        return result;
    }

    public List<GameObject> GetAllFieldUnits(bool isHuman) {
        GameObject[,] units = null;
        if (isHuman) units = humanUnits;
        else units = orcUnits;

        List<GameObject> result = new List<GameObject>();
        foreach (GameObject unit in units) {
            if (unit != null) result.Add(unit);
        }
        return result;
    }

    /// <summary>
    /// 한쪽 진영의 Line에 존재하는 모든 유닛
    /// </summary>
    /// <param name="col">Column</param>
    /// <returns></returns>
    public List<GameObject> GetAllFieldUnits(int col, bool isHuman) {
        List<GameObject> result = new List<GameObject>();
        GameObject[,] units = null;
        if (isHuman) units = humanUnits;
        else units = orcUnits;

        for (int i = 0; i < 2; i++) {
            if (units[col, i] != null) {
                result.Add(units[col, i]);
            }
        }
        //Debug.Log("라인 검사 결과 : " + _units.Count);
        return result;
    }

    /// <summary>
    /// 한 Line에 존재하는 모든 유닛(적 포함)
    /// </summary>
    /// <param name="col">Column</param>
    /// <returns></returns>
    public List<GameObject> GetAllFieldUnits(int col) {
        List<GameObject> result = new List<GameObject>();
        for (int i = 0; i < 2; i++) {
            if (humanUnits[col, i] != null) {
                result.Add(humanUnits[col, i]);
            }
            if(orcUnits[col, i] != null) {
                result.Add(orcUnits[col, i]);
            }
        }
        //Debug.Log("라인 검사 결과 : " + _units.Count);
        return result;
    }

    public GameObject GetUnit(Pos pos, bool isHuman) {
        GameObject[,] units = null;
        if (isHuman) units = humanUnits;
        else units = orcUnits;

        return units[pos.col, pos.row];
    }

    public Transform GetSlot(Pos pos, bool isPlayer) {
        PlayerController controller;
        if (isPlayer) controller = PlayMangement.instance.player;
        else controller = PlayMangement.instance.enemyPlayer;

        return controller
            .transform
            .GetChild(pos.row)
            .GetChild(pos.col)
            .transform;
    }

    public virtual Pos GetMyPos(GameObject gameObject) {
        Pos pos = new Pos();
        pos.col = gameObject.GetComponent<PlaceMonster>().x;
        pos.row = gameObject.GetComponent<PlaceMonster>().y;

        return pos;
    }

    public void RefreshFields(Transform[][] arr, bool isHuman) {
        int colCnt = 0;
        GameObject[,] units = null;
        if (isHuman) units = humanUnits;
        else units = orcUnits;

        foreach(Transform[] col in arr) {
            int rowCnt = 0;
            foreach(Transform row in col) {
                if(row.transform.childCount != 0) {
                    units[colCnt, rowCnt] = row.transform.GetChild(0).gameObject;
                }
                else {
                    units[colCnt, rowCnt] = null;
                }
                rowCnt++;
            }
            colCnt++;
        }
    }

    public struct Pos {
        public int col;
        public int row;

        public Pos(int col, int row) {
            this.col = col;
            this.row = row;
        }
    }
}
