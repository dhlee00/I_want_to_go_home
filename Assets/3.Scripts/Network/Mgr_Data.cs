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
        SaveItemInven();          // 인벤토리 아이템 저장
        SaveEquipItemInven();     // 인벤토리 장비 아이템 저장

        SaveEquipItemEquipSlot(); // 장착 슬롯 장비 아이템 저장
        SaveEquipSlot();          // 장착 슬롯 재료 아이템 저장
    }

    #region Inventory_Save
    public void SaveItemInven()
    {
        var allEntries = GlobalValue.User_Inventory.ToList(); // Dictionary → List<KeyValuePair>
        int chunkSize = 10;

        if (allEntries.Count == 0)
        {
            // 저장된 이전 데이터 모두 삭제
            var removeRequest = new UpdateUserDataRequest
            {
                KeysToRemove = new List<string>()
            };

            // Item_Inven_Part_1부터 최대 10 파트 정도라 가정
            for (int i = 1; i <= 10; i++)
            {
                removeRequest.KeysToRemove.Add($"Item_Inven_Part_{i}");
            }

            PlayFabClientAPI.UpdateUserData(removeRequest,
                (_result) =>
                {
                    Debug.Log("아이템 0개 상태 → 기존 인벤토리 데이터 삭제 완료(재료)");
                },
                (_error) =>
                {
                    Debug.Log("삭제 실패 : " + _error.GenerateErrorReport());
                });

            return; // 아래 저장 루프 실행하지 않음
        }

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

        if (allEntries.Count == 0)
        {
            // 저장된 이전 데이터 모두 삭제
            var removeRequest = new UpdateUserDataRequest
            {
                KeysToRemove = new List<string>()
            };

            // Item_Inven_Part_1부터 최대 10 파트 정도라 가정
            for (int i = 1; i <= 10; i++)
            {
                removeRequest.KeysToRemove.Add($"EquipItem_Inven_Part_{i}");
            }

            PlayFabClientAPI.UpdateUserData(removeRequest,
                (_result) =>
                {
                    Debug.Log("아이템 0개 상태 → 기존 인벤토리 데이터 삭제 완료(장비)");
                },
                (_error) =>
                {
                    Debug.Log("삭제 실패 : " + _error.GenerateErrorReport());
                });

            return; // 아래 저장 루프 실행하지 않음
        }

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

    #region Inven_Equip_Save
    void SaveEquipItemEquipSlot()
    {
        // 장착 슬롯 장비 저장
        #region
        var allEntries = GlobalValue.Equipment_EquipSlot;

        // 인벤이 비었을 때 → 기존 데이터 삭제
        if (allEntries.Count == 0)
        {
            var removeRequest = new UpdateUserDataRequest
            {
                KeysToRemove = new List<string> { "EquipItem_Inven" }
            };

            PlayFabClientAPI.UpdateUserData(removeRequest,
                (_result) => Debug.Log("장비 인벤토리 삭제 완료"),
                (_error) => Debug.Log("삭제 실패 : " + _error.GenerateErrorReport())
            );

            return;
        }

        // 리스트로 감싸서 한 번에 저장
        EquipInvenWrapper wrapper = new EquipInvenWrapper();
        foreach (var pair in allEntries)
        {
            wrapper.Items.Add(pair); // pair가 객체라면 그대로 저장
        }

        string json = JsonUtility.ToJson(wrapper);
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string> {
        { "EquipItem_EquipSlot", json }
    }
        };

        PlayFabClientAPI.UpdateUserData(request,
        (_result) =>
        {
            Debug.Log("장비 인벤토리 저장 완료");
        },
        (_error) =>
        {
            Debug.Log("저장 실패 : " + _error.GenerateErrorReport());
        });
        #endregion
    }

    void SaveEquipSlot()
    {
        // 장착 슬롯 재료 저장
        #region
        var allEntries = GlobalValue.User_EquipSlot.ToList(); // Dictionary → List<KeyValuePair>

        if (allEntries.Count == 0)
        {
            // 저장된 이전 데이터 모두 삭제
            var removeRequest = new UpdateUserDataRequest
            {
                KeysToRemove = new List<string>()
            };

            // Item_Inven_Part_1부터 최대 10 파트 정도라 가정
            for (int i = 1; i <= 10; i++)
            {
                removeRequest.KeysToRemove.Add($"Item_Inven_Part_{i}");
            }

            PlayFabClientAPI.UpdateUserData(removeRequest,
                (_result) =>
                {
                    Debug.Log("아이템 0개 상태 → 기존 인벤토리 데이터 삭제 완료");
                },
                (_error) =>
                {
                    Debug.Log("삭제 실패 : " + _error.GenerateErrorReport());
                });

            return; // 아래 저장 루프 실행하지 않음
        }

        InventoryDictWrapper wrapper = new InventoryDictWrapper();
        foreach (var pair in allEntries)
        {
            wrapper.Items.Add(new InventoryItemPair { key = pair.Key, value = pair.Value });
        }

        string json = JsonUtility.ToJson(wrapper);
        string keyName = $"Item_EquipSlot";

        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string> {
                { keyName, json }
            }
        };


        PlayFabClientAPI.UpdateUserData(request,
        (_result) =>
        {
            Debug.Log("아이템 리스트 저장 성공");
        },
        (_error) =>
        {
            Debug.Log("아이템 리스트 저장 실패 : " + _error.GenerateErrorReport());
        });
        #endregion
    }
    #endregion
    #endregion

    // 데이터 불러오기
    public void DataLoad()
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

                        if(serverData.Key.StartsWith("EquipItem_EquipSlot"))
                        {
                            LoadUserEquipSlotEquipment();
                        }

                        if(serverData.Key.StartsWith("Item_EquipSlot"))
                        {
                            LoadUserEquipSlotItem();
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
    #region Inven_Slot
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

    // 인벤토리 장비 아이템 불러오기
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
    #region Inven_EquipSlot
    // 장착 슬롯 장비 아이템 불러오기
    void LoadUserEquipSlotEquipment()
    {
        var request = new GetUserDataRequest();
        PlayFabClientAPI.GetUserData(request,
        result =>
        {
            // 저장된 데이터가 아예 없을 경우 (처음 실행 또는 데이터 삭제 상태)
            if (!result.Data.ContainsKey("EquipItem_EquipSlot"))
            {
                GlobalValue.Equipment_Inventory = new List<Item>(); // 빈 인벤
                Debug.Log("장비 인벤토리 데이터 없음 → 빈 리스트로 초기화");
                return;
            }

            string json = result.Data["EquipItem_EquipSlot"].Value;
            EquipInvenWrapper wrapper = JsonUtility.FromJson<EquipInvenWrapper>(json);

            if (wrapper != null && wrapper.Items != null)
            {
                GlobalValue.Equipment_EquipSlot = wrapper.Items;
            }
            else
            {
                GlobalValue.Equipment_EquipSlot = new List<Item>();
            }
             

            // 아이콘 로딩
            foreach (var item in GlobalValue.Equipment_EquipSlot)
            {
                item.Load_Image(item.Get_Item_IconPath);
            }

            Debug.Log($"장착 슬롯 장비 아이템 불러오기 완료! 총 {GlobalValue.Equipment_EquipSlot.Count}개");
        },
        error =>
        {
            Debug.LogError("불러오기 실패: " + error.GenerateErrorReport());
        });
    }

    // 장착 슬롯 재료 아이템 불러오기
    void LoadUserEquipSlotItem()
    {
        var request = new GetUserDataRequest();
        PlayFabClientAPI.GetUserData(request,
        result =>
        {
            // 데이터 없을 때
            if (!result.Data.ContainsKey("Item_EquipSlot"))
            {
                GlobalValue.User_EquipSlot = new Dictionary<int, Item>();
                Debug.Log("장비 슬롯 데이터 없음 → 빈 Dictionary로 초기화");
                return;
            }

            string json = result.Data["Item_EquipSlot"].Value;
            InventoryDictWrapper wrapper = JsonUtility.FromJson<InventoryDictWrapper>(json);

            // Wrapper → Dictionary 변환
            GlobalValue.User_EquipSlot = new Dictionary<int, Item>();

            if (wrapper != null && wrapper.Items != null)
            {
                foreach (var pair in wrapper.Items)
                {
                    GlobalValue.User_EquipSlot.Add(pair.key, pair.value);
                }
            }

            // 아이콘 로딩
            foreach (var item in GlobalValue.User_EquipSlot)
            {
                item.Value.Load_Image(item.Value.Get_Item_IconPath);
            }

            Debug.Log($"장착 슬롯 재료 아이템 불러오기 완료! 총 {GlobalValue.User_EquipSlot.Count}개");
        },
        error =>
        {
            Debug.LogError("불러오기 실패: " + error.GenerateErrorReport());
        });
    }
    #endregion
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