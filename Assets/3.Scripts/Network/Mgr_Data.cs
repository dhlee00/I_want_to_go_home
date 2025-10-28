using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Mgr_Data : MonoBehaviour
{
    public static Mgr_Data Inst;

    void Awake()
    {
        Inst = this;
    }

    // 데이터 저장
    #region Before
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
    #endregion

    // 데이터 저장
    #region Inven_Save
    public void SaveInven()
    {
        SaveItemInven();
        SaveEquipItemInven();
    }

    public void SaveItemInven()
    {
        var allEntries = GlobalValue.User_Inventory.ToList(); // Dictionary → List<KeyValuePair>
        int chunkSize = 10;

        int totalChunks = Mathf.CeilToInt(allEntries.Count / (float)chunkSize);

        for (int i = 0; i < totalChunks; i++)
        {
            var chunk = allEntries.Skip(i * chunkSize).Take(chunkSize);

            InventoryDictWrapper wrapper = new InventoryDictWrapper();
            foreach (var pair in chunk)
            {
                wrapper.Items.Add(new InventoryItemPair { key = pair.Key, value = pair.Value });
            }

            string json = JsonUtility.ToJson(wrapper);
            string keyName = $"Item_Inven_Part_{i + 1}";

            var request = new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string> {
                { keyName, json }
            }
            };

            // NetWaitTime = 0.5f;

            PlayFabClientAPI.UpdateUserData(request,
            (_result) =>
            {
                Debug.Log("아이템 리스트 저장 성공");
            },
            (_error) =>
            {
                Debug.Log("아이템 리스트 저장 실패 : " + _error.GenerateErrorReport());
            });
        }
    }

    public void SaveEquipItemInven()
    {
        var allEntries = GlobalValue.Equipment_Inventory;
        int chunkSize = 10;

        int totalChunks = Mathf.CeilToInt(allEntries.Count / (float)chunkSize);

        for (int i = 0; i < totalChunks; i++)
        {
            var chunk = allEntries.Skip(i * chunkSize).Take(chunkSize);

            EquipInvenWrapper wrapper = new EquipInvenWrapper();
            foreach (var pair in chunk)
            {
                wrapper.Items.Add(pair);
            }

            string json = JsonUtility.ToJson(wrapper);
            string keyName = $"EquipItem_Inven_Part_{i + 1}";

            var request = new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string> {
                { keyName, json }
            }
            };

            // NetWaitTime = 0.5f;

            PlayFabClientAPI.UpdateUserData(request,
            (_result) =>
            {
                Debug.Log("아이템 리스트 저장 성공");
            },
            (_error) =>
            {
                Debug.Log("아이템 리스트 저장 실패 : " + _error.GenerateErrorReport());
            });
        }
    }
    #endregion

    // 데이터 불러오기
    public void TestLoad()
    {
        List<string> InvenItem_Data = new List<string>();
        List<string> InvenEquipment_Data = new List<string>();

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
                        if (serverData.Key.StartsWith("Item_Inven_Part_"))
                        {
                            InvenItem_Data.Add(serverData.Key);
                        }

                        if(serverData.Key.StartsWith("EquipItem_Inven_Part_"))
                        {
                            InvenEquipment_Data.Add(serverData.Key);
                        }
                    }

                    #region Before
                    // 반장님 코드
                    //foreach (var serverData in result.Data)
                    //{
                    //    // 아이템 데이터 (키값이 "ItemIndex_"로 시작하면 불러오기)
                    //    if (serverData.Key.StartsWith("ItemIndex_"))
                    //    {
                    //        // JSON을 Item 객체로 변환
                    //        string itemData = serverData.Value.Value;
                    //        Item item = JsonUtility.FromJson<Item>(itemData);

                    //        // 아이템 추가
                    //        GlobalValue.User_Inventory.Add(item.Get_Item_Index, item);
                    //    }
                    //}
                    #endregion
                    LoadUserInvenFromChunks(InvenItem_Data);
                    LoadUserEquipInvenFromChunks(InvenEquipment_Data);

                    Debug.Log("데이터 불러오기 성공");
                }
            },
            error =>
            {
                Debug.Log($"데이터 불러오기 실패 : {error.GenerateErrorReport()}");
            }
        );
    }

    #region Item_load
    // 재료, 음식 아이템 불러오기
    void LoadUserInvenFromChunks(List<string> _keys)
    {
        var request = new GetUserDataRequest();
        PlayFabClientAPI.GetUserData(request,
        result =>
        {
            Dictionary<int, Item> loadedDict = new Dictionary<int, Item>();

            foreach (string key in _keys)
            {
                if (result.Data.ContainsKey(key))
                {

                    string json = result.Data[key].Value;
                    InventoryDictWrapper wrapper = JsonUtility.FromJson<InventoryDictWrapper>(json);

                    foreach (var pair in wrapper.Items)
                    {
                        loadedDict[pair.key] = pair.value;
                        loadedDict[pair.key].Load_Image(pair.value.Get_Item_IconPath);
                    }
                }
            }

            GlobalValue.User_Inventory = loadedDict;
        },
        error =>
        Debug.LogError("불러오기 실패: " + error.GenerateErrorReport())
        );
    }

    private void LoadUserEquipInvenFromChunks(List<string> _keys)
    {
        var request = new GetUserDataRequest();
        PlayFabClientAPI.GetUserData(request,
        result =>
        {
            List<Item> loadedList = new List<Item>();

            foreach (var key in _keys)
            {
                if (result.Data.ContainsKey(key))
                {
                    string json = result.Data[key].Value;
                    EquipInvenWrapper wrapper = JsonUtility.FromJson<EquipInvenWrapper>(json);

                    if (wrapper != null && wrapper.Items != null)
                        loadedList.AddRange(wrapper.Items);
                }
            }

            GlobalValue.Equipment_Inventory = loadedList;
            Debug.Log(GlobalValue.Equipment_Inventory.Count);
            for (int i = 0; i < loadedList.Count; i++)
            {
                loadedList[i].Load_Image(loadedList[i].Get_Item_IconPath);
            }
        },
        error =>
        Debug.LogError("불러오기 실패: " + error.GenerateErrorReport())
        );
    }
    #endregion
}

#region Wrapper_Json
[System.Serializable]
public class InventoryItemPair
{
    public int key;
    public Item value;
}

[System.Serializable]
public class InventoryDictWrapper
{
    public List<InventoryItemPair> Items = new List<InventoryItemPair>();
}

public class EquipInvenWrapper
{
    public List<Item> Items = new List<Item>();
}
#endregion