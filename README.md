# Narfox Game Tools

This is a collection of services and extensions that I have found helpful
across multiple, released [Flat Red Ball](https://flatredball.com) games.

## Docs

The classes in this repo are not documented outside of XML comments. This is a great place to contribute
if you want to give back to the project! See the [Docs Index](docs/index.md).

## NarfoxGameTools project

This project depends on building FlatRedBall from source to work out of the box
because there are some FRB-specific services. However, most of the services
and classes are pure C#. Feel free to take just what you need for your MonoGame
or Unity project.

### Services

Most of the services are singleton classes and most of them need to be initialized
before use. I recommend calling `ServiceClass.Instance.Initialize()` in your root
`Game` class before starting a screen. Usually, services that have not been
initialized will throw an exception.

- `FileService`: contains easy wrappers for loading and serializing/deserializing text.
- `GoogleAnalyticsService`: a cross-platform REST implementation that can be used to have game telemetry in a free Google Analytics account
- `LogService`: A really basic log system. You should probably use something else that's better!
- `RandomService`: The FRB implementation of Random does not allow you to specify a seed. This is just a central container to hold a seed-critical random instance.
- `ScreenshotService`: Allows you to easily save screenshots to disk. This is not slow and primarily intended for debugging.
- `SimpleCryptoService`: A simple and fast way to encrypt game data so it's not obvious to edit. NOT SAFE for sensitive data.
- `SoundService`: A basic 2D sound implementation with distance-based attenuation, pitch variance, and more
- `UIService`: A central manager that allows you to register menus and manage your game's UI state.

### Extensions

Extensions methods for several FRB types are available to make common tasks more convenient.

- `CameraExtensions`: Currently only offers a helper for getting a random position within the view of the camera.
- `CollectionExtensions`: Adds a Linq-like `Random()` method to many collection types and has some helpers for converting between collection types.
- `ColorExtensions`: Provides an HSB/HSL color mode and methods to convert between HSB and RGB
- `CursorExtensions`: Helpers for using cursors with gamepads
- `EntityExtensions`: Methods primarily useful for AI behavior. Helpers for finding distance and rotation between positioned entities.
- `MathExtensions`: Helpers for common 2D game math.
- `RandomExtensions`: Helpers for common random tasks like getting a random color or a random `float` within a range.
- `SpriteExtensions`: Makes it easy to colorize a sprite in a single line of code.

## Google Sheets Tools

Google Sheets is a powerful game design application! You can design all of your game data with the
indexes, checks, calculations, and comparisons you need to manage and balance your content.

But you need a simple system to get game design data into your game. That's what these tools are for!

1. Set up an App Script for your Google Sheets project using **Extensions** > **App Script** in Google Sheets
1. Add the JS and HTML file in this repo's folder to your App Script
1. This will add a new menu option: **Game Design** > **Convert to JSON**
1. Create your game data with headers and rows
1. Select the data, including headers, and choose the **Convert to JSON** menu option
1. A side panel will pop up with the selected data converted to JSON
1. Create a POCO model for your game data and deserialize the JSON into your game with the `FileService` provided in NarfoxGameTools
1. OPTIONAL: encrypt the data, if desiged using the `SimpleCryptoService` provided in NarfoxGameTools