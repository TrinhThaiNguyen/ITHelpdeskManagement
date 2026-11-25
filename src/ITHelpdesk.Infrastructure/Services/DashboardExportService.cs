
using ClosedXML.Excel;
using ITHelpdesk.Application.DTOs.Dashboard;
using ITHelpdesk.Application.Interfaces.Repositories;
using ITHelpdesk.Application.Interfaces.Services;

namespace ITHelpdesk.Infrastructure.Services;

public class DashboardExportService : IDashboardExportService
{
    private readonly IDashboardExportRepository _repository;

    public DashboardExportService(
        IDashboardExportRepository repository)
    {
        _repository = repository;
    }

    public async Task<byte[]> ExportDashboardSummaryAsync(
        DashboardExportRequest request,
        CancellationToken cancellationToken = default)
    {
        var data = await _repository.GetSummaryAsync(
            request.FromDate!.Value,
            request.ToDate!.Value,
            cancellationToken);

        using var workbook = CreateWorkbook(
            "Dashboard Summary",
            request);

        var worksheet = workbook.Worksheet(1);

        AddTable(
            worksheet,
            5,
            ["Metric", "Ticket Count"],
            [
                ["Total Tickets", data.TotalTickets],
                ["New Tickets", data.NewTickets],
                ["Open Tickets", data.OpenTickets],
                ["In Progress Tickets", data.InProgressTickets],
                ["Pending Tickets", data.PendingTickets],
                ["Resolved Tickets", data.ResolvedTickets],
                ["Closed Tickets", data.ClosedTickets]
            ]);

        return SaveWorkbook(workbook);
    }

    public async Task<byte[]> ExportSlaPerformanceAsync(
        DashboardExportRequest request,
        CancellationToken cancellationToken = default)
    {
        var data = await _repository.GetSlaPerformanceAsync(
            request.FromDate!.Value,
            request.ToDate!.Value,
            cancellationToken);

        using var workbook = CreateWorkbook(
            "SLA Performance",
            request);

        var worksheet = workbook.Worksheet(1);

        AddTable(
            worksheet,
            5,
            ["Metric", "Count", "Compliance Rate (%)"],
            [
                ["Total Tickets With SLA", data.TotalTicketsWithSla, null],
                ["Response Met", data.ResponseMetCount, null],
                ["Response Breached", data.ResponseBreachedCount, null],
                ["Response Pending", data.ResponsePendingCount, null],
                ["Response Compliance", null, data.ResponseComplianceRate],
                ["Resolution Met", data.ResolutionMetCount, null],
                ["Resolution Breached", data.ResolutionBreachedCount, null],
                ["Resolution Pending", data.ResolutionPendingCount, null],
                ["Resolution Compliance", null, data.ResolutionComplianceRate]
            ]);

        return SaveWorkbook(workbook);
    }

    public async Task<byte[]> ExportStaffPerformanceAsync(
        DashboardExportRequest request,
        CancellationToken cancellationToken = default)
    {
        var data = await _repository.GetStaffPerformanceAsync(
            request.FromDate!.Value,
            request.ToDate!.Value,
            cancellationToken);

        using var workbook = CreateWorkbook(
            "Staff Performance",
            request);

        var worksheet = workbook.Worksheet(1);

        AddTable(
            worksheet,
            5,
            [
                "Employee ID",
                "Full Name",
                "Email",
                "Department",
                "Total Assigned",
                "Resolved",
                "Avg Resolution Hours",
                "Avg First Response Hours",
                "SLA Met",
                "SLA Breached",
                "SLA Compliance (%)"
            ],
            data.Select(x => (object?[])[
                x.EmployeeId.ToString(),
                x.FullName,
                x.Email,
                x.DepartmentName,
                x.TotalAssignedTickets,
                x.ResolvedTickets,
                x.AverageResolutionTimeHours,
                x.AverageFirstResponseTimeHours,
                x.ResolutionSlaMetCount,
                x.ResolutionSlaBreachedCount,
                x.ResolutionSlaComplianceRate
            ]).ToList());

        return SaveWorkbook(workbook);
    }

