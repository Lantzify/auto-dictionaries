using CommandLine;

namespace AutoDictionaries.SchemaGenerator
{
	internal class Options
	{
		[Option('o', "outputFile", Required = false,
			HelpText = "",
			Default = "..\\..\\..\\..\\AutoDictionaries\\appsettings-schema.auto-dictionaries.json")]
		public string OutputFile { get; set; } = "..\\..\\..\\..\\AutoDictionaries\\appsettings-schema.auto-dictionaries.json";
	}
}