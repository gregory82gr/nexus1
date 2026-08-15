namespace Nexus1.BuildingBlocks.Application;

/// <summary>Invoked directly via constructor-injected DI — no dispatcher indirection (ADR-002-amend).</summary>
public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    Task<Result> Handle(TCommand command, CancellationToken cancellationToken);
}

public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken);
}
