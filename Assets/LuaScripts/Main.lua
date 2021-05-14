Global = {}
---引入lua脚本
Global.Lplus = require("Common/Lplus")
Global.JSON = require("Common/json")
require("Common/head")

LuaMain.awake = function()
    print("lua  Awake")
end

LuaMain.start = function()
    print("lua  start")
   
end

LuaMain.SceneLoadedEvent = function(sceneName)
    print(sceneName .. "加载完毕")
    if sceneName == "LoginScene" then
        local cube = ResourcesManager:LoadPefab("Cube")
        GameObject.Instantiate(cube)
        require("New");
    end
end

--每一帧都需要被调用。
function Global.Update()
    print("Update")
end

LuaMain.onDestroy = function()
    print("lua  ondestroy")
end
