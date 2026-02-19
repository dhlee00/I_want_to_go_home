using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemList : MonoBehaviour
{
    [SerializeField] List<Item> Item_List = new List<Item>();
    public Item GetItemData(int index) { return new Item(Item_List[index]); }

    [SerializeField] List<FCraftingRecipe> CraftingRecipe_List = new List<FCraftingRecipe>();

    public FCraftingRecipe GetCraftingRecipe(int inIndex)
    {
        FCraftingRecipe re = new FCraftingRecipe();

        foreach (FCraftingRecipe item in CraftingRecipe_List)
        {
            if(item.ResultItem.Item_Index == inIndex)
            {
                re = item;
                return re;
            }

        }

        return re;
    }


    public static ItemList Inst = null;

    void Awake()
    {
        if (Inst == null)
        {
            Inst = this;
        }

        for (int i = 0; i < GoogleSheetManager.SO<GoogleSheetSO>().Item_DataList.Count; i++)
        {
            ITEM_TYPE.TryParse(GoogleSheetManager.SO<GoogleSheetSO>().Item_DataList[i].ITEM_TYPE, out ITEM_TYPE itemType);

            Item item = new Item(GoogleSheetManager.SO<GoogleSheetSO>().Item_DataList[i].ITEM_NAME, itemType,
                GoogleSheetManager.SO<GoogleSheetSO>().Item_DataList[i].ITEM_DESC,
                GoogleSheetManager.SO<GoogleSheetSO>().Item_DataList[i].ITEM_AMOUNT,
                GoogleSheetManager.SO<GoogleSheetSO>().Item_DataList[i].ITEM_ICON_PATH,
                GoogleSheetManager.SO<GoogleSheetSO>().Item_DataList[i].ITEM_PREFAB,
                GoogleSheetManager.SO<GoogleSheetSO>().Item_DataList[i].ITEM_INDEX);
            Item_List.Add(item);
        }



        for (int i = 0; i < GoogleSheetManager.SO<GoogleSheetSO>().Craft_DataList.Count; i++)
        {
            // 제작될 아이템
            FCraftingRecipe item;
            item.ResultItem.Item_Index = GoogleSheetManager.SO<GoogleSheetSO>().Craft_DataList[i].ITEM_INDEX;
            item.ResultItem.Item_Amount = GoogleSheetManager.SO<GoogleSheetSO>().Craft_DataList[i].CRAFT_AMOUNT;

            item.IngredientsItemList = new List<FItemStack>();

            // 재료
            string[] ingredient = GoogleSheetManager.SO<GoogleSheetSO>().Craft_DataList[i].CRAFT_INGREDIENT.Split(",");

            for (int ii = 0; ii < ingredient.Length; ii++)
            {
                FItemStack itemStack = new FItemStack();

                // 인덱스
                itemStack.Item_Index = int.Parse(ingredient[ii]);

                ii++;
                // 갯수
                itemStack.Item_Amount = int.Parse(ingredient[ii]);

                item.IngredientsItemList.Add(itemStack);
            }

            CraftingRecipe_List.Add(item);
        }
    }
}
