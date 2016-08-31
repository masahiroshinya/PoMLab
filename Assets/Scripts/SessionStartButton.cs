using UnityEngine;
using System.Collections;

// for SceneManager
using UnityEngine.SceneManagement;

public class SessionStartButton : MonoBehaviour {


    // button click
    public void OnClick()
    {
        // Debug.Log("Session start!");
        SceneManager.LoadScene("PoMLab Task");

    }
    
}
