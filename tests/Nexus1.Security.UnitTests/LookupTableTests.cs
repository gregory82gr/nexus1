using Nexus1.Security.Domain;

namespace Nexus1.Security.UnitTests;

public class LookupTableTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 18, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void UserStatus_Create_with_valid_fields_succeeds()
    {
        var status = UserStatus.Create(new UserStatusId(1), "ACTIVE", "Active", NowUtc);
        Assert.Equal("ACTIVE", status.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void UserStatus_Create_with_blank_code_throws(string code)
    {
        Assert.Throws<ArgumentException>(() => UserStatus.Create(new UserStatusId(1), code, "Active", NowUtc));
    }

    [Fact]
    public void RoleType_Create_with_valid_fields_succeeds()
    {
        var roleType = RoleType.Create(new RoleTypeId(1), "OPERATOR", "Operator", NowUtc);
        Assert.Equal("OPERATOR", roleType.Code);
    }

    [Fact]
    public void PermissionCategory_Create_with_valid_fields_succeeds()
    {
        var category = PermissionCategory.Create(new PermissionCategoryId(1), "ALARM", "Alarm", NowUtc);
        Assert.Equal("ALARM", category.Code);
    }
}
