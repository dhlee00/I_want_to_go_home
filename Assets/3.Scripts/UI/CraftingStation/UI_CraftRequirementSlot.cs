using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_CraftRequirementSlot : MonoBehaviour
{
    [SerializeField] Image ItemIcon_Image;          // 아이템 이미지
    [SerializeField] TextMeshProUGUI ItemName_Text; // 아이템 이름
    [SerializeField] TextMeshProUGUI ItemAmount_Text; // 필요갯수 보유갯수

    public void SetUICraftRequirementSlot(Sprite inItemIcon, string inItemName, int inItemAmount, int inOwnedAmount)
    {
        ItemIcon_Image.sprite = inItemIcon;
        ItemName_Text.text = inItemName;
        ItemAmount_Text.text = $"{inOwnedAmount}/{inItemAmount}";
    }
}
