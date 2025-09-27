using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using tstat.Models;
using tstat.Services;

namespace tstat.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _configuration;

    public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<IActionResult> Index()
    {
        var model = new OperationsPageViewModel();
        
        var token = _configuration["TinkoffToken"];
        if (string.IsNullOrEmpty(token))
        {
            ViewBag.Error = "Токен Тинькофф не настроен";
            return View(model);
        }

        try
        {
            var service = new TinkoffService(token);
            model.Accounts = await service.GetAccountsAsync();
            
            if (model.Accounts.Any())
            {
                var firstAccount = model.Accounts.First();
                model.SelectedAccountId = firstAccount.Id;
                model.FromDate = DateTime.Today.AddYears(-10);
                model.ToDate = DateTime.Today;
                model.Operations = await service.GetOperationsAsync(firstAccount.Id, model.FromDate, model.ToDate);
            }
        }
        catch (Exception ex)
        {
            ViewBag.Error = ex.Message;
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> GetOperations(string accountId, DateTime fromDate, DateTime toDate)
    {
        var model = new OperationsPageViewModel
        {
            SelectedAccountId = accountId,
            FromDate = fromDate,
            ToDate = toDate
        };

        var token = _configuration["TinkoffToken"];
        if (string.IsNullOrEmpty(token))
        {
            ViewBag.Error = "Токен Тинькофф не настроен";
            return View("Index", model);
        }

        try
        {
            var service = new TinkoffService(token);
            model.Accounts = await service.GetAccountsAsync();
            
            if (!string.IsNullOrEmpty(accountId))
            {
                model.Operations = await service.GetOperationsAsync(accountId, fromDate, toDate);
            }
        }
        catch (Exception ex)
        {
            ViewBag.Error = ex.Message;
        }

        return View("Index", model);
    }



    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
