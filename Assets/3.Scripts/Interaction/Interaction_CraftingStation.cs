using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using UnityEngine;

public class Interaction_CraftingStation : Interaction
{
    // 아이템 조합법 구조체
    public List<FCraftingRecipe> CraftingRecipe = new List<FCraftingRecipe>();


    private void Awake()
    {
        InteractionType = EInteractionType.craftingStation;
    }


    // 상호작용 했을때
    public override void OnInteraction()
    {
        // 제작대 UI 출력
        Mgr_Game.Inst.OpenCraftingStationUI(true, this);
    }

}
