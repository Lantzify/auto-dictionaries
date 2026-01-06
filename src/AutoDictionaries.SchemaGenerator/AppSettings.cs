namespace AutoDictionaries.SchemaGenerator
{
	internal class AppSettings
	{
		public AutoDictionariesDefinition AutoDictionaries { get; set; } = new();

		internal class AutoDictionariesDefinition
		{
			public bool Translate { get; set; } = false;
			public TranslatorType? Translator { get; set; } = TranslatorType.DeepL;
			public string? ApiKey { get; set; }
			public string? ApiEndpoint { get; set; }
			public string? ApiRegion { get; set; }
		}
	}

	internal enum TranslatorType
	{
		DeepL,
		MicrosoftTranslation
	}
}
