using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Graph.ExternalConnectors;
using Microsoft.Identity.Web;
using MicrosoftEmbedPowerBI.Models;
using MicrosoftEmbedPowerBI.Services;
using MicrosoftEmbedPowerBI.Utils;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddScoped(typeof(AadService))
                .AddScoped(typeof(PbiEmbedService))
                .AddScoped(typeof(ApprovedDomainService));

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
    // Handling SameSite cookie according to https://docs.microsoft.com/en-us/aspnet/core/security/samesite?view=aspnetcore-3.1
    options.HandleSameSiteCookieCompatibility();
});

// Sign-in users with the Microsoft identity platform
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(options =>
        {
            
            builder.Configuration.Bind("AzureAd", options);
            options.Events.OnTokenValidated = async context =>
            {
                string? tenantId = context.SecurityToken.Claims.FirstOrDefault(x => x.Type == "tid" || x.Type == "http://schemas.microsoft.com/identity/claims/tenantid")?.Value;
                string? upn = context.SecurityToken.Claims.FirstOrDefault(x => x.Type == "upn" || x.Type == "preferred_username")?.Value;

                if (string.IsNullOrWhiteSpace(tenantId))
                    throw new UnauthorizedAccessException("Unable to get tenantId from token.");


                int atIndex = upn?.IndexOf("@") ?? -1;
                if (atIndex < 0)
                {
                    throw new UnauthorizedAccessException("Unable to find domain in upn");
                }
                string domain = upn!.Substring(atIndex + 1);

                var approvedDomainService = context.HttpContext.RequestServices.GetRequiredService<ApprovedDomainService>();
                if(!(await approvedDomainService.IsDomainApproved(domain)))
                {
                    throw new UnauthorizedTenantException();
                }
                 
            };
            options.Events.OnAuthenticationFailed = (context) =>
            {
                if (context.Exception != null && context.Exception is UnauthorizedTenantException)
                {
                    context.Response.Redirect("/Home/UnauthorizedTenant");
                    context.HandleResponse(); // Suppress the exception
                }

                return Task.FromResult(0);
            };
        }
        );
//.AddInMemoryTokenCaches();

builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));

    
});

builder.Services.Configure<AzureAd>(builder.Configuration.GetSection("AzureAd"))
                    .Configure<PowerBI>(builder.Configuration.GetSection("PowerBI"))
                    .Configure<ApprovedDomains>(builder.Configuration.GetSection("ApprovedDomains"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(name: "Index",
        pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(name: "default",
        pattern: "{controller}/{action}/{id?}");

app.Run();

