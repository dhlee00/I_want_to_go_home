using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using UnityEngine;

public class Mgr_UI : MonoBehaviour
{
    public static Mgr_UI Inst;

    [SerializeField] Transform UI_Parent;

    [Header("Inventory")]
    [SerializeField] GameObject Inventory_Prefab;
    GameObject Inventory_UI;
    

    void Start()
    {
        #region Singleton
        if (Inst == null)
        {
            Inst = this;
        }
        else
        {
            Destroy(this);
        }
        #endregion  

        Init_UI(Inventory_Prefab, ref Inventory_UI);
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            Spawn_UI(Inventory_Prefab, Inventory_UI);
        }
    }

    #region Spawn_UI
    void Spawn_UI(GameObject _uiPrefab, GameObject _ui)
    {
        // UI ���� ���ְ� ��Ȼ��ȭ ���̸� (����)
        if (_ui != null && _ui.activeSelf == false)
        {
            Inventory_UI.SetActive(true);
        }
        // UI ���� ���ְ� Ȼ��ȭ ���̸� (�ݱ�)
        else if (_ui != null && _ui.activeSelf == true)
        {
            _ui.GetComponent<Animator>().Play("Close");
        }
    }

    void Init_UI(GameObject _uiPrefab, ref GameObject _ui)
    {
        // UI ����
        if (_ui == null)
        {
            GameObject spawnUI = Instantiate(_uiPrefab);
            _ui = spawnUI;
            spawnUI.transform.SetParent(UI_Parent, false);
            spawnUI.SetActive(false);
        }
    }
    #endregion
}
