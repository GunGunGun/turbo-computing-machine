﻿using System.Collections.Concurrent;
using NahidaImpact.Gameserver.Network.Session;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NahidaImpact.Gameserver.Network;
internal class NetSessionManager(ILogger<NetSessionManager> logger, IServiceScopeFactory serviceScopeFactory)
{
    private readonly ConcurrentDictionary<long, NetSession> _sessions = new();
    private readonly ILogger _logger = logger;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task RunSessionAsync(long sessionId, INetworkUnit networkUnit)
    {
        await using AsyncServiceScope serviceScope = _serviceScopeFactory.CreateAsyncScope();
        NetSession session = serviceScope.ServiceProvider.GetRequiredService<NetSession>();

        try
        {
            session.Establish(sessionId, networkUnit);
            await session.RunAsync();
        }
        catch (OperationCanceledException)
        {
            // OperationCanceled
        }
        catch (Exception exception)
        {
            _logger.LogError("Exception occurred during handling a session, trace: {exception}", exception);
        }
    }

    public void Add(NetSession session)
    {
        _sessions[session.SessionId] = session;
        _logger.LogInformation("New connection from {endPoint}", session.EndPoint);
    }

    public bool TryRemove(NetSession session)
    {
        bool removed = _sessions.TryRemove(session.SessionId, out _);
        if (removed)
        {
            _logger.LogInformation("Client from {endPoint} disconnected", session.EndPoint);
        }

        return removed;
    }
}
