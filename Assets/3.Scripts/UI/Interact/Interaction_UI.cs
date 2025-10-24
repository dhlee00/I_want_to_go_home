using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Interaction_UI : MonoBehaviour
{
    [SerializeField] Text ItemName_Text;
    [SerializeField] Image Item_Icon;
    [SerializeField] Image Change_Icon;

    // ��ȣ�ۿ� Ÿ��
    public EInteractionType InteractionType;

    // ������ Ÿ���� ���� ������Ʈ
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


    // �������� ��� ����
    public void SetItmeInfo()
    {
        if (Item_Obj_List.Count <= 0) return;

        // ������ ����
        Item_Icon.sprite = Item_Obj_List[0].ItemData.Get_Item_Icon;


        // ��� �ƴϸ� ���� ǥ��
        if(Item_Obj_List[0].ItemData.Get_ItemType != ITEM_TYPE.EQUIPMENT)
        {
            int itemAmount = 0;

            foreach (Interaction_Item item in Item_Obj_List)
                itemAmount += item.ItemData.Get_Item_Amount;

            ItemName_Text.text = $"{Item_Obj_List[0].ItemData.Get_Item_Name} x{itemAmount}";
        }
        else // ���� �̸��� ǥ��
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
