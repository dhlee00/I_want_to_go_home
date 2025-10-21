using UnityEngine;

public class Interaction_Item : Interaction
{
    public int Item_Amount = 0;

    public override void OnInteraction()
    {
        Debug.Log(this.InteractionName);
        // 인벤토리로 들어가는 아이템 획득 코드
    }
}
