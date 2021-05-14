using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//1、 版本号检查
//2. 下载服务器索引文件
//3.、跟本地索引对比
//4.、根据差异（md5）下载需要更新的AB包
//5.、覆盖本地的旧AB包

public class HotFix : MonoBehaviour
{
    public Slider progressBar;
    public Text infoTxt;
    /// <summary>
    /// 服务器的资源根目录
    /// </summary>
    private string serverURL;
    /// <summary>
    /// 本地根目录
    /// </summary>
    private string localPath;
    /// <summary>
    /// 本地版本字符串
    /// </summary>
    private string localVersionStr; //本地的版本号字符串
    /// <summary>
    /// 本地版本
    /// </summary>
    private Version localVer;

    /// <summary>
    /// 服务器版本字符串
    /// </summary>
    private string ServerVerStr;
    /// <summary>
    /// 服务器版本。
    /// </summary>
    private Version serverVer;

    /// <summary>
    /// 解析出来的本地的资源列表（索引文件）
    /// </summary>
    private Dictionary<string, AssetItem> locaAssetList;
    /// <summary>
    /// 解析出来的服务器端的索引文件。
    /// </summary>
    private Dictionary<string, AssetItem> serverAssetList;
    /// <summary>
    /// 服务器端的索引文件字符串
    /// </summary>
    private string serverAssetListStr;
    /// <summary>
    /// 待下载的资源列表队列
    /// </summary>
    private Queue<AssetItem> downloadAssetQueue = new Queue<AssetItem>();

    /// <summary>
    /// 待下载的资源总大小
    /// </summary>
    private double totalBytes = 0;
    private double crtBytes = 0;


    private void Awake()
    {
        //"http://10.161.29.2/AB";
        //serverURL = "http://10.161.29.99:8080/下载内容/AB";
        serverURL = "http://10.161.26.26/AB";
        localPath = Application.persistentDataPath + "/AB";
    }
    // Start is called before the first frame update
    void Start()
    {
        //1.检查版本
        checkVersion();
    }


    private void checkVersion()
    {
        //2.下载服务器版本号。
        string verURL = this.serverURL + "/version.txt";
        DownloadUrl(verURL, (x) =>
        {
            this.ServerVerStr = UTF8Encoding.UTF8.GetString(x);
            Debug.Log("下载到服务器版本 = " + this.ServerVerStr);
            serverVer = Version.Get(this.ServerVerStr);

            //1.读本地版本。
            string path = localPath + "/version.txt";
            if (File.Exists(path)) //不是第一玩了。
            {
                localVersionStr = File.ReadAllText(path);
                localVer = Version.Get(localVersionStr);

                if (this.localVer.verStr != this.serverVer.verStr)
                {
                    if (this.localVer.big != this.serverVer.big)
                    {
                        Debug.Log("调android的应用商店界面，让用户重新下载");///强制更新
                    }
                    else
                    {
                        ///走热更新流程。
                        Debug.Log("服务器和客户端的版本号不一样，热更新流程");
                        HotUpdateLogic();
                    }
                }
                else
                {
                    Debug.Log("版本一致不需要更新");
                    EnterGame();
                }
            }
            else //第一次玩。
            {
                ///走热更新流程。
                Debug.Log("没有本地版本号，玩家第一次玩，走热更新流程");
                HotUpdateLogic();
            }


        },
       null,
       (x) =>
       {
           Debug.LogError("下载版本号出错" + x);
       });

    }

