using UnityEngine;

public class LobbyBtn : MonoBehaviour
{
    // �κ� id
    public string lobbyId;

    // �κ� �̸�
    public string lobbyName;

    // �κ� ���� �̸�
    public string leaderName;

    // �κ� �ִ� �ο� ��
    public int maxPlayer;

    // �κ� �Ұ�
    public string info;

    // �κ� ����
    public async void JoinLobby()
    {
        await Mgr_Lobby.instance.JoinLobby(lobbyId);
    }

    // �κ� ���� ǥ��
    public void ShowLobbyInfo()
    {
        Mgr_Lobby.instance.SendLobbyInfo(lobbyName, leaderName, maxPlayer, info);

        Mgr_Lobby.instance.lobbyInfo.SetActive(true);
    }

    // �κ� ���� �����
    public void HideLobbyInfo()
    {
        Mgr_Lobby.instance.lobbyInfo.SetActive(false);
    }
}
