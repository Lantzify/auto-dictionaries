using Asp.Versioning;
using Microsoft.OpenApi;
using Umbraco.Cms.Core.Composing;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Common.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Api.Management.OpenApi;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Umbraco.Cms.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using static Umbraco.Cms.Core.Diagnostics.MiniDump;

namespace UmbTreeClient.Composers
{
	public class UmbTreeClientApiComposer : IComposer
	{
		public void Compose(IUmbracoBuilder builder)
		{

			

			builder.Services.Configure<SwaggerGenOptions>(opt =>
			{
				// Related documentation:
				// https://docs.umbraco.com/umbraco-cms/tutorials/creating-a-backoffice-api
				// https://docs.umbraco.com/umbraco-cms/tutorials/creating-a-backoffice-api/adding-a-custom-swagger-document
				// https://docs.umbraco.com/umbraco-cms/tutorials/creating-a-backoffice-api/versioning-your-api
				// https://docs.umbraco.com/umbraco-cms/tutorials/creating-a-backoffice-api/access-policies

				// Configure the Swagger generation options
				// Add in a new Swagger API document solely for our own package that can be browsed via Swagger UI
				// Along with having a generated swagger JSON file that we can use to auto generate a TypeScript client
				opt.SwaggerDoc("UmbTreeClient", new OpenApiInfo
				{
					Title = "Umb Tree Client Backoffice API",
					Version = "1.0",
					// Contact = new OpenApiContact
					// {
					//     Name = "Some Developer",
					//     Email = "you@company.com",
					//     Url = new Uri("https://company.com")
					// }
				});

				// Enable Umbraco authentication for the "Example" Swagger document
				// PR: https://github.com/umbraco/Umbraco-CMS/pull/15699
				opt.CustomOperationIds(e => $"{e.ActionDescriptor.RouteValues["action"]}");
			});
		}
	}
}