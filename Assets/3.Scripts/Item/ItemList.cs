using System.Collections.Generic;
using UnityEngine;

public class ItemList : MonoBehaviour
{
    public List<Item> Item_List = new List<Item>();
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

            Item item = new Item(GoogleSheetManager.SO<GoogleSheetSO>().Item_DataList[i].ITEM_INDEX, GoogleSheetManager.SO<GoogleSheetSO>().Item_DataList[i].ITEM_NAME, itemType,
                GoogleSheetManager.SO<GoogleSheetSO>().Item_DataList[i].ITEM_DESC, GoogleSheetManager.SO<GoogleSheetSO>().Item_DataList[i].ITEM_AMOUNT, GoogleSheetManager.SO<GoogleSheetSO>().Item_DataList[i].ITEM_ICON_PATH);
            Item_List.Add(item);
        }
    }
}
