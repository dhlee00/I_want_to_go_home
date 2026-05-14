using UnityEngine;

public class SpaceShip_Craft : Interaction_CraftingStation
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void StartCraftItem(Item inMakeItem)
    {
        base.StartCraftItem(inMakeItem);
    }

    public override void OnInteraction()
    {
        // 제작대 UI 출력
        Mgr_Game.Inst.OpenCraftingSpaceUI(true, this);
    }
}
