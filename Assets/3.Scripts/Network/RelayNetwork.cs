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
    // 싱글톤
    public static RelayNetwork instance;

    // 로비 정보 받아올 변수
    [HideInInspector] public Lobby lobby;

    void Awake()
    {
        instance = this;
    }

    // 호스트로 시작
    public async Task StartRelay()
    {
        // Relay 연결 슬롯 생성 (연결 가능 인원 수는 생성한 로비의 최대 인원 수랑 동일하게)
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(lobby.MaxPlayers);

        // Relay 참여 코드 생성
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        // Lobby Data에 Relay 참여 코드 저장
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

        // 네트워크 접속 설정
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData
        (
            allocation.RelayServer.IpV4,
            (ushort)allocation.RelayServer.Port,
            allocation.AllocationIdBytes,
            allocation.Key,
            allocation.ConnectionData
        );

        // 호스트 모드로 접속
        NetworkManager.Singleton.StartHost();

        // 씬 전환
        NetworkManager.Singleton.SceneManager.LoadScene("1.Test_Scene", LoadSceneMode.Single);
    }

    // 클라이언트로 시작
    public async Task JoinRelay()
    {
        // Lobby Data에 저장 되어 있는 참여 코드가 없으면 return
        if (!lobby.Data.ContainsKey("JoinCode")) return;

        // Lobby Data에 저장 되어 있는 참여 코드로 Relay 접속
        JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(lobby.Data["JoinCode"].Value);

        // 네트워크 접속 설정
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData
        (
            joinAllocation.RelayServer.IpV4,
            (ushort)joinAllocation.RelayServer.Port,
            joinAllocation.AllocationIdBytes,
            joinAllocation.Key,
            joinAllocation.ConnectionData,
            joinAllocation.HostConnectionData
        );

        // 클라이언트 모드로 접속
        NetworkManager.Singleton.StartClient();
    }
}
