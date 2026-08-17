using Microsoft.EntityFrameworkCore;
using Nexus1.Robotics.Domain;

namespace Nexus1.Robotics.Infrastructure.Persistence;

/// <summary>
/// Shares AlarmManagementDb's physical database (ADR-023, following
/// ADR-006/ADR-015/ADR-019/ADR-020/ADR-021/ADR-022's precedent) but keeps
/// its own migration history and only owns Robotics.* tables. The
/// ExternalReferences type (ReactorFleetUnitReference) is also part of this
/// model — configured via IEntityTypeConfiguration and picked up by
/// ApplyConfigurationsFromAssembly below — but is excluded from this
/// context's migrations; it exists only so FK relationships to tables owned
/// by ReactorFleet can be declared.
/// </summary>
public sealed class RoboticsDbContext(DbContextOptions<RoboticsDbContext> options) : DbContext(options)
{
    public DbSet<RobotType> RobotTypes => Set<RobotType>();

    public DbSet<RobotStatus> RobotStatuses => Set<RobotStatus>();

    public DbSet<BatteryStatus> BatteryStatuses => Set<BatteryStatus>();

    public DbSet<CommunicationStatus> CommunicationStatuses => Set<CommunicationStatus>();

    public DbSet<MissionType> MissionTypes => Set<MissionType>();

    public DbSet<MissionStatus> MissionStatuses => Set<MissionStatus>();

    public DbSet<MissionPriority> MissionPriorities => Set<MissionPriority>();

    public DbSet<ReadinessStatus> ReadinessStatuses => Set<ReadinessStatus>();

    public DbSet<RobotModel> RobotModels => Set<RobotModel>();

    public DbSet<Robot> Robots => Set<Robot>();

    public DbSet<RobotHealthSnapshot> RobotHealthSnapshots => Set<RobotHealthSnapshot>();

    public DbSet<Mission> Missions => Set<Mission>();

    public DbSet<MissionEvent> MissionEvents => Set<MissionEvent>();

    public DbSet<MissionReadinessAssessment> MissionReadinessAssessments => Set<MissionReadinessAssessment>();

    public DbSet<MissionReadinessItem> MissionReadinessItems => Set<MissionReadinessItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RoboticsDbContext).Assembly);
    }
}
