using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemList : MonoBehaviour
{
    [SerializeField] List<Item> Item_List = new List<Item>();
    public Item GetItemData(int index) { return new Item(Item_List[index]); }
    
    [SerializeField] List<Craft_Item> CraftItem_List = new List<Craft_Item>();
    int[] _ingredientIndex;
    int[] _ingredientAmount;

    public static ItemList Inst = null;

    void Awake()
    {
        if(Inst == null)
        {
            Inst = this;
        }

        for(int i = 0; i < GoogleSheetManager.SO<GoogleSheetSO>().Item_DataList.Count; i++)
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
            string[] ingredient = GoogleSheetManager.SO<GoogleSheetSO>().Craft_DataList[i].CRAFT_INGREDIENT.Split(",");

            _ingredientIndex = new int[ingredient.Length / 2];
            _ingredientAmount = new int[ingredient.Length / 2];

            int arrayIndex = 0;
            for (int ii = 0; ii < ingredient.Length; ii++)
            {
                if (ii % 2 == 0)
                {
                    _ingredientIndex[arrayIndex] = int.Parse(ingredient[ii]);
                }
                else
                {
                    _ingredientAmount[arrayIndex] = int.Parse(ingredient[ii]);
                }
            }

            Craft_Item craftItem = new Craft_Item(GoogleSheetManager.SO<GoogleSheetSO>().Craft_DataList[i].CRAFT_INDEX,
                GoogleSheetManager.SO<GoogleSheetSO>().Craft_DataList[i].CRAFT_NAME,
                GoogleSheetManager.SO<GoogleSheetSO>().Craft_DataList[i].ITEM_DATA,
                _ingredientIndex,
                _ingredientAmount);
            CraftItem_List.Add(craftItem);
        }

        for (int i = 0; i < CraftItem_List.Count; i++)
        {
            Debug.Log(CraftItem_List[i].Get_Craft_Name);
            Debug.Log("재료 " + CraftItem_List[i].Get_Craft_Ingredient[0]);
            Debug.Log("재료 수 " + CraftItem_List[i].Get_Ingredient_Amount[0]);
        }
    }
}
