using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Mgr_Data : MonoBehaviour
{
    public static Mgr_Data Inst;

    void Awake()
    {
        Inst = this;
    }

    // �׽�Ʈ�� ���� ������ ���� (���Ŀ� �̵���Ű�ų� ����)
    public async Task TestSave()
    {
        // �Ϸ� ����
        var saveDataComplete = new TaskCompletionSource<bool>();

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
                        { saveItem.Get_Item_Index.ToString(), jsonData }
                    }
                },
                result =>
                {
                    Debug.Log("������ ���� ����");
                    saveDataComplete.SetResult(true);
                },
                error =>
                {
                    Debug.Log("������ ���� ���� " + error.GenerateErrorReport());
                    saveDataComplete.SetException(new Exception(error.ErrorMessage));
                }
            );
        }

        // ���� �Ϸ���� ���
        await saveDataComplete.Task;
    }

    // �׽�Ʈ�� ���� ������ �ҷ����� (���Ŀ� �̵���Ű�ų� ����)
    public async Task TestLoad()
    {
        // �Ϸ� ����
        var loadDataComplete = new TaskCompletionSource<bool>();

        PlayFabClientAPI.GetUserData
        (
            new GetUserDataRequest(),
            result =>
            {
                if (result.Data != null)
                {
                    // ���� �κ��丮 �ʱ�ȭ
                    GlobalValue.User_Inventory.Clear();

                    foreach (var jsonData in result.Data)
                    {
                        // ����� JSON ���ڿ�
                        string serverItem = jsonData.Value.Value;

                        // JSON �� Item ��ü
                        Item item = JsonUtility.FromJson<Item>(serverItem);

                        GlobalValue.User_Inventory.Add(item.Get_Item_Index, item);
                    }

                    Debug.Log("�κ��丮 �ҷ����� ����");
                    loadDataComplete.SetResult(true);
                }
            },
            error =>
            {
                Debug.Log("�κ��丮 �ҷ����� ����: " + error.GenerateErrorReport());
                loadDataComplete.SetException(new Exception(error.ErrorMessage));
            }
        );

        // ������ �ε� �Ϸ���� ���
        await loadDataComplete.Task;
    }
}
