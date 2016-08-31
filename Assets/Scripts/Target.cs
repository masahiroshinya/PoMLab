using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Target : MonoBehaviour {
    

    // private properties
    private float targetSize;
    private Vector3 targetPosition;
    private float t0;                   // when target displayed
    private float targetActivationDelay;
    private float targetTimeout = -1;   // -1: never timeout
    private bool useVibration = false;
    // materials ( set in inspector )
    public Material tagetBeforeGoMaterial;
    public Material tagetAfterGoMaterial;

    public float t;

    

    // when target become visible
    void Start()
    {
        t0 = Time.time;
    }

	
	// Update is called once per frame
	void Update () {

        t = Time.time - t0;
        if (targetTimeout != -1 && t > targetTimeout)
        {
            Destroy(gameObject);
        }
        
	}


    // public parameter setter (　typically called by TrialManager.RunTrial()　)
    public void Initialize(Dictionary<string, string> expPrm)
    {
        // change position
        float targetDistance = float.Parse(expPrm["Target Distance"]);
        float targetDirection = float.Parse(expPrm["Target Direction"]);
        float targetX = targetDistance * Mathf.Cos(targetDirection * Mathf.Deg2Rad);
        float targetY = targetDistance * Mathf.Sin(targetDirection * Mathf.Deg2Rad);
        targetPosition = new Vector3(targetX, targetY, 0.0f); // z value to be 0 so that cursor can collide with it
        transform.position = targetPosition;

        // change size
        targetSize = float.Parse(expPrm["Target Size"]);
        transform.localScale = new Vector3(targetSize, targetSize, 0.1f);

        // timeout
        t0 = Time.time;
        targetTimeout = float.Parse(expPrm["Target Timeout"]);

        // vibration
        useVibration = bool.Parse(expPrm["Vibration"]);

    }
    

    //
    void PresentGoCue()
    {
        GetComponent<Renderer>().material = tagetAfterGoMaterial ;
    }

    // hit by cursor
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Cursor")
        {
            // vibration
            if (useVibration)
            {
                Handheld.Vibrate();
            }

            // destroy
            Destroy(gameObject);
            
        }
    }

    // target destroyed ( hit by cursor / timeout )
    void OnDestroy()
    {
        // trial manager
        GameObject trialManagerObj = GameObject.FindGameObjectWithTag("Trial Manager");
        TrialManager trialManager = trialManagerObj.GetComponent<TrialManager>();

        // animation (not implemented 2016-01-28)



        // time 
        trialManager.timeTargetDestroyed = Time.time;

        // send result to TrialManager
        trialManager.OnTargetDestroyed();

    }


}
