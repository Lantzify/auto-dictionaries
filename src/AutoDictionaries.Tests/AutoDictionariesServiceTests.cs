using AutoDictionaries.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using AutoDictionaries.Services;
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
        public async Task GetStaticContentFromView_WithEmptyContent_ReturnsEmptyList()
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
        public void GetStaticContentFromView_WithNullContent_ThrowsException()
        {
            // Arrange
            string viewContent = null;

            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _service.GetStaticContentFromView(viewContent));
        }

        [Test]
        public async Task GetStaticContentFromView_WithSimpleStaticContent_ReturnsCorrectStaticContent()
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
        public async Task GetStaticContentFromView_WithMultipleStaticContent_ReturnsAllContent()
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
        public async Task GetStaticContentFromView_WithDuplicateContent_CountsCorrectly()
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
        public async Task GetStaticContentFromView_WithSpecialCharacters_HandlesCorrectly()
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
        public async Task GetStaticContentFromView_WithExistingDictionary_FindsMatchingDictionary()
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
        public async Task GetStaticContentFromView_WithEmailAddress_ExtractsPartially()
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
        public async Task GetStaticContentFromView_WithRazorCode_FiltersCorrectly()
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
        public async Task GetStaticContentFromView_WithWhitespaceOnly_FiltersOut()
        {
            // Arrange
            var viewContent = @"<div>   </div><p>
            </p><span>	</span>";

            // Act
            var result = await _service.GetStaticContentFromView(viewContent);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty(); // Whitespace-only content should be filtered out
        }

        [Test]
        public async Task GetStaticContentFromView_WithSingleCharacter_FiltersOut()
        {
            // Arrange
            var viewContent = @"<div>A</div><p>B</p><span>C</span>";

            // Act
            var result = await _service.GetStaticContentFromView(viewContent);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty(); // Single characters should be filtered out by the regex
        }

        [Test]
        public async Task GetStaticContentFromView_WithComplexHtmlStructure_ExtractsCorrectContent()
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
        public async Task GetStaticContentFromView_WithMixedContentAndRazor_ExtractsOnlyStaticContent()
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
        public async Task GetStaticContentFromView_WithValidContent_ReturnsExpectedResults()
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
        public async Task GetStaticContentFromView_WithIfStatement_FiltersOut()
        {
            // Arrange
            var viewContent = @"<div>@if (condition) { Some text }</div>";

            // Act
            var result = await _service.GetStaticContentFromView(viewContent);

            // Assert
            result.Should().NotBeNull();
            // If statements should be filtered out by the regex
            result.Should().NotContain(x => x.StaticContent.Contains("if ("));
        }

		[Test]
		public async Task GetStaticContentFromView_WithForLoop_FiltersOut()
		{
			// Arrange
			var viewContent = @"<div>@for (var item in items) { Some text }</div>";

			// Act
			var result = await _service.GetStaticContentFromView(viewContent);

			// Assert
			result.Should().NotBeNull();
			// If statements should be filtered out by the regex
			result.Should().NotContain(x => x.StaticContent.Contains("for ("));
		}

		[Test]
        public async Task GetStaticContentFromView_WithNestedElements_ExtractsFromAllLevels()
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
        public async Task GetStaticContentFromView_WithMultipleDuplicates_CountsAllOccurrences()
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
        public async Task GetStaticContentFromView_WithPartialMatches_DoesNotMatch()
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
        public async Task GetStaticContentFromView_WithExactMatch_FindsDictionary()
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
        public async Task GetStaticContentFromView_WithMultipleDictionariesAndPartialMatches_FindsCorrectMatches()
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
		public async Task GetStaticContentFromView_DontFindInIfStatement()
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
		public async Task GetStaticContentFromView_ShouldFindTextThatsBetweenHtmlTags()
		{
			// Arrange
			var viewContent = @"@inherits UmbracoViewPage
@using ContentModels = Umbraco.Cms.Web.Common.PublishedModels

@{
    var homePage = Model.AncestorOrSelf<ContentModels.Home>();
}

<!-- Footer-->
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
	}
}