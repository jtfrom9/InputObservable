using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using InputObservable;

public class ThrottleTest : MonoBehaviour
{
    void Start()
    {
        var throttleSlider = FindObjectOfType<SliderController>();
        throttleSlider.Text = "Throttle: ";
        var draw = FindObjectOfType<DrawTargetView>();

        var io = this.DefaultInputContext().GetObservable(0);

        io.OnBegin.Subscribe(e => {
            draw.DragBegin(e, Color.black);

            IObservable<InputEvent> move;
            if(throttleSlider.Enabled.Value) {
                move = io.MoveThrottle(throttleSlider.Value.Value);
            } else {
                move = io.OnMove;
            }
            move.TakeUntil(io.OnEnd).FrameInterval().Subscribe(ts =>
            {
                // Time.frameCount
                draw.Dragging(ts.Value, Color.red, ts.Interval.ToString());
            }).AddTo(this);

            io.OnEnd.First().Subscribe(ee =>
            {
                draw.DragEnd(ee, Color.gray);
            }).AddTo(this);

        }).AddTo(this);

    }
}
