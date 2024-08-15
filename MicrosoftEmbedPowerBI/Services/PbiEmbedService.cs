using Microsoft.Extensions.Options;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Rest;
using MicrosoftEmbedPowerBI.Models;
using MicrosoftEmbedPowerBI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

public class PbiEmbedService
{
    private readonly AadService aadService;
    private readonly IOptions<PowerBI> powerBI;
    private readonly string powerBiApiUrl = "https://api.powerbi.com";

    public PbiEmbedService(AadService aadService, IOptions<PowerBI> powerBI)
    {
        this.aadService = aadService;
        this.powerBI = powerBI;
    }

    /// <summary>
    /// Get Power BI client
    /// </summary>
    /// <returns>Power BI client object</returns>
    public PowerBIClient GetPowerBIClient()
    {
        var tokenCredentials = new TokenCredentials(aadService.GetAccessToken(powerBI.Value!.TenantId!), "Bearer");
        return new PowerBIClient(new Uri(powerBiApiUrl), tokenCredentials);
    }

    /// <summary>
    /// Get embed params for a report
    /// </summary>
    /// <returns>Wrapper object containing Embed token, Embed URL, Report Id, and Report name for single report</returns>
    public EmbedParams GetEmbedParams(Guid workspaceId, Guid reportId, [Optional] Guid additionalDatasetId, string? username=null, string? role=null)
    {
        PowerBIClient pbiClient = this.GetPowerBIClient();

        // Get report info
        var pbiReport = pbiClient.Reports.GetReportInGroup(workspaceId, reportId);

        //  Check if dataset is present for the corresponding report
        //  If isRDLReport is true then it is a RDL Report 
        var isRDLReport = String.IsNullOrEmpty(pbiReport.DatasetId);

        EmbedToken embedToken;

        // Generate embed token for RDL report if dataset is not present
        if (isRDLReport)
        {
            // Get Embed token for RDL Report
            embedToken = GetEmbedTokenForRDLReport(workspaceId, reportId);
        }
        else
        {
            // Create list of datasets
            var datasetIds = new List<Guid>();

            // Add dataset associated to the report
            datasetIds.Add(Guid.Parse(pbiReport.DatasetId));

            // Append additional dataset to the list to achieve dynamic binding later
            if (additionalDatasetId != Guid.Empty)
            {
                datasetIds.Add(additionalDatasetId);
            }

            // Get Embed token multiple resources
            embedToken = GetEmbedToken(reportId, datasetIds, workspaceId, username, role);
        }

        // Add report data for embedding
        var embedReports = new List<EmbedReport>() {
                new EmbedReport
                {
                    ReportId = pbiReport.Id, ReportName = pbiReport.Name, EmbedUrl = pbiReport.EmbedUrl
                }
            };

        // Capture embed params
        var embedParams = new EmbedParams
        {
            EmbedReport = embedReports,
            Type = "Report",
            EmbedToken = embedToken
        };

        return embedParams;
    }

    

    /// <summary>
    /// Get Embed token for single report, multiple datasets, and an optional target workspace
    /// </summary>
    /// <returns>Embed token</returns>
    /// <remarks>This function is not supported for RDL Report</remakrs>
    public EmbedToken GetEmbedToken(Guid reportId, IList<Guid> datasetIds, [Optional] Guid targetWorkspaceId, string? username = null, string? role = null)
    {
        PowerBIClient pbiClient = this.GetPowerBIClient();

        // Create a request for getting Embed token 
        // This method works only with new Power BI V2 workspace experience
        var tokenRequest = new GenerateTokenRequestV2(

            reports: new List<GenerateTokenRequestV2Report>() { new GenerateTokenRequestV2Report(reportId) },

            datasets: datasetIds.Select(datasetId => new GenerateTokenRequestV2Dataset(datasetId.ToString())).ToList(),

            targetWorkspaces: targetWorkspaceId != Guid.Empty ? new List<GenerateTokenRequestV2TargetWorkspace>() { new GenerateTokenRequestV2TargetWorkspace(targetWorkspaceId) } : null
        );

        if(username != null && role != null)
        {
            tokenRequest.Identities = new List<EffectiveIdentity>()
            {
                new EffectiveIdentity(username!,datasets: datasetIds.Select(x=>x.ToString()).ToList(), roles: new List<string>() { role!} ),
            };
        }

        // Generate Embed token
        var embedToken = pbiClient.EmbedToken.GenerateToken(tokenRequest);

        return embedToken;
    }


    /// <summary>
    /// Get Embed token for RDL Report
    /// </summary>
    /// <returns>Embed token</returns>
    public EmbedToken GetEmbedTokenForRDLReport(Guid targetWorkspaceId, Guid reportId, string accessLevel = "view", string? username = null)
    {
        PowerBIClient pbiClient = this.GetPowerBIClient();

        // Generate token request for RDL Report
        var generateTokenRequestParameters = new GenerateTokenRequest(
            accessLevel: accessLevel
        );

        if (username != null)
        {
            generateTokenRequestParameters.Identities.Add(new EffectiveIdentity(username!));
        }

        // Generate Embed token
        var embedToken = pbiClient.Reports.GenerateTokenInGroup(targetWorkspaceId, reportId, generateTokenRequestParameters);

        return embedToken;
    }
}