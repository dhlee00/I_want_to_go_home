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

    // UGS �α���
    async Task LoginUSG()
    {
        // Unity Service �ʱ�ȭ
        await UnityServices.InitializeAsync();

        // ���� �ʱ�ȭ (�׽�Ʈ������)
        AuthenticationService.Instance.ClearSessionToken();

        // �͸� �α���
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    // PlayFab �α���
    async Task LoginPlayFab()
    {
        // �Ϸ� ����
        var loginComplete = new TaskCompletionSource<bool>();

        // PlayFab Ŀ���� �α���
        PlayFabClientAPI.LoginWithCustomID
        (
            // ���� ��ȣ�� �α���
            new LoginWithCustomIDRequest
            {
                CustomId = "user_" + Guid.NewGuid().ToString(),
                CreateAccount = true
            },
            result =>
            {
                // �α��� ���� �� �Ϸ� ó��
                Debug.Log("PlayFab �α��� �Ϸ�");
                loginComplete.SetResult(true);
            },
            error =>
            {
                Debug.LogError($"PlayFab �α��� ����: {error.ErrorMessage}");
                loginComplete.SetException(new System.Exception(error.ErrorMessage));
            }
        );

        // �α��� �Ϸ���� ���
        await loginComplete.Task;
    }
}
