using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RelayNetwork : MonoBehaviour
{
    // �̱���
    public static RelayNetwork instance;

    // �κ� ���� �޾ƿ� ����
    [HideInInspector] public Lobby lobby;

    void Awake()
    {
        instance = this;
    }

    // ȣ��Ʈ�� ����
    public async Task StartRelay()
    {
        // Relay ���� ���� ���� (���� ���� �ο� ���� ������ �κ��� �ִ� �ο� ���� �����ϰ�)
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(lobby.MaxPlayers);

        // Relay ���� �ڵ� ����
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        // Lobby Data�� Relay ���� �ڵ� ����
        await LobbyService.Instance.UpdateLobbyAsync
        (
            lobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { "JoinCode", new DataObject(DataObject.VisibilityOptions.Public, joinCode, DataObject.IndexOptions.S3) }
                }
            }
        );

        // ��Ʈ��ũ ���� ����
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData
        (
            allocation.RelayServer.IpV4,
            (ushort)allocation.RelayServer.Port,
            allocation.AllocationIdBytes,
            allocation.Key,
            allocation.ConnectionData
        );

        // ȣ��Ʈ ���� ����
        NetworkManager.Singleton.StartHost();

        // �� ��ȯ
        NetworkManager.Singleton.SceneManager.LoadScene("1.Test_Scene", LoadSceneMode.Single);
    }

    // Ŭ���̾�Ʈ�� ����
    public async Task JoinRelay()
    {
        // Lobby Data�� ���� �Ǿ� �ִ� ���� �ڵ尡 ������ return
        if (!lobby.Data.ContainsKey("JoinCode")) return;

        // Lobby Data�� ���� �Ǿ� �ִ� ���� �ڵ�� Relay ����
        JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(lobby.Data["JoinCode"].Value);

        // ��Ʈ��ũ ���� ����
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData
        (
            joinAllocation.RelayServer.IpV4,
            (ushort)joinAllocation.RelayServer.Port,
            joinAllocation.AllocationIdBytes,
            joinAllocation.Key,
            joinAllocation.ConnectionData,
            joinAllocation.HostConnectionData
        );

        // Ŭ���̾�Ʈ ���� ����
        NetworkManager.Singleton.StartClient();
    }
}
