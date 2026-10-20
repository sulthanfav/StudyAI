namespace StudyAI.Application.Common.Interfaces;

public interface ICurrentDataSpaceAccessor : ICurrentDataSpaceContext
{
    new Guid? CurrentDataSpaceId { get; set; }
}
