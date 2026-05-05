using System;
using AuditFramework.ApiService.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuditFramework.ApiService.Data.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260505201500_AddProfileDetailsToUser")]
    public class AddProfileDetailsToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AddressLine1",
                table: "AspNetUsers",
                type: "text",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "AddressLine2",
                table: "AspNetUsers",
                type: "text",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "AspNetUsers",
                type: "text",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "AspNetUsers",
                type: "text",
                nullable: true
            );

            migrationBuilder.AddColumn<DateOnly>(
                name: "DateOfBirth",
                table: "AspNetUsers",
                type: "date",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "AspNetUsers",
                type: "text",
                nullable: true
            );

            migrationBuilder.AddColumn<string>(
                name: "StateOrProvince",
                table: "AspNetUsers",
                type: "text",
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressLine1",
                table: "AspNetUsers"
            );

            migrationBuilder.DropColumn(
                name: "AddressLine2",
                table: "AspNetUsers"
            );

            migrationBuilder.DropColumn(
                name: "City",
                table: "AspNetUsers"
            );

            migrationBuilder.DropColumn(
                name: "Country",
                table: "AspNetUsers"
            );

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "AspNetUsers"
            );

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "AspNetUsers"
            );

            migrationBuilder.DropColumn(
                name: "StateOrProvince",
                table: "AspNetUsers"
            );
        }
    }
}
