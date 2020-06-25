using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using InputObservable;

public class TouchSpecificTest : MonoBehaviour
{
    public DrawTargetView draw;
    public Text text;
    public Text text2;

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
        this.DefaultInputObservable(0).Any().Subscribe(touchDrawHandler(Color.green));
        this.DefaultInputObservable(1).Any().Subscribe(touchDrawHandler(Color.yellow));
        this.DefaultInputObservable(2).Any().Subscribe(touchDrawHandler(Color.gray));
        this.DefaultInputObservable(3).Any().Subscribe(touchDrawHandler(Color.gray));
        this.DefaultInputObservable(4).Any().Subscribe(touchDrawHandler(Color.gray));

        var ro = RectangleObservable.From(this.DefaultInputObservable(0), this.DefaultInputObservable(1));
        ro.Subscribe(rect => {
            Debug.Log($"Rect: {rect}");
            text.text = rect.ToString();
        }).AddTo(this);

        // Pinch In/Out Detection
        ro.Buffer(2, 1).ThrottleFirst(TimeSpan.FromMilliseconds(100f)).Subscribe(rects => {
            var diffh = rects[1].width - rects[0].width;
            var diffv = rects[1].height - rects[0].height;
            Debug.Log($"Horizontal: {diffh}, Vertical: {diffv}");
            if(diffh < 0 && diffv < 0)
            {
                text2.text = $"<color=blue>Pinch-In ({diffh},{diffv})</color>";
            } else if (diffh > 0 && diffv > 0)
            {
                text2.text = $"<color=red>Pinch-Out ({diffh},{diffv})</color>";
            } else 
            {
                text2.text = "?";
            }
        }).AddTo(this);
    }
}
