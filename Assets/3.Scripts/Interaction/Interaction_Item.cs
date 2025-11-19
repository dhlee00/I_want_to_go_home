using Unity.Services.Apis.Admin.CloudSave;
using UnityEngine;
using static UnityEditor.Timeline.Actions.MenuPriority;

public class Interaction_Item : Interaction
{
    // 아이템 정보
    public Item ItemData;


    void Awake()
    {
        InteractionType = EInteractionType.item;
    }

    void Start()
    {
        
    }

    public void SetInteractionItem(int Index, int Amount, Vector3 SpawnPos, Vector3 ForceDir = default)
    {
        ItemData = ItemList.Inst.GetItemData(Index);
        ItemData.Get_Item_Amount = Amount;

        this.gameObject.transform.position = SpawnPos;

        if(ForceDir != default)
        {
            GetComponent<Rigidbody>().AddForce(ForceDir * 4, ForceMode.Impulse);
        }
    }

    public override void OnInteraction()
    {
        GlobalValue.AddItme(ItemData);

        Destroy(this.gameObject);
    }

    
}
