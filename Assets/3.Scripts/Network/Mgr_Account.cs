using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class Mgr_Account : MonoBehaviour
{
    async void Awake()
    {
        await LoginUSG();
        LoginPlayFab();
    }

    // UGS 로그인
    async Task LoginUSG()
    {
        // Unity Service 초기화
        await UnityServices.InitializeAsync();

        // 세션 초기화 (테스트에서만)
        AuthenticationService.Instance.ClearSessionToken();

        // 익명 로그인
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        Debug.Log("UGS 로그인 완료");
    }

    // PlayFab 로그인
    void LoginPlayFab()
    {
        // PlayFab 커스텀 로그인
        PlayFabClientAPI.LoginWithCustomID
        (
            new LoginWithCustomIDRequest
            {
                CustomId = $"user_{Guid.NewGuid().ToString()}",  // 랜덤 번호로 로그인
                //CustomId = SystemInfo.deviceUniqueIdentifier,      // 기기 고유 번호로 로그인
                CreateAccount = true,
                InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
                {
                    GetPlayerProfile = true
                }
            },
            result =>
            {
                Debug.Log("PlayFab 로그인 완료");

                // 닉네임이 있다면 불러오기
                if (!string.IsNullOrEmpty(result.InfoResultPayload.PlayerProfile.DisplayName))
                {
                    Debug.Log("닉네임 불러오기 완료");
                    GlobalValue.Nickname = result.InfoResultPayload.PlayerProfile.DisplayName;
                }

                // 없다면 생성해서 저장하기
                else
                {
                    Debug.Log("닉네임 없음");

                    GlobalValue.Nickname = "테스트플레이어";
                    PlayFabClientAPI.UpdateUserTitleDisplayName
                    (
                        new UpdateUserTitleDisplayNameRequest
                        {
                            DisplayName = GlobalValue.Nickname
                        },
                        result =>
                        {
                            Debug.Log("닉네임 생성 완료");
                        },
                        error =>
                        {
                            Debug.LogError($"닉네임 생성 실패 : {error.GenerateErrorReport()}");
                        }
                    );
                }

                // 데이터 불러오기
                Mgr_Data.Inst.TestLoad();
            },
            error =>
            {
                Debug.LogError($"로그인 실패 : {error.GenerateErrorReport()}");
            }
        );
    }
}
