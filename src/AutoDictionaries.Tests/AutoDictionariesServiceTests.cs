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
                <div>
                    <h1>Main Header</h1>
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
    }
}