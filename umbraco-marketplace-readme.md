## What is Auto dictionaries
Auto dictionaries is a Umbraco package made for v8+. Made to help automate the process of replacing static content in templates and partial views with dictionary items. It can be found on the under the "Translation" section and is an admin only tool. Not visible for user group "editors".
## Hows does it work
Auto Dictionaries uses a regular expression to find "static content" between HTML tags. When "static content" is found, it checks if the content matches any value in an existing dictionary item. If a match is found, you can choose to associate it with the existing dictionary. It will then insert the dictionary item. If no match is found, it will create a new dictionary item and insert it into the template.
