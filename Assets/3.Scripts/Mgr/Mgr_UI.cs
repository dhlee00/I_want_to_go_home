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
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            // 인벤토리 생성
            if(Inventory_UI == null)
            {
                GameObject inven = Instantiate(Inventory_Prefab);
                Inventory_UI = inven;
                inven.transform.SetParent(UI_Parent, false);
            }
            // 인벤토리 생성 되있고 비횔성화 중이면
            else if (Inventory_UI != null && Inventory_UI.activeSelf == false) 
            {
                Inventory_UI.SetActive(true);
            }
            // 인벤토리 생성 되있고 횔성화 중이면
            else if (Inventory_UI != null && Inventory_UI.activeSelf == true)
            {
                Inventory_UI.GetComponent<Animator>().Play("Close");
            }
        }
    }
}
