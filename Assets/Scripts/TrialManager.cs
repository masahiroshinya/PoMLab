using UnityEngine;
using System.Collections;

// for List<T> （　List型を使うため　）
using System.Collections.Generic;

using System.IO;

public class TrialManager : MonoBehaviour {

    // prefab （　specified in inspector　）
    public GameObject startAreaPrefab;
    public GameObject targetPrefab;
    // reference to other objects
    private GameObject experimentManagerObj;
    private ExperimentManager experimentManager;
    private GameObject tiltManagerObj;
    private TiltManager tiltManager;
    private GameObject cursorObj;
    private Cursor cursor;
    private GameObject startAreaObj;
    private StartArea startArea;
    private GameObject targetObj;
    private Target target;

    // configuration & experimental parameters
    public Dictionary<string, string> configParameters;
    public Dictionary<string, string> experimentalParameters;

    // state constant （　ステートマシンのためのステート　）
    public int state = 0;
    private int stateIntialization = 0;
    private int stateStartAreaDisplayed = 1;
    private int stateRecordingStarted = 2;
    private int stateTargetDisplayed = 3;
    private int stateEndOfTrial = 5;
    private int stateOther = -1;

    //
    public float timeTargetDestroyed;
    private float timeToNextTrial;

    // result
    private List<Vector3> accRecord;
    private Dictionary<string, string> resultOfTrial; 

    // parameters (set as public just for debug)
    public Vector3 startAreaPosition;
    public float startAreaDuration;



	// Use this for initialization
	void Start () {
        
        // reference to cursor
        cursorObj = GameObject.Find("Cursor");
        cursor = cursorObj.GetComponent<Cursor>();

    }

    // Update is called once per frame
    void Update () {

        // state machine （　ステートマシンとして実装　）
        if (state == stateStartAreaDisplayed)
        {
            if (startArea.ReadyForRecording())
            {
                // set cursor parameter (config)
                cursor.SetCursorParameters(experimentalParameters);

                // 計測開始
                tiltManager.startRecording();
                cursor.startRecording();
                state = stateRecordingStarted;
            }
        }
        else if (state == stateRecordingStarted)
        {
            if (startArea.ReadyForTarget())
            {
                // activate target
                targetObj.SetActive(true);
                state = stateTargetDisplayed;
            }
        }
        else if (state == stateEndOfTrial)
        {
            
            if (Time.time - timeTargetDestroyed > timeToNextTrial)
            {
                
                // state
                state = stateOther;

                // destroy
                Destroy(startAreaObj);
                Destroy(gameObject);

                // send to ExperimentMangager
                experimentManager.OnTrialFinished();

            }

        }
        else
        {

        }

    }


    // run
    public void RunTrial()
    {
        // initialization
        // reference to ExperimentManager
        experimentManagerObj = GameObject.Find("Experiment Manager");
        experimentManager = experimentManagerObj.GetComponent<ExperimentManager>();
        // reference to TiltManager
        tiltManagerObj = GameObject.Find("Tilt Manager");
        tiltManager = tiltManagerObj.GetComponent<TiltManager>();
        // prepare start area
        startAreaObj = (GameObject)Instantiate(startAreaPrefab, Vector3.zero, Quaternion.identity);
        startArea = startAreaObj.GetComponent<StartArea>();
        startArea.SetStartAreaParameters(experimentalParameters); // 初期位置のサイズを設定・変更
        // prepare target
        targetObj = (GameObject)Instantiate(targetPrefab, Vector3.zero, Quaternion.identity);
        target = targetObj.GetComponent<Target>();
        target.Initialize(experimentalParameters); // 初期位置のサイズを設定・変更

        // set cursor parameter in 1st trial
        if (experimentManager.iTrial == 1)
        {
            cursorObj = GameObject.Find("Cursor");
            cursor = cursorObj.GetComponent<Cursor>();
            cursor.SetCursorParameters(experimentalParameters);
        }

        // activate start area
        startAreaObj.SetActive(true);

        // next state
        state = stateStartAreaDisplayed;

    }


    // setter (typically, called by ExperimentalManager)
    public void SetExpeimentalParameters(Dictionary<string, string> newPrm)
    {
        experimentalParameters = newPrm;
    }

    // taget destroyed (　called by Target.OnDestroy()　)
    public void OnTargetDestroyed(bool isTimeout)
    {

        // data analysis (result to be sent to ExperimentManager)
        analyze();

        // show & update score
        float score = float.Parse(experimentalParameters["Score"]);
        experimentManager.UpdateScore(isTimeout, score);

        /*
        if (isTimeout)
        {
            experimentManager.lastScore = 0;
        }
        else
        {
            experimentManager.lastScore = score;
            experimentManager.totalScore += score;
        }
        experimentManager.scoreText.text = "Total Score: " + experimentManager.totalScore;
        */

        // save acc data after n-sec delay
        float waitTime = float.Parse(experimentalParameters["Recording Time After Target Destruction"]);
        StartCoroutine(saveAccData(waitTime));

        // time to next trial
        timeToNextTrial = float.Parse(experimentalParameters["Time To Next Trial"]);

        // next state
        state = stateEndOfTrial;
    }


