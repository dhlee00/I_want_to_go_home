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

    // �κ� ���� ��ư
    public async void CreateLobby()
    {
        // �׽�Ʈ�� �κ� ����
        Lobby newLobby = await LobbyService.Instance.CreateLobbyAsync
        (
            "�׽�Ʈ �κ�", 4,
            new CreateLobbyOptions
            {
                IsPrivate = false
            }
        );

        // Relay�� �κ� ����
        RelayNetwork.instance.lobby = newLobby;

        // ȣ��Ʈ�� ����
        await RelayNetwork.instance.StartRelay();
    }

    // �κ� ���� ��ư
    public async void JoinLobby()
    {
        // �κ� ����
        Lobby joinLobby = await LobbyService.Instance.QuickJoinLobbyAsync();

        // Relay�� �κ� ����
        RelayNetwork.instance.lobby = joinLobby;

        // Ŭ���̾�Ʈ�� ����
        await RelayNetwork.instance.JoinRelay();
    }
}
