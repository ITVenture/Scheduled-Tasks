using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PeriodicTasks.DbContext.Migrations
{
    public partial class TenantSupport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "Plugins",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "PeriodicWeekDays",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "PeriodicTimes",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "PeriodicTasks",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "PeriodicSteps",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "PeriodicStepParameters",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "PeriodicSchedules",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "PeriodicRuns",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "PeriodicMonths",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "PeriodicMonthdays",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "PeriodicLog",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlugInTenant",
                table: "Plugins",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleWeekDayTenant",
                table: "PeriodicWeekDays",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleTimeTenant",
                table: "PeriodicTimes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskTenant",
                table: "PeriodicTasks",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_StepTenant",
                table: "PeriodicSteps",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_StepParamTenant",
                table: "PeriodicStepParameters",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleTenant",
                table: "PeriodicSchedules",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_RunTenant",
                table: "PeriodicRuns",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleMonthTenant",
                table: "PeriodicMonths",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleMonthDayTenant",
                table: "PeriodicMonthdays",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_LogMessageTenant",
                table: "PeriodicLog",
                column: "TenantId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlugInTenant",
                table: "Plugins");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleWeekDayTenant",
                table: "PeriodicWeekDays");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleTimeTenant",
                table: "PeriodicTimes");

            migrationBuilder.DropIndex(
                name: "IX_TaskTenant",
                table: "PeriodicTasks");

            migrationBuilder.DropIndex(
                name: "IX_StepTenant",
                table: "PeriodicSteps");

            migrationBuilder.DropIndex(
                name: "IX_StepParamTenant",
                table: "PeriodicStepParameters");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleTenant",
                table: "PeriodicSchedules");

            migrationBuilder.DropIndex(
                name: "IX_RunTenant",
                table: "PeriodicRuns");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleMonthTenant",
                table: "PeriodicMonths");

            migrationBuilder.DropIndex(
                name: "IX_ScheduleMonthDayTenant",
                table: "PeriodicMonthdays");

            migrationBuilder.DropIndex(
                name: "IX_LogMessageTenant",
                table: "PeriodicLog");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Plugins");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PeriodicWeekDays");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PeriodicTimes");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PeriodicTasks");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PeriodicSteps");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PeriodicStepParameters");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PeriodicSchedules");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PeriodicRuns");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PeriodicMonths");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PeriodicMonthdays");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "PeriodicLog");
        }
    }
}
