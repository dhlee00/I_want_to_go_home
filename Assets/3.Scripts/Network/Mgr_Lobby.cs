using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WebSocketSharp;

public class Mgr_Lobby : MonoBehaviour
{
    // ����ƽȭ
    public static Mgr_Lobby instance;

    // �������� �ҷ��� �κ� ��� �����ص� ����
    Dictionary<string, GameObject> lobbyList = new Dictionary<string, GameObject>();

    // �κ� ���
    public Transform lobbyContent;

    // �κ� ��Ͽ� �߰��� ��ư ������
    public GameObject lobbyBtnPrefab;

    // �κ� ���� â ���� ����
    [Header("LobbyInfo")]
    public GameObject lobbyInfo;  // �κ� ���� â
    public Text lobbyNameTxt;     // �κ��� �̸�
    public Text leaderNameTxt;    // �κ��� ���� �̸�
    public Text maxPlayerTxt;     // �κ� �ִ� �ο� ��
    public Text infoTxt;          // �κ� �Ұ���

    // �ڵ� �Է� â ���� ����
    [Header("CodeInput")]
    public GameObject codeInputPanel;  // �ڵ� �Է� â
    public InputField codeField;       // �ڵ� �Է��ʵ�

    // �κ� ���� â ���� ����
    [Header("CreateLobby")]
    public GameObject createLobbyPanel;  // �κ� ���� â
    public InputField lobbyNameField;    // �κ� �̸� �Է��ʵ�
    public Dropdown maxPlayerDD;         // �ִ� �ο��� ���� â
    public Dropdown visibilityDD;        // ���� / ����� ���� ���� â
    public InputField lobbyInfoField;    // �κ� �Ұ��� �Է��ʵ�

    [Header("")]
    // �ε� â
    public GameObject loadingView;

    // ���� �޼���
    public Text statusMessage;

    // �޼��� ��� �ð�
    float messageTimer = 0;

