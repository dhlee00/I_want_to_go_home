using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>You must approach through `GoogleSheetManager.SO<GoogleSheetSO>()`</summary>
public class GoogleSheetSO : ScriptableObject
{
	public List<Item_Data> Item_DataList;
}

[Serializable]
public class Item_Data
{
	public int ITEM_INDEX;
	public string ITEM_NAME;
	public string ITEM_TYPE;
	public int ITEM_AMOUNT;
	public string ITEM_DESC;
	public string ITEM_ICON_PATH;
}

