# Contribution Guidelines for PhoneGame

## Development Environment
- This repository contains a .NET 8 Blazor WebAssembly project located in the `PhoneGame` directory.
- Ensure you have the .NET 8 SDK installed to build and run the project.

## Continuous Integration
- `Program.CI.cs` is regenerated during the GitHub Pages build. The workflow
  overwrites this file with the current commit hash before publishing, so local
  contents are replaced on each CI run.

## Hosting Considerations
- This app is hosted on GitHub Pages. To allow opening from any sub-path, the routing logic uses `wwwroot/404.html` together with `Home.razor`.
- When navigating between pages, specify relative paths from the app root (e.g., `setting` instead of `/setting`).
