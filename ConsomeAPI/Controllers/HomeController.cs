﻿using ConsomeAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Identity;
using System.Net.Http.Headers;
using System.Drawing;
using ConsomeAPI.Services;

namespace ConsomeAPI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Admin()
        {
            var Response = await TokenManager.GetAccess("https://localhost:7254/api/Account/Admin", Request);
            if (Response.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine(Response.Content.ReadAsStringAsync().Result);
                return RedirectToAction(nameof(Index));
            }
            else
            {
                Console.WriteLine("token provavelmente expirado");
                return RedirectToAction(nameof(Index));
            }
        }

        [Authorize(Roles = "Member")]
        public IActionResult Authentication()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}