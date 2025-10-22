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
        await LoginPlayFab();
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
    }

    // PlayFab 로그인
    async Task LoginPlayFab()
    {
        // 완료 변수
        var loginComplete = new TaskCompletionSource<bool>();

        // PlayFab 커스텀 로그인
        PlayFabClientAPI.LoginWithCustomID
        (
            // 랜덤 번호로 로그인
            new LoginWithCustomIDRequest
            {
                CustomId = "user_" + Guid.NewGuid().ToString(),
                CreateAccount = true
            },
            result =>
            {
                // 로그인 성공 시 완료 처리
                Debug.Log("PlayFab 로그인 완료");
                loginComplete.SetResult(true);
            },
            error =>
            {
                Debug.LogError($"PlayFab 로그인 실패: {error.ErrorMessage}");
                loginComplete.SetException(new System.Exception(error.ErrorMessage));
            }
        );

        // 로그인 완료까지 대기
        await loginComplete.Task;
    }
}