    /// <summary>
    /// 热更新流程。
    /// </summary>
    private void HotUpdateLogic()
    {
        //1.读取本地索引文件
        locaAssetList = new Dictionary<string, AssetItem>();
        string path = localPath + "/AssetList.csv";
        if (File.Exists(path))
        {
            string[] lst = File.ReadAllLines(path);
            //解析
            foreach (string info in lst)
            {
                if (string.IsNullOrEmpty(info))
                {
                    continue;
                }
                string[] strs = info.Split('|');
                AssetItem ai = new AssetItem();
                ai.path = strs[0];
                ai.length = int.Parse(strs[1]);
                ai.md5 = strs[2];
                locaAssetList.Add(ai.path, ai);
            }
        }


        ///2.下载服务器端的清单文件（索引文件）
        serverAssetList = new Dictionary<string, AssetItem>();
        string assetListURL = this.serverURL + "/" + this.serverVer.verStr + "/AssetList.csv";
        DownloadUrl(assetListURL, (x) =>
        {
            serverAssetListStr = UTF8Encoding.UTF8.GetString(x);
            string[] tmpArrs = serverAssetListStr.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var info in tmpArrs)
            {
                if (string.IsNullOrEmpty(info))
                {
                    continue;
                }
                string[] strs = info.Split('|');
                AssetItem ai = new AssetItem();
                ai.path = strs[0];
                ai.length = int.Parse(strs[1]);
                ai.md5 = strs[2];
                this.serverAssetList.Add(ai.path, ai);
            }

            /// 解析完毕后，
            /// 跟本地索引对比
            findDownLoadAsset();
            ////4.、根据差异（md5）下载需要更新的AB包
            downloadAsset();

        },
       null,
       (x) =>
       {
           Debug.LogError("下载服务器端的索引文件出错" + x);
       });
    }


    /// <summary>
    /// 找出所有的待下载资源包信息。
    /// </summary>
    private void findDownLoadAsset()
    {
        foreach (var item in this.serverAssetList)
        {
            if (this.locaAssetList.ContainsKey(item.Value.path)) //本地有资源
            {
                if (item.Value.md5 != this.locaAssetList[item.Value.path].md5)
                {
                    downloadAssetQueue.Enqueue(item.Value);
                    totalBytes += item.Value.length;  //累加总大小
                }
            }
            else
            {
                downloadAssetQueue.Enqueue(item.Value);
                totalBytes += item.Value.length;
            }
        }
    }


    /// <summary>
    /// 下载所有待下载的资源包。
    /// </summary>
    private void downloadAsset()
    {
        ///该下载的资源队列时空的。
        if (downloadAssetQueue.Count <= 0)
        {
            //EnterGame();
            return;
        }

        AssetItem it = downloadAssetQueue.Dequeue();
        string url = this.serverURL + "/" + this.serverVer.verStr + "/" + it.path;
        DownloadUrl(url, (data) =>
        {
            this.crtBytes += it.length; //更新下载完成字节数。
            double pro = (this.crtBytes / this.totalBytes);
            Debug.Log("下载进度=" + pro.ToString("0.00"));
            progressBar.value = (float)pro;

            this.infoTxt.text = "目标版本:" + this.serverVer.verStr + "  下载进度=" + (pro * 100).ToString("0.00") + "%     " + (this.crtBytes / 1024 / 1024).ToString("0.0") + "MB/" + (this.totalBytes / 1024 / 1024).ToString("0.0") + "MB";
            //保存下载的东西
            //要保存的目录不存在创建一波。
            string savepath = Application.persistentDataPath + "/AB/" + it.path;
            string dir = Path.GetDirectoryName(savepath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            ///保存下载的资源包。
            File.WriteAllBytes(savepath, data);
            ///UI更新。。。
            if (downloadAssetQueue.Count > 0)
            {
                downloadAsset();
            }
            else
            {
                ///保存服务器的索引文件到本地
                saveIndexFile();
                Debug.Log("服务器清单文件保存本地完毕..");
                ///保存服务器端的版本号
                saveVersionTxt();
                Debug.Log("版本号保存本地完毕..");
                Debug.Log("版本更新已完毕，开始进入游戏.....");
                ///进入游戏场景
                EnterGame();
                return;
            }
        },
        (x) =>
        {

        }, (x) =>
        {
            Debug.Log("下载资源出错" + it.path + "  msg=" + x);

        });
    }

    /// <summary>
    /// 进入游戏。
    /// </summary>
    private void EnterGame()
    {
        this.Invoke("InvokeEnterGame", 2.0f);
    }
    private void InvokeEnterGame()
    {
        SceneManager.LoadScene("LoginScene");
    }

    private void saveVersionTxt()
    {
        string path = this.localPath + "/version.txt";
        File.WriteAllText(path, this.ServerVerStr);
    }

    private void saveIndexFile()
    {
        string path = this.localPath + "/AssetList.csv";
        File.WriteAllText(path, this.serverAssetListStr);
    }


    #region 下载的工具函数
    /// <summary>
    /// 
    /// </summary>
    /// <param name="url"></param>
    /// <param name="onComplete"></param>
    /// <param name="onProgress"></param>
    /// <param name="onError"></param>
    public void DownloadUrl(string url, Action<byte[]> onComplete, Action<double> onProgress = null, Action<string> onError = null)
    {
        StartCoroutine(startDownLoad(url, onComplete, onProgress, onError));
    }
    private IEnumerator startDownLoad(string url, Action<byte[]> onComplete, Action<double> onProgress, Action<string> onError)
    {
        Debug.Log(url);
        UnityWebRequest req = UnityWebRequest.Get(url);
        UnityWebRequestAsyncOperation op = req.SendWebRequest(); //开始发送网络请求，开始下载的过程
        op.completed += (x) =>
        {
            if (req.isNetworkError)
            {
                Debug.LogError("req.isNetworkError");
            }
            if (req.isHttpError)
            {
                Debug.LogError(req.isHttpError.ToString());
            }
            if (req.isNetworkError || req.isHttpError)
            {
                onError?.Invoke(req.error);
            }
            else
            {
                byte[] data = req.downloadHandler.data;
                onComplete?.Invoke(data);
            }
        };

        while (!op.isDone)
        {
            onProgress?.Invoke(op.progress);
            yield return null; //等待一帧、
        }
    }
    #endregion

}

public class Version
{
    public int big = 0;
    public int mid;
    public int small;
    public string verStr;
    public static Version Get(string verStr)
    {
        string[] strs = verStr.Split('.');
        Version ver = new Version();
        ver.big = int.Parse(strs[0]);
        ver.mid = int.Parse(strs[1]);
        ver.small = int.Parse(strs[2]);
        ver.verStr = verStr;
        return ver;
    }
}

public class AssetItem
{
    public string path;
    public int length;
    public string md5;

}

