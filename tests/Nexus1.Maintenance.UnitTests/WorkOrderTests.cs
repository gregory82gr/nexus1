using Nexus1.Maintenance.Domain;

namespace Nexus1.Maintenance.UnitTests;

public class WorkOrderTests
{
    private static readonly DateTime RequestedAtUtc = new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds_and_is_not_deleted()
    {
        var workOrder = WorkOrder.Create(
            new WorkOrderId(1), unitId: 1, new WorkOrderTypeId(1), new WorkOrderStatusId(1), new WorkPriorityId(1),
            "WO-2026-0001", "Replace feedwater pump seal", RequestedAtUtc);

        Assert.Equal("WO-2026-0001", workOrder.WorkOrderCode);
        Assert.False(workOrder.IsDeleted);
        Assert.Null(workOrder.AssetId);
        Assert.Null(workOrder.OriginOperationalEventId);
        Assert.Null(workOrder.OriginIncidentActionId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_work_order_code_throws(string workOrderCode)
    {
        Assert.Throws<ArgumentException>(() => WorkOrder.Create(
            new WorkOrderId(1), 1, new WorkOrderTypeId(1), new WorkOrderStatusId(1), new WorkPriorityId(1),
            workOrderCode, "Replace feedwater pump seal", RequestedAtUtc));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_title_throws(string title)
    {
        Assert.Throws<ArgumentException>(() => WorkOrder.Create(
            new WorkOrderId(1), 1, new WorkOrderTypeId(1), new WorkOrderStatusId(1), new WorkPriorityId(1),
            "WO-2026-0001", title, RequestedAtUtc));
    }

    [Fact]
    public void Create_with_passport_only_origin_ids_sets_them_with_no_enforced_fk()
    {
        var workOrder = WorkOrder.Create(
            new WorkOrderId(1), unitId: 1, new WorkOrderTypeId(1), new WorkOrderStatusId(1), new WorkPriorityId(1),
            "WO-2026-0001", "Investigate alarm flood", RequestedAtUtc,
            originOperationalEventId: 12345L, originIncidentActionId: 67890L);

        Assert.Equal(12345L, workOrder.OriginOperationalEventId);
        Assert.Equal(67890L, workOrder.OriginIncidentActionId);
    }

    [Fact]
    public void Create_with_asset_id_sets_the_real_internal_fk()
    {
        var workOrder = WorkOrder.Create(
            new WorkOrderId(1), unitId: 1, new WorkOrderTypeId(1), new WorkOrderStatusId(1), new WorkPriorityId(1),
            "WO-2026-0001", "Replace feedwater pump seal", RequestedAtUtc, assetId: new AssetId(7));

        Assert.Equal(new AssetId(7), workOrder.AssetId);
    }
}
