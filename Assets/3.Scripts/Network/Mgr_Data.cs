using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using UnityEngine;

public class Mgr_Data : MonoBehaviour
{
    public static Mgr_Data Inst;

    void Awake()
    {
        Inst = this;
    }

    // ������ ����
    public void TestSave()
    {
        // ������ ������ �����鸸ŭ �ݺ�
        for (int itemCount = 0; itemCount < GlobalValue.User_Inventory.Count; itemCount++)
        {
            Item saveItem = GlobalValue.User_Inventory[itemCount];
            string jsonData = JsonUtility.ToJson(saveItem);

            PlayFabClientAPI.UpdateUserData
            (
                new UpdateUserDataRequest()
                {
                    // ������ ���� json ����
                    Data = new Dictionary<string, string>()
                    {
                        { $"ItemIndex_{saveItem.Get_Item_Index.ToString()}", jsonData }
                    }
                },
                result =>
                {
                    Debug.Log("������ ���� ����");
                },
                error =>
                {
                    Debug.Log($"������ ���� ���� : {error.GenerateErrorReport()}");
                }
            );
        }
    }

    // ������ �ҷ�����
    public void TestLoad()
    {
        // ������ �ҷ�����
        PlayFabClientAPI.GetUserData
        (
            new GetUserDataRequest(),
            result =>
            {
                if (result.Data != null)
                {
                    // ���� �κ��丮 �ʱ�ȭ
                    GlobalValue.User_Inventory.Clear();

                    foreach (var serverData in result.Data)
                    {
                        // ������ ������ (Ű���� "ItemIndex_"�� �����ϸ� �ҷ�����)
                        if (serverData.Key.StartsWith("ItemIndex_"))
                        {
                            // JSON�� Item ��ü�� ��ȯ
                            string itemData = serverData.Value.Value;
                            Item item = JsonUtility.FromJson<Item>(itemData);

                            // ������ �߰�
                            GlobalValue.User_Inventory.Add(item.Get_Item_Index, item);
                        }
                    }

                    Debug.Log("������ �ҷ����� ����");
                }
            },
            error =>
            {
                Debug.Log($"������ �ҷ����� ���� : {error.GenerateErrorReport()}");
            }
        );
    }
}
