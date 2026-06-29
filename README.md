# AccessKit.NET

.NET bindings for [AccessKit](https://accesskit.dev/) (via the `accesskit-c` C API), exposing a single
cross-platform semantic tree to OS accessibility stacks — Windows UI Automation, macOS NSAccessibility,
and Linux AT-SPI — for **custom-drawn UIs** that have no native controls (game engines, Skia/GL apps, etc.).

- Bundled native version: **accesskit-c 0.22.2** (win-x64; `accesskit.dll` under `runtimes/win-x64/native/`).
- Target framework: `netstandard2.0`.

## Status

Early. Currently implemented:

- Full `Role` and `AkAction` enums (generated from `accesskit.h`).
- `Node`, `Tree`, `TreeUpdate` builders with ownership-transfer semantics.
- `ActionRequest` decoding (`Action`, `TargetNode`).
- `Windows.WindowsSubclassingAdapter` — subclasses the HWND to answer `WM_GETOBJECT`; lazy
  activation; `UpdateIfActive`.

Not yet implemented: macOS/Unix adapters, the text/`TextRun` model, most node property setters.

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
    button.SetBounds(new Rect(10, 10, 110, 40));

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
download the new release zip and replace the header and `runtimes/<rid>/native/` libraries; the
`Role`/`AkAction` enums are generated from the header.

Licensed Apache-2.0 OR MIT (see `third-party/LICENSE-APACHE`).
