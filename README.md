# AccessKit.NET

[![NuGet](https://img.shields.io/nuget/v/AccessKit.NET?label=nuget&logo=nuget&color=004e89)](https://www.nuget.org/packages/AccessKit.NET/)
[![License](https://img.shields.io/badge/License-Apache%202.0%20OR%20MIT-brightgreen.svg)](LICENSE.md)
[![.NET Standard 2.0](https://img.shields.io/badge/.NET-Standard%202.0-blue.svg)](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)
[![GitHub](https://img.shields.io/badge/GitHub-AccessKit.NET-181717?logo=github)](https://github.com)

Cross-platform accessibility bindings for .NET, exposing a unified semantic tree to **Windows UI Automation**, **macOS NSAccessibility**, and **Linux AT-SPI** for custom-drawn UIs — perfect for game engines, OpenGL/Skia applications, and any custom UI framework.

Built on the robust [AccessKit](https://accesskit.dev/) C API, bringing enterprise-grade accessibility to .NET applications without requiring native controls.

## ✨ Features

- **Complete API coverage** — All 444+ functions from `accesskit-c 0.22.2` fully bound
- **Cross-platform support** — Windows (bundled), macOS (native library), Linux ready
- **Rich node semantics** — 18+ ARIA roles, properties, relations, and actions
- **Geometry types** — `Rect`, `Affine`, `Point`, `Size` with kurbo math helpers
- **Type-safe bindings** — Nullable properties, enums, and action payloads
- **Smart adapters** — Automatic HWND subclassing (Windows) or manual NSAccessibility integration (macOS)
- **Action handling** — Decode and respond to screen reader requests in your UI
- **Dual-licensed** — Apache 2.0 or MIT

## 🚀 Quick Start

### Installation

```bash
dotnet add package AccessKit.NET
```

### Windows Example

```csharp
using AccessKit;
using AccessKit.Windows;

const ulong Root = 1, Button = 2;

TreeUpdate BuildTree()
{
    var window = new Node(Role.Window);
    window.SetLabel("My App");
    window.AddChild(Button);

    var button = new Node(Role.Button);
    button.SetLabel("Click me");
    button.AddAction(AkAction.Click);
    button.Bounds = new Rect(10, 10, 110, 40);

    var update = TreeUpdate.WithCapacityAndFocus(2, Button);
    update.PushNode(Root, window);
    update.PushNode(Button, button);
    update.SetTree(new Tree(Root));
    update.SetFocus(Button);
    return update;
}

// Create adapter (window must be hidden at construction)
var adapter = new WindowsSubclassingAdapter(
    hwnd,
    buildInitialTree: BuildTree,
    onAction: req => Console.WriteLine($"Action {req.Action} on node {req.TargetNode}"));

// Update when UI changes
adapter.UpdateIfActive(BuildTree);
```

## 📋 What's Included

### Enums & Types
- **18+ ARIA Roles** — `Button`, `Window`, `ScrollBar`, `Slider`, etc.
- **20+ Actions** — `Click`, `Focus`, `Scroll`, `SetTextSelection`, custom actions
- **Geometry** — `Point`, `Size`, `Rect`, `Affine` transforms with math helpers
- **Properties** — Visibility, disabled state, text alignment, colors, decorations
- **Relations** — Parent/child, controls, labelled-by, member-of, etc.

### Platform Adapters
| Platform | Adapter | Status | Notes |
|----------|---------|--------|-------|
| **Windows** | `WindowsSubclassingAdapter`<br>`WindowsAdapter` | ✅ Production Ready | HWND subclassing or manual integration |
| **macOS** | `MacosSubclassingAdapter`<br>`MacosAdapter` | ✅ Production Ready | NSAccessibility; requires native dylib |
| **Linux** | — | ⚠️ Future | AT-SPI planned |

### Generated vs. Hand-Written
- **Generated** (from `accesskit.h`): P/Invoke signatures, enums, basic node properties
- **Hand-written**: Marshaling helpers, adapter implementations, tree updates

## 🔧 Platform-Specific Setup

### Windows
The `accesskit.dll` (win-x64) is bundled and ready to use. No additional setup required.

```csharp
using AccessKit.Windows;

var adapter = new WindowsSubclassingAdapter(hwnd, buildInitialTree, onAction);
```

**Important:** Window must be hidden when constructing the adapter.

### macOS
Only the win-x64 binary is bundled. To enable macOS support:

1. Download `libaccesskit.dylib` from [accesskit-c releases](https://github.com/AccessKit/accesskit-c/releases)
2. Place it in:
   - `runtimes/osx-arm64/native/libaccesskit.dylib` (Apple Silicon)
   - `runtimes/osx-x64/native/libaccesskit.dylib` (Intel)
3. The `.csproj` automatically picks them up (guarded by `Exists()`)

```csharp
using AccessKit.Macos;

// Subclassing adapter
var adapter = MacosSubclassingAdapter.ForView(nsView);

// Or manual NSAccessibility integration
var children = adapter.ViewChildren(nsView);
```

**Important:** All adapter calls must run on the main thread.

### Linux
AT-SPI support is planned for a future release.

## 📚 Usage Guide

### Building the Accessibility Tree

Each node is assigned a stable, unique ID (e.g., `ulong`). Build a tree by creating nodes, setting properties, and establishing parent-child relations.

```csharp
// Node IDs
const ulong Window = 1, Title = 2, Button = 3;

var window = new Node(Role.Window);
window.SetLabel("My Application");
window.SetName("MainWindow");
window.AddChild(Title);
window.AddChild(Button);

var button = new Node(Role.Button);
button.SetLabel("Save");
button.AddAction(AkAction.Click);
button.Bounds = new Rect(20, 60, 100, 30);
button.SetDescription("Saves the current document");
```

### Handling Actions

Respond to screen reader user interactions:

```csharp
adapter = new WindowsSubclassingAdapter(
    hwnd,
    buildInitialTree: BuildTree,
    onAction: request =>
    {
        if (request.Action == AkAction.Click && request.TargetNode == Button)
        {
            // Handle click
            SaveDocument();
        }
        else if (request.Action == AkAction.SetTextSelection)
        {
            // Handle text selection
        }
    });
```

### Updating the Tree at Runtime

When your UI changes, push an updated tree to the accessibility stack:

```csharp
// After UI change (e.g., button disabled)
var disabledButton = new Node(Role.Button);
disabledButton.SetLabel("Save");
disabledButton.SetDisabled(true);

var update = new TreeUpdate();
update.PushNode(Button, disabledButton);

adapter.UpdateIfActive(update);
```

## 🛠️ Development

### Regenerating Bindings

Bindings are auto-generated from `third-party/accesskit.h` using `tools/generate_bindings.py`:

```bash
# Bump to a new accesskit-c release
# 1. Download release from https://github.com/AccessKit/accesskit-c/releases
# 2. Replace third-party/accesskit.h and runtimes/*/native/ libraries
# 3. Regenerate bindings
python tools/generate_bindings.py

# Regenerated files:
# - Interop.Generated.cs       (P/Invoke signatures)
# - Interop.Macos.Generated.cs (macOS-specific P/Invoke)
# - Enums.Generated.cs         (All enums)
# - Node.Generated.cs          (Node property wrappers)
```

### Project Structure
```
src/AccessKit.NET/
├── Interop.*.cs           # P/Invoke signatures (auto-generated)
├── Enums.Generated.cs     # ARIA roles, actions, etc. (auto-generated)
├── Node.cs, Node.Generated.cs
├── Tree.cs, TreeUpdate.cs
├── Callbacks.cs, Structs.cs, Geometry.cs
├── Windows/
│   ├── WindowsAdapter.cs
│   └── WindowsSubclassingAdapter.cs
├── Macos/
│   ├── MacosAdapter.cs
│   └── MacosSubclassingAdapter.cs
└── AccessKit.NET.csproj

tools/
└── generate_bindings.py  # Binding generator

third-party/
├── accesskit.h
└── LICENSE-APACHE
```

## 📖 API Reference

### Core Types
- **`Node`** — Represents an accessibility node with properties, actions, and relations
- **`Tree`** — Root of the accessibility tree (name, version, debug info)
- **`TreeUpdate`** — Batch updates to push to the OS
- **`ActionRequest`** — Decoded action from a screen reader

### Adapters
- **`WindowsSubclassingAdapter`** — Auto-subclasses HWND, responds to `WM_GETOBJECT`
- **`WindowsAdapter`** — Manual integration for custom window procedures
- **`MacosSubclassingAdapter`** — Subclasses NSView/NSWindow for NSAccessibility
- **`MacosAdapter`** — Manual NSAccessibility integration

### Geometry & Values
- `Point`, `Size`, `Rect`, `Vec2`, `Affine` — Spatial types with math helpers
- `Color` — RGBA color representation
- `TextDecoration`, `TextPosition`, `TextSelection` — Text properties

For full documentation, see the [AccessKit](https://accesskit.dev/) project and inline code comments.

## 📋 Requirements

- **.NET Standard 2.0+** (compatible with .NET Framework 4.6.1+, .NET Core 2.0+, .NET 5+)
- **Windows**: Built-in support (win-x64)
- **macOS**: Requires `libaccesskit.dylib` (osx-arm64 or osx-x64)
- **Linux**: Coming soon

## 🤝 Contributing

Contributions are welcome! Areas of interest:
- Linux AT-SPI adapter
- Additional platform support
- Expanded examples and documentation
- Bug fixes and performance improvements

## 📝 License

Licensed under either of:
- Apache License, Version 2.0 ([LICENSE-APACHE](third-party/LICENSE-APACHE) or http://www.apache.org/licenses/LICENSE-2.0)
- MIT license ([LICENSE-MIT](LICENSE-MIT) or http://opensource.org/licenses/MIT)

at your option. See `third-party/LICENSE-APACHE` for details on the bundled AccessKit C API.

## 🙏 Acknowledgments

- [AccessKit](https://accesskit.dev/) — The Rust accessibility toolkit this is based on
- [accesskit-c](https://github.com/AccessKit/accesskit-c) — The C API bindings
- Inspired by platforms' native accessibility frameworks

## 📞 Support

- **Issues & Feedback**: [GitHub Issues](https://github.com)
- **Upstream**: [AccessKit.dev](https://accesskit.dev/) • [accesskit-c GitHub](https://github.com/AccessKit/accesskit-c)

---

Built with ❤️ for accessible .NET applications.
