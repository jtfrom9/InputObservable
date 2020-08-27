Input Observable: Input Handling Utility with UniRx
===

Providing screen touch / mouse event Rx stream (`IObservable<T>`) and its Extensions.

- Common input source abstraction related with a finger or button id (`IInputObservable`) and its manager object (`InputObservableContext`)
- Gyroscope, Mouse wheel operation also supported (`GyroInputObservable`, `IMouseWheelObservable`)
- Verious input operation handling supported
    - long press (`LongSequence()`)
    - double tap/clik (`DoubleSequence()`)
    - swipe (`Verocity()`, `TakeLastVerocities()`)
    - pinch in/out (`PinchSequence()`)
    - rotation conversion (`ToEulerAngle()`)

![android](https://user-images.githubusercontent.com/1320102/91451285-3d66df80-e8b8-11ea-9de7-a549bdbd26d3.gif)

### How To Use

```csharp
    // Get IInputObservableContext
    IInputObservableContext context = this.DefaultInputContext();

    // Get IInputObservable with id=0, left button for Editor, fingerId=0 for Android/iOS
    IInputObservable io    = context.GetObservable(0);

    // Get IInputObservable with id=1, right button for Editor, fingerId=1 for Android/iOS
    IInputObservable altio = context.GetObservable(1);

    // Draw Point at the location of first touch (or left button clicked)
    io.OnBegin.Subscribe(e =>
    {
        // e is InputEvent
        DrawPoint(e.position);
    });

    // Slide (or dragging) Detection
    io.OnMoved.Subscribe(e => ...);

    // Touch Up (or left button release) detection
    io.OnEnd.Subscribe(e => ...);

    // Any input event
    io.Any().Subscribe(e => ...);

    // Double Tap/click detection during 200 msec
    io.DoubleSequence(200).Subscribe(e => {})

    // Long Touch/press detection over 1000 msec
    io.LongSequence(1000).Subscribe(e => {})

    // Slide/Move speed detection between two consecutive points
    io.Verocity().Subscribe(v =>
    {
        // v is VerocityInfo
        DrawPoint(v.@event.position);
        DrawArrow(v.vector)
    });

    // Slide/Move speed detection between two consecutive points in the last 8 points
    io.TakeLastVerocities(8).Subscribe(verocities =>
    {
        foreach (var v in verocities) {
            DrawPoint(v.@event.position);
            DrawArrow(v.vector)
        }
    });

    // Rotation event conversion from slide/move operation, 
    // at a rate of 90 degrees of slide at both ends of the screen (both up and down and left and right).
    io.Difference().ToEulerAngle(-90, -90).Subscribe(rot => ...);

    // Mouse wheel operation, type check is usefull for shared code between Editor and Android/iOS
    (context as IMouseWheelObservable)?.Wheel.Subscribe(w => {
        // w is MouseWheelEvent
    });

    // Pinch In/Out
    RectangleObservable.From(io, altio)
        .PinchSequence()
        .RepeatUntilDestroy(this)
        .Subscribe(diff =>
    {
    });

    // Gyroscope to pose rotation
    var gyro = new GyroInputObservable(this);
    gyro.EulerAngles.Subscribe(e =>
    {
        // e is Vector3
        var rot = Quaternion.Euler(e);
    });
```

### Requirements

- 2019.3.4f1 or later

### Install

add below lines to `Packages/manifest.json` in your project.

```json
    "com.neuecc.unirx": "https://github.com/neuecc/UniRx.git?path=Assets/Plugins/UniRx/Scripts",
    "com.jtfrom9.input-observable": "https://github.com/jtfrom9/InputObservable.git?path=Assets/InputObservable"
```
