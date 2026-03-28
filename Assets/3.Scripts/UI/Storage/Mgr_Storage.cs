using UnityEngine.UI;
using UnityEngine;

public class Mgr_Storage : MonoBehaviour
{
    [SerializeField] Image DragItem;
    public Image Get_DragItem { get => DragItem; }

    [SerializeField] Item_Desc ItemDesc;
    public Item_Desc Get_Item_Desc { get => ItemDesc; }

    [SerializeField] Storage_UI StorageUI_Ref;
    public Storage_UI Get_StorageUI_Ref { get => StorageUI_Ref; }

    public static Mgr_Storage Inst = null;

    void Awake()
    {
        if (Inst == null)
        {
            Inst = this;
        }
    }
}
