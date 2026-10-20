using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using StudyAI.Application.Common.Interfaces;

namespace StudyAI.Infrastructure.Persistence.Interceptors;

public sealed class TenantSessionInterceptor : DbTransactionInterceptor
{
    private readonly ICurrentDataSpaceContext _dataSpaceContext;
    private readonly ICurrentActorContext _actorContext;

    public TenantSessionInterceptor(ICurrentDataSpaceContext dataSpaceContext, ICurrentActorContext actorContext)
    {
        _dataSpaceContext = dataSpaceContext;
        _actorContext = actorContext;
    }

    public override DbTransaction TransactionStarted(
        DbConnection connection,
        TransactionEndEventData eventData,
        DbTransaction result)
    {
        ApplyTenantContext(result);
        return base.TransactionStarted(connection, eventData, result);
    }

    public override async ValueTask<DbTransaction> TransactionStartedAsync(
        DbConnection connection,
        TransactionEndEventData eventData,
        DbTransaction result,
        CancellationToken cancellationToken = default)
    {
        await ApplyTenantContextAsync(result, cancellationToken);
        return await base.TransactionStartedAsync(connection, eventData, result, cancellationToken);
    }

    private void ApplyTenantContext(DbTransaction transaction)
    {
        if (transaction.Connection is null)
        {
            return;
        }

        using var command = transaction.Connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT set_config('app.current_data_space_id', @space_id, true), set_config('app.current_actor_id', @actor_id, true);";

        var parameter = command.CreateParameter();
        parameter.ParameterName = "space_id";
        parameter.Value = _dataSpaceContext.CurrentDataSpaceId.HasValue
            ? _dataSpaceContext.CurrentDataSpaceId.Value.ToString()
            : string.Empty;
        command.Parameters.Add(parameter);
        var actorParameter = command.CreateParameter();
        actorParameter.ParameterName = "actor_id";
        actorParameter.Value = _actorContext.CurrentActorId?.ToString() ?? string.Empty;
        command.Parameters.Add(actorParameter);

        command.ExecuteNonQuery();
    }

    private async Task ApplyTenantContextAsync(DbTransaction transaction, CancellationToken cancellationToken)
    {
        if (transaction.Connection is null)
        {
            return;
        }

        await using var command = transaction.Connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT set_config('app.current_data_space_id', @space_id, true), set_config('app.current_actor_id', @actor_id, true);";

        var parameter = command.CreateParameter();
        parameter.ParameterName = "space_id";
        parameter.Value = _dataSpaceContext.CurrentDataSpaceId.HasValue
            ? _dataSpaceContext.CurrentDataSpaceId.Value.ToString()
            : string.Empty;
        command.Parameters.Add(parameter);
        var actorParameter = command.CreateParameter();
        actorParameter.ParameterName = "actor_id";
        actorParameter.Value = _actorContext.CurrentActorId?.ToString() ?? string.Empty;
        command.Parameters.Add(actorParameter);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
