using UnityEngine;
using XLua;

[LuaCallCSharp]
public static class UIUtils
{
    public static Transform FindByName(this Transform parent, string name)
    {
        Transform[] arr = parent.GetComponentsInChildren<Transform>();
        foreach (Transform item in arr)
        {
            if (item.name.Equals(name))
            {
                return item;
            }
        }
        return null;
    }


    public static Vector3 GetPosWithRayCast(string layername)
    {
        Ray ray = Camera.main.ScreenPointToRay((Input.mousePosition));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, LayerMask.GetMask(layername)))
        {
            return hit.point;
        }
        return default(Vector3);
    }

    public static GameObject GetObjWithRayCast(string layername)
    {
        Ray ray = Camera.main.ScreenPointToRay((Input.mousePosition));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, LayerMask.GetMask(layername)))
        {
            return hit.transform.gameObject;
        }
        return null;
    }









}
