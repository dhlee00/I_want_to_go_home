using UnityEngine;

public class LobbyBtn : MonoBehaviour
{
    // 로비 id
    public string lobbyId;

    // 로비 이름
    public string lobbyName;

    // 로비 방장 이름
    public string leaderName;

    // 로비 최대 인원 수
    public int maxPlayer;

    // 로비 소개
    public string info;

    // 로비 참여
    public async void JoinLobby()
    {
        await Mgr_Lobby.instance.JoinLobby(lobbyId);
    }

    // 로비 정보 표시
    public void ShowLobbyInfo()
    {
        Mgr_Lobby.instance.SendLobbyInfo(lobbyName, leaderName, maxPlayer, info);

        Mgr_Lobby.instance.lobbyInfo.SetActive(true);
    }

    // 로비 정보 숨기기
    public void HideLobbyInfo()
    {
        Mgr_Lobby.instance.lobbyInfo.SetActive(false);
    }
}
