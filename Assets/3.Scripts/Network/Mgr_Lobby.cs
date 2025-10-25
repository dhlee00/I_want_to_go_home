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
    // 스태틱화
    public static Mgr_Lobby instance;

    // 서버에서 불러온 로비 목록 저장해둘 변수
    Dictionary<string, GameObject> lobbyList = new Dictionary<string, GameObject>();

    // 로비 목록
    public Transform lobbyContent;

    // 로비 목록에 추가할 버튼 프리팹
    public GameObject lobbyBtnPrefab;

    // 로비 정보 창 관련 변수
    [Header("LobbyInfo")]
    public GameObject lobbyInfo;  // 로비 정보 창
    public Text lobbyNameTxt;     // 로비의 이름
    public Text leaderNameTxt;    // 로비의 리더 이름
    public Text maxPlayerTxt;     // 로비 최대 인원 수
    public Text infoTxt;          // 로비 소개문

    // 코드 입력 창 관련 변수
    [Header("CodeInput")]
    public GameObject codeInputPanel;  // 코드 입력 창
    public InputField codeField;       // 코드 입력필드

    // 로비 생성 창 관련 변수
    [Header("CreateLobby")]
    public GameObject createLobbyPanel;  // 로비 생성 창
    public InputField lobbyNameField;    // 로비 이름 입력필드
    public Dropdown maxPlayerDD;         // 최대 인원수 선택 창
    public Dropdown visibilityDD;        // 공개 / 비공개 여부 선택 창
    public InputField lobbyInfoField;    // 로비 소개문 입력필드

    [Header("")]
    // 로딩 창
    public GameObject loadingView;

    // 상태 메세지
    public Text statusMessage;

    // 메세지 출력 시간
    float messageTimer = 0;

    // 버튼 중복 처리
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
        // 메세지는 1초간 출력하고 1초 후 비활성화
        //if (messageTimer > 0)
        //{
        //    messageTimer -= Time.deltaTime;
        //}

        //else
        //{
        //    statusMessage.gameObject.SetActive(false);
        //}
    }

    // 로비 목록 갱신
    public async Task UpdateLobbyList()
    {
        while (SceneManager.GetActiveScene().name == "TitleScene")
        {
            try
            {
                // 로그인 안된 상태에서는 갱신하지 않음
                if (!AuthenticationService.Instance.IsSignedIn) continue;

                // 생성된 로비 목록 불러오기
                var serverLobbyList = await CloudCodeService.Instance.CallModuleEndpointAsync<List<Lobby>>
                (
                    "ServerModule", "UpdateLobbyList"
                );

                // 서버에서 불러온 로비 목록에는 있는데 현재 로컬 로비 목록에는 없다면
                foreach (var serverLobby in serverLobbyList)
                {
                    if (!lobbyList.ContainsKey(serverLobby.Id))
                    {
                        if (serverLobby.Data == null) continue;

                        // 로비 목록에 버튼 추가
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

                // 생성된 로비들의 아이디 복사본 생성
                List<string> serverLobbyIds = new List<string>();
                foreach (var lobbyId in serverLobbyList)
                {
                    serverLobbyIds.Add(lobbyId.Id);
                }

                // 로컬에는 있는데 서버에는 없는 로비 ID들 찾기
                List<string> deleteLobbyIds = new List<string>();
                foreach (var localLobbyIds in lobbyList.Keys)
                {
                    if (!serverLobbyIds.Contains(localLobbyIds))
                    {
                        // 삭제할 로비 목록에 추가
                        deleteLobbyIds.Add(localLobbyIds);
                    }
                }

                // 없어진 로비들 삭제
                foreach (var lobbyId in deleteLobbyIds)
                {
                    Destroy(lobbyList[lobbyId]);
                    lobbyList.Remove(lobbyId);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("로비 목록 갱신 실패: " + e);
            }
            finally
            {
                await Task.Delay(1000);
            }
        }
    }

    // 로비 정보 전달
    public void SendLobbyInfo(string lobbyName, string leaderName, int maxPlayer, string info)
    {
        lobbyNameTxt.text = lobbyName;             // 로비 이름
        leaderNameTxt.text = leaderName;           // 리더 닉네임
        maxPlayerTxt.text = maxPlayer.ToString();  // 최대 인원 수
        infoTxt.text = info;                       // 로비 소개
    }

    // 로비 참여
    public async Task JoinLobby(string lobbyId)
    {
        // 로비의 id로 로비에 참여
        Lobby joinLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);

        // 참여하려는 로비 전달
        RelayNetwork.instance.lobby = joinLobby;

        // 로딩 창 활성화
        loadingView.SetActive(true);

        // 클라이언트 모드로 참여
        await RelayNetwork.instance.JoinRelay();
    }

    // 코드 입력 창 열기 버튼
    public void OpenCodeInputPanel()
    {
        // 창 활성화
        codeInputPanel.SetActive(true);
    }

    // 코드 입력 버튼
    public async void CodeInputBtn()
    {
        // 아무 것도 입력하지 않으면 경고 문구 출력
        if (codeField.text.IsNullOrEmpty())
        {
            PrintMessage("빈 칸 없이 입력해주세요.");
            return;
        }

        // 생성된 로비 목록 불러오기
        var serverLobbyList = await CloudCodeService.Instance.CallModuleEndpointAsync<List<Lobby>>
        (
            "ServerModule", "UpdateLobbyList"
        );

        // 참여 코드 확인
        foreach (var joinCode in serverLobbyList)
        {
            // 입력한 참여 코드가 존재하면 참여
            if (joinCode.Data["JoinCode"].Value == codeField.text)
            {
                await JoinLobby(joinCode.Id);
                return;
            }
        }

        PrintMessage("참여코드를 다시 확인해주세요.");
    }

    // 로비 생성 창 열기 버튼
    public void OpenCreateLobbyPanel()
    {
        // 창 활성화
        createLobbyPanel.SetActive(true);
    }

    // 로비 생성 버튼
    public async void CreateLobbyBtn()
    {
        // 아무 것도 입력하지 않으면 경고 문구 출력
        if (lobbyNameField.text.IsNullOrEmpty() || lobbyInfoField.text.IsNullOrEmpty())
        {
            PrintMessage("빈 칸 없이 입력해주세요.");
            return;
        }

        if (!isClick)
        {
            isClick = true;

            PrintMessage("로비 생성 중...");

            // 로비 생성
            Lobby newLobby = await LobbyService.Instance.CreateLobbyAsync
            (
                lobbyNameField.text,                                     // 로비 이름
                int.Parse(maxPlayerDD.options[maxPlayerDD.value].text),  // 로비 최대 인원 수
                new CreateLobbyOptions
                {
                    IsPrivate = visibilityDD.value == 1                  // 공개 / 비공개 여부
                }
            );

            // 로비 데이터에 로비의 리더 이름 저장
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

            // 생성한 로비 정보 전달
            RelayNetwork.instance.lobby = newLobby;

            // 로딩 창 활성화
            loadingView.SetActive(true);

            // 호스트로 Relay 시작
            await RelayNetwork.instance.StartRelay();

            isClick = false;
        }
    }

    // 닫기 버튼
    public void CloseBtn()
    {
        // 코드 입력 창 닫기
        if (codeInputPanel.activeSelf)
        {
            codeInputPanel.SetActive(false);

            // 입력값 초기화
            codeField.text = null;
        }

        // 로비 생성 창 닫기
        if (createLobbyPanel.activeSelf)
        {
            createLobbyPanel.SetActive(false);

            // 입력값 초기화
            lobbyNameField.text = null;
            lobbyInfoField.text = null;
        }
    }

    // 상태 출력
    public void PrintMessage(string text, float time = 1.0f)
    {
        statusMessage.gameObject.SetActive(true);
        messageTimer = time;

        statusMessage.text = text;
    }

    #region 테스트용 함수
    // 테스트용 로비 생성 버튼
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

    // 테스트용 로비 참여 버튼
    public async void JoinLobby()
    {
        // 로비 참여
        Lobby joinLobby = await LobbyService.Instance.QuickJoinLobbyAsync();

        // Relay에 로비 전달
        RelayNetwork.instance.lobby = joinLobby;

        // 클라이언트로 시작
        await RelayNetwork.instance.JoinRelay();
    }
    #endregion
}
