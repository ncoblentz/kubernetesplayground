using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using diagnosticapp.Models;
using System.Collections;

namespace diagnosticapp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        ViewBag.ClientIP = Request.HttpContext.Connection.RemoteIpAddress;
        ViewBag.ServerIP = Request.HttpContext.Connection.LocalIpAddress;
        ViewBag.ServerPort = Request.HttpContext.Connection.LocalPort;
        ViewBag.Path = Request.Path.ToString();
        ViewBag.Host = Request.Host.ToString();
        string environmentVariables = "";
        foreach(DictionaryEntry envVar in Environment.GetEnvironmentVariables())
        {
            environmentVariables+=String.Format("{0}: {1}\n",envVar.Key,envVar.Value);
        }
        ViewBag.EnvironmentVariables=environmentVariables;
        string headers = "";
        
        foreach(System.Collections.Generic.KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues> headerEntry in Request.Headers)
        {            
            headers+=String.Format("{0}: {1}\n",headerEntry.Key,headerEntry.Value);
        }
        ViewBag.Headers=headers;
        
        return View(ViewBag);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
