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

    // 테스트를 위한 데이터 저장 (추후에 이동시키거나 삭제)
    public async Task TestSave()
    {
        // 완료 변수
        var saveDataComplete = new TaskCompletionSource<bool>();

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
                        { saveItem.Get_Item_Index.ToString(), jsonData }
                    }
                },
                result =>
                {
                    Debug.Log("데이터 저장 성공");
                    saveDataComplete.SetResult(true);
                },
                error =>
                {
                    Debug.Log("데이터 저장 실패 " + error.GenerateErrorReport());
                    saveDataComplete.SetException(new Exception(error.ErrorMessage));
                }
            );
        }

        // 저장 완료까지 대기
        await saveDataComplete.Task;
    }

    // 테스트용 유저 데이터 불러오기 (추후에 이동시키거나 삭제)
    public async Task TestLoad()
    {
        // 완료 변수
        var loadDataComplete = new TaskCompletionSource<bool>();

        PlayFabClientAPI.GetUserData
        (
            new GetUserDataRequest(),
            result =>
            {
                if (result.Data != null)
                {
                    // 기존 인벤토리 초기화
                    GlobalValue.User_Inventory.Clear();

                    foreach (var jsonData in result.Data)
                    {
                        // 저장된 JSON 문자열
                        string serverItem = jsonData.Value.Value;

                        // JSON → Item 객체
                        Item item = JsonUtility.FromJson<Item>(serverItem);

                        GlobalValue.User_Inventory.Add(item.Get_Item_Index, item);
                    }

                    Debug.Log("인벤토리 불러오기 성공");
                    loadDataComplete.SetResult(true);
                }
            },
            error =>
            {
                Debug.Log("인벤토리 불러오기 실패: " + error.GenerateErrorReport());
                loadDataComplete.SetException(new Exception(error.ErrorMessage));
            }
        );

        // 데이터 로드 완료까지 대기
        await loadDataComplete.Task;
    }
}
