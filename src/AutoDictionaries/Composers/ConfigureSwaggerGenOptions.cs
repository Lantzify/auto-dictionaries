using System;
using System.Text;
using Microsoft.OpenApi;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.Extensions.DependencyInjection;

namespace AutoDictionaries.Composers
{
	internal class ConfigureSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions>
	{
		public void Configure(SwaggerGenOptions options)
		{
			options.SwaggerDoc(
			  "autoDictionaries",
			  new OpenApiInfo
			  {
				  Title = "Auto Dictionaries Management Api",
				  Version = "Latest"
			  });

			options.CustomOperationIds(e => $"{e.ActionDescriptor.RouteValues["action"]}");
		}
	}
}
