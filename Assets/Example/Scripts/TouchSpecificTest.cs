using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Diagnostics;
using InputObservable;

public static class TouchFeedback
{
    static Action<InputEvent> touchDrawHandler(DrawTargetView draw, Color c)
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

    public static List<IInputObservable> Setup(InputObservableContext context, DrawTargetView draw, Component component)
    {
        var ios = new List<IInputObservable>();
        if (draw == null || component == null)
        {
            return ios;
        }
        var colors = new List<Color> { Color.green, Color.yellow, Color.red, Color.blue, Color.black };
        foreach (var (index, color) in colors.Select((color, index) => (index, color)))
        {
            try
            {
                var io = context.GetObservable(index);
                io.Any().Subscribe(touchDrawHandler(draw, color)).AddTo(component);
                ios.Add(io);
            }catch(IndexOutOfRangeException e) {
                Debug.Log($"{e} ignore");
                break;
            }
        }
        return ios;
    }

    public static void DrawSwipeArrow(DrawTargetView draw, Vector2 pos, Vector2 vector, string msg)
    {
        draw.Put(pos, vector, Color.red, msg);
    }
}

public class TouchSpecificTest : MonoBehaviour
{
    public DrawTargetView draw;
    public Text text;
    public Text text2;


    void Start()
    {
        var context = this.DefaultInputContext();
        var ios = TouchFeedback.Setup(context, draw, this);

        // Reset by finger release
        Observable.Merge(ios[0].OnEnd, ios[1].OnEnd).Subscribe(_ =>
        {
            text.text = "";
            text2.text = "";
        }).AddTo(this);

        var ro = RectangleObservable.From(ios[0], ios[1]);

        // Rectangle
        ro.RepeatUntilDestroy(this).Subscribe(rect =>
        {
            Debug.Log($"Rect: {rect}");
            text.text = rect.ToString();
        }).AddTo(this);

        // Pinch In/Out Detection
        ro.PinchSequence()
            .RepeatUntilDestroy(this)
            .Subscribe(diff =>
            {
                Debug.Log($"Horizontal: {diff.x}, Vertical: {diff.y}");
                string th = "";
                string tv = "";

                if (diff.x < 0) th = $"<color=red>dX={diff.x}</color>";
                else if (diff.x > 0) th = $"<color=blue>dX={diff.x}</color>";
                else th = $"{diff.x}";

                if (diff.y < 0) tv = $"<color=red>dY={diff.y}</color>";
                else if (diff.y > 0) tv = $"<color=blue>dY={diff.y}</color>";
                else tv = $"{diff.x}";

                text2.text = $"{th}, {tv}";
            }).AddTo(this);
    }
}
