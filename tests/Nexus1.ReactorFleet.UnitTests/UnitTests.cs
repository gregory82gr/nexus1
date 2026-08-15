using Nexus1.ReactorFleet.Domain;

namespace Nexus1.ReactorFleet.UnitTests;

public class UnitTests
{
    [Fact]
    public void Create_with_valid_code_and_name_succeeds()
    {
        var unit = Unit.Create(new UnitId(1), "UNIT-1", "Demonstrator Unit 1");

        Assert.Equal(new UnitId(1), unit.Id);
        Assert.Equal("UNIT-1", unit.Code);
        Assert.Equal("Demonstrator Unit 1", unit.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_code_throws(string code)
    {
        Assert.Throws<ArgumentException>(() => Unit.Create(new UnitId(1), code, "Demonstrator Unit 1"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_name_throws(string name)
    {
        Assert.Throws<ArgumentException>(() => Unit.Create(new UnitId(1), "UNIT-1", name));
    }
}
