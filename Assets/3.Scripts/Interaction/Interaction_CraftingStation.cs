using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Interaction_CraftingStation : Interaction
{
    [Header("Interaction_CraftingStation")]
    // 아이템 조합법 구조체
    public Image CraftProgressCircle_Image;
    public List<FCraftingRecipe> CraftingRecipe = new List<FCraftingRecipe>();  // 제작 가능한 아이템 리스트
    public bool IsCraftItem = false;    // 아이템이 제작중인지 체크
    bool IsPlayerCraftItem = false;
    Item MakeItem;  // 만들어질 아이템

    private void Awake()
    {
        InteractionType = EInteractionType.craftingStation;

        CraftProgressCircle_Image.gameObject.SetActive(false);
    }

    private void Update()
    {
        if(IsPlayerCraftItem)
        {
            // 아이템 제작
            CraftProgressCircle_Image.fillAmount -= ((Player_Ctrl.LocalPlayer.Craft_Speed / 100f) * Time.deltaTime);

            // 제작 완료시
            if (CraftProgressCircle_Image.fillAmount <= 0f)
            {
                Debug.Log("제작 완료");
                IsCraftItem = false;
                IsPlayerCraftItem = false;
                CraftProgressCircle_Image.gameObject.SetActive(false);
                GlobalValue.AddItme(MakeItem);

                Player_Ctrl.LocalPlayer.AE_CraftItem(false);
                Mgr_Game.Inst.bCanMove = true;
            }
        }
    }

    public void StartCraftItem(Item inMakeItem)
    {
        Debug.Log("제작 시작");
        Mgr_Game.Inst.OpenCraftingStationUI(false, this); // UI닫기

        IsCraftItem = true;
        MakeItem = inMakeItem;
        CraftProgressCircle_Image.gameObject.SetActive(true);
        CraftProgressCircle_Image.fillAmount = 1f;
    }

    // 상호작용 했을때
    public override void OnInteraction()
    {
        if(IsCraftItem)
        {
            IsPlayerCraftItem = !IsPlayerCraftItem;

            if (IsPlayerCraftItem)
            {
                Player_Ctrl.LocalPlayer.AE_CraftItem(true);
                Mgr_Game.Inst.bCanMove = false;
            }
            else
            {
                Player_Ctrl.LocalPlayer.AE_CraftItem(false);
                Mgr_Game.Inst.bCanMove = true;
            }

                
        }
        else
        {
            // 제작대 UI 출력
            Mgr_Game.Inst.OpenCraftingStationUI(true, this);
        }
        
    }

}
