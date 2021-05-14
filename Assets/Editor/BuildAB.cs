using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
/// <summary>
/// Resources目录下的每一个资源
/// 策略：一个资源一个包，一个包里包含一个资源。
/// </summary>
public class BuildAB : Editor
{
    [MenuItem("AssetBundle/一键打包")]
    public static void Build()
    {
        ///代码设置包名
        SetNames();
        ///开始打包
        StartBuild();
        //lua脚本读取和加密
        CopyLuaFiles();
        ///索引文件生成。
        CreateIndexFile();
        ///保存版本号
        CreateVersionFile();

    }

    /// <summary>
    /// 版本信息进行保存。
    /// </summary>
    private static void CreateVersionFile()
    {
        string toPath = Directory.GetCurrentDirectory() + "/AB/" + "/version.txt";
        File.WriteAllText(toPath, Application.version);
    }
    /// <summary>
    /// 创建索引文件
    /// </summary>
    private static void CreateIndexFile()
    {
        string fromPath = GetOutputPath();
        string[] allAbFilenameLst = GameUtils.GetAllFilesAtPath(fromPath, new string[] { ".u3d", ".lua", "" }, true);

        StringBuilder sb = new StringBuilder();
        foreach (var item in allAbFilenameLst)
        {
            ///获取存储的文件的相对路径
            string p = item.Replace(GetOutputPath() + @"\", "").Replace(@"\", "/");
            ///获取文件大小
            FileInfo f = new FileInfo(item);
            p += "|" + f.Length;
            ///获取文件md5值
            p += "|" + GameUtils.GetMD5(item) + "\r\n";
            sb.Append(p);
        }
        string toPath = GetOutputPath() + "/AssetList.csv";
        File.WriteAllText(toPath, sb.ToString());

    }

    /// <summary>
    /// 把lua脚本从开发阶段时的Assets/LuaScripts下加密拷贝到ab包输出目录下。
    /// </summary>
    private static void CopyLuaFiles()
    {
        string fromPath = Application.dataPath + "/LuaScripts";

        string topath = GetOutputPath();

        string[] lst = GameUtils.GetAllFilesAtPath(fromPath, new string[] { ".lua" }, true);


        foreach (var luaPath in lst)
        {
            byte[] luaData = File.ReadAllBytes(luaPath);  //读取到lua脚本
            luaData = SecurityUtil.Xor(luaData);//加密了。

            string tmp = luaPath.Replace("LuaScripts", "#LuaScripts").Split('#')[1];///luaScripts/common/head.lua
            string savePath = Path.Combine(topath, tmp);    ///盘符：//...../AB/版本号/AssetBundle/LuaScripts/common/head.lua
            string dirPath = Path.GetDirectoryName(savePath); ///盘符：//...../AB/版本号/AssetBundle/LuaScripts/common/  文件夹不存在进行创建
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            ///开始保存了。
            File.WriteAllBytes(savePath, luaData);

        }


    }

    [MenuItem("AssetBundle/打包选中的文件夹内资源（分类打包）")]
    public static void BuildSelectFolder()
    {
        ///代码设置包名
        Object[] objs = Selection.GetFiltered<UnityEngine.Object>(SelectionMode.DeepAssets);


        foreach (var item in objs)
        {
            UnityEngine.Debug.Log(item.name);
            string path = AssetDatabase.GetAssetPath(item);
            AssetImporter importer = AssetImporter.GetAtPath(path);
            importer.assetBundleName = "";
            //importer.assetBundleVariant = "";
            if (item is GameObject)
            {
                importer = AssetImporter.GetAtPath(path);
                importer.assetBundleName = "model";
                importer.assetBundleVariant = "u3d";
            }




        }
        ///开始打包
        StartBuild();
    }



    private static void StartBuild()
    {

        string outputPath = GetOutputPath() + "/AssetBundle";
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

#if UNITY_ANDROID
        BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.Android);
#elif UNITY_STANDALONE_WIN
        BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows64);
#endif
        Process.Start(outputPath);
    }

    /// <summary>
    /// 设置包名
    /// </summary>
    private static void SetNames()
    {
        string path = Path.Combine(Application.dataPath, "Resources");
        string[] allfiles = GameUtils.GetAllFilesAtPath(path, new string[] { ".meta", ".pdf", ".doc" }, false);
        foreach (string p in allfiles)
        {
            string tmp = p.Replace(@"\", "/");
            tmp = tmp.Replace("Assets", "#Assets").Split('#')[1];

            ///重点：以下为设置标签的核心代码
            AssetImporter importer = AssetImporter.GetAtPath(tmp);
            importer.assetBundleName = tmp.Replace("Assets/Resources/", "").Split('.')[0];
            importer.assetBundleVariant = "u3d";
        }
    }

    public static string GetOutputPath()
    {
        string path = Directory.GetCurrentDirectory() + "/AB/" + Application.version;
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        return path;
    }


    [MenuItem("AssetBundle/打开AB包的输出目录")]
    public static void OpenABOutputPath()
    {
        string outputPath = Directory.GetCurrentDirectory() + "/AB/";
        Process.Start(outputPath);
    }

    [MenuItem("AssetBundle/打开S目录")]
    public static void OpenSPath()
    {
        Process.Start(Application.streamingAssetsPath);
    }

    [MenuItem("AssetBundle/打开P目录")]
    public static void OpenPPath()
    {
        Process.Start(Application.persistentDataPath);
    }



}
