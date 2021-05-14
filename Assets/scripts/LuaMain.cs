using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using XLua;
 

public class LuaMain : MonoBehaviour
{
    public static LuaEnv luaEnv;
    /// <summary>
    /// 使用lua环境，获取到lua的函数（Glob.Update）赋值给这个变量
    /// </summary>
    private Action luaUpdate;
    /// <summary>
    /// 在lua那头给这个委托赋值，
    /// </summary>
    public Action awake;
    public Action start;
    public Action onDestroy;
    public Action<string> SceneLoadedEvent;

    private void Awake()
    {

        DontDestroyOnLoad(this.gameObject);

        luaEnv = new LuaEnv();

        luaEnv.AddLoader(customLoader);

        ////使用lua环境，在lua那头声明了一个全局变量，它的值是luaMain的实例。
        luaEnv.Global.Set("LuaMain", this);

        luaEnv.DoString("require('Main')");

        ///c#如何调用lua？？？？
        luaUpdate = luaEnv.Global.GetInPath<Action>("Global.Update");

        ///场景加载完毕触发
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;

        awake?.Invoke();
      
    }

    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        SceneLoadedEvent?.Invoke(arg0.name);
    }

    private byte[] customLoader(ref string filepath)
    {

#if USE_AB
        string path = Application.persistentDataPath + "/AB/luaScripts/" + filepath.Replace(".", "/") + ".lua";
        byte[] data = File.ReadAllBytes(path);
        return SecurityUtil.Xor(data);

#elif USE_RES
        string path = Application.dataPath + "/LuaScripts/" + filepath.Replace(".", "/") + ".lua";
        return File.ReadAllBytes(path);
#endif
    }

    void Start()
    {
        start?.Invoke();
    }

    void Update()
    {
        luaUpdate?.Invoke();

        NetManager.GetInstance().Update();
    }

    private void OnDestroy()
    {
        onDestroy?.Invoke();
    }


}
