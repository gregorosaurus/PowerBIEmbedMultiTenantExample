using System;
using Microsoft.PowerBI.Api.Models;

namespace MicrosoftEmbedPowerBI.Models
{
    public class EmbedParams
    {
        // Type of the object to be embedded
        public string? Type { get; set; }

        // Report to be embedded
        public List<EmbedReport> EmbedReport { get; set; } = new List<EmbedReport>();

        // Embed Token for the Power BI report
        public EmbedToken? EmbedToken { get; set; }
    }
}

