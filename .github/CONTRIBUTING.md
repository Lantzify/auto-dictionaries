# Contributing to Auto Dictionaries

Contributions are welcome! Here's how you can help:
1. Fork the repository
2. Create a feature branch (`git checkout -b amazing-feature`)
3. Make your changes
4. Run tests (if applicable)
5. Commit your changes (`git commit -m 'Add amazing feature'`)
6. Push to the branch (`git push origin amazing-feature`)
7. Open a Pull Request

## Development Setup

1. Clone the repository
2. Install dependencies (both .NET and npm)
3. Use NPM Task Runner in Visual Studio for client-side development

### Frontend (TypeScript)

The client-side code is located in `AutoDictionaries/Client/` and uses npm for package management.

1. **Install NPM Task Runner** extension in Visual Studio
2. Run `install-client` task (performs `npm install`)
3. Run `build-watch` task to build and watch for changes
