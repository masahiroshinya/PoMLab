using UnityEngine;
using System.Collections;

// for List<T> （　List型を使うため　）
using System.Collections.Generic;

// for Select
using UnityEngine.SceneManagement;
using System.IO;

public class ExperimentManager : MonoBehaviour {

    // prefab （　specified in inspector　）
    public GameObject trialManagerPrefab;
    
    // reference
    // private Config config;
    private GameObject trialManagerObj ;
    private TrialManager trialManager;

    // button
    private GameObject startButton;
    private GameObject backButton;
    private GameObject plotButton;

    // protocol
    public List<Dictionary<string, string>> protocol;    // list of experimental parameters

    // result
    public List<Dictionary<string, string>> resultList; // list of results

    // trial number
    private int nTrial; // number of trials in the session
    public int iTrial; // current trial number (starts from 1)
    private string dirName;
    private bool readyForNextTrial;


    // Use this for initialization
    void Awake()
    {

        startButton = GameObject.Find("TapToStartButton");
        backButton = GameObject.Find("TapToBackButton");
        plotButton = GameObject.Find("TapToPlotButton");

        // deactivate back button
        backButton.SetActive(false);

        // deactivate plot button
        if (Config.instance.hasPaidAssets)
        {
            plotButton.SetActive(false);
        }
        else
        {
            Destroy(plotButton);
        }
    }


    // run 1 session according to protocol
    // （　プロトコールファイルを読み取り、それに従い1セッションの実験を行う　）
    // （　tapToStartボタンクリック時に呼び出される　）
    public void RunExperimentalSession()
    {
        
        // configuration setting
        GameObject.Find("Tilt Manager").GetComponent<TiltManager>().SetConfiguration();
        
        // import protocol
        protocol = Protocol.instance.GetProtocol();
        string protocolName = Protocol.instance.GetProtocolName();
        nTrial = protocol.Count;

        // create data folder
        // filename
        dirName = Config.instance.subjectName + "_" + protocolName + "_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string dataDir = Application.persistentDataPath + "/Data/" + dirName;
        if (Directory.Exists(dataDir) == false) // create destination folder if it does
        {
            Directory.CreateDirectory(dataDir);
        }

        // initialize resultList
        resultList = new List<Dictionary<string, string>>();

        // deactivate start button
        startButton.SetActive(false);

        // make it ready
        iTrial = 1;
        readyForNextTrial = true;

    }

    // Update is called once per frame
    void Update () {

        if (readyForNextTrial)
        {
            Run1Trial();
        }
	
	}


    // TrialManagerをInstantiateして、1試行を行う
    void Run1Trial()
    {
        // trial manager
        trialManagerObj = (GameObject)Instantiate(trialManagerPrefab, Vector3.zero, Quaternion.identity);
        trialManager = trialManagerObj.GetComponent<TrialManager>();
        trialManager.SetExpeimentalParameters(protocol[iTrial - 1]);
        trialManager.RunTrial();

        // Debug.Log(protocol[iTrial - 1]["Target Direction"]);

        readyForNextTrial = false;
    }
    
    
    // end of 1 trial (　called by TrialManager　)
    public void OnTrialFinished()
    {
        // go to next trial or finish session
        iTrial++;
        if (iTrial <= nTrial)
        {
            readyForNextTrial = true;
        }
        else
        {
            EndOfSession();
        }
    }

    // handling results (　called by TrialManager　)
    public void AddResultToList(Dictionary<string, string> resultOfTrial)
    {
        // initialize result for 1 trial
        Dictionary<string, string> result = new Dictionary<string, string>();
        // add trial # etc.
        result.Add("Trial Number", iTrial.ToString());
        result.Add("Acc Data File Name", GetAccDataFileName());
        result.Add("Cursor Data File Name", GetCursorDataFileName());
        // add result calculated by trialManager.analyze()
        foreach (KeyValuePair<string, string> pair in resultOfTrial)
        {
            result.Add(pair.Key, pair.Value);
        }
        // add result to resultList
        resultList.Add(result);
    }


    // end of session
    void EndOfSession()
    {
        // save protocol & results
        // string dataDir = Application.persistentDataPath + "/Data/" + dirName;
        // string path = dataDir + "/" + "used protocol.csv";
        saveDictionaryList(getProtocolPath(), protocol);
        saveDictionaryList(getSummaryPath(), resultList);

        // activate finish button
        backButton.SetActive(true);
        plotButton.SetActive(true);
    }


    // save protocol/resultList
    private void saveDictionaryList(string path, List<Dictionary<string, string>> dataList)
    {
        if (dataList.Count >= 1)
        {
            // streamwriter
            StreamWriter sw = new StreamWriter(path, false);

            // 1st row: key names (all elements of the dataList should have same key)
            string header = "";
            Dictionary<string, string> data1 = dataList[0];
            foreach(KeyValuePair<string, string> pair in data1)
            {
                header = header + pair.Key + ",";
            }
            header.Remove(header.Length - 1, 1);
            sw.WriteLine(header);

            // 2nd~ row: key values
            string record;
            foreach(Dictionary<string, string> data in dataList)
            {
                record = "";
                foreach (KeyValuePair<string, string> pair in data)
                {
                    record = record+ pair.Value + ",";
                }
                record.Remove(record.Length - 1, 1);
                sw.WriteLine(record);
            }

            sw.Flush();
            sw.Close();

        }
    }


    
    // OnClick event
    public void BackToTitleScene()
    {
        
        Destroy(GameObject.Find("Protocol"));
        SceneManager.LoadScene("PoMLab Title");

    }

    // path for acceleration data
    public string GetAccDataPath()
    {
        string dataDir = Application.persistentDataPath + "/Data/" + dirName;
        string path = dataDir + "/" + GetAccDataFileName();
        return path;
    }
    private string GetAccDataFileName()
    {
        string accDataFilename = "AccData_" + dirName + "_T" + iTrial.ToString("D3") + ".csv";
        return accDataFilename;
    }
    // path for cursor data
    public string GetCursorDataPath()
    {
        string dataDir = Application.persistentDataPath + "/Data/" + dirName;
        string path = dataDir + "/" + GetCursorDataFileName();
        return path;
    }
    private string GetCursorDataFileName()
    {
        string cursorDataFilename = "CursorData_" + dirName + "_T" + iTrial.ToString("D3") + ".csv";
        return cursorDataFilename;
    }
    // path for summary
    private string getSummaryPath()
    {
        string dataDir = Application.persistentDataPath + "/Data/" + dirName;
        string summaryPath = dataDir + Path.DirectorySeparatorChar + "summary_" + dirName + ".csv";
        return summaryPath;
    }
    private string getProtocolPath()
    {
        string dataDir = Application.persistentDataPath + "/Data/" + dirName;
        string summaryPath = dataDir + Path.DirectorySeparatorChar + "protocol_" + dirName + ".csv";
        return summaryPath;
    }





}
