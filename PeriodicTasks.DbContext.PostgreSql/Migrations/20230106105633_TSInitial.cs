using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PeriodicTasks.DbContext.PostgreSql.Migrations
{
    public partial class TSInitial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PeriodicTasks",
                columns: table => new
                {
                    PeriodicTaskId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    ExclusiveAreaName = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodicTasks", x => x.PeriodicTaskId);
                });

            migrationBuilder.CreateTable(
                name: "Plugins",
                columns: table => new
                {
                    PluginId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UniqueName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Constructor = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    LoadOrder = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    Disabled = table.Column<bool>(type: "boolean", nullable: true),
                    DisabledReason = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plugins", x => x.PluginId);
                });

            migrationBuilder.CreateTable(
                name: "PeriodicRuns",
                columns: table => new
                {
                    PeriodicRunId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PeriodicTaskId = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodicRuns", x => x.PeriodicRunId);
                    table.ForeignKey(
                        name: "FK_PeriodicRuns_PeriodicTasks_PeriodicTaskId",
                        column: x => x.PeriodicTaskId,
                        principalTable: "PeriodicTasks",
                        principalColumn: "PeriodicTaskId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PeriodicSchedules",
                columns: table => new
                {
                    PeriodicScheduleId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Period = table.Column<string>(type: "text", nullable: true),
                    FirstDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodicTaskId = table.Column<int>(type: "integer", nullable: false),
                    Occurrence = table.Column<int>(type: "integer", nullable: false),
                    Mod = table.Column<int>(type: "integer", nullable: false),
                    OnServiceStart = table.Column<bool>(type: "boolean", nullable: false),
                    SchedulerName = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodicSchedules", x => x.PeriodicScheduleId);
                    table.ForeignKey(
                        name: "FK_PeriodicSchedules_PeriodicTasks_PeriodicTaskId",
                        column: x => x.PeriodicTaskId,
                        principalTable: "PeriodicTasks",
                        principalColumn: "PeriodicTaskId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PeriodicSteps",
                columns: table => new
                {
                    PeriodicStepId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PeriodicTaskId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    WorkerName = table.Column<string>(type: "text", nullable: true),
                    OutputVariable = table.Column<string>(type: "text", nullable: true),
                    Command = table.Column<string>(type: "text", nullable: true),
                    StepOrder = table.Column<int>(type: "integer", nullable: false),
                    ExclusiveAreaName = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodicSteps", x => x.PeriodicStepId);
                    table.ForeignKey(
                        name: "FK_PeriodicSteps_PeriodicTasks_PeriodicTaskId",
                        column: x => x.PeriodicTaskId,
                        principalTable: "PeriodicTasks",
                        principalColumn: "PeriodicTaskId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PeriodicMonthdays",
                columns: table => new
                {
                    PeriodicMonthdayId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DayNum = table.Column<int>(type: "integer", nullable: false),
                    PeriodicScheduleId = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodicMonthdays", x => x.PeriodicMonthdayId);
                    table.ForeignKey(
                        name: "FK_PeriodicMonthdays_PeriodicSchedules_PeriodicScheduleId",
                        column: x => x.PeriodicScheduleId,
                        principalTable: "PeriodicSchedules",
                        principalColumn: "PeriodicScheduleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PeriodicMonths",
                columns: table => new
                {
                    PeriodicMonthId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PeriodicScheduleId = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<string>(type: "text", nullable: true),
                    Jan = table.Column<bool>(type: "boolean", nullable: false),
                    Feb = table.Column<bool>(type: "boolean", nullable: false),
                    Mar = table.Column<bool>(type: "boolean", nullable: false),
                    Apr = table.Column<bool>(type: "boolean", nullable: false),
                    May = table.Column<bool>(type: "boolean", nullable: false),
                    Jun = table.Column<bool>(type: "boolean", nullable: false),
                    Jul = table.Column<bool>(type: "boolean", nullable: false),
                    Aug = table.Column<bool>(type: "boolean", nullable: false),
                    Sep = table.Column<bool>(type: "boolean", nullable: false),
                    Oct = table.Column<bool>(type: "boolean", nullable: false),
                    Nov = table.Column<bool>(type: "boolean", nullable: false),
                    Dec = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodicMonths", x => x.PeriodicMonthId);
                    table.ForeignKey(
                        name: "FK_PeriodicMonths_PeriodicSchedules_PeriodicScheduleId",
                        column: x => x.PeriodicScheduleId,
                        principalTable: "PeriodicSchedules",
                        principalColumn: "PeriodicScheduleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PeriodicTimes",
                columns: table => new
                {
                    PeriodicTimeId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Time = table.Column<string>(type: "text", nullable: true),
                    PeriodicScheduleId = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodicTimes", x => x.PeriodicTimeId);
                    table.ForeignKey(
                        name: "FK_PeriodicTimes_PeriodicSchedules_PeriodicScheduleId",
                        column: x => x.PeriodicScheduleId,
                        principalTable: "PeriodicSchedules",
                        principalColumn: "PeriodicScheduleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PeriodicWeekDays",
                columns: table => new
                {
                    PeriodicWeekdayId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PeriodicScheduleId = table.Column<int>(type: "integer", nullable: false),
                    Wednesday = table.Column<bool>(type: "boolean", nullable: false),
                    Tuesday = table.Column<bool>(type: "boolean", nullable: false),
                    Thursday = table.Column<bool>(type: "boolean", nullable: false),
                    Sunday = table.Column<bool>(type: "boolean", nullable: false),
                    Saturday = table.Column<bool>(type: "boolean", nullable: false),
                    Monday = table.Column<bool>(type: "boolean", nullable: false),
                    Friday = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodicWeekDays", x => x.PeriodicWeekdayId);
                    table.ForeignKey(
                        name: "FK_PeriodicWeekDays_PeriodicSchedules_PeriodicScheduleId",
                        column: x => x.PeriodicScheduleId,
                        principalTable: "PeriodicSchedules",
                        principalColumn: "PeriodicScheduleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PeriodicLog",
                columns: table => new
                {
                    PeriodicLogId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PeriodicRunId = table.Column<int>(type: "integer", nullable: false),
                    PeriodicStepId = table.Column<int>(type: "integer", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: true),
                    MessageType = table.Column<int>(type: "integer", nullable: false),
                    LogTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TenantId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodicLog", x => x.PeriodicLogId);
                    table.ForeignKey(
                        name: "FK_PeriodicLog_PeriodicRuns_PeriodicRunId",
                        column: x => x.PeriodicRunId,
                        principalTable: "PeriodicRuns",
                        principalColumn: "PeriodicRunId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PeriodicLog_PeriodicSteps_PeriodicStepId",
                        column: x => x.PeriodicStepId,
                        principalTable: "PeriodicSteps",
                        principalColumn: "PeriodicStepId");
                });

            migrationBuilder.CreateTable(
                name: "PeriodicStepParameters",
                columns: table => new
                {
                    PeriodicStepParameterId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PeriodicStepId = table.Column<int>(type: "integer", nullable: false),
                    ParameterName = table.Column<string>(type: "text", nullable: true),
                    ParameterType = table.Column<string>(type: "text", nullable: true),
                    Value = table.Column<string>(type: "text", nullable: true),
                    Settings = table.Column<string>(type: "text", nullable: true),
                    TenantId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodicStepParameters", x => x.PeriodicStepParameterId);
                    table.ForeignKey(
                        name: "FK_PeriodicStepParameters_PeriodicSteps_PeriodicStepId",
                        column: x => x.PeriodicStepId,
                        principalTable: "PeriodicSteps",
                        principalColumn: "PeriodicStepId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LogMessageTenant",
                table: "PeriodicLog",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodicLog_PeriodicRunId",
                table: "PeriodicLog",
                column: "PeriodicRunId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodicLog_PeriodicStepId",
                table: "PeriodicLog",
                column: "PeriodicStepId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodicMonthdays_PeriodicScheduleId",
                table: "PeriodicMonthdays",
                column: "PeriodicScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleMonthDayTenant",
                table: "PeriodicMonthdays",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodicMonths_PeriodicScheduleId",
                table: "PeriodicMonths",
                column: "PeriodicScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleMonthTenant",
                table: "PeriodicMonths",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodicRuns_PeriodicTaskId",
                table: "PeriodicRuns",
                column: "PeriodicTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_RunTenant",
                table: "PeriodicRuns",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodicSchedules_PeriodicTaskId",
                table: "PeriodicSchedules",
                column: "PeriodicTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleTenant",
                table: "PeriodicSchedules",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodicStepParameters_PeriodicStepId",
                table: "PeriodicStepParameters",
                column: "PeriodicStepId");

            migrationBuilder.CreateIndex(
                name: "IX_StepParamTenant",
                table: "PeriodicStepParameters",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodicSteps_PeriodicTaskId",
                table: "PeriodicSteps",
                column: "PeriodicTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_StepTenant",
                table: "PeriodicSteps",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskTenant",
                table: "PeriodicTasks",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodicTimes_PeriodicScheduleId",
                table: "PeriodicTimes",
                column: "PeriodicScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleTimeTenant",
                table: "PeriodicTimes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodicWeekDays_PeriodicScheduleId",
                table: "PeriodicWeekDays",
                column: "PeriodicScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleWeekDayTenant",
                table: "PeriodicWeekDays",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PlugInTenant",
                table: "Plugins",
                column: "TenantId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PeriodicLog");

            migrationBuilder.DropTable(
                name: "PeriodicMonthdays");

            migrationBuilder.DropTable(
                name: "PeriodicMonths");

            migrationBuilder.DropTable(
                name: "PeriodicStepParameters");

            migrationBuilder.DropTable(
                name: "PeriodicTimes");

            migrationBuilder.DropTable(
                name: "PeriodicWeekDays");

            migrationBuilder.DropTable(
                name: "Plugins");

            migrationBuilder.DropTable(
                name: "PeriodicRuns");

            migrationBuilder.DropTable(
                name: "PeriodicSteps");

            migrationBuilder.DropTable(
                name: "PeriodicSchedules");

            migrationBuilder.DropTable(
                name: "PeriodicTasks");
        }
    }
}
