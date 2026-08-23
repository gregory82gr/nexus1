using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus1.CorePlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEngineeringUnitQuantityTypeCheckConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_CorePlatform_EngineeringUnit_QuantityType",
                schema: "CorePlatform",
                table: "EngineeringUnit",
                sql: "[QuantityType] IN (N'Power', N'ThermalPower', N'Temperature', N'Pressure', N'Reactivity', N'Flow', N'Frequency', N'Percentage', N'RadiationDoseRate', N'RadiationDose', N'CountRate', N'Mass', N'Time', N'Other')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_CorePlatform_EngineeringUnit_QuantityType",
                schema: "CorePlatform",
                table: "EngineeringUnit");
        }
    }
}
