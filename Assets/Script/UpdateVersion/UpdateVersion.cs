using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class UpdateVersion : MonoBehaviour {
    public ProgressBar progressbar;
    public Text txtMsg;
    public Text txtPercent;
    public Text txtVersion;
    //public Text txtLog;

    private bool need_update = false;
    private string update_url = "";
    private float update_progress = 0.0f;

    private void Awake()
    {
        if (Application.platform != RuntimePlatform.Android)
        {
            SceneManager.LoadScene("Scene/Login");
        }
        txtVersion.text = VersionInfo.version;
    }
    // Use this for initialization
    void Start () {
        StartCoroutine(downLoadFromServer());
    }

    // Update is called once per frame
    void Update () {
	}

    private void Log(string log)
    {
        //txtLog.text = log + "\r\n" + txtLog.text;
    }

    public void SetPercent(float percent)
    {
        progressbar.SetPercent(percent);
        txtPercent.text = (percent * 100).ToString("F0")+"%";
    }

    public void SetMessage(string msg)
    {
        txtMsg.text = msg;
    }

    IEnumerator checkNewVersion()
    {
        SetMessage("Checking new version ...");

        var r = new UnityWebRequest("http://118.184.23.103:8012/get_version", UnityWebRequest.kHttpVerbGET);
        r.downloadHandler = new DownloadHandlerBuffer();
        yield return r.SendWebRequest();

        if (r.isNetworkError || r.isHttpError)
        {
            Log(r.error);
        }
        else
        {
            // parse xml
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(System.Text.Encoding.Default.GetString(r.downloadHandler.data));
            var versionInfo = doc.DocumentElement;
            var ver_code = int.Parse(versionInfo.GetAttribute("ver_code"));
            
            if (ver_code > VersionInfo.version_code)
            {
                need_update = true;
                update_url = versionInfo.GetAttribute("url");
            }
            else
            {
                SceneManager.LoadScene("Scene/Login");
            }
        }
    }

    IEnumerator downLoadFromServer()
    {
        yield return StartCoroutine("checkNewVersion");

        if (!need_update)
        {
            yield break;
        }

        Log("downLoadFromServer file:"+update_url);

        SetMessage("Updating to lastest version ...");
        //update_url = "https://storage.360buyimg.com/jdmobile/JDMALL-PC2.apk";

        string savePath = Path.Combine(Application.persistentDataPath, "Download");
        savePath = Path.Combine(savePath, "new.apk");

        //Debug.Log("savePath:"+savePath);

        if (!Directory.Exists(Path.GetDirectoryName(savePath)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(savePath));
        }

        var r = new UnityWebRequest(update_url, UnityWebRequest.kHttpVerbGET);
        //r.downloadHandler = new DownloadHandlerBuffer();
        r.downloadHandler = new DownloadHandlerFile(savePath);
        var op = r.SendWebRequest();


        while (!op.isDone)
        {
            //Must yield below/wait for a frame
            //Log(op.progress.ToString());
            update_progress = op.progress;
            SetPercent(op.progress);
            yield return null;
        }

        if (r.isNetworkError||r.isHttpError)
        {
            Log(r.error);
        }
        else
        {
            SetPercent(1.0f);
            Log("File successfully downloaded and saved to " + savePath);
            Log("sdk version:" + getSDKInt().ToString());
            if (getSDKInt() >= 24)
            {
                installAppApi24Above(savePath);
            }
            else
            {
                installApp(savePath);
            }
        }
    }

    public bool installApp(string apkPath)
    {
        try
        {
            AndroidJavaClass intentObj = new AndroidJavaClass("android.content.Intent");
            string ACTION_VIEW = intentObj.GetStatic<string>("ACTION_VIEW");
            int FLAG_ACTIVITY_NEW_TASK = intentObj.GetStatic<int>("FLAG_ACTIVITY_NEW_TASK");
            AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", ACTION_VIEW);

            AndroidJavaObject fileObj = new AndroidJavaObject("java.io.File", apkPath);
            AndroidJavaClass uriObj = new AndroidJavaClass("android.net.Uri");
            AndroidJavaObject uri = uriObj.CallStatic<AndroidJavaObject>("fromFile", fileObj);

            intent.Call<AndroidJavaObject>("setDataAndType", uri, "application/vnd.android.package-archive");
            intent.Call<AndroidJavaObject>("addFlags", FLAG_ACTIVITY_NEW_TASK);
            intent.Call<AndroidJavaObject>("setClassName", "com.android.packageinstaller", "com.android.packageinstaller.PackageInstallerActivity");

            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            currentActivity.Call("startActivity", intent);

            return true;
        }
        catch (System.Exception e)
        {
            Log("Error: " + e.Message);
            return false;
        }
    }

    private bool installAppApi24Above(string apkPath)
    {
        bool success = true;

        try
        {
            //Get Activity then Context
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject unityContext = currentActivity.Call<AndroidJavaObject>("getApplicationContext");

            //Get the package Name
            string packageName = unityContext.Call<string>("getPackageName");
            string authority = packageName + ".fileprovider";

            AndroidJavaClass intentObj = new AndroidJavaClass("android.content.Intent");
            string ACTION_VIEW = intentObj.GetStatic<string>("ACTION_VIEW");
            AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", ACTION_VIEW);

            Log("Step 1");
            //int FLAG_ACTIVITY_NEW_TASK = intentObj.GetStatic<int>("FLAG_ACTIVITY_NEW_TASK");
            int FLAG_GRANT_READ_URI_PERMISSION = intentObj.GetStatic<int>("FLAG_GRANT_READ_URI_PERMISSION");
            Log("Step 2");
            //File fileObj = new File(String pathname);
            AndroidJavaObject fileObj = new AndroidJavaObject("java.io.File", apkPath);
            Log("Step 3");
            //FileProvider object that will be used to call it static function
            AndroidJavaClass fileProvider = new AndroidJavaClass("android.support.v4.content.FileProvider");
            Log("Step 4");
            Log(authority);
            //getUriForFile(Context context, String authority, File file)
            AndroidJavaObject uri = fileProvider.CallStatic<AndroidJavaObject>("getUriForFile", unityContext, authority, fileObj);
            Log("Step 5");
            intent.Call<AndroidJavaObject>("setDataAndType", uri, "application/vnd.android.package-archive");
            Log("Step 6");
            //intent.Call<AndroidJavaObject>("addFlags", FLAG_ACTIVITY_NEW_TASK);
            intent.Call<AndroidJavaObject>("addFlags", FLAG_GRANT_READ_URI_PERMISSION);
            Log("Step 7");
            currentActivity.Call("startActivity", intent);

            Log("Success");
        }
        catch (System.Exception e)
        {
            Log("Error: " + e.Message);
            success = false;
        }

        return success;
    }

    static int getSDKInt()
    {
        using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
        {
            return version.GetStatic<int>("SDK_INT");
        }
    }
}

