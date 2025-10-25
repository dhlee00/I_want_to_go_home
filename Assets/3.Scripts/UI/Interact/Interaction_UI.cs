using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Interaction_UI : MonoBehaviour
{
    [SerializeField] Text ItemName_Text;
    [SerializeField] Image Item_Icon;
    [SerializeField] Image Change_Icon;

    // 상호작용 타입
    public EInteractionType InteractionType;

    // 아이템 타입을 가진 오브젝트
    public List<Interaction_Item> Item_Obj_List = new List<Interaction_Item>();

    

    
    public void UI_Update()
    {
        switch(InteractionType)
        {
            case EInteractionType.Itme:
                {
                    SetItmeInfo();
                    break;
                }
            
                    



        }


    }


    // 아이템일 경우 셋팅
    public void SetItmeInfo()
    {
        if (Item_Obj_List.Count <= 0) return;

        // 아이콘 설정
        Item_Icon.sprite = Item_Obj_List[0].ItemData.Get_Item_Icon;


        // 장비가 아니면 수량 표시
        if(Item_Obj_List[0].ItemData.Get_ItemType != ITEM_TYPE.EQUIPMENT)
        {
            int itemAmount = 0;

            foreach (Interaction_Item item in Item_Obj_List)
                itemAmount += item.ItemData.Get_Item_Amount;

            ItemName_Text.text = $"{Item_Obj_List[0].ItemData.Get_Item_Name} x{itemAmount}";
        }
        else // 장비면 이름만 표시
        {
            ItemName_Text.text = $"{Item_Obj_List[0].ItemData.Get_Item_Name}";
        }
        
    }



    public void Interaction()
    {
        switch (InteractionType)
        {
            case EInteractionType.Itme:
                {
                    foreach(Interaction_Item itme in Item_Obj_List)
                    {
                        itme.OnInteraction();
                    }


                    break;
                }





        }
    }

    public void Change(bool isChange)
    {
        Change_Icon.gameObject.SetActive(isChange);
    }
}
