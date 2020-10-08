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
    IInputObservable touch0 = context.GetObservable(0);

    // Get IInputObservable with id=1, right button for Editor, fingerId=1 for Android/iOS
    IInputObservable touch1 = context.GetObservable(1);

    // Draw Point at the location of first touch (or left button clicked)
    touch0.OnBegin.Subscribe(e => // e is InputEvent
    {
        DrawPoint(e.position);
    });

    // Slide (or dragging) Detection
    touch0.OnMoved.Subscribe(e => ...);

    // Touch Up (or left button release) detection
    touch0.OnEnd.Subscribe(e => ...);

    // Any input event
    touch0.Any().Subscribe(e => ...);

    // Double Tap/click detection during 200 msec
    touch0.DoubleSequence(200).Subscribe(e => {})

    // Long Touch/press detection over 1000 msec
    touch0.LongSequence(1000).Subscribe(e => {})

    // Slide/Move speed detection between two consecutive points
    touch0.Verocity().Subscribe(v => // v is VerocityInfo
    {
        DrawPoint(v.@event.position);
        DrawArrow(v.vector); // v.vector is Vector2 per milliseconds
    });

    // Slide/Move speed detection between two consecutive points in the last 8 points
    touch0.TakeLastVerocities(8).Subscribe(verocities =>
    {
        foreach (var v in verocities) {
            DrawPoint(v.@event.position);
            DrawArrow(v.vector)
        }
    });

    // Rotation event conversion from slide/move operation,
    // at a rate of 90 degrees of slide at both ends of the screen (both up and down and left and right).
    touch0.Difference().Subscribe(diff =>  // diff is Vector2
    {
        var rot = diff.ToEulerAngle(-90.0f / Screen.width, -90.0f / Screen.height);
        target.Rotate(rot, Space.World);
    }

    // Differences of the width/height value of the rectangle over time (two consecutive rectangles)
    // for multi touch pinch operation
    RectangleObservable.From(touch0, touch1)
        .PinchSequence()
        .RepeatUntilDestroy(this)
        .Subscribe(diff =>  // diff is Vector2
    {
        if(diff.x > 0) { /* larger horizontally*/ }
        else if(diff.x <= 0) { /* smaller horizontally */ }
        if(diff.y > 0) { /* larger vertically */ }
        else if(diff.y <= 0) { /* smaller vertically */ }
    });

    // Mouse wheel operation, type check is usefull for shared code between Editor and Android/iOS
    (context as IMouseWheelObservable)?.Wheel.Subscribe(w => {
        // w is MouseWheelEvent
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
