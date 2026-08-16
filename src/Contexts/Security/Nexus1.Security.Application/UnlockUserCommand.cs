using Nexus1.BuildingBlocks.Application;

namespace Nexus1.Security.Application;

public sealed record UnlockUserCommand(int ApplicationUserId) : ICommand;
