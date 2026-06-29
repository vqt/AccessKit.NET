# AccessKit.NET

.NET bindings for [AccessKit](https://accesskit.dev/) (via the `accesskit-c` C API), exposing a single
cross-platform semantic tree to OS accessibility stacks — Windows UI Automation, macOS NSAccessibility,
and Linux AT-SPI — for **custom-drawn UIs** that have no native controls (game engines, Skia/GL apps, etc.).

- Bundled native version: **accesskit-c 0.22.2** (win-x64; `accesskit.dll` under `runtimes/win-x64/native/`).
- Target framework: `netstandard2.0`.

## Status

The complete cross-platform + Windows C API is bound — all **444** functions exported by the
bundled `accesskit.dll`:

- All enums (`Role`, `AkAction`, `Invalid`, `Toggled`, `Live`, `Orientation`, `SortDirection`,
  `AriaCurrent`, `AutoComplete`, `HasPopup`, `ListStyle`, `TextAlign`, `TextDirection`,
  `VerticalOffset`, `ScrollUnit`, `ScrollHint`, `TextDecorationStyle`).
- `Node` with its **full** property surface — presence flags, strings, numeric/index/float values,
  colors, text decorations, enum properties, node-id relations (children, controls, labelled-by,
  …), bounds, transform, text selection, custom actions, and the action set. Optional native
  properties surface as C# `Nullable<T>` getters.
- `Tree` (toolkit name/version/debug) and `TreeUpdate` (push nodes, set/clear tree, focus, tree id).
- `CustomAction`.
- Geometry value types `Point`, `Size`, `Vec2`, `Rect`, `Affine`, plus `Color`, `TextDecoration`,
  `TextPosition`, `TextSelection`, `TreeId`, with the kurbo-derived math helpers (`Rect.Union`,
  `Affine.TransformPoint`, …).
- `ActionRequest` decoding including the action-data payload (`Value`, `NumericValue`,
  `CustomActionId`, `ScrollToPoint`, `SetTextSelection`, …).
- `Windows.WindowsSubclassingAdapter` (subclasses the HWND to answer `WM_GETOBJECT`; lazy
  activation; `UpdateIfActive`) and `Windows.WindowsAdapter` (for apps running their own window
  procedure: `HandleWmGetObject`, `UpdateWindowFocusState`).
- `Macos.MacosSubclassingAdapter` (subclasses an `NSView`/`NSWindow` content view to answer
  `NSAccessibility`; `ForView`/`ForWindow`, `AddFocusForwarderToWindowClass`) and `Macos.MacosAdapter`
  (for apps implementing `NSAccessibility` themselves: `ViewChildren`/`Focus`/`HitTest` bridges,
  `UpdateViewFocusState`). **Requires the macOS native library** — see below.

The P/Invoke layer, enums, and the regular `Node` properties are generated from `accesskit.h` by
[`tools/generate_bindings.py`](tools/generate_bindings.py); marshaling-heavy members are hand-written.

Not included: the iOS/Android/Unix platform adapters — out of scope for now.

### macOS native library

Only the win-x64 `accesskit.dll` is bundled in this repo. To use the macOS adapters at runtime, drop
the matching `libaccesskit.dylib` from the [accesskit-c releases](https://github.com/AccessKit/accesskit-c/releases)
into `runtimes/osx-arm64/native/` and/or `runtimes/osx-x64/native/`; the `.csproj` picks them up
automatically (the entries are guarded by `Exists`, so the binding compiles without them). All macOS
adapter calls must be made on the main thread.

## Usage (Windows)

```csharp
using AccessKit;
using AccessKit.Windows;

// Window MUST be hidden when the adapter is constructed (native panics otherwise).
const ulong Root = 1, Button = 2;

TreeUpdate BuildTree()
{
    var window = new Node(Role.Window);
    window.SetLabel("My App");
    window.AddChild(Button);

    var button = new Node(Role.Button);
    button.SetLabel("Test button");
    button.AddAction(AkAction.Click);
    button.AddAction(AkAction.Focus);
    button.Bounds = new Rect(10, 10, 110, 40);

    var update = TreeUpdate.WithCapacityAndFocus(2, Button);
    update.PushNode(Root, window);
    update.PushNode(Button, button);
    update.SetTree(new Tree(Root));
    update.SetFocus(Button);
    return update;
}

var adapter = new WindowsSubclassingAdapter(
    hwnd,
    buildInitialTree: BuildTree,
    onAction: req => Console.WriteLine($"AT requested {req.Action} on node {req.TargetNode}"));

// ...later, when UI state changes:
adapter.UpdateIfActive(BuildTree);
```

## Native sources & regenerating

The header (`third-party/accesskit.h`) and `accesskit.dll` come from the official
[accesskit-c releases](https://github.com/AccessKit/accesskit-c/releases). To bump the version,
download the new release zip and replace the header and `runtimes/<rid>/native/` libraries, then
re-run `python tools/generate_bindings.py` to regenerate `Interop.Generated.cs`,
`Interop.Macos.Generated.cs`, `Enums.Generated.cs`, and `Node.Generated.cs`.

Licensed Apache-2.0 OR MIT (see `third-party/LICENSE-APACHE`).
