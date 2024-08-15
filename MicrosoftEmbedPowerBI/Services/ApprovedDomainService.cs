using System;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using MicrosoftEmbedPowerBI.Models;

namespace MicrosoftEmbedPowerBI.Services
{
	public class ApprovedDomainService
	{
		private readonly IOptions<ApprovedDomains> _approvedDomainsSettings;

		private BlobServiceClient _blobServiceClient;
		public ApprovedDomainService(IOptions<ApprovedDomains> approvedDomainSettings)
		{
			_approvedDomainsSettings = approvedDomainSettings;

			_blobServiceClient = new BlobServiceClient(approvedDomainSettings.Value.AzureStorageConnectionString);
		}

		public async Task<List<string>> GetApprovedDomainsAsync()
		{
			var container = _blobServiceClient.GetBlobContainerClient(_approvedDomainsSettings.Value.Container);
			var blob = container.GetBlobClient(_approvedDomainsSettings.Value.BlobName);

			var downloadResponse = await blob.DownloadContentAsync();
			var csvContent = downloadResponse.Value.Content.ToString();

			List<string> lines = csvContent.Split("\n").Select(x=>x.ToLower().Trim()).ToList();
			
			return lines;
		}

		public async Task<bool> IsDomainApproved(string domain)
		{
			return (await GetApprovedDomainsAsync()).Contains(domain.ToLower());
		}
	}
}

