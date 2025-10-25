using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.Lobby.Api;
using Unity.Services.Lobby.Model;

public class ServerModule
{
    // Unity�� �κ� ����� �ٷ� �� �ִ� API ��ü (�̰� ���� �κ� ����ų� ������ �� �ִ�)
    readonly ILobbyApi lobbyApi;

    // Cloud Code ���� �� �α׸� ����� ���� ����
    readonly ILogger loggerApi;

    public ServerModule(IGameApiClient gameApiClient, ILogger<ServerModule> logger)
    {
        // ���� ���� ��� ����(IGameApiClient)���� �κ� ���� ��ɸ� �����´�
        lobbyApi = gameApiClient.Lobby;

        // ���޹��� �ΰŸ� ������ �ξ��ٰ�, ���߿� Console.WriteLineó�� ����Ѵ�
        loggerApi = logger;
    }

    // �κ� ��� ����
    [CloudCodeFunction("UpdateLobbyList")]
    public async Task<List<Lobby>> UpdateLobbyList(IExecutionContext ec)
    {
        // ���� ��ū���� �κ� ��� �ҷ����� ��û
        var lobbyList = await lobbyApi.QueryLobbiesAsync(ec, ec.ServiceToken);

        // ��� ��ȯ
        return lobbyList.Data.Results;
    }
}
