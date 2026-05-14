using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class UI_WaterOxidizer : MonoBehaviour
{
    [Header("Water")]
    [SerializeField] Image Water_Image;
    [SerializeField] TextMeshProUGUI Water_Text;


    [Header("WaterPail")]
    public int WaterPail_ItemIndex = 12;
    [SerializeField] Image WaterPail_Icon_Image;
    [SerializeField] TextMeshProUGUI WaterPail_Icon_Text;
    [SerializeField] TextMeshProUGUI Oxidizer_Text;

    [Header("WaterGauge")]
    [SerializeField] float Max_WaterGauge = 100f;
    [SerializeField] float WaterGauge = 0f;
    


    // 물이 채워량
    [SerializeField] float Water_In_Amount = 15f;

    // 초당 물 소모량
    [SerializeField] float waterConsumeRate = 0.5f;
    // 산소 생성량
    [SerializeField] float oxygenGenRate = 10f;


    void Update()
    {
        // 산소게이지
        Oxidizer_Text.text = $"산소 / 최대량\n{Mgr_BaseCamp.Inst.OxygenAmount:F1}/{Mgr_BaseCamp.Inst.Max_OxygenAmount:F1}";
        
        // 물게이지
        Water_Image.fillAmount = WaterGauge / Max_WaterGauge;
        Water_Text.text = $"{WaterGauge:F1}/{Max_WaterGauge:F1}";

        if (Mgr_BaseCamp.Inst != null)
        {
            // 물이 남아 있고 && 베이스캠프의 현재 산소량이 최대치보다 적은지 확인
            if (WaterGauge > 0f && Mgr_BaseCamp.Inst.OxygenAmount < Mgr_BaseCamp.Inst.Max_OxygenAmount)
            {
                //  소모/생성할 양 계산
                float consumeThisFrame = waterConsumeRate * Time.deltaTime;
                float produceThisFrame = oxygenGenRate * Time.deltaTime;

                // 남은 물보다 더 많이 소모하려는 경우 처리
                if (consumeThisFrame > WaterGauge)
                {
                    // 남은 물의 비율만큼만 산소를 생성하도록 보정
                    float ratio = WaterGauge / consumeThisFrame;
                    consumeThisFrame = WaterGauge;
                    produceThisFrame *= ratio;
                }

                // 3. 수치 반영
                WaterGauge -= consumeThisFrame;

                // 산소량 갱신 (Mathf.Clamp를 사용해 0 ~ Max 사이로 고정)
                Mgr_BaseCamp.Inst.OxygenAmount += produceThisFrame;
            }
        }
    }

    public void UIUpdate()
    {
        // 물양동이 아이콘
        WaterPail_Icon_Image.sprite = ItemList.Inst.GetItemData(WaterPail_ItemIndex).Get_Item_Icon;

        // 물양동이 보유 개수
        WaterPail_Icon_Text.text = $"{Mgr_Inventory.Inst?.FindInventoryItme(WaterPail_ItemIndex)}";

        
    }

    #region 버튼 클릭 함수
    // 투입 버튼 클릭(얼음)
    public void OnWaterPailButtonClick()
    {
        if (Mgr_Inventory.Inst?.FindInventoryItme(WaterPail_ItemIndex) <= 0)
        {
            Debug.Log("물양동이 갯수 부족");
            return;
        }

        if (WaterGauge + Water_In_Amount > Max_WaterGauge)
        {
            Debug.Log("물 게이지 초과");
            return;
        }

        // 얼음 제거 후 물 게이지 더하기
        Mgr_Inventory.Inst.UseInventoryItem(WaterPail_ItemIndex, 1);
        WaterGauge += Water_In_Amount;

        UIUpdate();
    }

    // 창닫기 버튼 클릭
    public void OnCloseButtonClick()
    {
        Mgr_Game.Inst.OpenWaterOxidizerUI(false);
    }
    #endregion
}
