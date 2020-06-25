using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UniRx;
using InputObservable;

public class MouseSpecificTest : MonoBehaviour
{
    public GameObject target;
    public DrawTargetView draw;

    Action<InputEvent> mouseDrawHandler(Color c)
    {
        return (InputEvent e) =>
        {
            Debug.Log(e);
            if (e.type == InputEventType.Begin)
                draw.DragBegin(e, c);
            else
                draw.Dragging(e, c);
        };
    }

    void Start()
    {
        var left = new MouseInputObservable(this, 0, null);
        left.Wheel.Subscribe(e =>
        {
            target.transform.Translate(Vector3.forward * e.wheel);
        }).AddTo(this);

        left.Any().Subscribe(mouseDrawHandler(Color.blue));
        new MouseInputObservable(this, 1, null).Any().Subscribe(mouseDrawHandler(Color.yellow));
        new MouseInputObservable(this, 2, null).Any().Subscribe(mouseDrawHandler(Color.magenta));
    }
}
