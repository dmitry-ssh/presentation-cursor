# PresentationCursor

A WPF application that creates visual trailing effects for cursor movement, perfect for presentations, screen recordings, or demonstrations where enhanced cursor visibility is needed.

## Features

- **Visual Cursor Trail**: Creates a colorful trailing effect that follows your mouse cursor
- **Dynamic Color Transitions**: Trail segments transition from yellow to orange to red as they fade
- **Automatic Fading**: Trail segments gradually fade out and thin over time
- **Performance Optimized**: Configurable segment limits to maintain smooth performance
- **Smart Movement Detection**: Only creates new segments when meaningful cursor movement is detected

## Requirements

- .NET 8.0 or later
- Windows OS (WPF application)
- Visual Studio 2022 (for development)

## Installation

1. Clone the repository:
```bash
git clone https://github.com/dmitry-ssh/presentation-cursor.git
```

2. Open the solution in Visual Studio 2022

3. Build and run the project (F5)

## Configuration

The `TrailingPolyline` class provides several configurable properties:

| Property | Default | Description |
|----------|---------|-------------|
| `MaxTrailSegments` | 50 | Maximum number of trail segments to display |
| `FadeRate` | 0.025 | How quickly segments fade out (higher = faster) |
| `MinMovementDistance` | 3.0 | Minimum pixels of movement required to create a new segment |

## Usage

```csharp
// Initialize with a Canvas
var canvas = new Canvas();
var trail = new TrailingPolyline(canvas);

// Configure as needed
trail.MaxTrailSegments = 75;
trail.FadeRate = 0.02;
trail.MinMovementDistance = 2.0;

// Update trail position on each tick
trail.OnTick(currentMousePosition);

// Handle cursor leaving the tracking area
trail.OnCursorLeft();

// Clear all trail segments
trail.Clear();
```

## How It Works

1. **Trail Generation**: As the cursor moves, new line segments are created between the previous and current positions
2. **Visual Effects**: Each segment starts with full opacity and a yellow color, gradually fading and changing color over time
3. **Performance Management**: Old segments are automatically removed to maintain performance
4. **Smart Tracking**: The system detects when the cursor leaves the tracking area to prevent unwanted line connections

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

[Specify your license here - e.g., MIT, Apache 2.0, etc.]

## Acknowledgments

Built with WPF and .NET 8 for enhanced presentation and demonstration experiences.
