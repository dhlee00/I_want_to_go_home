using System.Collections.Generic;
using UnityEngine;

public class UI_ObjPool : MonoBehaviour
{
    int MakeSize = 5; 

    [Header("Interact_UI")]
    [SerializeField] GameObject Interact_UI_Prefab;
    [SerializeField] public List<Item_Interact_UI> Interact_UI_List = new List<Item_Interact_UI>();
    [SerializeField] Transform Interact_UI_Tr;

    public static UI_ObjPool Inst = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(Inst == null)
        {
            Inst = this;
        }

        // 惑龋累侩 UI 积己
        Spawn_UI<Item_Interact_UI>(MakeSize, Interact_UI_Prefab, Interact_UI_Tr, Interact_UI_List);
    }

    // UI 固府 积己
    #region Spawn_UI
    void Spawn_UI<T>(int _size, GameObject _prefab, Transform _parent, List<T> _list)
        where T : Component
    {
        for (int i = 0; i < _size; i++)
        {
            GameObject obj = Instantiate(_prefab);
            T comp = obj.GetComponent<T>();
            obj.transform.SetParent(_parent, false);
            obj.SetActive(false);
            _list.Add(comp);
        }
    }
    #endregion

    #region Get_Interact_UI
    public GameObject Get_Interact_UI(Item _item, Interaction _interaction)
    {
        for(int i = 0; i < Interact_UI_List.Count; i++)
        {
            if(Interact_UI_List[i].gameObject.activeSelf == true)
            {
                continue;
            }
            else
            {
                Interact_UI_List[i].gameObject.SetActive(true);
                Interact_UI_List[i].GetComponent<Item_Interact_UI>().Set_ItemInfo(_item.Get_Item_Name, _item.Get_Item_Icon, _item.Get_Item_Amount,
                    _interaction);
                return Interact_UI_List[i].gameObject;
            }
        }

        GameObject obj = Instantiate(Interact_UI_Prefab);
        obj.GetComponent<Item_Interact_UI>().Set_ItemInfo(_item.Get_Item_Name, _item.Get_Item_Icon, _item.Get_Item_Amount,
            _interaction);
        obj.transform.SetParent(Interact_UI_Tr, false);
        Interact_UI_List.Add(obj.GetComponent<Item_Interact_UI>());
        return obj;
    }
    #endregion
}
