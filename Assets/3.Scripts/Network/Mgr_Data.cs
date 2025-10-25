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

    // 데이터 저장
    public void TestSave()
    {
        // 보유한 아이템 종류들만큼 반복
        for (int itemCount = 0; itemCount < GlobalValue.User_Inventory.Count; itemCount++)
        {
            Item saveItem = GlobalValue.User_Inventory[itemCount];
            string jsonData = JsonUtility.ToJson(saveItem);

            PlayFabClientAPI.UpdateUserData
            (
                new UpdateUserDataRequest()
                {
                    // 아이템 정보 json 저장
                    Data = new Dictionary<string, string>()
                    {
                        { $"ItemIndex_{saveItem.Get_Item_Index.ToString()}", jsonData }
                    }
                },
                result =>
                {
                    Debug.Log("데이터 저장 성공");
                },
                error =>
                {
                    Debug.Log($"데이터 저장 실패 : {error.GenerateErrorReport()}");
                }
            );
        }
    }

    // 데이터 불러오기
    public void TestLoad()
    {
        // 데이터 불러오기
        PlayFabClientAPI.GetUserData
        (
            new GetUserDataRequest(),
            result =>
            {
                if (result.Data != null)
                {
                    // 기존 인벤토리 초기화
                    GlobalValue.User_Inventory.Clear();

                    foreach (var serverData in result.Data)
                    {
                        // 아이템 데이터 (키값이 "ItemIndex_"로 시작하면 불러오기)
                        if (serverData.Key.StartsWith("ItemIndex_"))
                        {
                            // JSON을 Item 객체로 변환
                            string itemData = serverData.Value.Value;
                            Item item = JsonUtility.FromJson<Item>(itemData);

                            // 아이템 추가
                            GlobalValue.User_Inventory.Add(item.Get_Item_Index, item);
                        }
                    }

                    Debug.Log("데이터 불러오기 성공");
                }
            },
            error =>
            {
                Debug.Log($"데이터 불러오기 실패 : {error.GenerateErrorReport()}");
            }
        );
    }
}
