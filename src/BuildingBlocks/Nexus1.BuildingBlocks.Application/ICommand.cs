namespace Nexus1.BuildingBlocks.Application;

/// <summary>
/// Structured to mirror MediatR's IRequest shape without depending on the
/// package — hand-rolled dispatch for now, ADR-002-amend's MediatR decision.
/// </summary>
public interface ICommand;

public interface ICommand<TResponse>;
