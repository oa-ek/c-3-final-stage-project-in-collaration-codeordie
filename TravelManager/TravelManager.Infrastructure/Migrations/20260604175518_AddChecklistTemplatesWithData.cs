using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TravelManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChecklistTemplatesWithData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChecklistTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IconClass = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChecklistTemplates_AspNetUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ChecklistTemplateItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChecklistTemplateId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistTemplateItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChecklistTemplateItems_ChecklistTemplates_ChecklistTemplateId",
                        column: x => x.ChecklistTemplateId,
                        principalTable: "ChecklistTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ChecklistTemplates",
                columns: new[] { "Id", "Description", "IconClass", "OwnerId", "Title" },
                values: new object[,]
                {
                    { 1, "Документи, одяг, гігієна, електроніка", "bi-backpack4-fill", null, "Базовий пакувальний список" },
                    { 2, "Все для відпочинку біля моря", "bi-umbrella-fill", null, "Пляжна відпустка" },
                    { 3, "Документи, ноутбук, ділові матеріали", "bi-briefcase-fill", null, "Бізнес-поїздка" },
                    { 4, "Спорядження, медикаменти, їжа", "bi-tree-fill", null, "Гірський похід" }
                });

            migrationBuilder.InsertData(
                table: "ChecklistTemplateItems",
                columns: new[] { "Id", "ChecklistTemplateId", "Content", "SortOrder" },
                values: new object[,]
                {
                    { 1, 1, "Паспорт / ID-картка", 1 },
                    { 2, 1, "Квитки (роздруківка або PDF)", 2 },
                    { 3, 1, "Страховий поліс", 3 },
                    { 4, 1, "Готівка / банківська картка", 4 },
                    { 5, 1, "Зарядний пристрій та кабелі", 5 },
                    { 6, 1, "Павербанк", 6 },
                    { 7, 1, "Зубна щітка та паста", 7 },
                    { 8, 1, "Шампунь / гель для душу", 8 },
                    { 9, 1, "Рушник", 9 },
                    { 10, 1, "Нижня білизна (кількість днів + 1)", 10 },
                    { 11, 1, "Шкарпетки", 11 },
                    { 12, 1, "Футболки / блузки", 12 },
                    { 13, 1, "Штани / спідниця", 13 },
                    { 14, 1, "Куртка / кофта", 14 },
                    { 15, 1, "Зручне взуття", 15 },
                    { 16, 1, "Ліки першої необхідності", 16 },
                    { 17, 1, "Навушники", 17 },
                    { 18, 1, "Книга / планшет для дороги", 18 },
                    { 19, 2, "Сонцезахисний крем SPF 50+", 1 },
                    { 20, 2, "Купальник / плавки", 2 },
                    { 21, 2, "Пляжний рушник", 3 },
                    { 22, 2, "Сонцезахисні окуляри", 4 },
                    { 23, 2, "Шляпа / кепка", 5 },
                    { 24, 2, "Пляжні сандалі / в'єтнамки", 6 },
                    { 25, 2, "Засіб від комарів", 7 },
                    { 26, 2, "Водонепроникна сумка / чохол для телефону", 8 },
                    { 27, 2, "Книга або електронна читалка", 9 },
                    { 28, 2, "Пляжна парасолька / шезлонг", 10 },
                    { 29, 3, "Ноутбук та зарядник", 1 },
                    { 30, 3, "Візитки", 2 },
                    { 31, 3, "Ділові документи / презентації", 3 },
                    { 32, 3, "Ручка та блокнот", 4 },
                    { 33, 3, "Ділове вбрання (костюм / плаття)", 5 },
                    { 34, 3, "Дорожній адаптер", 6 },
                    { 35, 3, "USB-хаб", 7 },
                    { 36, 3, "Маска для сну в літаку", 8 },
                    { 37, 4, "Трекінгові черевики", 1 },
                    { 38, 4, "Трекінгові палиці", 2 },
                    { 39, 4, "Рюкзак (30-50 л)", 3 },
                    { 40, 4, "Термобілизна", 4 },
                    { 41, 4, "Дощовик / мембранна куртка", 5 },
                    { 42, 4, "Аптечка першої допомоги", 6 },
                    { 43, 4, "Компас / GPS-навігатор", 7 },
                    { 44, 4, "Ліхтарик з запасними батарейками", 8 },
                    { 45, 4, "Запас їжі та води на маршрут", 9 },
                    { 46, 4, "Спальник відповідний до температури", 10 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistTemplateItems_ChecklistTemplateId",
                table: "ChecklistTemplateItems",
                column: "ChecklistTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistTemplates_OwnerId",
                table: "ChecklistTemplates",
                column: "OwnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChecklistTemplateItems");

            migrationBuilder.DropTable(
                name: "ChecklistTemplates");
        }
    }
}
