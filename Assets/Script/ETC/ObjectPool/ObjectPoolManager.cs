using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ObjectPool {
    public class ObjectPoolManager : MonoBehaviour {
        public delegate void OnPoolInitFinished();

        public ObjectPool objectPool;

        public void Initialize(OnPoolInitFinished callback) {
            StartCoroutine(InitObjectPool(callback));
        }

        IEnumerator InitObjectPool(OnPoolInitFinished callback) {
            objectPool.unusedList = new List<GameObject>();
            foreach(Transform prevObj in objectPool.content) {
                Destroy(prevObj.gameObject);
            }

            foreach (Transform item in objectPool.poolParent) {
                objectPool.unusedList.Add(item.gameObject);
                yield return new WaitForEndOfFrame();
            }

            objectPool.maxAmount = objectPool.unusedList.Count;
            callback();
        }

        /// <summary>
        /// Pool에서 가져오기
        /// </summary>
        /// <returns>가져오는 Object</returns>
        public GameObject GetObject() {
            if(objectPool.unusedList.Count > 0) {
                GameObject obj = objectPool.unusedList[0];
                objectPool.unusedList.RemoveAt(0);
                obj.transform.SetParent(objectPool.content);
                obj.transform.localScale = Vector3.one;
                obj.SetActive(true);
                return obj;
            }
            else {
                GameObject obj = Instantiate(objectPool.source);
                obj.transform.SetParent(objectPool.content);
                obj.name = objectPool.source.name;
                obj.transform.localScale = Vector3.one;
                obj.SetActive(true);
                return obj;
            }
        }

        /// <summary>
        /// 반납
        /// </summary>
        public void ReturnObject(GameObject obj) {
            objectPool.unusedList.Add(obj);
            obj.SetActive(false);

            PoolOptimize();
        }

        private void PoolOptimize() {
            var maxAmount = objectPool.maxAmount;

            if(objectPool.unusedList.Count > maxAmount) {
                int overNumber = objectPool.unusedList.Count - maxAmount;

                for(int i=0; i<overNumber; i++) {
                    var selectedObject = objectPool.unusedList[0];
                    objectPool.unusedList.Remove(selectedObject);
                    Destroy(selectedObject);
                }
            }
        }
    }

    [System.Serializable]
    public class ObjectPool {
        public GameObject source;   //원본 프리팹 (pool이 모자를 때 충원용)
        public Transform 
            poolParent,             //pool이 위치할 곳
            content;                //실제 pool을 가져와서 사용되는 곳
        public int maxAmount;       //pool 최적화를 위한 기준 허용치
        public List<GameObject> unusedList = new List<GameObject>();
    }
}