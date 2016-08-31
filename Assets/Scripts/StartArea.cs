using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StartArea : MonoBehaviour {

    // private properties
    private float startAreaSize;
    private float startAreaPositionX;
    private float startAreaPositionY;
    private float t0;
    private float timeOnStartArea;
    private float startAreaDuration;
    private float recordingTimeBeforeTarget;
    // materials ( set in inspector )
    public Material startAreaMaterial;
    public Material startAreaOffCursorMaterial;
    public Material startAreaOnCursorMaterial;
    
    
    // public parameter setter (　typically called by TrialManager.RunTrial()　)
    public void SetStartAreaParameters(Dictionary<string, string> expPrm)
    {
        // change location of the start area
        startAreaPositionX = float.Parse(expPrm["Start Area Position X"]);
        startAreaPositionY = float.Parse(expPrm["Start Area Position Y"]);
        Vector3 startAreaPosition = new Vector3(startAreaPositionX, startAreaPositionY, 1); // z value should be 1 so that start area is behind the cursor
        transform.position = startAreaPosition;

        // change size of the start area
        startAreaSize = float.Parse(expPrm["Start Area Size"]);
        transform.localScale = new Vector3(startAreaSize, startAreaSize, 0.1f);

        // start area duration & recording delay
        startAreaDuration = float.Parse(expPrm["Start Area Duration"]);
        recordingTimeBeforeTarget = float.Parse(expPrm["Recording Time Before Target"]);

    }

    // public getter (　typically called by TrialManager.RunTrial()　)
    public bool ReadyForRecording()
    {
        return timeOnStartArea > startAreaDuration;
    }
    public bool ReadyForTarget()
    {
        return timeOnStartArea > startAreaDuration + recordingTimeBeforeTarget;
    }


    // change color (material) if cursors on/off
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Cursor")
        {
            GetComponent<Renderer>().material = startAreaOnCursorMaterial;
            t0 = Time.time;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Cursor")
        {
            GetComponent<Renderer>().material = startAreaOffCursorMaterial;
            timeOnStartArea = 0;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Cursor")
        {
            timeOnStartArea = Time.time - t0 ;
        }
    }

}
