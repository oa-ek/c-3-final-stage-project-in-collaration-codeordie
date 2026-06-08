using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelManager.Domain.Entities;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace TravelManager.Infrastructure.Services
{
    public class ExcelExportService
    {
        private static readonly Color ColHeaderBg = ColorTranslator.FromHtml("#1E293B"); 
        private static readonly Color ColHeaderFg = Color.White;
        private static readonly Color ColAccent = ColorTranslator.FromHtml("#F59E0B"); 
        private static readonly Color ColGreen = ColorTranslator.FromHtml("#10B981"); 
        private static readonly Color ColRed = ColorTranslator.FromHtml("#F43F5E");
        private static readonly Color ColRowAlt = ColorTranslator.FromHtml("#F8FAFC"); 
        private static readonly Color ColBorder = ColorTranslator.FromHtml("#E2E8F0");
        private static readonly Color ColSectionBg = ColorTranslator.FromHtml("#F1F5F9"); 
        private static readonly Color ColTotalBg = ColorTranslator.FromHtml("#FFFBEB"); 

        private static readonly Dictionary<int, string> CategoryNames = new()
        {
            { 1, "Проживання" }, { 2, "Транспорт" }, { 3, "Їжа" },
            { 4, "Розваги" },    { 5, "Шопінг" },    { 6, "Інше" }
        };

        public byte[] GenerateTripReport(
            Trip trip,
            IEnumerable<Expense> expenses,
            IEnumerable<ExpenseSplit> splits,
            string? filterUserId = null,
            string? filterUserName = null)
        {
            ExcelPackage.License.SetNonCommercialOrganization("NaU_OA");

            using var pkg = new ExcelPackage();

            BuildExpensesSheet(pkg, trip, expenses, filterUserId, filterUserName);
            BuildDebtsSheet(pkg, trip, splits, filterUserId, filterUserName);
            BuildNetSettlementSheet(pkg, trip, splits);

            return pkg.GetAsByteArray();
        }

        private void BuildExpensesSheet(
            ExcelPackage pkg,
            Trip trip,
            IEnumerable<Expense> expenses,
            string? filterUserId,
            string? filterUserName)
        {
            var ws = pkg.Workbook.Worksheets.Add("Витрати");

            if (filterUserId != null)
                expenses = expenses.Where(e => e.PayerId == filterUserId);

            var expList = expenses.OrderBy(e => e.Date).ToList();

            int row = WriteReportHeader(ws, trip,
                filterUserName != null ? $"Витрати — {filterUserName}" : "Витрати",
                1);
            row++;

            string[] headers = { "№", "Дата", "Назва витрати", "Категорія", "Сплатив", "Сума", "Валюта" };
            int[] widths = { 4, 12, 35, 16, 20, 14, 10 };

            WriteTableHeader(ws, row, headers, widths);
            row++;

            int num = 1;
            foreach (var e in expList)
            {
                bool alt = num % 2 == 0;
                var rowBg = alt ? ColRowAlt : Color.White;

                ws.Cells[row, 1].Value = num++;
                ws.Cells[row, 2].Value = e.Date;
                ws.Cells[row, 2].Style.Numberformat.Format = "dd.mm.yyyy";
                ws.Cells[row, 3].Value = e.Title;
                ws.Cells[row, 4].Value = CategoryNames.TryGetValue(e.CategoryId, out var cat) ? cat : "Інше";
                ws.Cells[row, 5].Value = e.Payer?.UserName ?? e.PayerId;
                ws.Cells[row, 6].Value = e.TotalAmount;
                ws.Cells[row, 6].Style.Numberformat.Format = "#,##0.00";
                ws.Cells[row, 7].Value = e.Currency;

                StyleDataRow(ws, row, 1, 7, rowBg);
                ws.Cells[row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                row++;
            }

            row++;
            WriteSectionTitle(ws, row, 1, 7, "Підсумок за категоріями");
            row++;

            WriteTableHeader(ws, row, new[] { "Категорія", "Кількість", "Сума" }, new[] { 16, 12, 14 }, startCol: 1, colCount: 3);
            row++;

            var byCategory = expList
                .GroupBy(e => CategoryNames.TryGetValue(e.CategoryId, out var c) ? c : "Інше")
                .OrderByDescending(g => g.Sum(e => e.TotalAmount));

            foreach (var g in byCategory)
            {
                ws.Cells[row, 1].Value = g.Key;
                ws.Cells[row, 2].Value = g.Count();
                ws.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[row, 3].Value = g.Sum(e => e.TotalAmount);
                ws.Cells[row, 3].Style.Numberformat.Format = "#,##0.00";
                ws.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                StyleDataRow(ws, row, 1, 3, Color.White);
                row++;
            }

            WriteTotalRow(ws, row, 1, 3,
                "РАЗОМ", expList.Count, expList.Sum(e => e.TotalAmount),
                trip.BaseCurrency);

            for (int c = 1; c <= 7; c++)
                ws.Column(c).AutoFit();
            ws.Column(1).Width = 4;
        }

        private void BuildDebtsSheet(
            ExcelPackage pkg,
            Trip trip,
            IEnumerable<ExpenseSplit> splits,
            string? filterUserId,
            string? filterUserName)
        {
            var ws = pkg.Workbook.Worksheets.Add("Борги");

            if (filterUserId != null)
                splits = splits.Where(s => s.DebtorId == filterUserId || s.Expense?.PayerId == filterUserId);

            var list = splits.OrderBy(s => s.IsSettled).ThenBy(s => s.Expense?.Date).ToList();

            int row = WriteReportHeader(ws, trip,
                filterUserName != null ? $"Борги — {filterUserName}" : "Борги",
                1);
            row++;

            string[] headers = { "№", "Дата витрати", "Витрата", "Боржник", "Кредитор", "Сума", "Валюта", "Статус" };
            int[] widths = { 4, 13, 30, 20, 20, 14, 10, 12 };

            WriteTableHeader(ws, row, headers, widths);
            row++;

            int num = 1;
            foreach (var s in list)
            {
                bool alt = num % 2 == 0;
                ws.Cells[row, 1].Value = num++;
                ws.Cells[row, 2].Value = s.Expense?.Date;
                ws.Cells[row, 2].Style.Numberformat.Format = "dd.mm.yyyy";
                ws.Cells[row, 3].Value = s.Expense?.Title ?? "—";
                ws.Cells[row, 4].Value = s.Debtor?.UserName ?? s.DebtorId;
                ws.Cells[row, 5].Value = s.Expense?.Payer?.UserName ?? s.Expense?.PayerId ?? "—";
                ws.Cells[row, 6].Value = s.OwedAmount;
                ws.Cells[row, 6].Style.Numberformat.Format = "#,##0.00";
                ws.Cells[row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                ws.Cells[row, 7].Value = s.Expense?.Currency ?? trip.BaseCurrency;

                var statusCell = ws.Cells[row, 8];
                if (s.IsSettled)
                {
                    statusCell.Value = "✓ Оплачено";
                    statusCell.Style.Font.Color.SetColor(ColGreen);
                }
                else
                {
                    statusCell.Value = "⏳ Висить";
                    statusCell.Style.Font.Color.SetColor(ColRed);
                }
                statusCell.Style.Font.Bold = true;
                statusCell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                StyleDataRow(ws, row, 1, 8, alt ? ColRowAlt : Color.White);
                ws.Cells[row, 8].Style.Font.Color.SetColor(s.IsSettled ? ColGreen : ColRed);
                row++;
            }

            row++;
            var pending = list.Where(s => !s.IsSettled).ToList();
            var settled = list.Where(s => s.IsSettled).ToList();

            WriteDebtsTotal(ws, row, list, pending, settled, trip.BaseCurrency);

            for (int c = 1; c <= 8; c++) ws.Column(c).AutoFit();
            ws.Column(1).Width = 4;
        }

        private void BuildNetSettlementSheet(
            ExcelPackage pkg,
            Trip trip,
            IEnumerable<ExpenseSplit> splits)
        {
            var ws = pkg.Workbook.Worksheets.Add("Взаєморозрахунки");

            int row = WriteReportHeader(ws, trip, "Взаєморозрахунки (нетто)", 1);
            row++;

            ws.Cells[row, 1].Value = "Показано мінімальну кількість транзакцій для закриття всіх боргів між учасниками.";
            ws.Cells[row, 1].Style.Font.Italic = true;
            ws.Cells[row, 1].Style.Font.Color.SetColor(ColorTranslator.FromHtml("#64748B"));
            ws.Cells[row, 1, row, 6].Merge = true;
            row += 2;

            var balance = new Dictionary<string, (string name, decimal amount)>();

            foreach (var s in splits.Where(s => !s.IsSettled))
            {
                string debtorId = s.DebtorId;
                string creditorId = s.Expense?.PayerId ?? "";
                string debtorName = s.Debtor?.UserName ?? debtorId;
                string creditorName = s.Expense?.Payer?.UserName ?? creditorId;

                if (debtorId == creditorId) continue; 

                if (!balance.ContainsKey(debtorId)) balance[debtorId] = (debtorName, 0);
                if (!balance.ContainsKey(creditorId)) balance[creditorId] = (creditorName, 0);

                balance[debtorId] = (debtorName, balance[debtorId].amount - s.OwedAmount);
                balance[creditorId] = (creditorName, balance[creditorId].amount + s.OwedAmount);
            }

            WriteSectionTitle(ws, row, 1, 6, "Баланс кожного учасника");
            row++;
            WriteTableHeader(ws, row, new[] { "Учасник", "Баланс", "Валюта", "Роль" }, new[] { 25, 14, 10, 16 }, startCol: 1, colCount: 4);
            row++;

            foreach (var kvp in balance.OrderBy(b => b.Value.amount))
            {
                var (name, amt) = kvp.Value;
                ws.Cells[row, 1].Value = name;
                ws.Cells[row, 2].Value = Math.Abs(amt);
                ws.Cells[row, 2].Style.Numberformat.Format = "#,##0.00";
                ws.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                ws.Cells[row, 3].Value = trip.BaseCurrency;

                string role = amt > 0.01m ? "Кредитор (йому винні)" :
                              amt < -0.01m ? "Боржник (він винен)" : "Закрито";
                ws.Cells[row, 4].Value = role;

                var rowBg = amt > 0.01m ? Color.FromArgb(240, 253, 244)   
                          : amt < -0.01m ? Color.FromArgb(255, 241, 242)  
                          : ColRowAlt;

                StyleDataRow(ws, row, 1, 4, rowBg);
                ws.Cells[row, 2].Style.Font.Color.SetColor(amt >= 0 ? ColGreen : ColRed);
                ws.Cells[row, 2].Style.Font.Bold = true;
                row++;
            }

            row += 2;

            WriteSectionTitle(ws, row, 1, 6, "План виплат (мінімальна кількість транзакцій)");
            row++;
            WriteTableHeader(ws, row, new[] { "Хто платить", "Кому платить", "Сума", "Валюта" },
                             new[] { 25, 25, 14, 10 }, startCol: 1, colCount: 4);
            row++;

            var settlements = ComputeMinimalSettlements(balance, trip.BaseCurrency);

            if (!settlements.Any())
            {
                ws.Cells[row, 1].Value = "Усі борги закриті — жодної транзакції не потрібно.";
                ws.Cells[row, 1].Style.Font.Italic = true;
                ws.Cells[row, 1].Style.Font.Color.SetColor(ColGreen);
                ws.Cells[row, 1, row, 4].Merge = true;
                row++;
            }
            else
            {
                int num = 1;
                foreach (var (from, to, amt, curr) in settlements)
                {
                    bool alt = num++ % 2 == 0;
                    ws.Cells[row, 1].Value = from;
                    ws.Cells[row, 2].Value = to;
                    ws.Cells[row, 3].Value = amt;
                    ws.Cells[row, 3].Style.Numberformat.Format = "#,##0.00";
                    ws.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    ws.Cells[row, 3].Style.Font.Bold = true;
                    ws.Cells[row, 3].Style.Font.Color.SetColor(ColRed);
                    ws.Cells[row, 4].Value = curr;
                    StyleDataRow(ws, row, 1, 4, alt ? ColRowAlt : Color.White);
                    ws.Cells[row, 3].Style.Font.Color.SetColor(ColRed);
                    row++;
                }
            }

            for (int c = 1; c <= 6; c++) ws.Column(c).AutoFit();
        }

        private static List<(string from, string to, decimal amt, string currency)> ComputeMinimalSettlements(
            Dictionary<string, (string name, decimal amount)> balance,
            string currency)
        {
            var result = new List<(string, string, decimal, string)>();

            var debtors = balance.Where(b => b.Value.amount < -0.005m)
                                   .Select(b => (b.Key, b.Value.name, -b.Value.amount))
                                   .OrderByDescending(x => x.Item3).ToList();
            var creditors = balance.Where(b => b.Value.amount > 0.005m)
                                   .Select(b => (b.Key, b.Value.name, b.Value.amount))
                                   .OrderByDescending(x => x.Item3).ToList();

            var debtAmt = debtors.Select(d => d.Item3).ToList();
            var credAmt = creditors.Select(c => c.Item3).ToList();

            int i = 0, j = 0;
            while (i < debtors.Count && j < creditors.Count)
            {
                decimal payment = Math.Min(debtAmt[i], credAmt[j]);
                if (payment > 0.005m)
                {
                    result.Add((debtors[i].name, creditors[j].name,
                                Math.Round(payment, 2), currency));
                }
                debtAmt[i] -= payment;
                credAmt[j] -= payment;
                if (debtAmt[i] < 0.005m) i++;
                if (credAmt[j] < 0.005m) j++;
            }

            return result;
        }

        private int WriteReportHeader(ExcelWorksheet ws, Trip trip, string sheetTitle, int startRow)
        {
            int r = startRow;

            ws.Cells[r, 1, r, 8].Merge = true;
            ws.Cells[r, 1].Value = "TravelManager — Фінансовий звіт";
            ws.Cells[r, 1].Style.Font.Bold = true;
            ws.Cells[r, 1].Style.Font.Size = 9;
            ws.Cells[r, 1].Style.Font.Color.SetColor(ColorTranslator.FromHtml("#94A3B8"));
            r++;

            ws.Cells[r, 1, r, 8].Merge = true;
            ws.Cells[r, 1].Value = sheetTitle;
            ws.Cells[r, 1].Style.Font.Bold = true;
            ws.Cells[r, 1].Style.Font.Size = 20;
            ws.Cells[r, 1].Style.Font.Color.SetColor(ColHeaderBg);
            r++;

            ws.Cells[r, 1, r, 8].Merge = true;
            ws.Cells[r, 1].Value = $"Подорож: {trip.Title}   |   " +
                                   $"{trip.StartDate:dd.MM.yyyy} – {trip.EndDate:dd.MM.yyyy}   |   " +
                                   $"Валюта: {trip.BaseCurrency}   |   " +
                                   $"Звіт: {DateTime.Now:dd.MM.yyyy HH:mm}";
            ws.Cells[r, 1].Style.Font.Size = 9;
            ws.Cells[r, 1].Style.Font.Color.SetColor(ColorTranslator.FromHtml("#64748B"));
            r++;

            ws.Cells[r, 1, r, 8].Merge = true;
            ws.Cells[r, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[r, 1].Style.Fill.BackgroundColor.SetColor(ColAccent);
            ws.Row(r).Height = 3;
            r++;

            return r; 
        }

        private void WriteTableHeader(ExcelWorksheet ws, int row, string[] headers, int[] widths,
                                      int startCol = 1, int colCount = -1)
        {
            int count = colCount < 0 ? headers.Length : colCount;
            for (int i = 0; i < headers.Length; i++)
            {
                int col = startCol + i;
                var cell = ws.Cells[row, col];
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Font.Color.SetColor(ColHeaderFg);
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(ColHeaderBg);
                cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Row(row).Height = 22;
                ApplyBorder(cell);

                if (i < widths.Length)
                    ws.Column(col).Width = widths[i];
            }
        }

        private void StyleDataRow(ExcelWorksheet ws, int row, int fromCol, int toCol, Color bg)
        {
            for (int col = fromCol; col <= toCol; col++)
            {
                var cell = ws.Cells[row, col];
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(bg);
                cell.Style.Font.Name = "Arial";
                cell.Style.Font.Size = 10;
                cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ApplyBorder(cell);
            }
            ws.Row(row).Height = 18;
        }

        private void WriteSectionTitle(ExcelWorksheet ws, int row, int fromCol, int toCol, string title)
        {
            ws.Cells[row, fromCol, row, toCol].Merge = true;
            var cell = ws.Cells[row, fromCol];
            cell.Value = title;
            cell.Style.Font.Bold = true;
            cell.Style.Font.Size = 11;
            cell.Style.Font.Color.SetColor(ColHeaderBg);
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(ColSectionBg);
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            ws.Row(row).Height = 20;
        }

        private void WriteTotalRow(ExcelWorksheet ws, int row, int fromCol, int toCol,
                                   string label, int count, decimal total, string currency)
        {
            ws.Cells[row, fromCol, row, fromCol].Value = label;
            ws.Cells[row, fromCol + 1].Value = count;
            ws.Cells[row, fromCol + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ws.Cells[row, fromCol + 2].Value = total;
            ws.Cells[row, fromCol + 2].Style.Numberformat.Format = "#,##0.00";
            ws.Cells[row, fromCol + 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            ws.Cells[row, fromCol + 2].Style.Font.Color.SetColor(ColAccent);

            for (int col = fromCol; col <= toCol; col++)
            {
                var cell = ws.Cells[row, col];
                cell.Style.Font.Bold = true;
                cell.Style.Font.Size = 10;
                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(ColTotalBg);
                ApplyBorder(cell);
            }
            ws.Row(row).Height = 20;
        }

        private void WriteDebtsTotal(ExcelWorksheet ws, int row,
            List<ExpenseSplit> all, List<ExpenseSplit> pending, List<ExpenseSplit> settled, string currency)
        {
            ws.Cells[row, 1, row, 5].Merge = true;
            ws.Cells[row, 1].Value = "Непогашені борги";
            ws.Cells[row, 6].Value = pending.Sum(s => s.OwedAmount);
            ws.Cells[row, 6].Style.Numberformat.Format = "#,##0.00";
            ws.Cells[row, 6].Style.Font.Color.SetColor(ColRed);
            ws.Cells[row, 6].Style.Font.Bold = true;
            ws.Cells[row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            ws.Cells[row, 7].Value = currency;
            for (int c = 1; c <= 8; c++) ApplyBorder(ws.Cells[row, c]);
            ws.Cells[row, 1, row, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[row, 1, row, 8].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 241, 242));
            ws.Cells[row, 1].Style.Font.Bold = true;
            row++;

            ws.Cells[row, 1, row, 5].Merge = true;
            ws.Cells[row, 1].Value = "Погашені борги";
            ws.Cells[row, 6].Value = settled.Sum(s => s.OwedAmount);
            ws.Cells[row, 6].Style.Numberformat.Format = "#,##0.00";
            ws.Cells[row, 6].Style.Font.Color.SetColor(ColGreen);
            ws.Cells[row, 6].Style.Font.Bold = true;
            ws.Cells[row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            ws.Cells[row, 7].Value = currency;
            for (int c = 1; c <= 8; c++) ApplyBorder(ws.Cells[row, c]);
            ws.Cells[row, 1, row, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[row, 1, row, 8].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(240, 253, 244));
            ws.Cells[row, 1].Style.Font.Bold = true;
            row++;

            ws.Cells[row, 1, row, 5].Merge = true;
            ws.Cells[row, 1].Value = "ЗАГАЛЬНА СУМА БОРГІВ";
            ws.Cells[row, 6].Value = all.Sum(s => s.OwedAmount);
            ws.Cells[row, 6].Style.Numberformat.Format = "#,##0.00";
            ws.Cells[row, 6].Style.Font.Color.SetColor(ColAccent);
            ws.Cells[row, 6].Style.Font.Bold = true;
            ws.Cells[row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            ws.Cells[row, 7].Value = currency;
            for (int c = 1; c <= 8; c++) ApplyBorder(ws.Cells[row, c]);
            ws.Cells[row, 1, row, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[row, 1, row, 8].Style.Fill.BackgroundColor.SetColor(ColTotalBg);
            ws.Cells[row, 1].Style.Font.Bold = true;
        }

        private static void ApplyBorder(ExcelRange cell)
        {
            var b = cell.Style.Border;
            b.Top.Style = b.Bottom.Style = b.Left.Style = b.Right.Style = ExcelBorderStyle.Thin;
            b.Top.Color.SetColor(ColBorder);
            b.Bottom.Color.SetColor(ColBorder);
            b.Left.Color.SetColor(ColBorder);
            b.Right.Color.SetColor(ColBorder);
        }
    }
}