    public async Task<byte[]> ExportDepartmentPerformanceAsync(
        DashboardExportRequest request,
        CancellationToken cancellationToken = default)
    {
        var data = await _repository.GetDepartmentPerformanceAsync(
            request.FromDate!.Value,
            request.ToDate!.Value,
            cancellationToken);

        using var workbook = CreateWorkbook(
            "Department Performance",
            request);

        var worksheet = workbook.Worksheet(1);

        AddTable(
            worksheet,
            5,
            [
                "Department ID",
                "Department",
                "Total Tickets",
                "Assigned",
                "Unassigned",
                "Resolved",
                "Avg Resolution Hours",
                "Avg First Response Hours",
                "SLA Met",
                "SLA Breached",
                "SLA Compliance (%)"
            ],
            data.Select(x => (object?[])[
                x.DepartmentId.ToString(),
                x.DepartmentName,
                x.TotalTickets,
                x.AssignedTickets,
                x.UnassignedTickets,
                x.ResolvedTickets,
                x.AverageResolutionTimeHours,
                x.AverageFirstResponseTimeHours,
                x.ResolutionSlaMetCount,
                x.ResolutionSlaBreachedCount,
                x.ResolutionSlaComplianceRate
            ]).ToList());

        return SaveWorkbook(workbook);
    }

    private static XLWorkbook CreateWorkbook(
        string title,
        DashboardExportRequest request)
    {
        var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Report");

        worksheet.Cell("A1").Value = title;
        worksheet.Range("A1:K1").Merge();

        worksheet.Cell("A2").Value = "From Date";
        worksheet.Cell("B2").Value = request.FromDate!.Value.ToString("yyyy-MM-dd");

        worksheet.Cell("D2").Value = "To Date";
        worksheet.Cell("E2").Value = request.ToDate!.Value.ToString("yyyy-MM-dd");

        worksheet.Cell("A3").Value = "Generated At (UTC)";
        worksheet.Cell("B3").Value = DateTime.UtcNow;

        worksheet.Cell("B3").Style.DateFormat.Format =
            "yyyy-MM-dd HH:mm:ss";

        worksheet.Cell("A1").Style.Font.Bold = true;
        worksheet.Cell("A1").Style.Font.FontSize = 18;
        worksheet.Cell("A1").Style.Font.FontColor = XLColor.White;
        worksheet.Cell("A1").Style.Fill.BackgroundColor =
            XLColor.FromHtml("#1F4E78");

        worksheet.Range("A1:K1").Style.Alignment.Horizontal =
            XLAlignmentHorizontalValues.Center;

        worksheet.Range("A2:E3").Style.Font.Bold = true;

        return workbook;
    }

    private static void AddTable(
        IXLWorksheet worksheet,
        int headerRow,
        string[] headers,
        List<object?[]> rows)
    {
        for (var column = 0; column < headers.Length; column++)
        {
            worksheet.Cell(headerRow, column + 1).Value =
                headers[column];
        }

        var headerRange = worksheet.Range(
            headerRow,
            1,
            headerRow,
            headers.Length);

        headerRange.Style.Font.Bold = true;
        headerRange.Style.Font.FontColor = XLColor.White;
        headerRange.Style.Fill.BackgroundColor =
            XLColor.FromHtml("#4472C4");

        for (var row = 0; row < rows.Count; row++)
        {
            for (var column = 0; column < headers.Length; column++)
            {
                worksheet.Cell(
                    headerRow + row + 1,
                    column + 1).Value =
                    XLCellValue.FromObject(rows[row][column]);
            }
        }

        if (rows.Count > 0)
        {
            var dataRange = worksheet.Range(
                headerRow,
                1,
                headerRow + rows.Count,
                headers.Length);

            var table = dataRange.CreateTable();
            table.Theme = XLTableTheme.TableStyleMedium2;
        }

        worksheet.SheetView.FreezeRows(headerRow);
        worksheet.Columns().AdjustToContents();
    }

    private static byte[] SaveWorkbook(XLWorkbook workbook)
    {
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
