using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;

public class SelectProtocol : MonoBehaviour {

    // protocol selection panel
    public GameObject protocolScrollView;
    public GameObject scrollbarContents;
    public GameObject fileSelectButtonPrefab;
    public GameObject selectedProtocolText;

    public void onClick()
    {
        
        // protocol folder in persistentDataPath
        string protcolDirPath = Application.persistentDataPath + Path.DirectorySeparatorChar + "Protocols";
        // get list of protocol files in the folder
        DirectoryInfo dir = new DirectoryInfo(protcolDirPath);
        FileInfo[] fileInfoArray = dir.GetFiles();
        // add buttons
        foreach (FileInfo fileInfo in fileInfoArray)
        {
            GameObject fileSelectButton = (GameObject)Instantiate(fileSelectButtonPrefab, Vector3.zero, Quaternion.identity);
            fileSelectButton.transform.SetParent(scrollbarContents.transform, false);
            fileSelectButton.GetComponentInChildren<Text>().text = fileInfo.Name;
        };
        
        // activate selection panel
        protocolScrollView.SetActive(true);
    }


}
