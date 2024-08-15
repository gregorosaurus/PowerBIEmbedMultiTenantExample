using System;
namespace MicrosoftEmbedPowerBI.Models
{
	public class ApprovedDomains
	{
		public string? AzureStorageConnectionString { get; set; }
		public string? Container { get; set; } = "control";
		public string? BlobName { get; set; } = "domains.csv";
	}
}

