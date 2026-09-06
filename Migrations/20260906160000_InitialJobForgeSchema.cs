using System;

using JobForge.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobForge.Migrations;

[DbContext(typeof(JobForgeDbContext))]
[Migration("20260906160000_InitialJobForgeSchema")]
public partial class InitialJobForgeSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AuditEntries",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                EventType = table.Column<int>(type: "integer", nullable: false),
                EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: true),
                Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_AuditEntries", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Jobs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                JobCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Priority = table.Column<int>(type: "integer", nullable: false),
                OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                JobName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Payload = table.Column<string>(type: "text", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                QueuedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                Result = table.Column<string>(type: "jsonb", nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_Jobs", x => x.Id));

        migrationBuilder.CreateTable(
            name: "RetryAttempts",
            columns: table => new
            {
                JobId = table.Column<Guid>(type: "uuid", nullable: false),
                AttemptNumber = table.Column<int>(type: "integer", nullable: false),
                ErrorMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                FailedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                NextRetryAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_RetryAttempts", x => new { x.JobId, x.AttemptNumber }));

        migrationBuilder.CreateTable(
            name: "Schedules",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                JobId = table.Column<Guid>(type: "uuid", nullable: false),
                Type = table.Column<int>(type: "integer", nullable: false),
                RunAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Schedules", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                NormalizedUsername = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Role = table.Column<int>(type: "integer", nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                PasswordHash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Users", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Webhooks",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                Url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                Event = table.Column<int>(type: "integer", nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Webhooks", x => x.Id));

        migrationBuilder.CreateIndex(
            name: "IX_Users_NormalizedUsername",
            table: "Users",
            column: "NormalizedUsername",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "AuditEntries");
        migrationBuilder.DropTable(name: "Jobs");
        migrationBuilder.DropTable(name: "RetryAttempts");
        migrationBuilder.DropTable(name: "Schedules");
        migrationBuilder.DropTable(name: "Users");
        migrationBuilder.DropTable(name: "Webhooks");
    }
}