using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedTest : MonoBehaviour
{
    private Hashtable tweenArguments;
    private Vector3[] vector3s;
    private Transform thisTransform;
    public bool isLocal;

    public float percentage = 0f;
    public float runningTime = 0f;
    public float time = 10f;

    public bool isRunning = false;

    public float start = 0f;

    public Transform temper;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isRunning == true) {
            float a = easeInOutExpo(temper.position.x, 2f, percentage);
            temper.position = new Vector3(a, temper.position.y, temper.position.z);
            UpdatePercentage();


        }
        else {
            if (percentage > .5f) {
                percentage = 1f;
            }
            else {
                percentage = 0;
            }
        }
    }

    private float easeInOutExpo(float start, float end, float value) {
        value /= .5f;
        end -= start;
        if (value < 1) return end * 0.5f * Mathf.Pow(2, 10 * (value - 1)) + start;
        value--;
        return end * 0.5f * (-Mathf.Pow(2, -10 * value) + 2) + start;
    }
    
    private void UpdatePercentage() {
        //// Added by PressPlay   
        //if (useRealTime) {
        //    runningTime += (Time.realtimeSinceStartup - lastRealTime);
        //}
        //else {
        //    runningTime += Time.deltaTime;
        //}

        //if (reverse) {
        //    percentage = 1 - runningTime / time;
        //}
        //else {
        //    percentage = runningTime / time;
        //}

        //lastRealTime = Time.realtimeSinceStartup; // Added by PressPlay
        runningTime += Time.deltaTime;
        percentage = runningTime / time;
    }

}
