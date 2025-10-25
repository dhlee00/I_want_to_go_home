using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.Lobby.Api;
using Unity.Services.Lobby.Model;

public class ServerModule
{
    // Unity의 로비 기능을 다룰 수 있는 API 객체 (이걸 통해 로비를 만들거나 수정할 수 있다)
    readonly ILobbyApi lobbyApi;

    // Cloud Code 실행 중 로그를 남기기 위한 도구
    readonly ILogger loggerApi;

    public ServerModule(IGameApiClient gameApiClient, ILogger<ServerModule> logger)
    {
        // 게임 서버 기능 모음(IGameApiClient)에서 로비 관련 기능만 꺼내온다
        lobbyApi = gameApiClient.Lobby;

        // 전달받은 로거를 저장해 두었다가, 나중에 Console.WriteLine처럼 사용한다
        loggerApi = logger;
    }

    // 로비 목록 갱신
    [CloudCodeFunction("UpdateLobbyList")]
    public async Task<List<Lobby>> UpdateLobbyList(IExecutionContext ec)
    {
        // 서비스 토큰으로 로비 목록 불러오기 요청
        var lobbyList = await lobbyApi.QueryLobbiesAsync(ec, ec.ServiceToken);

        // 결과 반환
        return lobbyList.Data.Results;
    }
}
