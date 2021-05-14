using UnityEngine;

[XLua.LuaCallCSharp]
public class ResourcesManager : Singleton<ResourcesManager>
{
    public T LoadAsset<T>(string v) where T : UnityEngine.Object
    {
#if USE_RES  ///开发阶段不用反复打ab包。
        return Resources.Load<T>(v);
#elif USE_AB
        return AssetBundleManager.GetInstance().LoadAsset<T>(v);
#endif
    }


    public GameObject LoadPefab(string path)
    {
        Debug.LogError("LoadPefab==" + path);
        return LoadAsset<GameObject>(path);
    }

    public Sprite LoadSprite(string path)
    {
        return LoadAsset<Sprite>(path);
    }
}