    // wait n sec & stop recording & get acc data from tilt manager
    private IEnumerator saveAccData(float waitTime)
    {
        // wait
        // Debug.Log("wait: " + waitTime);
        yield return new WaitForSeconds(waitTime);

        // stop recording 
        tiltManager.stopRecording();
        cursor.stopRecording();

        // get acc data from tilt manager
        List<float> timeStampRecord = tiltManager.getTimeStampRecord();
        List<Vector3> rawAccRecord = tiltManager.getRawAccRecord();
        List<Vector3> filtAccRecord = tiltManager.getFiltAccRecord();
        
        // get cursor data from cursor
        List<float> cursorTimeStampRecord = cursor.getTimeStampRecord();
        List<Vector3> cursorPositionRecord = cursor.getCursorPositionRecord();
        
        // save acc data
        string path = experimentManager.GetAccDataPath();
        StreamWriter sw = new StreamWriter(path, false);
        string header = "header sample \n" +
                        "time stamp, raw acc x, raw acc y, raw acc z, filt acc x, filt acc y, filt acc z";
        sw.WriteLine(header);
        int nRecord = timeStampRecord.Count;
        for (int iRecord = 0; iRecord < nRecord; iRecord++)
        {
            string record = timeStampRecord[iRecord] + "," +
                            rawAccRecord[iRecord].x + "," + rawAccRecord[iRecord].y + "," + rawAccRecord[iRecord].z + "," +
                            filtAccRecord[iRecord].x + "," + filtAccRecord[iRecord].y + "," + filtAccRecord[iRecord].z;
            sw.WriteLine(record);
        }
        sw.Flush();
        sw.Close();

        
        // save cursor position
        path = experimentManager.GetCursorDataPath();
        sw = new StreamWriter(path, false);
        header = "header sample \n" +
                 "time stamp, cursor x, cursor y";
        sw.WriteLine(header);
        nRecord = cursorTimeStampRecord.Count;
        for (int iRecord = 0; iRecord < nRecord; iRecord++)
        {
            string record = cursorTimeStampRecord[iRecord] + "," +
                            cursorPositionRecord[iRecord].x + "," + cursorPositionRecord[iRecord].y ;
            sw.WriteLine(record);
        }
        sw.Flush();
        sw.Close();

    }


    // Data analysis (called in OnTargetDestroyed)
    private void analyze()
    {

        // get filtered acceleration (from start to when target destroyed)
        List<Vector3> accData = tiltManager.filtAccRecord;

        // on-device analysis (not implemented 2016-01-28)
        float targetDirectionRad = float.Parse(experimentalParameters["Target Direction"]) * Mathf.Deg2Rad;
        Vector3 unitVecToTarget = new Vector3(Mathf.Cos(targetDirectionRad), Mathf.Sin(targetDirectionRad), 0f);
        int tMaxDev = 0;
        float maxDev = 0f;
        int tMaxVel = 0;
        float maxVel = 0f;

        int nRecord = accData.Count;
        float[] deviationFromStraight = new float[nRecord];
        float[] angularDeviation = new float[nRecord];
        float[] playerVelocity = new float[nRecord];

        for (int iRecord = 0; iRecord < nRecord; iRecord++)
        {
            // hi-freq (200Hz for Nexus, same as acceleration) cursor data (not presented to subjects) from filtered acceleration
            Vector3 playerPos = cursor.Acc2CursorPosition(accData[iRecord]);

            // angular deviation
            float xx = Mathf.Cos(-targetDirectionRad) * playerPos.x - Mathf.Sin(-targetDirectionRad) * playerPos.y;
            float yy = Mathf.Sin(-targetDirectionRad) * playerPos.x + Mathf.Cos(-targetDirectionRad) * playerPos.y;
            angularDeviation[iRecord] = Mathf.Atan2(yy, xx) * Mathf.Rad2Deg;

            // deviation from a straight line to the target
            Vector3 projectionVec = Vector3.Project(unitVecToTarget, playerPos);
            Vector3 orthogonalVec = playerPos - projectionVec;
            float dev = orthogonalVec.magnitude;
            deviationFromStraight[iRecord] = dev;
            if (dev > maxDev)
            {
                tMaxDev = iRecord;
                maxDev = dev;
            }

            // player velocity
            // Debug.Log("index: " + iRecord);
            float vel;
            if (iRecord == 0)
            {
                vel = 0f;
            }
            else
            {
                Vector3 prevPlayerPos = cursor.Acc2CursorPosition(accData[iRecord-1]);
                vel = (playerPos - prevPlayerPos).magnitude / float.Parse(Config.instance.configParameters["Sampling Interval"]); ;
            }
            playerVelocity[iRecord] = vel;
            
            if (vel > maxVel)
            {
                tMaxVel = iRecord;
                maxVel = vel;
            }

        }

        // maximum deviation
        maxDev = maxDev * Mathf.Sign(angularDeviation[tMaxDev]);
        float angDev = angularDeviation[tMaxVel];

        // result dictionary
        Dictionary<string, string> result = new Dictionary<string, string>();
        result.Add("tMaxDevFromStraight", tMaxDev.ToString("N0"));
        result.Add("MaxDevFromStraight", maxDev.ToString("N3")) ;
        result.Add("tMaxVel", tMaxVel.ToString("N0"));
        result.Add("MaxAngDevAtTMaxVel", angDev.ToString("N3"));

        // output
        resultOfTrial = result;
        experimentManager.AddResultToList(result);

    }




}
