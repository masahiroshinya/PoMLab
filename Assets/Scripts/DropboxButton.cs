using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.IO;

public class DropboxButton : MonoBehaviour {


    void Awake()
    {

        // check if there's Boxit asset
        if (Config.instance.hasPaidAssets)
        {
            // no dropbox button
            Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    // Button
    public void onClick()
    {
        SceneManager.LoadScene("Dropbox");
    }




}
