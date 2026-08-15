namespace Nexus1.BuildingBlocks.Application;

/// <summary>
/// Every aggregate's strongly-typed id is ValueGeneratedNever() at the
/// persistence layer (domain factories always require a caller-supplied id
/// — see the ReactorFleet/AlarmManagement/RootCause EF configurations), so
/// the Application layer needs a way to assign one before construction.
/// </summary>
public interface IIdGenerator
{
    long NextLong();

    int NextInt();
}
