using UnityEngine;

[System.Serializable]
public class Craft_Item
{
    int Craft_Index;
    public int Get_Craft_Index { get => Craft_Index; }

    int ItemData_Index;
    public int Get_ItemData_Index { get => ItemData_Index; }

    string Craft_Name;
    public string Get_Craft_Name { get => Craft_Name; }

    int[] Craft_Ingredient;
    public int[] Get_Craft_Ingredient { get => Craft_Ingredient; }

    int[] Ingredient_Amount;
    public int[] Get_Ingredient_Amount { get => Ingredient_Amount; }

    public Craft_Item(int _index, string _name, int _item, int[] _ingredient, int[] _ingredientAmount)
    {
        Craft_Index = _index;
        Craft_Name = _name;
        ItemData_Index = _item;
        Craft_Ingredient = _ingredient;
        Ingredient_Amount = _ingredientAmount;
    }
}
