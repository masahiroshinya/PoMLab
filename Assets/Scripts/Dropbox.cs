using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.UI;
using System.Collections.Generic;

public class Dropbox : MonoBehaviour {

    public Boxit.BoxitClient boxitClient;
    public GameObject linkMenu;
    public GameObject syncMenu;
    public GameObject syncProgress;
    public Text syncText;
    public Text syncProgressText;
    public InputField appKeyText;
    public InputField appSecretText;
    public GameObject uploadRetryButton;
    public GameObject downloadRetryButton;


    private string boxitDir;
    public bool isLinked;

    // to be private
    public int nFilesToBeSynched;
    public int nFilesSynched;
    public List<string> localPathListToBeUpdated;
    public List<string> remotePathListToBeUpdated;

    void Awake()
    {

        // check if there's Boxit asset
        //if (Config.instance.hasPaidAssets)
        if (true)
        {
            Debug.Log("Boxit found");


            createBoxitFolder();
            readAppKey();

            // check link status
            isLinked = boxitClient.IsLinked;
            if (isLinked)
            {
                linkMenu.SetActive(false);
                syncMenu.SetActive(true);
            }
            else
            {
                // not linked
                linkMenu.SetActive(true);
                syncMenu.SetActive(false);
            }
            syncProgress.SetActive(false);
            
            /*
            string localPath = "C:/Users/MShinya/AppData/LocalLow/PoMLabProject/PoMLabApp/config.csv"; 
            boxitClient.UploadFile(Boxit.ROOT.sandbox, localPath, "a/b/c/config.csv", UploadSuccess, UploadFailure);
            */

        }
        else
        {
            Destroy(gameObject);
        }

    }


    // create boxit folder ( called by Awake() )
    private void createBoxitFolder()
    {
        boxitDir = Application.persistentDataPath + Path.DirectorySeparatorChar + "Boxit";
        if (Directory.Exists(boxitDir) == false) // create destination folder if it does
        {
            Directory.CreateDirectory(boxitDir);
        }
    }

