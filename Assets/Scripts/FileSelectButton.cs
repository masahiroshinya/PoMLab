using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class FileSelectButton : MonoBehaviour {

    

    // button click
    public void OnClick()
    {

        // get filename to read
        string filename = GetComponentInChildren<Text>().text;
        Debug.Log("Protocol selected: " + filename);

        // import protocol
        Dictionary <string, string> defaultExpPrm = Config.instance.defExperimentalParameters;
        GameObject protocol = GameObject.Find("Protocol");
        protocol.GetComponent<Protocol>().ImportProtocol(filename);

        // update selected protocol text
        GameObject selectedProtocolText = GameObject.Find("Selected Protocol Text");
        string str = filename.Remove(filename.Length-4, 4); // remove ".csv"
        selectedProtocolText.GetComponent<Text>().text = str;

        // activate session start button
        GameObject canvas = GameObject.Find("Canvas"); // can't directly access non-active object
        GameObject sessionStartButton = canvas.transform.FindChild("Session Start Button").gameObject;
        sessionStartButton.SetActive(true);

        // remove buttons
        GameObject[] buttons = GameObject.FindGameObjectsWithTag("File Select Button");
        foreach(GameObject button in buttons)
        {
            Destroy(button);
        }

        // deactivate select panel
        GameObject protocolScrollView = GameObject.Find("Protocol Scroll View");
        protocolScrollView.SetActive(false);

    }
    

}
