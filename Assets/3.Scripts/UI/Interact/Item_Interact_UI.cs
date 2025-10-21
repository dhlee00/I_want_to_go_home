using UnityEngine;
using UnityEngine.UI;

public class Item_Interact_UI : MonoBehaviour
{
    [SerializeField] Text ItemName_Text;
    [SerializeField] Image Item_Icon;

    [SerializeField] Interaction interaction;
    public Interaction Get_interaction { get => interaction; }

    public void Set_ItemInfo(string _name, Sprite _sprite, int _amount, Interaction _interaction)
    {
        ItemName_Text.text = _name;
        Item_Icon.sprite = _sprite;
        ItemName_Text.text += $" x{_amount}";
        interaction = _interaction;
    }
}
