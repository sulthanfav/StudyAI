namespace StudyAI.Application.Common.Interfaces;

public interface ICurrentActorContext
{
    Guid? CurrentActorId { get; }
}

public interface ICurrentActorAccessor : ICurrentActorContext
{
    new Guid? CurrentActorId { get; set; }
}
