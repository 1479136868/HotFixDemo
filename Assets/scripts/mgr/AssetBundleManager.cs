using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XLua;
/// <summary>
/// 对ab包的一个管理
/// 
/// 1.从ab包里加载资源
/// 2. 卸载资源包或者资源。
/// </summary>
[LuaCallCSharp]
public class AssetBundleManager : Singleton<AssetBundleManager>
{
    /// <summary>
    /// ab包的缓存
    /// </summary>
    private Dictionary<string, MyAssetBundle> abCache = new Dictionary<string, MyAssetBundle>();

    /// <summary>
    /// 保存所有依赖数据的字典
    /// </summary>
    private Dictionary<string, string[]> allDependences;
    private string abPath;

    public AssetBundleManager()
    {
        abPath = Path.Combine(Application.persistentDataPath, "AB/AssetBundle");
        InitDependence();
    }

    /// <summary>
    /// 读取并解析包含ab包和ab包依赖关系的AssetBundle文件，获取里面的数据
    /// </summary>
    private void InitDependence()
    {
        if (allDependences == null)
        {
            allDependences = new Dictionary<string, string[]>();

            string path = Path.Combine(abPath, "AssetBundle");
            ///加载资源包
            AssetBundle manifestAB = AssetBundle.LoadFromFile(path);
            //读取资源
            var at = manifestAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            //从资源中获取数据。
            ///获取项目中所有的资源包（ab包）的名字
            string[] allAssetBundles = at.GetAllAssetBundles();

            foreach (var item in allAssetBundles)
            {
                ///获取ab包的依赖的资源包。
                allDependences[item] = at.GetAllDependencies(item);
            }
        }
    }

    /// <summary>
    /// 对外的，从ab包中加载资源时，调用
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public T LoadAsset<T>(string name) where T : UnityEngine.Object
    {
        ///先获取一下的依赖关系数据。
        InitDependence();

        ///由要加载的资源名，转换成要加载的ab包名
        string assetbundlename = name.ToLower() + ".u3d";
        Debug.Log("================================================"+assetbundlename);

        ///加载依赖的资源包
        if (allDependences.ContainsKey(assetbundlename))
        {
            string[] dependenceArr = allDependences[assetbundlename];
            foreach (var item in dependenceArr)
            {
                LoadAssetBundle(item); //被依赖的资源只要加载到内存中就可以了。
            }
        }
        ///加载真正需要的资源自己
        MyAssetBundle my = LoadAssetBundle(assetbundlename);
        return my.ab.LoadAllAssets<T>()[0];///因为打包工具中，一个资源包里就只有一个资源。所以是[0]
    }

    /// <summary>
    /// 加载单个资源包的方法 
    /// </summary>
    /// <param name="assetbundlename"></param>
    private MyAssetBundle LoadAssetBundle(string assetbundlename)
    {
        string path = Path.Combine(abPath, assetbundlename);
        if (abCache.ContainsKey(assetbundlename))
        {
            abCache[assetbundlename].count++;///之前加载过这个AB包，计数增加就可以了。
            return abCache[assetbundlename];
        }
        else
        {
            try
            {
                ///没加载过，加载一波，放入缓存。
                AssetBundle ab = AssetBundle.LoadFromFile(path);
                MyAssetBundle my = new MyAssetBundle(ab);
                abCache.Add(assetbundlename, my);
                return my;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"加载资源包={path},发生错误  msg= {ex.Message}");
            }
        }
        return null;

    }

    public void UnLoad(string name)
    {
        string assetbundlename = name.ToLower() + ".u3d";

        if (allDependences.ContainsKey(assetbundlename))
        {
            string[] dependenceArr = allDependences[assetbundlename];
            foreach (var item in dependenceArr)
            {
                UnLoadAssetBundle(item);
            }
        }

        UnLoadAssetBundle(assetbundlename);


    }

    /// <summary>
    /// 卸载一个资源
    /// </summary>
    /// <param name="abName"></param>
    private void UnLoadAssetBundle(string abName)
    {
        if (abCache.ContainsKey(abName))
        {
            abCache[abName].count--;///之前加载过这个AB包，计数增加就可以了。
            if (abCache[abName].count <= 0)
            {
                abCache[abName].ab.Unload(false);
                abCache.Remove(abName);
            }
        }
    }


}

public class MyAssetBundle
{
    /// <summary>
    /// 代表一个资源包被加载的次数。
    /// </summary>
    public int count;

    public AssetBundle ab;
    public MyAssetBundle(AssetBundle ab)
    {
        count = 1;
        this.ab = ab;
    }
}
