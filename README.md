# ImageViewerMaui

Image viewer control for .NET MAUI applications. This control provides a zooming and panning experience, similar to native gallery apps like Google Photos.

## Demo

| Bounce Enabled | Bounce Disabled |
| :---: | :---: |
| <img src="ImageViewerMaui%20IsBounceEnabled%20True.gif" height="600" /> | <img src="ImageViewerMaui%20IsBounceEnabled%20False.gif" height="600" /> |

## Features

- **Localized Double Tap Zoom**: Double-tapping zooms into the specific area you tapped, rather than just the center of the image.
- **Smart Zoom-to-Fill**: Automatically calculates the optimal zoom level to eliminate black bars (letterboxing) on widescreen or tall images. 
- **Automatic Size Detection**: The control automatically detects the intrinsic dimensions of the loaded image, ensuring correct aspect ratio calculations without requiring manual configuration.
- **Smart Bounds**: Panning and zooming is clamped to the visual edges of the image, preventing users from dragging the image into empty space when it fits within the viewport.
- **Bindable Properties**:
    - `Source`: The image source to display.
    - `Aspect`: Controls how the image fits within the view (e.g., `AspectFit`, `AspectFill`).
    - `IsBounceEnabled`: Toggles the bounce animation effect on zoom/pan.
- **Performance Optimized**: 
    - Efficient memory management with `IDisposable` support.
    - Cached animations to reduce GC allocations.

## Usage

Simply add the namespace and use the `ImageViewer` control in your XAML.

### Basic Usage

```xml
<xmlns:controls="clr-namespace:ImageViewerMaui;assembly=ImageViewerMaui">

<controls:ImageViewer 
    Source="https://example.com/image.jpg"
    Aspect="AspectFit"
    IsBounceEnabled="True" />
```

### Automatic "Zoom-to-Fill"
No extra configuration is needed! The control will automatically measure the image. If you load a widescreen image and double-tap, it will zoom in enough to cover the vertical letterboxing while keeping the tapped point in focus.

## Installation

[![NuGet](https://img.shields.io/nuget/v/ImageViewerMaui.svg)](https://www.nuget.org/packages/ImageViewerMaui/)

You can install the package via the NuGet Package Manager or the CLI:

```bash
dotnet add package ImageViewerMaui
```