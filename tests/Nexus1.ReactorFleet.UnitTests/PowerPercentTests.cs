using Nexus1.ReactorFleet.Domain;

namespace Nexus1.ReactorFleet.UnitTests;

public class PowerPercentTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(200)]
    [InlineData(57.5)]
    public void Construction_within_zero_to_two_hundred_succeeds(decimal value)
    {
        var powerPercent = new PowerPercent(value);

        Assert.Equal(value, powerPercent.Value);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(200.01)]
    public void Construction_outside_zero_to_two_hundred_throws(decimal value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PowerPercent(value));
    }
}
