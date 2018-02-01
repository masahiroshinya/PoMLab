using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;

public class Protocol : MonoBehaviour {

    // singleton
    public static Protocol instance;

    // protocol selection panel
    public GameObject protocolScrollView;
    public GameObject scrollbarContents;
    public GameObject fileSelectButtonPrefab;
    public GameObject selectedProtocolText;
    
    // variable used in ImportProtocol()
    string[] keyNameArray;

    public List<Dictionary<string, string>> protocol;
    public string protocolName;


    // Use this for initialization
    void Awake () {

        // singleton
        if (instance != null)
        {
            // 既に存在したら破棄
            Destroy(gameObject);
            Debug.Log("protocol destroyed");

        }
        else if (instance == null)
        {
            // singleton
            instance = this;

            // PersistentDataPathにプロトコールがない場合、TextAssetからコピー
            CopyProtocolToPersistentDataPath();

            protocolScrollView.SetActive(false);
            DontDestroyOnLoad(this);
            
        }

    }
	
	// Update is called once per frame
	void Update () {

        
	
	}
    


    // getter (called by ExperimentalMangager)
    public List<Dictionary<string, string>> GetProtocol()
    {
        return protocol;
    }
    public string GetProtocolName()
    {
        return protocolName;
    }
    

    // read protocol from csv file
    // called by FileSelectButton.onClick()
    public List<Dictionary<string, string>> ImportProtocol(string protocolFileName)
    {

        protocolName = protocolFileName.Remove(protocolFileName.Length - 4, 4);

        // initialize return
        protocol = new List<Dictionary<string, string>>();

        // read protocol csv file in PersistentDataPath\Protocol
        string protcolDirPath = Application.persistentDataPath + Path.DirectorySeparatorChar + "Protocols";
        StreamReader sr = new StreamReader(protcolDirPath + Path.DirectorySeparatorChar + protocolFileName);
        string protocolText = sr.ReadToEnd();
        sr.Close();

        Debug.Log("protocol imported");
        Debug.Log(protocolText);

        // split texts with new line （　改行文字で分割する　）
        string newLineString = System.Environment.NewLine;
        char[] newLineChar = newLineString.ToCharArray();   // string.Split() requires char[] to split　（　string.Split() の第一引数はchar[]　）
        string[] protocolTextArray = protocolText.Split(newLineChar, System.StringSplitOptions.RemoveEmptyEntries); // split text （　中身の無いエントリーが削除しつつ、テキストを分割　）

        // read parameters from the csv file
        string delimeterString = ",";
        char[] delimeterChar = delimeterString.ToCharArray();
        int nRow = protocolTextArray.Length;
        bool flag = false;
        // int rowProtocol = -1;

        for (int iRow = 0; iRow < nRow; ++iRow)
        {

            string protocolTextLine = protocolTextArray[iRow];

            if (protocolTextLine.IndexOf("Trial") == 0) // protocol key names
            {
                // add key names to the dectionary
                keyNameArray = protocolTextLine.Split(delimeterChar, System.StringSplitOptions.RemoveEmptyEntries);
                flag = true;
            }
            else if (flag) // protocol field values
            {

                // copy of default value
                // （　単に expPrm = default~ とすると、動作しない　） 
                //  (　Dictionary型は参照型？のため、複製するためにはnewしなければならない？　）
                Dictionary<string, string> defaultExpPrm = Config.instance.defExperimentalParameters;
                Dictionary<string, string> expPrm = new Dictionary<string, string>();
                foreach (KeyValuePair<string, string> pair in defaultExpPrm)
                {
                    expPrm.Add(pair.Key, pair.Value);
                }

                // overwrite key values
                string[] keyValueArray = protocolTextLine.Split(delimeterChar, System.StringSplitOptions.RemoveEmptyEntries);
                int nKey = keyNameArray.Length;
                for (int iKey = 0; iKey < nKey; ++iKey)
                {
                    string keyName = keyNameArray[iKey];
                    string keyValue = keyValueArray[iKey];
                    expPrm[keyName] = keyValue;
                }
                protocol.Add(expPrm);
            }
        }
        return protocol;
    }



    // called by this.Awake()
    void CopyProtocolToPersistentDataPath()
    {
        // copy protocols in Resources/Protocols to Application.persistentDataPath/Protocol
        //string srcDir = "Protocols"; // under Resources
        string dstDir = Application.persistentDataPath + Path.DirectorySeparatorChar + "Protocols";
        if (Directory.Exists(dstDir) == false) // create destination folder if it does not exist
        {
            Directory.CreateDirectory(dstDir);
        }

        // list of files under the folder
        TextAsset[] textAssetArray = Resources.LoadAll<TextAsset>("Protocols");
        // copy files
        foreach (TextAsset textAsset in textAssetArray)
        {
            string filename = textAsset.name;
            string dstPath = Application.persistentDataPath + Path.DirectorySeparatorChar + "Protocols" + Path.DirectorySeparatorChar + filename + ".csv";
            // copy protocol files, ignore meta file, do not overwrite
            if (filename.IndexOf(".meta") == -1 && File.Exists(dstPath) == false) // meta ファイル以外をコピー
            {
                Debug.Log("copy protocol: " + filename);
                string textContent = textAsset.text;
                File.WriteAllText(dstPath, textContent);

            };
        };

    }


}
