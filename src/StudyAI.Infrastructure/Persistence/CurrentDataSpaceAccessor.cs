using StudyAI.Application.Common.Interfaces;

namespace StudyAI.Infrastructure.Persistence;

public sealed class CurrentDataSpaceAccessor : ICurrentDataSpaceAccessor, ICurrentActorAccessor
{
    public Guid? CurrentDataSpaceId { get; set; }
    public Guid? CurrentActorId { get; set; }
}
