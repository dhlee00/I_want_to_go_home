using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class UI_IceMelter : MonoBehaviour
{
    [Header("Water")]
    [SerializeField] Image Water_Image;
    [SerializeField] TextMeshProUGUI Water_Text;

    [Header("Ice")]
    public int Ice_ItemIndex = 4;
    [SerializeField] Image IceIcon_Image;
    [SerializeField] TextMeshProUGUI IceIcon_Text;

    [Header("Pail")]
    public int Pail_ItemIndex = 9;
    public int WaterPail_ItemIndex = 12;
    [SerializeField] Image PailIcon_Image;
    [SerializeField] TextMeshProUGUI PailIcon_Text;

    // 물 게이지
    [Header("Gauge")]
    [SerializeField] float Max_WaterGauge = 100f;
    [SerializeField] float WaterGauge = 0f;

    [SerializeField] float IceToWater_Amount = 10f;
    [SerializeField] float WaterToPail_Amount = 15f;


    // UI 업데이트
    public void UIUpdate()
    {
        // 아이콘 이미지 설정
        IceIcon_Image.sprite = ItemList.Inst.GetItemData(Ice_ItemIndex).Get_Item_Icon;
        PailIcon_Image.sprite = ItemList.Inst.GetItemData(Pail_ItemIndex).Get_Item_Icon;

        // 물게이지
        Water_Image.fillAmount = WaterGauge / Max_WaterGauge;
        Water_Text.text = $"{WaterGauge}/{Max_WaterGauge}";

        // 얼음
        IceIcon_Text.text = $"{Mgr_Inventory.Inst?.FindInventoryItme(Ice_ItemIndex)}";

        // 양동이
        PailIcon_Text.text = $"{Mgr_Inventory.Inst?.FindInventoryItme(Pail_ItemIndex)}";
    }

    #region 버튼 클릭 함수
    // 투입 버튼 클릭(얼음)
    public void OnIceButtonClick()
    {
        if(Mgr_Inventory.Inst?.FindInventoryItme(Ice_ItemIndex) <= 0)
        {
            Debug.Log("얼음 갯수 부족");
            return;
        }
        
        if(WaterGauge + IceToWater_Amount > Max_WaterGauge)
        {
            Debug.Log("물 게이지 초과");
            return;
        }

        // 얼음 제거 후 물 게이지 더하기
        Mgr_Inventory.Inst.UseInventoryItem(Ice_ItemIndex, 1);
        WaterGauge += IceToWater_Amount;

        UIUpdate();
    }

    // 추출 버튼 클릭 (양동이)
    public void OnPailButtonClick()
    {
        if (Mgr_Inventory.Inst?.FindInventoryItme(Pail_ItemIndex) <= 0)
        {
            Debug.Log("양동이 갯수 부족");
            return;
        }

        if (WaterGauge - WaterToPail_Amount < 0f)
        {
            Debug.Log("물 게이지 부족");
            return;
        }

        // 양동이 제거 후 물양동이 추가
        Mgr_Inventory.Inst.UseInventoryItem(Pail_ItemIndex, 1);
        WaterGauge -= WaterToPail_Amount;

        GlobalValue.AddItme(ItemList.Inst.GetItemData(WaterPail_ItemIndex));


        UIUpdate();
    }

    // 창닫기 버튼 클릭
    public void OnCloseButtonClick()
    {
        Mgr_Game.Inst.OpenIceMelterUI(false);
    }
    #endregion
}
