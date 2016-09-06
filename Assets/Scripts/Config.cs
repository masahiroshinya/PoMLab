using UnityEngine;
using System.Collections;

// for streamreader&writer
using System.IO;
// for List<T>, Dictionary （　List型を使うため　）
using System.Collections.Generic;

public class Config : MonoBehaviour {

    // singleton
    public static Config instance;

    // configuration & experimental parameters
    public Dictionary<string, string> configParameters;
    public Dictionary<string, string> defExperimentalParameters ;

    // other parameters
    public string subjectName;
    public bool hasPaidAssets = false; // boxit & graphmaker
	


    // initialization
    void Awake () {
        
		// singleton
		if (instance != null) {
            // 既に存在したら破棄
			Destroy (gameObject);
            Debug.Log("config destroyed");

		} else if (instance == null) {

			instance = this;
			// set tag
			// gameObject.tag = "Config";

			// config should not be destroyed
			DontDestroyOnLoad(this);

			// import default config if there's no config.csv file
			importDefaultConfig();

			// create data folder if it does not exist
			createDataFolder();

			// load config
			loadConfig();
		}

        // fix the orientation of the screen （ スクリーンの向きをportraitに固定　）
        Screen.orientation = ScreenOrientation.Portrait;
        

    }

    void Start()
    {
        

    }


    // import defalt config values
    void importDefaultConfig()
    {
        // copy "default config.csv" in Resources to "config.csv" in Application.persistentDataPath
        // if "config.csv" doesn't exist
        string dstPath = Application.persistentDataPath + Path.DirectorySeparatorChar + "config.csv";
        if (File.Exists(dstPath) == false)
        {
            TextAsset textAsset = Resources.Load("default config") as TextAsset;
            string textContent = textAsset.text;
            File.WriteAllText(dstPath, textContent);
        };

        // if "default config.csv" doesn't exist
        dstPath = Application.persistentDataPath + Path.DirectorySeparatorChar + "default config.csv";
        if (File.Exists(dstPath) == false)
        {
            TextAsset textAsset = Resources.Load("default config") as TextAsset;
            string textContent = textAsset.text;
            File.WriteAllText(dstPath, textContent);
        };
    }

    // create data folder
    void createDataFolder()
    {
        string dstDir = Application.persistentDataPath + Path.DirectorySeparatorChar + "Data";
        if (Directory.Exists(dstDir) == false) // create destination folder if it does
        {
            Directory.CreateDirectory(dstDir);
        }
    }


    // load configuration parameters from "config.csv" in persistentDataPath
    void loadConfig()
    {
        // load config.csv from persistentDataPath
        string folderName = Application.persistentDataPath;
        string fileName = "config.csv";
        StreamReader sr = new StreamReader(folderName + Path.DirectorySeparatorChar + fileName);
        string configText = sr.ReadToEnd();                             // read all data in the file
        sr.Close();
        Debug.Log("config loaded");
        
        // split texts with new line （　改行文字で分割する　）
        string newLineString = System.Environment.NewLine;
        char[] newLineChar = newLineString.ToCharArray();   // string.Split() requires char[] to split　（　string.Split() の第一引数はchar[]　）
        string[] configTextArray = configText.Split(newLineChar, System.StringSplitOptions.RemoveEmptyEntries); // split text （　中身の無いエントリーが削除しつつ、テキストを分割　）

        // symbols in config.csv extracts texts without comment symbol　（　*がついたコメント行を飛ばしてフィールド名・値をリストに格納　）
        // string commentString = "*";         // comments start with * will be ignored        （　* コメント行として無視する　）
        string configString = "#";          // configuration parameters start with #        （　# 実験を通して用いる定数　）
        string expPrameterString = "%";     // default experimental parameters start with % （　% 試行ごとに変わる可能性のあるパラメータ　）
        string delimeterString = ",";
        char[] delimeterChar = delimeterString.ToCharArray();

        // configuration and experimental parameters
        configParameters = new Dictionary<string, string>();
        defExperimentalParameters = new Dictionary<string, string>();
        foreach (string configTextLine in configTextArray)
        {
            if (configTextLine.IndexOf(configString) == 0)
            {
                string[] str = configTextLine.Split(delimeterChar, System.StringSplitOptions.RemoveEmptyEntries); // split record
                configParameters.Add(str[1], str[2]);
            }else if(configTextLine.IndexOf(expPrameterString) == 0)
            {
                string[] str = configTextLine.Split(delimeterChar, System.StringSplitOptions.RemoveEmptyEntries); // split record
                defExperimentalParameters.Add(str[1], str[2]);
            }
        }
    }
    


}

