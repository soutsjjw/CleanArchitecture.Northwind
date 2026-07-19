using System.Globalization;
using CleanArchitecture.Northwind.Application.Common.Interfaces;
using CleanArchitecture.Northwind.Application.Common.Models;
using CleanArchitecture.Northwind.Application.Features.PersonalDataAccessLogs.Queries.GetPersonalDataAccessLogsByDate;
using CleanArchitecture.Northwind.Domain.Enums;
using CleanArchitecture.Northwind.Infrastructure.Extensions;
using CleanArchitecture.Northwind.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.Northwind.Web.Controllers;

[Authorize(Roles = "SystemAdmin")]
public class AuditController : BaseController<AuditController>
{
    private readonly IApplicationDbContext _context;
    private readonly IExcelExporter _excel;
    private readonly IAppLogFileService _appLogs;

    public AuditController(IApplicationDbContext context,
        IExcelExporter excel,
        IAppLogFileService appLogs,
        ILogger<AuditController> logger)
        : base(logger)
    {
        _context = context;
        _excel = excel;
        _appLogs = appLogs;
    }

    [HttpGet]
    public IActionResult Index()
    {
        // 產生從今天起往回 1 個月（含今天、含一個月前那天）
        var today = DateOnly.FromDateTime(DateTime.Today);
        var start = today.AddMonths(-1);

        var dates = new List<DateOnly>();
        for (var d = today; d >= start; d = d.AddDays(-1))
            dates.Add(d);

        var vm = new LogCalendarViewModel { Dates = dates };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DownloadLog([FromForm] string date, CancellationToken ct)
    {
        if (!TryParseDate(date, out var d))
            return BadRequest("日期參數錯誤");

        var stream = await _appLogs.OpenAsync(d, ct);
        if (stream is null)
            return NotFound("紀錄檔不存在");

        var fileName = _appLogs.GetFileName(d);
        return File(stream, _appLogs.ContentType, fileName);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DownloadPersonalDataAccess([FromForm] string date, CancellationToken ct)
    {
        if (!TryParseDate(date, out var d))
            return BadRequest("日期參數錯誤");

        // 依你先前的 Query 取資料（這裡假設回 DTO；若回 object 也可）
        var result = await Mediator.Send(new GetPersonalDataAccessLogsByDateQuery
        {
            Date = d
        });

        if (!result.Data.Any())
            return NotFound("紀錄檔不存在");

        foreach (var item in result.Data)
        {
            item.Action = ((ActionType)Enum.Parse(typeof(ActionType), item.Action)).GetDisplayName();
        }

        var cols = new List<ExcelColumn<PersonalDataAccessLogDto>>
        {
            new("Row#", x => x.Id),
            new("檢視個資人員", x => x.ViewerUserName),
            new("被檢視個資人員", x => x.TargetUserName),
            new("動作", x => x.Action),
            new("日期", x => x.Accessed.ToLocalTime()),
            new("說明", x => x.Description),
        };

        // 匯出（用反射把屬性展開）
        var bytes = _excel.Export(result.Data, cols, "個資瀏覽紀錄");

        var fileName = $"個資瀏覽紀錄_{d:yyyyMMdd}.xlsx";
        const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        return File(bytes, contentType, fileName);
    }

    private static bool TryParseDate(string? s, out DateOnly date)
        => DateOnly.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
}
