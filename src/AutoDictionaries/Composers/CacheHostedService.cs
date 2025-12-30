using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using AutoDictionaries.Core.Services.Interfaces;

namespace AutoDictionaries.Composers
{
	public class CacheHostedService : IHostedService
	{
		private readonly IADTemplateService _adTemplateService;
		private readonly IADPartialViewService _adPartialViewService;

		public CacheHostedService(IADTemplateService adTemplateService,
			IADPartialViewService adPartialViewService)
		{
			_adTemplateService = adTemplateService;
			_adPartialViewService = adPartialViewService;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			_ = Task.Run(() => _adTemplateService.GetAllTemplates(), cancellationToken);
			_ = Task.Run(() => _adPartialViewService.GetAllPartialViews(), cancellationToken);
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
	}
}
