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
	public string ITEM_NAME;
	public int AMOUNT;
}

