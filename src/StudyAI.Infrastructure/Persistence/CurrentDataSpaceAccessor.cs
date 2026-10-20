using StudyAI.Application.Common.Interfaces;

namespace StudyAI.Infrastructure.Persistence;

public sealed class CurrentDataSpaceAccessor : ICurrentDataSpaceAccessor
{
    private static readonly AsyncLocal<Guid?> CurrentScope = new();

    public Guid? CurrentDataSpaceId
    {
        get => CurrentScope.Value;
        set => CurrentScope.Value = value;
    }
}
