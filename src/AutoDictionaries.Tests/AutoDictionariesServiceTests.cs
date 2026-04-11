using AutoDictionaries.Models;
using FluentAssertions.Common;
using Umbraco.Cms.Core.Models;
using AutoDictionaries.Services;
using Umbraco.Cms.Core.Services;
using AutoDictionaries.Core.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace AutoDictionaries.Tests
{
	[TestFixture]
	public class AutoDictionariesServiceTests
	{
		private Mock<ILanguageService> _languageServiceMock;
		private Mock<IDictionaryItemService> _dictionaryItemServiceMock;
		private Mock<IWebHostEnvironment> _webHostEnvironmentMock;
		private Mock<IOptions<AutoDictionariesSettings>> _adSettingsMock;
		private AutoDictionariesService _service;

		[SetUp]
		public void Setup()
		{
			_languageServiceMock = new Mock<ILanguageService>();
			_dictionaryItemServiceMock = new Mock<IDictionaryItemService>();
			_webHostEnvironmentMock = new Mock<IWebHostEnvironment>();
			_adSettingsMock = new Mock<IOptions<AutoDictionariesSettings>>();

			// Setup default mocks
			var settings = new AutoDictionariesSettings();
			_adSettingsMock.Setup(x => x.Value).Returns(settings);

			var mockLanguages = new List<ILanguage>
			{
				CreateMockLanguage("en-US"),
				CreateMockLanguage("de-DE")
			};

			_languageServiceMock.Setup(x => x.GetAllAsync()).ReturnsAsync(mockLanguages);
			_dictionaryItemServiceMock.Setup(x => x.GetAtRootAsync()).ReturnsAsync(new List<IDictionaryItem>());

			_service = new AutoDictionariesService(_languageServiceMock.Object, _webHostEnvironmentMock.Object, _adSettingsMock.Object, _dictionaryItemServiceMock.Object);
		}

		private static ILanguage CreateMockLanguage(string isoCode)
		{
			var mock = new Mock<ILanguage>();
			mock.Setup(x => x.IsoCode).Returns(isoCode);
			return mock.Object;
		}

		private static IDictionaryItem CreateMockDictionaryItem(Guid key, string itemKey, params string[] translations)
		{
			var mock = new Mock<IDictionaryItem>();
			mock.Setup(x => x.Key).Returns(key);
			mock.Setup(x => x.ItemKey).Returns(itemKey);
			mock.Setup(x => x.Id).Returns(1);

			var dictionaryTranslations = new List<IDictionaryTranslation>();
			foreach (var translation in translations)
			{
				var translationMock = new Mock<IDictionaryTranslation>();
				translationMock.Setup(x => x.Value).Returns(translation);
				dictionaryTranslations.Add(translationMock.Object);
			}
			mock.Setup(x => x.Translations).Returns(dictionaryTranslations);

			return mock.Object;
		}

		private AutoDictionariesService CreateServiceWithPreloadedDictionaries(List<DictionaryModel> dictionaries)
		{
			// Create mock dictionary items from the DictionaryModels
			var mockDictionaryItems = dictionaries.Select(d =>
				CreateMockDictionaryItem(d.Guid, d.Key, d.Translations.ToArray())
			).ToList();

			// Setup the mock to return these at root level
			_dictionaryItemServiceMock.Setup(x => x.GetAtRootAsync()).ReturnsAsync(mockDictionaryItems);

			// Setup GetAsync for each dictionary item
			foreach (var dict in dictionaries)
			{
				var mockItem = CreateMockDictionaryItem(dict.Guid, dict.Key, dict.Translations.ToArray());
				_dictionaryItemServiceMock.Setup(x => x.GetAsync(dict.Guid)).ReturnsAsync(mockItem);
				_dictionaryItemServiceMock.Setup(x => x.GetAsync(dict.Key)).ReturnsAsync(mockItem);
			}

			// Setup GetChildrenAsync to return empty list (no nested items for these tests)
			_dictionaryItemServiceMock.Setup(x => x.GetChildrenAsync(It.IsAny<Guid>()))
				.ReturnsAsync(new List<IDictionaryItem>());

			return new AutoDictionariesService(_languageServiceMock.Object, _webHostEnvironmentMock.Object, _adSettingsMock.Object, _dictionaryItemServiceMock.Object);
		}

		[Test]
		public async Task GetStaticContentFromView_EmptyString_ReturnsEmptyList()
		{
			// Arrange
			var viewContent = "";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().BeEmpty();
		}

		[Test]
		public void GetStaticContentFromView_NullInput_ThrowsArgumentNullException()
		{
			// Arrange
			string viewContent = null;

			// Act & Assert
			Assert.ThrowsAsync<ArgumentNullException>(async () => await _service.GetStaticContentFromView(viewContent));
		}

		[Test]
		public async Task GetStaticContentFromView_SimpleHtmlContent_ExtractsTextSuccessfully()
		{
			// Arrange
			var viewContent = @"<div>Hello World</div>";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().HaveCount(1);
			result[0].StaticContent.Should().Be("Hello World");
			result[0].Used.Should().Be(1);
			result[0].Dictionary.Should().BeNull(); // No matching dictionary
		}

		[Test]
		public async Task GetStaticContentFromView_MultipleElements_ExtractsAllTextContent()
		{
			// Arrange
			var viewContent = @"<div>Welcome</div><p>Thank you</p><span>Goodbye</span>";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().HaveCount(3);
			result.Should().Contain(x => x.StaticContent == "Welcome");
			result.Should().Contain(x => x.StaticContent == "Thank you");
			result.Should().Contain(x => x.StaticContent == "Goodbye");
			result.All(x => x.Used == 1).Should().BeTrue();
		}

		[Test]
		public async Task GetStaticContentFromView_DuplicateText_AggregatesUsageCount()
		{
			// Arrange
			var viewContent = @"<div>Hello</div><p>Hello</p><span>Hello</span>";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().HaveCount(1);
			result[0].StaticContent.Should().Be("Hello");
			result[0].Used.Should().Be(3);
		}

		[Test]
		public async Task GetStaticContentFromView_PunctuationAndApostrophes_PreservesSpecialCharacters()
		{
			// Arrange
			var viewContent = @"<div>Hello, World!</div><p>What's up?</p>";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().HaveCount(2);
			result.Should().Contain(x => x.StaticContent == "Hello, World!");
			result.Should().Contain(x => x.StaticContent == "What's up?");
		}

		[Test]
		public async Task GetStaticContentFromView_ExistingDictionaryTranslation_MatchesDictionaryItem()
		{
			// Arrange
			var dictionaries = new List<DictionaryModel>
			{
				new DictionaryModel
				{
					Id = 1,
					Key = "welcome_message",
					Guid = Guid.NewGuid(),
					Translations = new List<string> { "Welcome", "Willkommen" }
				}
			};

			var serviceWithDictionaries = CreateServiceWithPreloadedDictionaries(dictionaries);
			var viewContent = @"<div>Welcome</div>";

			// Act
			var result = await serviceWithDictionaries.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().HaveCount(1);
			result[0].StaticContent.Should().Be("Welcome");
			result[0].Dictionary.Should().NotBeNull();
			result[0].Dictionary.Key.Should().Be("welcome_message");
		}

		[Test]
		public async Task GetStaticContentFromView_EmailAddressWithAtSymbol_ExtractsTextBeforeAtSign()
		{
			// Arrange
			var viewContent = @"<span>Email: test@example.com</span>";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().HaveCount(1);

			result[0].StaticContent.Should().Be("Email");
		}

		[Test]
		public async Task GetStaticContentFromView_MixedStaticAndRazorContent_ExtractsOnlyStaticText()
		{
			// Arrange
			var viewContent = @"<div>Welcome</div><div>@Model.Title</div>";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().Contain(x => x.StaticContent == "Welcome");
		}

		[Test]
		public async Task GetStaticContentFromView_WhitespaceOnlyContent_FiltersOutEmptyStrings()
		{
			// Arrange
			var viewContent = @"<div>   </div><p>
            </p><span>	</span>";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().BeEmpty();
		}

		[Test]
		public async Task GetStaticContentFromView_SingleCharacterStrings_FiltersOutShortContent()
		{
			// Arrange
			var viewContent = @"<div>A</div><p>B</p><span>C</span>";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().BeEmpty();
		}

		[Test]
		public async Task GetStaticContentFromView_TextStartingWithNumber_ExtractsContent()
		{
			// Arrange
			var viewContent = @"<div>404 Not Found</div><span>50% off today</span>";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().HaveCount(2);
			result.Should().Contain(x => x.StaticContent == "Not Found");
			result.Should().Contain(x => x.StaticContent == "off today");
		}

		[Test]
		public async Task GetStaticContentFromView_NestedHtmlStructure_ExtractsAllLevelsOfContent()
		{
			// Arrange
			var viewContent = @"
                <div class='header'>
                    <h1>Main Title</h1>
                    <nav>
                        <ul>
                            <li>Home</li>
                            <li>About Us</li>
                            <li>Contact</li>
                        </ul>
                    </nav>
                </div>
                <main>
                    <p>This is the main content area</p>
                    <button>Click here</button>
                </main>";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().Contain(x => x.StaticContent == "Main Title");
			result.Should().Contain(x => x.StaticContent == "Home");
			result.Should().Contain(x => x.StaticContent == "About Us");
			result.Should().Contain(x => x.StaticContent == "Contact");
			result.Should().Contain(x => x.StaticContent == "This is the main content area");
			result.Should().Contain(x => x.StaticContent == "Click here");
		}

		[Test]
		public async Task GetStaticContentFromView_MixedStaticAndDynamicContent_IgnoresRazorExpressions()
		{
			// Arrange
			var viewContent = @"
                <h1>@Model.Title</h1>
                <p>Welcome to our site</p>
                <span>Copyright 2024</span>";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().Contain(x => x.StaticContent == "Welcome to our site");
			result.Should().Contain(x => x.StaticContent == "Copyright 2024");
		}

		[Test]
		public async Task GetStaticContentFromView_ValidMultipleElements_ReturnsCorrectCount()
		{
			// Arrange
			var viewContent = @"
                <div>Valid Content</div>
                <span>Another Valid Text</span>";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().HaveCount(2);
			result.Should().Contain(x => x.StaticContent == "Valid Content");
			result.Should().Contain(x => x.StaticContent == "Another Valid Text");
		}

		[Test]
		public async Task GetStaticContentFromView_RazorIfStatement_FiltersOutConditionalLogic()
		{
			// Arrange
			var viewContent = @"<div>@if (condition) { <p>Some text<p> }</div>";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			// If statements should be filtered out by the regex
			result.Should().NotContain(x => x.StaticContent.Contains("if ("));
		}

		[Test]
		public async Task GetStaticContentFromView_RazorForLoop_FiltersOutLoopSyntax()
		{
			// Arrange
			var viewContent = @"<div>@for (var item in items) { <p>Some text</p> }</div>";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			// If statements should be filtered out by the regex
			result.Should().NotContain(x => x.StaticContent.Contains("for ("));
		}

		[Test]
		public async Task GetStaticContentFromView_DeeplyNestedElements_ExtractsAllNestedText()
		{
			// Arrange
			var viewContent = @"
                    <h1>Main Header</h1>
                <div>
                    <div>
                        <p>Nested paragraph</p>
                        <span>
                            <strong>Bold text</strong>
                        </span>
                    </div>
                </div>";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().Contain(x => x.StaticContent == "Main Header");
			result.Should().Contain(x => x.StaticContent == "Nested paragraph");
			result.Should().Contain(x => x.StaticContent == "Bold text");
		}

		[Test]
		public async Task GetStaticContentFromView_RepeatedTextInMultipleElements_CountsAllOccurrences()
		{
			// Arrange
			var viewContent = @"
                <div>Error</div>
                <p>Success</p>
                <span>Error</span>
                <h1>Success</h1>
                <div>Error</div>";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().HaveCount(2);

			var errorItem = result.FirstOrDefault(x => x.StaticContent == "Error");
			errorItem.Should().NotBeNull();
			errorItem!.Used.Should().Be(3);

			var successItem = result.FirstOrDefault(x => x.StaticContent == "Success");
			successItem.Should().NotBeNull();
			successItem!.Used.Should().Be(2);
		}

		[Test]
		public async Task GetStaticContentFromView_SubstringMatch_DoesNotMatchPartialDictionary()
		{
			// Arrange
			var dictionaries = new List<DictionaryModel>
			{
				new DictionaryModel
				{
					Id = 1,
					Key = "greeting",
					Guid = Guid.NewGuid(),
					Translations = new List<string> { "Hello" }
				}
			};

			var serviceWithDictionaries = CreateServiceWithPreloadedDictionaries(dictionaries);
			var viewContent = @"<div>Hello World</div>"; // Should not match "Hello" dictionary

			// Act
			var result = await serviceWithDictionaries.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().HaveCount(1);
			result[0].StaticContent.Should().Be("Hello World");
			result[0].Dictionary.Should().BeNull(); // Should not match partial content
		}

		[Test]
		public async Task GetStaticContentFromView_ExactDictionaryMatch_FindsMatchingDictionary()
		{
			// Arrange
			var dictionaries = new List<DictionaryModel>
			{
				new DictionaryModel
				{
					Id = 1,
					Key = "greeting",
					Guid = Guid.NewGuid(),
					Translations = new List<string> { "Hello World" }
				}
			};

			var serviceWithDictionaries = CreateServiceWithPreloadedDictionaries(dictionaries);
			var viewContent = @"<div>Hello World</div>";

			// Act
			var result = await serviceWithDictionaries.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().HaveCount(1);
			result[0].StaticContent.Should().Be("Hello World");
			result[0].Dictionary.Should().NotBeNull();
			result[0].Dictionary.Key.Should().Be("greeting");
		}

		[Test]
		public async Task GetStaticContentFromView_MultipleDictionariesWithMixedMatches_MatchesExactTranslationsOnly()
		{
			// Arrange
			var dictionaries = new List<DictionaryModel>
			{
				new DictionaryModel
				{
					Id = 1,
					Key = "welcome",
					Guid = Guid.NewGuid(),
					Translations = new List<string> { "Welcome", "Willkommen" }
				},
				new DictionaryModel
				{
					Id = 2,
					Key = "goodbye",
					Guid = Guid.NewGuid(),
					Translations = new List<string> { "Goodbye", "Auf Wiedersehen" }
				}
			};

			var serviceWithDictionaries = CreateServiceWithPreloadedDictionaries(dictionaries);
			var viewContent = @"<div>Welcome</div><p>Goodbye</p><span>Hello</span>";

			// Act
			var result = await serviceWithDictionaries.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().HaveCount(3);

			var welcomeItem = result.FirstOrDefault(x => x.StaticContent == "Welcome");
			welcomeItem.Should().NotBeNull();
			welcomeItem!.Dictionary.Should().NotBeNull();
			welcomeItem.Dictionary.Key.Should().Be("welcome");

			var goodbyeItem = result.FirstOrDefault(x => x.StaticContent == "Goodbye");
			goodbyeItem.Should().NotBeNull();
			goodbyeItem!.Dictionary.Should().NotBeNull();
			goodbyeItem.Dictionary.Key.Should().Be("goodbye");

			var helloItem = result.FirstOrDefault(x => x.StaticContent == "Hello");
			helloItem.Should().NotBeNull();
			helloItem!.Dictionary.Should().BeNull(); // No matching dictionary
		}

		[Test]
		public async Task GetStaticContentFromView_ComplexUmbracoSearchView_FiltersOutAllRazorCode()
		{
			// Arrange
			var viewContent = @"@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage<ContentModels.Search>

@using Clean.Core.Models.ViewModels
@using ContentModels = Umbraco.Cms.Web.Common.PublishedModels
@using Umbraco.Cms.Core
@using Clean.Core.Extensions
@inject Umbraco.Cms.Core.IPublishedContentQuery publishedContentQuery

@{
    Layout = ""master.cshtml"";
    var searchQuery = Context.Request.Query[""q""];
    var docTypesToIgnore = new[] { Category.ModelTypeAlias, CategoryList.ModelTypeAlias, Error.ModelTypeAlias, Search.ModelTypeAlias, XMlsitemap.ModelTypeAlias };
}

@await Html.PartialAsync(""~/Views/Partials/pageHeader.cshtml"", new PageHeaderViewModel(Model.Name, Model.Title, Model.Subtitle, Model.MainImage))

<div class=""container"">
    <form action=""@Model.Url()"" method=""GET"" id=""search"">
        <div class=""row"">
            <div class=""col-lg-8 col-md-10 mx-auto"">
                <div class=""form-group controls"">
                    <input type=""text"" class=""form-control col-xs-6"" placeholder=""@Umbraco.GetDictionaryValue(""Search.Placeholder"")"" name=""q"" value=""@searchQuery"" />
                </div>
            </div>
            <div class=""col-lg-8 col-md-10 mx-auto my-3"">
                <div class=""form-group"">
                    <button class=""btn btn-primary search-button float-end"">@Umbraco.GetDictionaryValue(""Search.SearchButton"") <i class=""fa fa-search""></i></button>
                </div>
            </div>
            <div class=""col-lg-8 col-md-10 mx-auto"">
                @if (!string.IsNullOrWhiteSpace(searchQuery))
                {
                    var results = publishedContentQuery.Search(searchQuery).Where(x => !docTypesToIgnore.Contains(x.Content.ContentType.Alias));
                    long resultCount = results != null && results.Any() ? results.Count() : 0;
                    @Html.Raw(string.Format(Umbraco.GetDictionaryValue(""Search.Results""), resultCount, searchQuery.ToString().StripHtml()))
                    if (resultCount > 0)
                    {
                        foreach (var result in results)
                        {
                            <div class=""post-preview"">
                                <a href=""@result.Content.Url()"">
                                    <h2 class=""post-title"">
                                        @(result.Content.HasProperty(""title"") && result.Content.HasValue(""title"") && !string.IsNullOrWhiteSpace(result.Content.Value<string>(""title"")) ? result.Content.Value(""title"") : result.Content.Name)
                                    </h2>
                                    @if (result.Content.HasProperty(""subtitle"") && result.Content.HasValue(""subtitle"") && !string.IsNullOrWhiteSpace(result.Content.Value<string>(""subtitle"")))
                                    {
                                        <h3 class=""post-subtitle"">@(result.Content.Value<string>(""subtitle""))</h3>
                                    }
                                </a>
                                @if (result.Content is IArticleControls article && ((result.Content.HasProperty(""author"") && result.Content.HasValue(""author""))
                               || (result.Content.HasProperty(""articleDate"") && result.Content.HasValue(""articleDate"") && result.Content.Value<DateTime>(""articleDate"") > DateTime.MinValue)))
                                {
                                    var author = article.Author;
                                    <p class=""post-meta"">
                                        @Umbraco.GetDictionaryValue(""Article.Posted"")
                                        @Umbraco.GetDictionaryValue(""Article.By"")@Html.Raw(""&nbsp;"")@(author.Name)

                                        @if (article.ArticleDate != null && article.ArticleDate > DateTime.MinValue)
                                        {
                                            @Umbraco.GetDictionaryValue(""Article.On"")

                                            @:&nbsp;@(article.ArticleDate.ToString(""MMMM dd, yyyy""))
                                        }
                                    </p>
                                }
                            </div>
                        }
                        <hr>
                    }
                }
            </div>
        </div>
    </form>
</div>";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().BeEmpty();
		}

		[Test]
		public async Task GetStaticContentFromView_UmbracoFooterWithAnchors_ExtractsStaticTextBetweenTags()
		{
			// Arrange
			var viewContent = @"<!-- Footer-->
<footer>
    <div class=""container px-4 px-lg-5"">
        <div class=""row gx-4 gx-lg-5 justify-content-center"">
            <div class=""col-md-10 col-lg-8 col-xl-7"">
                @if (homePage.SocialIconLinks != null && homePage.SocialIconLinks.Any())
                {
                    <ul class=""list-inline text-center"">
                        @Html.GetBlockListHtml(homePage.SocialIconLinks)
                    </ul>
                }
                <p class=""small text-center text-muted fst-italic"">@Umbraco.GetDictionaryValue(""Footer.CopyrightTitle"") &copy; @DateTime.Now.Year @Umbraco.GetDictionaryValue(""Footer.CopyrightStatement"")</p>
                <p class=""small text-center text-muted fst-italic"">Theme by <a href=""https://startbootstrap.com/"" target=""_blank"" rel=""noopener"">Start Bootstrap</a>, implemented in Umbraco by Paul Seal from <a href=""https://codeshare.co.uk"" target=""_blank"" rel=""noopener"">codeshare.co.uk</a></p>
            </div>
        </div>
    </div>
</footer>
";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().HaveCount(3);
			result[0].StaticContent.Should().Be("Theme by");
			result[1].StaticContent.Should().Be("Start Bootstrap");
			result[2].StaticContent.Should().Be("implemented in Umbraco by Paul Seal from");
		}

		[Test]
		public async Task GetStaticContentFromView_ImageAltAttribute_ExtractsAccessibilityText()
		{
			// Arrange - alt attributes are critical for WCAG accessibility (screen readers)
			var viewContent = @"<img src=""logo.png"" alt=""Company Logo"" />";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().HaveCount(1);
			result[0].StaticContent.Should().Be("Company Logo");
		}

		[Test]
		public async Task GetStaticContentFromView_AnchorTitleAttribute_ExtractsTooltipText()
		{
			// Arrange - title attributes provide additional context (tooltips)
			var viewContent = @"<a href=""/about"" title=""Learn more about our company"">About Us</a>";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().HaveCount(2);
			result.Should().Contain(x => x.StaticContent == "Learn more about our company");
			result.Should().Contain(x => x.StaticContent == "About Us");
		}

		[Test]
		public async Task GetStaticContentFromView_InputPlaceholderAttribute_ExtractsFormGuidanceText()
		{
			// Arrange - placeholder attributes guide users in form inputs
			var viewContent = @"<input type=""text"" placeholder=""Enter your name"" />";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().HaveCount(1);
			result[0].StaticContent.Should().Be("Enter your name");
		}

		[Test]
		public async Task GetStaticContentFromView_SubmitButtonValueAttribute_ExtractsButtonText()
		{
			// Arrange - value attributes on buttons contain translatable text
			var viewContent = @"<input type=""submit"" value=""Submit Form"" />";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().HaveCount(1);
			result[0].StaticContent.Should().Be("Submit Form");
		}

		[Test]
		public async Task GetStaticContentFromView_MultipleAccessibilityAttributes_ExtractsAllWcagContent()
		{
			// Arrange - complex form with multiple accessibility attributes
			var viewContent = @"
				<form>
					<label for=""email"">Email Address</label>
					<input type=""email"" id=""email"" placeholder=""you@example.com"" title=""Enter a valid email address"" />
					<input type=""submit"" value=""Subscribe"" />
					<img src=""help.png"" alt=""Help icon"" title=""Click for help"" />
				</form>";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().Contain(x => x.StaticContent == "Email Address");
			result.Should().Contain(x => x.StaticContent == "Enter a valid email address");
			result.Should().Contain(x => x.StaticContent == "Subscribe");
			result.Should().Contain(x => x.StaticContent == "Help icon");
			result.Should().Contain(x => x.StaticContent == "Click for help");
		}

		[Test]
		public async Task GetStaticContentFromView_SingleQuotedAttributes_HandlesAlternativeQuoteSyntax()
		{
			// Arrange - attributes can use single quotes
			var viewContent = @"<img src='image.jpg' alt='Profile picture' title='User avatar' />";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().HaveCount(2);
			result.Should().Contain(x => x.StaticContent == "Profile picture");
			result.Should().Contain(x => x.StaticContent == "User avatar");
		}

		[Test]
		public async Task GetStaticContentFromView_RazorExpressionInAttribute_SkipsDynamicAttributes()
		{
			// Arrange - attributes containing Razor expressions should be filtered
			var viewContent = @"<input type=""text"" placeholder=""@Model.PlaceholderText"" />";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().BeEmpty();
		}

		[Test]
		public async Task GetStaticContentFromView_UrlInTitleAttribute_ExtractsOnlyTranslatableContent()
		{
			// Arrange - URL values shouldn't be extracted as translatable content
			var viewContent = @"<a href=""https://example.com"" title=""Visit our website"">Link</a>";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			// Should contain title and link text, but not the URL
			result.Should().Contain(x => x.StaticContent == "Visit our website");
			result.Should().Contain(x => x.StaticContent == "Link");
			result.Should().NotContain(x => x.StaticContent.Contains("https://"));
		}

		[Test]
		public async Task GetStaticContentFromView_AriaLabelAttribute_ExtractsAccessibilityLabel()
		{
			// Arrange - aria-label is important for accessibility but not currently extracted
			var viewContent = @"<button aria-label=""Close dialog"">X</button>";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();

			result.Should().Contain(x => x.StaticContent == "Close dialog");
		}

		[Test]
		public async Task GetStaticContentFromView_RepeatedAttributeValues_AggregatesAllInstances()
		{
			// Arrange - same text in multiple attributes
			var viewContent = @"
				<input type=""submit"" value=""Submit"" />
				<input type=""submit"" value=""Submit"" />
				<button>Submit</button>";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().HaveCount(1);
			result[0].StaticContent.Should().Be("Submit");
			result[0].Used.Should().Be(3); // 2 from value attributes + 1 from button text
		}

		[Test]
		public async Task GetStaticContentFromView_EmptyAttributeValues_IgnoresBlankAttributes()
		{
			// Arrange - empty attributes shouldn't produce results
			var viewContent = @"<input type=""text"" placeholder="""" title="""" /><img src=""x.png"" alt="""" />";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().BeEmpty();
		}

		[Test]
		public async Task GetStaticContentFromView_ComplexFormWithMixedAttributes_ExtractsBothContentAndAttributes()
		{
			// Arrange - realistic form with both element content and attributes
			var viewContent = @"
				<div class=""form-container"" aria-label=""Contact form"">
					<h2>Contact Us</h2>
					<p>Fill out the form below to get in touch.</p>
					<form>
						<input type=""text"" name=""name"" placeholder=""Your full name"" title=""Required field"" />
						<input type=""email"" name=""email"" placeholder=""Email address"" />
						<textarea placeholder=""Your message""></textarea>
						<button type=""submit"">Send Message</button>
					</form>
					<img src=""contact.jpg"" alt=""Customer service representative"" />
				</div>";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().Contain(x => x.StaticContent == "Contact form");
			result.Should().Contain(x => x.StaticContent == "Contact Us");
			result.Should().Contain(x => x.StaticContent == "Fill out the form below to get in touch");
			result.Should().Contain(x => x.StaticContent == "Your full name");
			result.Should().Contain(x => x.StaticContent == "Required field");
			result.Should().Contain(x => x.StaticContent == "Email address");
			result.Should().Contain(x => x.StaticContent == "Your message");
			result.Should().Contain(x => x.StaticContent == "Send Message");
			result.Should().Contain(x => x.StaticContent == "Customer service representative");
		}

		[Test]
		public async Task GetStaticContentFromView_NestedConditionalLogicWithIfAnd_FiltersOutComplexRazorSyntax()
		{
			// Arrange - realistic form with both element content and attributes
			var viewContent = @"@model IPublishedContent
@{

    var homePage = Model.AncestorOrSelf(""home"");
    void RenderChildPages(IEnumerable<IPublishedContent> contentItems)
    {
        if (contentItems.Any())
        {
            foreach (var content in contentItems.Where(x => x.IsVisible()))
            {
                if (!(content.HasProperty(""excludeFromSitemap"") && content.Value<bool>(""excludeFromSitemap"")))
                {
<url><loc>@content.Url(mode:UrlMode.Absolute)</loc><lastmod>@content.UpdateDate.ToString(""yyyy-MM-ddTHH:mm:sszzz"")</lastmod></url>
                    if (content.Children.Any(x => x.IsVisible()))
                    {
                        RenderChildPages(content.Children);
                    }
                }
            }
        }
    };
}
";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().BeEmpty();
		}

		[Test]
		public async Task GetStaticContentFromView_UmbracoAuthorListView_FiltersOutAllDynamicContent()
		{
			// Arrange - realistic form with both element content and attributes
			var viewContent = @"@model IPublishedContent
@{
    AuthorList authorList = UmbracoContext.Content.GetAtRoot().DescendantsOrSelf<AuthorList>().FirstOrDefault();
    int modelId = Model.Id;
    var isAuthorListPage = modelId == authorList?.Id;
    var fallbackPageSize = isAuthorListPage ? 10 : 3;

    var pageSize = QueryStringHelper.GetIntFromQueryString(Context.Request.Query, ""size"", fallbackPageSize);
    var pageNumber = QueryStringHelper.GetIntFromQueryString(Context.Request.Query, ""page"", 1);
    var allAuthors = authorList?.Children<Author>().Where(x => x.IsVisible()) ?? Enumerable.Empty<Author>();
    var pageOfAuthors = allAuthors.Skip((pageNumber - 1) * pageSize).Take(pageSize);
    var totalItemCount = allAuthors.Count();
    var pageCount = totalItemCount > 0 ? Math.Ceiling((double)totalItemCount / pageSize) : 1;

}

<div class=""container"">
    <div class=""row"">
        <div class=""col-lg-8 col-md-10 mx-auto"">
            <div class=""container-fluid"">
                <div class=""row"">
                    @foreach (var author in pageOfAuthors)
                    {
                        <div class=""col-4 mx-auto"">
                            <div class=""card"">
                                <header>
                                    <img src=""@(author.MainImage.Url())"" alt=""@author.Name"" class=""w-100"" />
                                </header>
                                <div class=""card-body"">
                                    <div class=""content-left text-start my-auto py-4"">
                                        <h2 class=""card-title"">@author.Name</h2>
                                        <p class=""card-description"">@author.MetaDescription</p>
                                        <a href=""@author.Url()"" class=""text-primary"">
                                            @Umbraco.GetDictionaryValue(""Author.ReadMore"")
                                        </a>
                                    </div>
                                </div>
                            </div>
                        </div>
                    }
                </div>
            </div>

            @if (isAuthorListPage)
            {
                @await Component.InvokeAsync(""Pagination"", new { totalItems = totalItemCount, url = Model.Url(), pageNumber = pageNumber, pageSize = pageSize })
            }
        </div>
    </div>
</div>
";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().BeEmpty();
		}

		[Test]
		public async Task GetStaticContentFromView_UmbracoPageHeaderView_FiltersOutComplexConditionals()
		{
			// Arrange - realistic form with both element content and attributes
			var viewContent = @"@inherits UmbracoViewPage<Clean.Core.Models.ViewModels.PageHeaderViewModel>

@{
    string mainImageUrl = Model.HasBackgroundImage ? Model.BackgroundImage.GetCropUrl(1903, 628) : ""/media/f01jqvmq/2.jpg"";
}

<header class=""masthead"" style=""background-image: url('@mainImageUrl')"">
    <div class=""container position-relative px-4 px-lg-5"">
        <div class=""row gx-4 gx-lg-5 justify-content-center"">
            <div class=""col-md-10 col-lg-8 col-xl-7"">
                <div class=""@(Model.ArticleDate.HasValue ? ""post-heading"" : ""site-heading"")"">
                    <h1>@(!string.IsNullOrWhiteSpace(Model.Title) ? Model.Title : Model.Name)</h1>

                    @if (Model.IsArticle)
                    {
                        if (Model.HasSubtitle)
                        {
                            <h2 class=""subheading mb-4"">@Model.Subtitle</h2>
                        }
                        <span class=""meta"">
                            @Umbraco.GetDictionaryValue(""Article.Posted"")
                            @if (Model.HasAuthor)
                            {
                                @Umbraco.GetDictionaryValue(""Article.By"")@Html.Raw(""&nbsp;"")@Model.AuthorName
                            }
                            @Umbraco.GetDictionaryValue(""Article.On"")@Html.Raw(""&nbsp;"")@Model.ArticleDate.Value.ToString(""MMMM dd, yyyy"")
                        </span>
                        @if (Model.Categories != null && Model.Categories.Any())
                        {
                            <span class=""mt-4 d-block""></span>
                            @foreach (var category in Model.Categories.Select(x => x.Name).OrderBy(y => y))
                            {
                                <span class=""badge rounded-pill bg-light text-dark border-dark border-5"">@category</span>
                            }
                        }
                    }
                    else
                    {
                        if (Model.HasSubtitle)
                        {
                            <span class=""subheading"">@(Model.Subtitle)</span>
                        }
                    }
                </div>
            </div>
        </div>
    </div>
</header>

";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().BeEmpty();
		}

		[Test]
		public async Task GetStaticContentFromView_XmlSitemapTemplate_IgnoresTimeFormatsAndDates()
		{
			// Arrange - realistic form with both element content and attributes
			var viewContent = @"@model IPublishedContent
@{
    var metaKeywords = Model.Value<IEnumerable<string>>(""metaKeywords"");
    var homePage = Model.AncestorOrSelf(""home"");
    void RenderChildPages(IEnumerable<IPublishedContent> contentItems)
    {
        if (contentItems.Any())
        {
            foreach (var content in contentItems.Where(x => x.IsVisible()))
            {
                if (!(content.HasProperty(""excludeFromSitemap"") && content.Value<bool>(""excludeFromSitemap"")))
                {
<url><loc>@content.Url(mode:UrlMode.Absolute)</loc><lastmod>12:20</lastmod></url>
                    if (content.Children.Any(x => x.IsVisible()))
                    {
                        RenderChildPages(content.Children);
                    }
                }
            }
        }
    };
}

<urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9""
    xmlns:image=""http://www.google.com/schemas/sitemap-image/1.1""
    xmlns:video=""http://www.google.com/schemas/sitemap-video/1.1"">
<url><loc>@homePage.Url(mode: UrlMode.Absolute)</loc><priority>1.0</priority><lastmod>+00:00</lastmod></url>
    @{
        RenderChildPages(homePage.Children);
    }
</urlset>
<div>12:30</div>
<div>2024-10-12</div>
";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().BeEmpty();
		}

		[Test]
		public async Task GetStaticContentFromView_DynamicMarkupTemplate_FiltersOutRazorStringManipulation()
		{
			// Arrange - realistic form with both element content and attributes
			var viewContent = @"@model dynamic

@if (Model?.editor.config.markup is not null)
{
    string markup = Model.editor.config.markup.ToString();
    markup = markup.Replace(""#value#"", Html.ReplaceLineBreaks((string)Model.value.ToString()).ToString());

    if (Model.editor.config.style != null)
    {
        markup = markup.Replace(""#style#"", Model.editor.config.style.ToString());
    }

    <text>
        @Html.Raw(markup)
    </text>
}
else
{
    <text>
        <div style=""@Model?.editor.config.style"">@Model?.value</div>
    </text>
}
";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().BeEmpty();
		}

		[Test]
		public async Task GetStaticContentFromView_RazorFunctionsBlock_IgnoresCodeDefinitions()
		{
			// Arrange - realistic form with both element content and attributes
			var viewContent = @"@functions {
	private int GetFractionSortOrder(string fractionName)
	{
		var orderMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase) {};

		if (orderMap.TryGetValue(fractionName, out int order))
		{
			return order;
		}

		return 1000;
	}
}";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().BeEmpty();
		}

		[Test]
		public async Task GetStaticContentFromView_HtmlHelperTextBoxes_FiltersOutHtmlHelpers()
		{
			// Arrange - realistic form with both element content and attributes
			var viewContent = @"@Html.TextBox(""password"", """", new { @type = ""password"", @class = ""form-control""})
            @Html.ValidationMessage(""password"", ViewData.ModelState.ConvertErrorToDictionaryKey(""Password"", Umbraco))

            @Html.TextBox(""confirmPassword"", """", new { @type = ""password"", @class = ""form-control""})
            @Html.ValidationMessage(""confirmPassword"", ViewData.ModelState.ConvertErrorToDictionaryKey(""ConfirmPassword"", Umbraco))

            @Html.ValidationMessage(""IsTrue"", ViewData.ModelState.ConvertErrorToDictionaryKey(""IsTrue"", Umbraco))


            @Html.Hidden(""email"", email)
            @Html.Hidden(""token"", token)

            @Html.ValidationMessage(""updatePasswordModel"")
        </div>";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().BeEmpty();
		}

		[Test]
		public async Task GetStaticContentFromView_LinqQueryWithMethodChaining_IgnoresComplexQueries()
		{
			// Arrange - realistic form with both element content and attributes
			var viewContent = @"@{
    var wasteCategoriesPage = Umbraco.ContentAtRoot().DescendantsOrSelfOfType(""wasteCategories"")?.First();
    var wasteCategories = wasteCategoriesPage.GetChildren().Where(x => x.IsVisible() && x.Value<bool>(""showOnHomepage""))
                                                           .OrderBy(x => x.HasValue(""order"") ? x.Value(""order"") : null);


}";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().BeEmpty();
		}

		[Test]
		public async Task GetStaticContentFromView_LinqWithTernaryOperators_FiltersOutConditionalExpressions()
		{
			// Arrange - realistic form with both element content and attributes
			var viewContent = @"@{
    var newsPage = Model.ContentType.Alias == ""newsPage"" ? Model : Model.Parent;

    var newsItems = newsPage.GetChildren().OrderByDescending(x => x.HasValue(""publishedDate"") ? x.Value<DateTime>(""publishedDate"") : x.CreateDate);
    var selectedNewsItem = Model.ContentType.Alias == ""newsItem"" ? Model : newsItems.First();
}";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().BeEmpty();
		}

		[Test]
		public async Task GetStaticContentFromView_UmbracoFormsEmailTemplate_FiltersOutForeachAndSwitch()
		{
			// Arrange - realistic form with both element content and attributes
			var viewContent = @"@inherits UmbracoViewPage<Umbraco.Forms.Core.Models.FormsHtmlModel>
<!DOCTYPE html>
<html>
<head>
    <title></title>
    <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"" />
    <link href=""https://fonts.googleapis.com/css2?family=Poppins:wght@400;600&display=swap"" rel=""stylesheet"">
</head>
<body style=""background-color: #fff; margin: 0 !important; padding: 0 !important;"">
    <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""margin-bottom: 40px;"">
      
        <!-- HERO -->
        <tr>
            <td bgcolor=""#fff"" align=""center"" style=""padding: 0px 10px 0px 10px;"">
                <!--[if (gte mso 9)|(IE)]>
                <table align=""center"" border=""0"" cellspacing=""0"" cellpadding=""0"" width=""600"">
                <tr>
                <td align=""center"" valign=""top"" width=""600"">
                </td>
                </tr>
                </table>
            </td>
        </tr>

        <!-- COPY BLOCK -->
        <tr>
            <td bgcolor=""#F3F3F5"" align=""center"" style=""padding: 0px 10px 0px 10px;"">
                <!--[if (gte mso 9)|(IE)]>
                <table align=""center"" border=""0"" cellspacing=""0"" cellpadding=""0"" width=""600"">
                <tr>
                <td align=""center"" valign=""top"" width=""600"">
            <![endif]-->
                <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px;"">

      
  

                    <!-- COPY -->
                    <tr>
                        <td bgcolor=""#ffffff"" align=""left"" style=""padding: 20px 30px 40px 30px; color: #303033; font-family: 'Lato', Helvetica, Arial, sans-serif; font-size: 18px; font-weight: 400; line-height: 25px;"">

                            @foreach (var field in Model.Fields)
                            {
                                <h4 style=""font-weight: 700; margin: 0; color: #000000;"">@field.Name</h4>

                                switch (field.FieldType)
                                {
                                    case ""FieldType.FileUpload.cshtml"":
                                        <p style=""margin-top: 0;""><a href=""@siteDomain/@field.GetValue()"" target=""_blank"" style=""color: #00AEA2;"">@field.GetValue()</a></p>
                                        break;

                                    case ""FieldType.DatePicker.cshtml"":
                                        DateTime dt;
                                        var fieldValue = field.GetValue();
                                        var dateValid = DateTime.TryParse(fieldValue != null ? fieldValue.ToString() : string.Empty, out dt);
                                        var dateStr = dateValid ? dt.ToString(""f"") : """";
                                        <p style=""margin-top: 0;"">@dateStr</p>
                                        break;

                                    case ""FieldType.CheckboxList.cshtml"":
                                        <p style=""margin-top: 0;"">
                                            @foreach (var color in field.GetValues())
                                            {
                                                @color<br />
                                            }
                                        </p>
                                        break;
                                    default:
                                        <p style=""margin-top: 0;"">@field.GetValue()</p>
                                        break;
                                }
                            }

                        </td>
                    </tr>
                </table>
                <!--[if (gte mso 9)|(IE)]>
                </td>
                </tr>
                </table>
            <![endif]-->
            </td>
        </tr>



    </table>
</body>
</html>
";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().BeEmpty();
		}

		[Test]
		public async Task GetStaticContentFromView_SwedishTextWithParentheses_ExtractsSeparateTextSegments()
		{
			// Arrange
			var viewContent = @"<span>Din sökning på <strong>@Model.SearchTerm</strong> gav (<strong>@Model.TotalSearchResults</strong>) resultat</span>";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().HaveCount(3);
			result.Should().Contain(x => x.StaticContent == "Din sökning på");
			result.Should().Contain(x => x.StaticContent == "gav");
			result.Should().Contain(x => x.StaticContent == "resultat");
		}

		[Test]
		public async Task GetStaticContentFromView_EnglishTextWithParentheses_ExtractsSeparateTextSegments()
		{
			// Arrange
			var viewContent = @"<p>Search results for '@query' (@results.TotalItemCount)</p>";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().HaveCount(1);
			result.Should().Contain(x => x.StaticContent == "Search results for");
		}

		

		[Test]
		public async Task GetStaticContentFromView_ExamineIndexSearchCode_IgnoresDotNotationAndNullChecks()
		{
			// Arrange
			var viewContent = @"@using Umbraco.Cms.Web.Common.PublishedModels;
@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage

	if (ExamineManager.TryGetIndex(""ExternalIndex"", out var index))
	{
		var searcher = index.Searcher;

		if (results.NullCheck())
		{
			<ul>
				@foreach (var result in results.Skip((page - 1) * pageSize).Take(pageSize))
				{
					if (result.Id != null)
					{
						var item = Umbraco.Content(result.Id);

						<li>
							<a href=""@item?.Url()"">@item?.Name</a>
						</li>
					}
				}
			</ul>
		}
	}";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().BeEmpty();
		}

		[Test]
		public async Task GetStaticContentFromView_NavigationPartialWithTernary_FiltersOutConditionalClassAssignment()
		{
			// Arrange
			var viewContent = @"@await Html.PartialAsync(""Navigation/Navigation"", new NavbarItem {
                    Model = Model,
                    CurrentPage = Model,
                    IsMobile = false
                })";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().BeEmpty();
		}

		[Test]
		public async Task GetStaticContentFromView_TextWithEqualSign_PreservesNonRazorEquals()
		{
			// Arrange
			var viewContent = @"<p>So in conclusion, Together = Stronger</p>";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().Contain(x => x.StaticContent == "So in conclusion, Together = Stronger");
		}

		[Test]
		public async Task GetStaticContentFromView_JapaneseCharacters_ExtractsNonLatinScript()
		{
			// Arrange
			var viewContent = @"<p>こんにちは</p>";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().Contain(x => x.StaticContent == "こんにちは");
		}

		[Test]
		public async Task GetStaticContentFromView_GetDictionaryValue()
		{
			// Arrange
			var viewContent = @"<p>@Umbraco.GetDictionaryValue(""key"")</p>";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().BeEmpty();
		}

		[Test]
		public async Task GetStaticContentFromView_Parhentesis()
		{
			// Arrange
			var viewContent = @"<p>This is text (should be extracted) for sure</p>";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().HaveCount(1);
			result.Should().Contain(x => x.StaticContent == "This is text (should be extracted) for sure");
		}

		[Test]
		public async Task GetStaticContentFromView_InlineRazor()
		{
			// Arrange
			var viewContent = @"
        <button class=""nav-link collapsed main-menu-link"" @(Model.CurrentPage.Level > navbarItem.Level ? ""tabindex=-1"" : null) data-toggle=""offcanvas-submenu"" data-target=""#subMenu@(navbarItem.Key)"" aria-expanded=""false"" aria-label=""@Umbraco.GetDictionaryValue(""Expand_Subpages_For_The_Page"") @navbarItem.GetStringValue(""navigationName"")"">
            @navbarItem.GetStringValue(""navigationName"") <i class=""fal fa-chevron-right""></i>
        </button>";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().BeEmpty();
		}

		[Test]
		public async Task GetStaticContentFromView_UmbracoFormsInline_DontGet_Checked_aria_describedby()
		{
			// Arrange
			var viewContent = @"
<input type=""checkbox"" name=""@Model.Name"" id=""@Model.Id"" value=""true""  data-umb=""@Model.Id""
       @if(Model.Mandatory) { <text>  data-val=""true"" data-val-requiredcb=""@Model.RequiredErrorMessage"" aria-required=""true""</text> }
       @if (Model.ContainsValue(true) || Model.ContainsValue(""true"") || Model.ContainsValue(""on"")) { <text>checked=""checked""</text> }
       @if (!string.IsNullOrEmpty(Model.ToolTip)) { <text> aria-describedby=""@(Model.Id)_description"" </text> }                                                                                                                                                                 
/>";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().BeEmpty();
		}

		[Test]
		public async Task GetStaticContentFromView_DontGet_RenderSection()
		{
			// Arrange
			var viewContent = @"
    <footer>

    </footer>


    @RenderSection(""scripts"", false)

    @Html.Raw()
</body>
</html>";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().BeEmpty();
		}

		[Test]
		public async Task GetStaticContentFromView_UmbracoForms_DontGet_Brackets()
		{
			// Arrange
			var viewContent = @"
<div id=""@Model.Id"" data-umb=""@Model.Id"" class=""@Html.GetFormFieldClass(Model.FieldTypeName)"">
    @if (hasCaption)
    {
        @Html.Raw(""<"" + captionTag + "">"")@settings[""Caption""]@Html.Raw(""</"" + captionTag + "">"")
    }
    @if (hasBody)
    {
        if (Configuration.Value.AllowUnsafeHtmlRendering)
        {
            <p>@Html.Raw(settings[""BodyText""].Replace(""\r\n"", ""\n"").Replace(""\r"", ""\n"").Replace(""\n"", ""<br />""))</p>
        }
        else
        {
            <p>@settings[""BodyText""]</p>
        }
    }
</div>";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().BeEmpty();
		}

		[Test]
		public async Task GetStaticContentFromView_UmbracoForms_DontGet_NestedInlineRazor()
		{
			// Arrange
			var viewContent = @"@model Umbraco.Forms.Web.Models.FieldViewModel
@using Umbraco.Forms.Web

@{
    var autocompleteAttribute = Model.GetSettingValue<string>(""AutocompleteAttribute"", string.Empty);
    var numberOfRows = Model.GetSettingValue<int>(""NumberOfRows"", global::Umbraco.Forms.Core.Providers.FieldTypes.Textarea.DefaultNumberOfRows);
    var maxLength = Model.GetSettingValue<int>(""MaximumLength"", 0);
}
<textarea class=""@Html.GetFormFieldClass(Model.FieldTypeName)""
          name=""@Model.Name""
          id=""@Model.Id""
          data-umb=""@Model.Id""
          rows=""@numberOfRows""
          cols=""20""
          @{if (string.IsNullOrEmpty(Model.PlaceholderText) == false) { <text> placeholder=""@Model.PlaceholderText"" </text> } }
          @{if (string.IsNullOrEmpty(autocompleteAttribute) == false) { <text> autocomplete=""@autocompleteAttribute"" </text> } }
          @{if (maxLength > 0) { <text> maxlength=""@maxLength"" </text> } }
          @{if (Model.Mandatory || Model.Validate) { <text> data-val=""true"" </text> } }
          @{if (Model.Mandatory) { <text> data-val-required=""@Model.RequiredErrorMessage"" aria-required=""true"" </text> }}
          @{if (Model.Validate) { <text> data-val-regex=""@Model.InvalidErrorMessage"" data-val-regex-pattern=""@Html.Raw(Model.Regex)"" </text> }}
          @{if (!string.IsNullOrEmpty(Model.ToolTip)) { <text> aria-describedby=""@(Model.Id)_description"" </text> } }>@Model.ValueAsHtmlString</textarea>

";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().BeEmpty();
		}

		[Test]
		public async Task GetStaticContentFromView_UmbracoForms_DontGet_MultiPageFormPagingDetails()
		{
			// Arrange
			var viewContent = @"@using System.Text
@model Umbraco.Forms.Web.Models.FormViewModel

@{
    var html = new StringBuilder();

    string formatStringWithHtml = Model.PagingDetailsFormat
        .Replace(""{0}"", ""<span class=\""umbraco-forms-paging-count-number\"">{0}</span>"")
        .Replace(""{1}"", ""<span class=\""umbraco-forms-paging-count-number\"">{1}</span>"");
    html.Append(""<div class=\""umbraco-forms-paging-count\"">"");
    html.AppendFormat(formatStringWithHtml, Model.PageNumber, Model.PageCount);
    html.Append(""</div>"");

    html.Append(""<ol class=\""umbraco-forms-paging-captions\"">"");
    for (int i = 0; i < Model.Pages.Count; i++)
    {
        html.AppendFormat(
            ""<li class=\""umbraco-forms-paging-captions-caption"" + (Model.FormStep == i ? "" umbraco-forms-paging-captions-caption-current"" : string.Empty) + ""\"">{0}</li>"",
            Model.GetPageCaption(i));
    }
    if (Model.HasSummaryPage)
    {
        html.AppendFormat(
            ""<li class=\""umbraco-forms-paging-captions-caption\"">{0}</li>"",
            @Model.SummaryCaption);
    }
    html.Append(""</ol>"");

    <div class=""umbraco-forms-paging"">
        @Html.Raw(html.ToString())
    </div>
}
";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().BeEmpty();
		}

		[Test]
		public async Task GetStaticContentFromView_DontGet_Numbers()
		{
			// Arrange
			var viewContent = @"<h1 class=""w-100 text-center"">33%</h1>";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().BeEmpty();
		}

		[Test]
		public async Task GetStaticContentFromView_DontGet_CodeBlock()
		{
			// Arrange
			var viewContent = @"    // Handle address
    if (footerAddress.NullCheck())
    {
        var addressParts = footerAddress.FirstOrDefault()?.Split(' ') ?? Array.Empty<string>();
        var addressProperties = new List<string>();

        addressProperties.Add(""\""@type\"": \""PostalAddress\"""");

        if (addressParts.Length > 1)
        {
            addressProperties.Add($""\""streetAddress\"": \""{addressParts[0]} {addressParts[1].Trim("","")}\"""");
        }

        if (addressParts.Length > 2)
        {
            addressProperties.Add($""\""addressLocality\"": \""{addressParts[2]}\"""");
        }

        if (addressParts.Length > 4)
        {
            addressProperties.Add($""\""postalCode\"": \""{addressParts[3]} {addressParts[4]}\"""");
        }

        var addressJson = string.Join("",\n    "", addressProperties);
        schemaProperties.Add($""\""address\"": {{\n    {addressJson}\n  }}"");
    }";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			result.Should().BeEmpty();
		}
	}
}

