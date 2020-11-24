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
        var context = new MouseInputContext(this, null);
        context.Wheel.Subscribe(e =>
        {
            target.transform.Translate(Vector3.forward * e.wheel);
        }).AddTo(this);

        var left = context.GetObservable(0);
        left.Any().Subscribe(mouseDrawHandler(Color.blue));
        context.GetObservable(1).Any().Subscribe(mouseDrawHandler(Color.yellow));
        context.GetObservable(2).Any().Subscribe(mouseDrawHandler(Color.magenta));
    }
}
