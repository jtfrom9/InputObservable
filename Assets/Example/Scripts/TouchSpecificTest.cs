using System;
using System.Linq;
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
        var ios = new List<IInputObservable>();
        var colors = new List<Color> { Color.green, Color.yellow, Color.gray, Color.gray, Color.gray};
        foreach(var (index,color) in colors.Select((color,index) => (index,color))) {
            var io = this.DefaultInputObservable(index);
            io.Any().Subscribe(touchDrawHandler(color)).AddTo(this);
            ios.Add(io);
        }

        var ro = RectangleObservable.From(ios[0], ios[1]);
        ro.Subscribe(rect =>
        {
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
