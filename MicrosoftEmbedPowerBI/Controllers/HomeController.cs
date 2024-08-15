using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MicrosoftEmbedPowerBI.Models;

namespace MicrosoftEmbedPowerBI.Controllers
{
	[Authorize]
	public class HomeController : Controller
	{
        private readonly PbiEmbedService pbiEmbedService;
        private readonly IOptions<AzureAd> azureAd;
        private readonly IOptions<PowerBI> powerBI;

        public HomeController(PbiEmbedService pbiEmbedService, IOptions<AzureAd> azureAd, IOptions<PowerBI> powerBI)
        {
            this.pbiEmbedService = pbiEmbedService;
            this.azureAd = azureAd;
            this.powerBI = powerBI;
        }

        [HttpGet]
		public IActionResult Index()
		{
            EmbedParams embedParams = pbiEmbedService.GetEmbedParams(new Guid(powerBI.Value.WorkspaceId!), new Guid(powerBI.Value.ReportId!), username: User.Identity!.Name, role: powerBI.Value.Role);
            ViewBag.EmbedParams = embedParams;
            return View();
		}

        [HttpGet]
        [AllowAnonymous]
        public IActionResult UnauthorizedTenant()
        {
            return View();
        }


    }
}

