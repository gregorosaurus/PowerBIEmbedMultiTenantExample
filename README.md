# Multi-Tenant Power BI Embed Example

This repository is *simple* example of hosting/embedding a Row Level Security enabled Power BI
report within a web application for the purpose of multi-tenant access. 

## General setup steps:
1. Create a App Registration
2. Set to multi-tenant
3. Enable id_tokens for a web platform
4. Add service principal access as member/contributor to Workspace with embedded report
5. Enable service principals accessing Fabric APIs
6. Deploy Web App, configure settings

//TODO: add screenshots

## Architecture

### Required Services
 - App Service
 - Storage Account
 - Power BI A SKU

## App Settings Required

```json
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",

    "Domain": "company.onmicrosoft.com",
    "TenantId": "Common",
    "ClientId": "", //guid of the application registration
    "CallbackPath": "/signin-oidc",
    "SignedOutCallbackPath": "/signout-callback-oidc",
    "ScopeBase": [ "https://analysis.windows.net/powerbi/api/.default" ],
    "ClientSecret": "" //client secret created
  },
  "PowerBI": {
    "TenantId": "", //Entra ID tenant ID
    "WorkspaceId": "", //PBI Workspace ID
    "ReportId": "", //PBI Report ID
    "Role": "Sample"
  },
  "ApprovedDomains": {
    "AzureStorageConnectionString": "", //connection string where we host our approved domains list
    "Container": "control",
    "BlobName": "domains.csv"
  },
```
