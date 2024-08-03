
Auto dictionaries is a package made to help automate the process of replacing static content in templates with dictionary items. By enabling Auto dictionaries translation setting. Auto dictionaries can now also translates newly created dictionary items.

## What is Auto dictionaries
Auto dictionaries is a Umbraco package made for v8+. Made to help automate the process of replacing static content in templates and partial views with dictionary items. It can be found on the under the "Translation" section and is an admin only tool. Not visible for user group "editors".

## Hows does it work
Auto Dictionaries uses a regular expression to find "static content" between HTML tags. When "static content" is found, it checks if the content matches any value in an existing dictionary item. If a match is found, you can choose to associate it with the existing dictionary. It will then insert the dictionary item. If no match is found, it will create a new dictionary item and insert it into the template. If the translation setting is enabled and correctly setup. The "static content" will be translated into all available langues that has been setup in Umbraco.

### Settings
Default settings are:
```
"AutoDictionaries": {
     Translate: false,
     Translator: "DeepL",
     ApiKey: "",
     ApiEndpoint: "",
     ApiRegion: "",
}
```
- ``Translate`` Determines if auto dictionaries should present the option to translate newly created dictionaries.
- ``Translator`` Determines translator that will be used default is ``DeepL``. Available translator ``DeepL``, ``MicrosoftTranslation``.
- ``ApiKey`` If the translator needs a Api key.
- ``ApiEndpoint`` (Microsoft Translation) Endpoint used for the translation.
- ``ApiRegion``  (Microsoft Translation) Endpoint used for the translation.

### Requirements for translations
Right now the services below are built in with Auto dictionaries. Thease are however not required in order to use the basic fucntions.

#### DeepL
 - Requires a DeepL Account and API Key 

 [Create Free API Account](https://www.deepl.com/en/pro/change-plan?cta=apiDocsHeader#developer)

#### Microsoft Translation
- Requires a microsoft azure account 
- Translation subscription key

Azure documentation [Create a Translator resource](https://learn.microsoft.com/en-us/azure/ai-services/Translator/create-translator-resource)