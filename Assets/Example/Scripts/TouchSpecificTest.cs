using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using InputObservable;

public class TouchSpecificTest : MonoBehaviour
{
    public DrawTargetView draw;

    Action<InputEvent> touchDrawHandler(Color c)
    {
        return (InputEvent e) =>
        {
            Debug.Log(e);
            switch (e.type)
            {
                case InputEventType.Begin:
                    draw.NewCross(e, c);
                    break;
                case InputEventType.Move:
                    draw.MoveCross(e);
                    break;
                case InputEventType.End:
                    draw.EndCross(e);
                    break;
            }
        };
    }

    void Start()
    {
        // var default = new TouchInputObservable(this, 0, null);
        this.DefaultInputObservable(0).Any().Subscribe(touchDrawHandler(Color.red));
        this.DefaultInputObservable(1).Any().Subscribe(touchDrawHandler(Color.blue));
        this.DefaultInputObservable(2).Any().Subscribe(touchDrawHandler(Color.yellow));
        this.DefaultInputObservable(3).Any().Subscribe(touchDrawHandler(Color.green));
        this.DefaultInputObservable(4).Any().Subscribe(touchDrawHandler(Color.black));
    }
}
