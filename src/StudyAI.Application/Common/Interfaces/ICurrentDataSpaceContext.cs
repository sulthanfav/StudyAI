namespace StudyAI.Application.Common.Interfaces;

public interface ICurrentDataSpaceContext
{
    Guid? CurrentDataSpaceId { get; }
    bool HasContext => CurrentDataSpaceId.HasValue;
}