    // ��ư �ߺ� ó��
    bool isClick;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        _ = UpdateLobbyList();
    }

    void Update()
    {
        // �޼����� 1�ʰ� ����ϰ� 1�� �� ��Ȱ��ȭ
        //if (messageTimer > 0)
        //{
        //    messageTimer -= Time.deltaTime;
        //}

        //else
        //{
        //    statusMessage.gameObject.SetActive(false);
        //}
    }

    // �κ� ��� ����
    public async Task UpdateLobbyList()
    {
        while (SceneManager.GetActiveScene().name == "TitleScene")
        {
            try
            {
                // �α��� �ȵ� ���¿����� �������� ����
                if (!AuthenticationService.Instance.IsSignedIn) continue;

                // ������ �κ� ��� �ҷ�����
                var serverLobbyList = await CloudCodeService.Instance.CallModuleEndpointAsync<List<Lobby>>
                (
                    "ServerModule", "UpdateLobbyList"
                );

                // �������� �ҷ��� �κ� ��Ͽ��� �ִµ� ���� ���� �κ� ��Ͽ��� ���ٸ�
                foreach (var serverLobby in serverLobbyList)
                {
                    if (!lobbyList.ContainsKey(serverLobby.Id))
                    {
                        if (serverLobby.Data == null) continue;

                        // �κ� ��Ͽ� ��ư �߰�
                        GameObject btn = Instantiate(lobbyBtnPrefab, lobbyContent);
                        btn.GetComponentInChildren<Text>().text = serverLobby.Name;

                        btn.GetComponent<LobbyBtn>().lobbyId = serverLobby.Id;
                        btn.GetComponent<LobbyBtn>().lobbyName = serverLobby.Name;
                        btn.GetComponent<LobbyBtn>().leaderName = serverLobby.Data["LeaderName"].Value;
                        btn.GetComponent<LobbyBtn>().maxPlayer = serverLobby.MaxPlayers;
                        btn.GetComponent<LobbyBtn>().info = serverLobby.Data["LobbyInfo"].Value;

                        lobbyList.Add(serverLobby.Id, btn);
                    }
                }

                // ������ �κ���� ���̵� ���纻 ����
                List<string> serverLobbyIds = new List<string>();
                foreach (var lobbyId in serverLobbyList)
                {
                    serverLobbyIds.Add(lobbyId.Id);
                }

                // ���ÿ��� �ִµ� �������� ���� �κ� ID�� ã��
                List<string> deleteLobbyIds = new List<string>();
                foreach (var localLobbyIds in lobbyList.Keys)
                {
                    if (!serverLobbyIds.Contains(localLobbyIds))
                    {
                        // ������ �κ� ��Ͽ� �߰�
                        deleteLobbyIds.Add(localLobbyIds);
                    }
                }

                // ������ �κ�� ����
                foreach (var lobbyId in deleteLobbyIds)
                {
                    Destroy(lobbyList[lobbyId]);
                    lobbyList.Remove(lobbyId);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("�κ� ��� ���� ����: " + e);
            }
            finally
            {
                await Task.Delay(1000);
            }
        }
    }

    // �κ� ���� ����
    public void SendLobbyInfo(string lobbyName, string leaderName, int maxPlayer, string info)
    {
        lobbyNameTxt.text = lobbyName;             // �κ� �̸�
        leaderNameTxt.text = leaderName;           // ���� �г���
        maxPlayerTxt.text = maxPlayer.ToString();  // �ִ� �ο� ��
        infoTxt.text = info;                       // �κ� �Ұ�
    }

    // �κ� ����
    public async Task JoinLobby(string lobbyId)
    {
        // �κ��� id�� �κ� ����
        Lobby joinLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);

        // �����Ϸ��� �κ� ����
        RelayNetwork.instance.lobby = joinLobby;

        // �ε� â Ȱ��ȭ
        loadingView.SetActive(true);

        // Ŭ���̾�Ʈ ���� ����
        await RelayNetwork.instance.JoinRelay();
    }

    // �ڵ� �Է� â ���� ��ư
    public void OpenCodeInputPanel()
    {
        // â Ȱ��ȭ
        codeInputPanel.SetActive(true);
    }

    // �ڵ� �Է� ��ư
    public async void CodeInputBtn()
    {
        // �ƹ� �͵� �Է����� ������ ��� ���� ���
        if (codeField.text.IsNullOrEmpty())
        {
            PrintMessage("�� ĭ ���� �Է����ּ���.");
            return;
        }

        // ������ �κ� ��� �ҷ�����
        var serverLobbyList = await CloudCodeService.Instance.CallModuleEndpointAsync<List<Lobby>>
        (
            "ServerModule", "UpdateLobbyList"
        );

        // ���� �ڵ� Ȯ��
        foreach (var joinCode in serverLobbyList)
        {
            // �Է��� ���� �ڵ尡 �����ϸ� ����
            if (joinCode.Data["JoinCode"].Value == codeField.text)
            {
                await JoinLobby(joinCode.Id);
                return;
            }
        }

        PrintMessage("�����ڵ带 �ٽ� Ȯ�����ּ���.");
    }

    // �κ� ���� â ���� ��ư
    public void OpenCreateLobbyPanel()
    {
        // â Ȱ��ȭ
        createLobbyPanel.SetActive(true);
    }

    // �κ� ���� ��ư
    public async void CreateLobbyBtn()
    {
        // �ƹ� �͵� �Է����� ������ ��� ���� ���
        if (lobbyNameField.text.IsNullOrEmpty() || lobbyInfoField.text.IsNullOrEmpty())
        {
            PrintMessage("�� ĭ ���� �Է����ּ���.");
            return;
        }

        if (!isClick)
        {
            isClick = true;

            PrintMessage("�κ� ���� ��...");

            // �κ� ����
            Lobby newLobby = await LobbyService.Instance.CreateLobbyAsync
            (
                lobbyNameField.text,                                     // �κ� �̸�
                int.Parse(maxPlayerDD.options[maxPlayerDD.value].text),  // �κ� �ִ� �ο� ��
                new CreateLobbyOptions
                {
                    IsPrivate = visibilityDD.value == 1                  // ���� / ����� ����
                }
            );

            // �κ� �����Ϳ� �κ��� ���� �̸� ����
            await LobbyService.Instance.UpdateLobbyAsync
            (
                newLobby.Id,
                new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        {
                            "LeaderName",
                            new DataObject(DataObject.VisibilityOptions.Public, GlobalValue.Nickname, DataObject.IndexOptions.S1)
                        },
                        {
                            "LobbyInfo",
                            new DataObject(DataObject.VisibilityOptions.Public, lobbyInfoField.text, DataObject.IndexOptions.S2)
                        }
                    }
                }
            );

            // ������ �κ� ���� ����
            RelayNetwork.instance.lobby = newLobby;

            // �ε� â Ȱ��ȭ
            loadingView.SetActive(true);

            // ȣ��Ʈ�� Relay ����
            await RelayNetwork.instance.StartRelay();

            isClick = false;
        }
    }

    // �ݱ� ��ư
    public void CloseBtn()
    {
        // �ڵ� �Է� â �ݱ�
        if (codeInputPanel.activeSelf)
        {
            codeInputPanel.SetActive(false);

            // �Է°� �ʱ�ȭ
            codeField.text = null;
        }

        // �κ� ���� â �ݱ�
        if (createLobbyPanel.activeSelf)
        {
            createLobbyPanel.SetActive(false);

            // �Է°� �ʱ�ȭ
            lobbyNameField.text = null;
            lobbyInfoField.text = null;
        }
    }

    // ���� ���
    public void PrintMessage(string text, float time = 1.0f)
    {
        statusMessage.gameObject.SetActive(true);
        messageTimer = time;

        statusMessage.text = text;
    }

    #region �׽�Ʈ�� �Լ�
    // �׽�Ʈ�� �κ� ���� ��ư
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

    // �׽�Ʈ�� �κ� ���� ��ư
    public async void JoinLobby()
    {
        // �κ� ����
        Lobby joinLobby = await LobbyService.Instance.QuickJoinLobbyAsync();

        // Relay�� �κ� ����
        RelayNetwork.instance.lobby = joinLobby;

        // Ŭ���̾�Ʈ�� ����
        await RelayNetwork.instance.JoinRelay();
    }
    #endregion
}
