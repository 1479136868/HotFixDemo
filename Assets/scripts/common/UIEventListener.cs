using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using XLua;

/// <summary>
/// UGUI的事件侦听
/// </summary>
[LuaCallCSharp]
public class UIEventListener : EventTrigger
{
    public Action luaUpdate;

    private Dictionary<EventTriggerType, Action<GameObject, BaseEventData>> map = new Dictionary<EventTriggerType, Action<GameObject, BaseEventData>>();
    public  static UIEventListener Add(GameObject go)
    {
        UIEventListener tmp = go.GetComponent<UIEventListener>();
        if (tmp==null)
        {
            tmp = go.AddComponent<UIEventListener>();
        }
        return tmp;
  
    }

 

    /// <summary>
    /// lua侧注册事件时调用
    /// </summary>
    /// <param name="eventType">事件类型</param>
    /// <param name="Callback"> lua侧的函数</param>
    public void AddListener(EventTriggerType eventType,Action<GameObject, BaseEventData> Callback)
    {
        if (map.ContainsKey(eventType))
        {
            map[eventType] += Callback;
        }
        else
        {
            map.Add(eventType, Callback);
        }

        
    }

 

    public void RemoveListener(EventTriggerType eventType)
    {
        if (map.ContainsKey(eventType))
        {
            map.Remove(eventType);
        }
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (map.ContainsKey(EventTriggerType.PointerClick))
        {
            map[EventTriggerType.PointerClick]?.Invoke(this.gameObject, eventData);
        }
    }


    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (map.ContainsKey(EventTriggerType.PointerEnter))
        {
            map[EventTriggerType.PointerEnter]?.Invoke(this.gameObject, eventData);
        }
    }


    public override void OnPointerExit(PointerEventData eventData)
    {
        if (map.ContainsKey(EventTriggerType.PointerExit))
        {
            map[EventTriggerType.PointerExit]?.Invoke(this.gameObject, eventData);
           
        }
    }

    // Update is called once per frame
    void Update()
    {
        luaUpdate?.Invoke();
    }


}
