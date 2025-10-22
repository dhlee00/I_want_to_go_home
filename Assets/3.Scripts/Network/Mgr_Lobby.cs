using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class Mgr_Lobby : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    // 로비 생성 버튼
    public async void CreateLobby()
    {
        // 테스트용 로비 생성
        Lobby newLobby = await LobbyService.Instance.CreateLobbyAsync
        (
            "테스트 로비", 4,
            new CreateLobbyOptions
            {
                IsPrivate = false
            }
        );

        // Relay에 로비 전달
        RelayNetwork.instance.lobby = newLobby;

        // 호스트로 시작
        await RelayNetwork.instance.StartRelay();
    }

    // 로비 참여 버튼
    public async void JoinLobby()
    {
        // 로비 참여
        Lobby joinLobby = await LobbyService.Instance.QuickJoinLobbyAsync();

        // Relay에 로비 전달
        RelayNetwork.instance.lobby = joinLobby;

        // 클라이언트로 시작
        await RelayNetwork.instance.JoinRelay();
    }
}
