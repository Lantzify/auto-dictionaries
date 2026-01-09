using BenchmarkDotNet.Attributes;
using AutoDictionaries.Benchmarks.FindStaticContent;

namespace AutoDictionaries.Benchmarks
{
	[MemoryDiagnoser]
	[RankColumn]
	public class RegexPerformanceBenchmark
	{
		private string _sampleContent;

		[GlobalSetup]
		public void Setup()
		{
			_sampleContent = @"
				@if (Model.Items.Any(x => x.IsValid()))
				{
					<div>Welcome</div>
					@foreach (var item in Model.Items.Where(x => x.Price > 10))
					{
						<p>@item.Name</p>
					}
				}";
		}

		[Benchmark(Baseline = true)]
		public List<string> WithCompiled()
		{
			return Helpers.FindStaticContentHelper.ExtractStaticContent(_sampleContent);
		}

		[Benchmark]
		public List<string> NoneCompiled()
		{
			return FindStaticContentHelperNoneCompiled.ExtractStaticContent(_sampleContent);
		}

		[Benchmark]
		public List<string> InlineCompiled()
		{
			return FindStaticContentHelperInlineCompiled.ExtractStaticContent(_sampleContent);
		}
	}
}