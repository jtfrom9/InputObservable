﻿using System;
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
        var colors = new List<Color> { Color.green, Color.yellow, Color.gray, Color.gray, Color.gray };
        foreach (var (index, color) in colors.Select((color, index) => (index, color)))
        {
            var io = this.DefaultInputObservable(index);
            io.Any().Subscribe(touchDrawHandler(color)).AddTo(this);
            ios.Add(io);
        }

        var ro = RectangleObservable.From(ios[0], ios[1]);
        ro.DoOnCompleted(() => {
            text2.text = "";
        }).RepeatUntilDestroy(this).Subscribe(rect =>
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