    // read dropbox app key
    private void readAppKey()
    {

        string path = boxitDir + Path.DirectorySeparatorChar + "dropbox key";
        if (File.Exists(path))
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            string keyText = br.ReadString();
            br.Close();

            string[] keyTextArray = keyText.Split(",".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries);
            boxitClient.applicationKey = keyTextArray[0];
            boxitClient.applicationSecret = keyTextArray[1];

        }
        else
        {   
            Debug.Log("key file not found");
        }
        
    }
    

    // Button
    public void LoadDropboxScene()
    {
        SceneManager.LoadScene("Dropbox");
    }
    public void BackToTitleScene()
    {
        SceneManager.LoadScene("PoMLab Title");
    }
    public void OnLinkButtonClick()
    {
        // Debug.Log(appKeyText.text);
        // Debug.Log(appSecretText.text);

        // key
        boxitClient.applicationKey = appKeyText.text;
        boxitClient.applicationSecret = appSecretText.text;
        // link to dropbox
        boxitClient.Link(LinkSuccess, LinkFailure);
    }
    public void OnUnlinkButtonClick()
    {
        boxitClient.Unlink();
        linkMenu.SetActive(true);
        syncMenu.SetActive(false);
        // delete key file
        string path = boxitDir + Path.DirectorySeparatorChar + "dropbox key";
        File.Delete(path);
        Debug.Log("dropbox key deleted");
    }

    public void OnSyncDataButtonClick()
    {
        uploadRetryButton.SetActive(true);
        downloadRetryButton.SetActive(false);

        Debug.Log("Sync data");
        syncText.text = "upload data...";
        syncProgressText.text = "0 / 0";
        syncMenu.SetActive(false);
        syncProgress.SetActive(true);
        
        // list of directroy in LOCAL Data folder
        string dataDir = Application.persistentDataPath + Path.DirectorySeparatorChar + "Data";
        Debug.Log("parent data folder: " + dataDir);
        string[] dataDirs = Directory.GetDirectories(dataDir);

        // count # of files to be uploaded
        /*
        nFilesToBeSynched = 0;
        localPathListToBeUpdated = new List<string>();
        remotePathListToBeUpdated = new List<string>();
        foreach (string dataFolderPath in dataDirs)
        {
            string folderName = Path.GetFileNameWithoutExtension(dataFolderPath) ;
            string[] dataFiles = Directory.GetFiles(dataFolderPath) ;
            nFilesToBeSynched = nFilesToBeSynched + dataFiles.Length;

            foreach (string localFilePath in dataFiles)
            {
                // create data folder in dropbox
                string fileName = Path.GetFileName(localFilePath);
                string remoteFilePath = "Data" + "/" + folderName + "/" + fileName;

                Debug.Log("data file path: " + localFilePath);
                Debug.Log("remote file path: " + remoteFilePath);

                // check if the file exists in dropbox
                boxitClient.GetMetaData(Boxit.ROOT.sandbox, remoteFilePath, FileExistsInDropbox, FileDoesNotExistInDropbox);

                // upload data files
                boxitClient.UploadFile(Boxit.ROOT.sandbox, localFilePath, remoteFilePath, UploadSuccess, UploadFailure);

                syncProgressText.text = nFilesSynched + " / " + nFilesToBeSynched;
            }
        }
        */

        
        // count # of files to be uploaded
        nFilesToBeSynched = 0;
        nFilesSynched = 0;
        syncProgressText.text = nFilesSynched + " / " + nFilesToBeSynched;
        foreach (string dataFolderPath in dataDirs)
        {
            string folderName = Path.GetFileNameWithoutExtension(dataFolderPath);
            string[] files = Directory.GetFiles(dataFolderPath);
            nFilesToBeSynched = nFilesToBeSynched + files.Length;
        }

        // upload
        StartCoroutine(UploadFiles(0.5f, dataDirs));
        

    }


    public void OnSyncProtocolButtonClick()
    {
        uploadRetryButton.SetActive(false);
        downloadRetryButton.SetActive(true);
        
        Debug.Log("Sync protocol");
        syncText.text = "download protocol...";
        syncProgressText.text = "0 / 0";
        syncMenu.SetActive(false);
        syncProgress.SetActive(true);
        
        // delete local protocols
        /*
        string localProtocolDir = Application.persistentDataPath + Path.DirectorySeparatorChar + "Protocols";
        string[] protocolFilePaths = Directory.GetFiles(localProtocolDir);
        foreach (string protocolFilePath in protocolFilePaths)
        {
            Debug.Log("protocol deleted: " + protocolFilePath);
            File.Delete(protocolFilePath);
        }
        */

        // count # of files to be downloaded
        nFilesSynched = 0;
        syncProgressText.text = nFilesSynched + " / " + nFilesToBeSynched;

        // download remote protocols
        string remoteProtocolDir = "Protocols";
        boxitClient.GetMetaData(Boxit.ROOT.sandbox, remoteProtocolDir, DownloadProtocols);
        
    }

    private void DownloadProtocols(long requestID, Boxit.MetaData protocolFolderMetaData)
    {
        nFilesToBeSynched = protocolFolderMetaData.Contents.Count;

        Debug.Log("remote protocol folder");
        Debug.Log(protocolFolderMetaData.ToString());
        string localProtocolDir = Application.persistentDataPath + Path.DirectorySeparatorChar + "Protocols";
        foreach (Boxit.MetaData protocolFileMetaData in protocolFolderMetaData.Contents)
        {
            Debug.Log("protocol file: " + protocolFileMetaData.Path);
            string localProtocolPath = localProtocolDir + Path.DirectorySeparatorChar + Path.GetFileName(protocolFileMetaData.Path);
            boxitClient.DownloadFile(Boxit.ROOT.sandbox, protocolFileMetaData.Path, localProtocolPath, DownloadSuccess);
        }
    }
    void DownloadSuccess(long requestID, string localFilePath)
    {
        Debug.Log("download protocol: " + localFilePath);
        nFilesSynched++;
        syncProgressText.text = nFilesSynched + " / " + nFilesToBeSynched;
    }


    // upload coroutine
    private IEnumerator UploadFiles(float sec, string[] dirs)
    {

        foreach (string dataFolderPath in dirs)
        {
            string folderName = Path.GetFileNameWithoutExtension(dataFolderPath);
            string[] files = Directory.GetFiles(dataFolderPath);
            foreach (string localPath in files)
            {
                // create data folder in dropbox
                string fileName = Path.GetFileName(localPath);
                string remotePath = "Data" + "/" + folderName + "/" + fileName;

                Debug.Log("data file path: " + localPath);
                Debug.Log("remote file path: " + remotePath);

                // upload data files
                boxitClient.UploadFileIfLocalNewer(Boxit.ROOT.sandbox, localPath, remotePath, UploadSuccess, UploadFailure);

                syncProgressText.text = nFilesSynched + " / " + nFilesToBeSynched;

                // wait
                yield return new WaitForSeconds(sec);
            }
        }
    }

    

    public void OnSyncOKButtonClick()
    {
        syncMenu.SetActive(true);
        syncProgress.SetActive(false);
    }
    public void OnUploadRetryButtonClick()
    {
        OnSyncDataButtonClick();
    }
    public void OnDownloadRetryButtonClick()
    {
        OnSyncProtocolButtonClick();
    }
    public void OnSyncCancelButtonClick()
    {
        syncMenu.SetActive(true);
        syncProgress.SetActive(false);
    }

    // ------- boxit delegate functions ------
    private void LinkSuccess(long requestID, Boxit.oAuthToken accessToken)
    {
        isLinked = true;
        linkMenu.SetActive(false);
        syncMenu.SetActive(true);
        Debug.Log("Successfully linked to Dropbox!");

        // create key file
        // --- attention ---
        // The key is NOT encrypted. 
        // Users may see the key & secret so that they can access your dropbox app folder
        // ------------------
        string path = boxitDir + Path.DirectorySeparatorChar + "dropbox key";
        FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite);
        BinaryReader Reader = new BinaryReader(fs);
        BinaryWriter bw = new BinaryWriter(fs);
        string str = boxitClient.applicationKey + "," + boxitClient.applicationSecret;
        bw.Write(str);
        bw.Flush();
        bw.Close();

    }

    private void LinkFailure(long requestID, string error)
    {
        isLinked = false;
        linkMenu.SetActive(true);
        syncMenu.SetActive(false);
        Debug.Log("Link Failure!");
    }

    /*
    void CreateFolderFailure(long requestID, string error)
    {
        // handle the metaData here
        Debug.Log(error);
    }
    */

    void UploadSuccess(long requestID, Boxit.MetaData metaData)
    {
        // process after the file uploads
        nFilesSynched++;
        syncProgressText.text = nFilesSynched + " / " + nFilesToBeSynched;
        Debug.Log("uploaded: " + metaData.Path);
    }

    void UploadFailure(long requestID, string error)
    {
        // handle the metaData here
        Debug.Log(error);
    }

    /*
    void FileExist()
    {

    }
    void FileUpload(long requestID, string error)
    {

    }
    */



}
