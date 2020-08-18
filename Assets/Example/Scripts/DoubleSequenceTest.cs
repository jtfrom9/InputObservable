using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using InputObservable;

public class DoubleSequenceTest : MonoBehaviour
{
    SliderController slider;
    DrawTargetView draw;
    IInputObservable inputObservable = null;
    IDisposable disposable = null;

    void clear()
    {
        if(disposable!=null) {
            disposable.Dispose();
            disposable = null;
        }
    }

    void setup()
    {
        clear();
        disposable = inputObservable.DoubleSequence(slider.Value.Value).Subscribe(e => {
            draw.Put(e, Color.red);
        });
    }

    void Start()
    {
        slider = FindObjectOfType<SliderController>();
        slider.Text = "Interval: ";
        draw = FindObjectOfType<DrawTargetView>();

        inputObservable = this.DefaultInputObservable();
        inputObservable.Begin.TimeInterval().Subscribe(ts => {
            Debug.Log($"[{ts.Value.sequenceId}] {ts.Interval.Milliseconds}");
        }).AddTo(this);

        slider.Enabled.Subscribe(v =>
        {
            if (!v)
            {
                clear();
            }
            else
            {
                setup();
            }
        }).AddTo(this);
        slider.Value.Subscribe(v => setup()).AddTo(this);
    }
}
