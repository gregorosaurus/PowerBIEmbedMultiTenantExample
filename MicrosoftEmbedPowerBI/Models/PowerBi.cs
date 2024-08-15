using System;
namespace MicrosoftEmbedPowerBI.Models
{
    public class PowerBI
    {
        // Workspace Id for which Embed token needs to be generated
        public string? WorkspaceId { get; set; }

        // Report Id for which Embed token needs to be generated
        public string? ReportId { get; set; }

        public string? Role { get; set; }

        public string? TenantId { get; set; }
    }
}

