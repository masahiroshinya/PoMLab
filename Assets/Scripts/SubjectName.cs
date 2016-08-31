using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SubjectName : MonoBehaviour {

	// Use this for initialization
	void Start () {

        if (Config.instance.subjectName != "")
        {
            Debug.Log("subject name not null");
            GetComponent<InputField>().text = Config.instance.subjectName;
        }

    }
	
	// Update is called once per frame
	void Update () {
	
	}


    public void SetSubjectName()
    {
        // Debug.Log("set subject name");
        Config.instance.subjectName = this.GetComponent<InputField>().text;
    }

}
