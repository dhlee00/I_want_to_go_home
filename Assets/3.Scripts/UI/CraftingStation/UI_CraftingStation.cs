using NUnit.Framework.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Unity.Burst.Intrinsics.X86.Avx;

public class UI_CraftingStation : MonoBehaviour
{
    [Header("Close_Button")]
    [SerializeField] Button CraftingStationClose_Button;

    [Header("CraftingListPanel")]
    [SerializeField] GameObject CraftingListContent;        // 제작 아이템 리스트 패널
    [SerializeField] UI_CraftingItem_Button UI_CraftingItem_Button_Prefab;  // 버튼 프리팹

    [Header("CraftingDetailPanel")]
    [SerializeField] GameObject CraftingDetailPanel;        // 제작에 필요한 아이템 정보 패널

    [SerializeField] Image CraftingItme_Image;              // 제작 아이템 이미지
    [SerializeField] TextMeshProUGUI CraftingItmeName_Text; // 제작 아이템 이름 + 갯수 텍스트

    [SerializeField] GameObject CraftingDetailContent;      // 재료 아이템 리스트 패널
    [SerializeField] UI_CraftRequirementSlot CraftRequirementSlot_Prefab; // 재료 아이템 리스트 프리팹


    [SerializeField] Button Make_Button;                    // 제작버튼

    Interaction_CraftingStation NowCraftingStation; // 제작대 오브젝트

    // 현재 선택된 제작 아이템 정보를 담는 변수
    FCraftingRecipe SelectedCraftItem;

    public void Start()
    {
        // 닫기 버튼
        if (CraftingStationClose_Button)
            CraftingStationClose_Button.onClick.AddListener(() => { Mgr_Game.Inst.OpenCraftingStationUI(false); });

        // Make_Button 버튼 활성화
        if (Make_Button)
            Make_Button.onClick.AddListener(ClickMakeButton);
    }

    public void SetUICraftingStation(Interaction_CraftingStation inCraftingStationData)
    {
        // 리스트 초기화
        {
            foreach (Transform child in CraftingListContent.transform)
            {
                Destroy(child.gameObject);
            }
            foreach (Transform child in CraftingDetailContent.transform)
            {
                Destroy(child.gameObject);
            }
        }

        // 제작 우측 패널 닫기 (리스트 버튼 클릭을 통해 켜질 예정)
        SetCraftingDetailPanel(false);

        NowCraftingStation = inCraftingStationData;

        // 제작대의 제작가능한 리스트
        foreach (FCraftingRecipe data in inCraftingStationData.CraftingRecipe)
        {
            GameObject obj = Instantiate(UI_CraftingItem_Button_Prefab.gameObject, CraftingListContent.transform);
            UI_CraftingItem_Button btn = obj.GetComponent<UI_CraftingItem_Button>();

            btn.SetUICraftingItemButton(this, data);
        }

    }

    // 제작아이템 버튼을 클릭했을때의 우측 패널 셋팅
    public void SetCraftingDetailPanel(bool isOn, UI_CraftingItem_Button inButton = null)
    {
        CraftingDetailPanel.SetActive(isOn);

        if (!isOn || inButton == null) return;

        foreach (Transform child in CraftingDetailContent.transform)
        {
            Destroy(child.gameObject);
        }

        // 제작 아이템 데이터
        Item craftingItem = ItemList.Inst.GetItemData(inButton.CraftingRecipe.ResultItem.Item_Index);

        // 제작 아이템 이미지
        CraftingItme_Image.sprite = craftingItem.Get_Item_Icon;

        // 제작 아이템 이름 + 갯수 텍스트
        CraftingItmeName_Text.text = $"{craftingItem.Get_Item_Name} x{inButton.CraftingRecipe.ResultItem.Item_Amount}";


        foreach (FItemStack data in inButton.CraftingRecipe.IngredientsItemList)
        {
            GameObject obj = Instantiate(CraftRequirementSlot_Prefab.gameObject, CraftingDetailContent.transform);
            UI_CraftRequirementSlot slot = obj.GetComponent<UI_CraftRequirementSlot>();

            Item item = ItemList.Inst.GetItemData(data.Item_Index);

            slot.SetUICraftRequirementSlot(item.Get_Item_Icon, item.Get_Item_Name, data.Item_Amount, Mgr_Inventory.Inst.FindInventoryItme(item.Get_Item_Index));
        }


        // 선택된 조합식 저장
        SelectedCraftItem = inButton.CraftingRecipe;
    }


    // Make_Button 버튼
    void ClickMakeButton()
    {
        // 제작이 가능한지 체크
        foreach(FItemStack item in SelectedCraftItem.IngredientsItemList)
        {
            if (item.Item_Amount > Mgr_Inventory.Inst.FindInventoryItme(item.Item_Index))
            {
                Debug.Log($"제작 실패 (갯수 부족) Item_Index[{item.Item_Index}]");
                return;
            }
        }

        // 아이템 제거
        foreach (FItemStack item in SelectedCraftItem.IngredientsItemList)
        {
            Mgr_Inventory.Inst.UseInventoryItem(item.Item_Index, item.Item_Amount);
        }

        // 아이템 제작 시작
        Item itme = ItemList.Inst.GetItemData(SelectedCraftItem.ResultItem.Item_Index);
        itme.Get_Item_Amount = SelectedCraftItem.ResultItem.Item_Amount;

        NowCraftingStation.StartCraftItem(itme);

    }
}
