using UnityEngine;
using UnityEngine.UI;

public class UI_CraftingItem_Button : MonoBehaviour
{
    public FCraftingRecipe CraftingRecipe;


    public void SetUICraftingItemButton(UI_CraftingStation inOnwer, FCraftingRecipe inCraftingRecipe)
    {
        Button CraftingItem_Button = GetComponent<Button>();

        CraftingRecipe = inCraftingRecipe;

        if (CraftingItem_Button)
            CraftingItem_Button.onClick.AddListener(() => { inOnwer.SetCraftingDetailPanel(true, this); });

        Item item = ItemList.Inst.GetItemData(inCraftingRecipe.ResultItem.Item_Index);
        GetComponent<Image>().sprite = item.Get_Item_Icon;
    }

}
