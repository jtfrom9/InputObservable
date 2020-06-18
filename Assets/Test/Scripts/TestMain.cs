using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using InputObservable;

public class TestMain : MonoBehaviour
{
    public DrawTargetView draw;
    public GameObject cube;

    void log(string msg)
    {
        Debug.Log(msg);
    }

    void Start()
    {
        var io = this.DefaultInputObservable();

        io.Sequence().Subscribe(es =>
        {
            log($"<color=red>Seqeuence {es.Count} points</color>");
            log($"  {es[0]}");
            if (es.Count > 2)
            {
                log($"  {string.Join(",", es.Select((e, i) => (e, i)).Where((e, i) => 1 <= i && i < es.Count - 1).Select(x => x.e.position.ToString()))}");
            }
            log($"  {es[es.Count - 1]}");
        }).AddTo(this);

        // Long
        io.Long(500).Subscribe(e => {
            Debug.Log($"<color=red>long press at {e}</color>");
        }).AddTo(this);

        // Drag & Drop
        io.Long(500).Subscribe(begin =>
        {
            Debug.Log($"<color=green>Drag begin at. {begin}</color>");
            draw.DragBegin(begin);
            io.MoveThrottle(100).TakeUntil(io.End).Subscribe(drag =>
            {
                Debug.Log($"Dragging. {drag}");
                draw.Dragging(drag);
            }).AddTo(this);
            io.End.First().Subscribe(end =>
            {
                Debug.Log($"<color=green>Drop at. {end}</color>");
                draw.DragEnd(end);
            }).AddTo(this);
        }).AddTo(this);

        // Swipe Gesture
        // io.Sequence().BeginEndVerocity().Subscribe(vs => {
        //     draw.Put(vs[0]);
        //     if (vs.Count() > 1)
        //     {
        //         draw.Put(vs[0] + vs[1]);
        //     }
        // }).AddTo(this);

        io.Verocity(50).Subscribe(verocities =>
        {
            foreach(var v in verocities) {
                Debug.Log($"verocity: {v}");
            }
        }).AddTo(this);
        // bool draging = false;
        // var longpress = io.Begin
        //     .SelectMany(e => Observable.Interval(TimeSpan.FromMilliseconds(500)).Select(_ => e))
        //     .TakeUntil(io.End)
        //     .RepeatUntilDestroy(this.gameObject)
        //     .Where(_ => !draging);
        // longpress.Subscribe(e =>
        // {
        //     Debug.Log($"<color=green>Long Press Begin at. {e}</color>");
        //     draging = true;
        //     draw.DragBegin(e);
        // }).AddTo(this);
        // Observable.Merge(io.Move, io.End).ThrottleFirst(TimeSpan.FromMilliseconds(100))
        //     .Where(_ => draging)
        //     .Subscribe(e =>
        //     {

        //         if (e.type == InputEventType.Move)
        //         {
        //             Debug.Log($"Dragging. {e}");
        //         }
        //         else
        //         {
        //             Debug.Log($"<color=green>Drag End at. {e}</color>");
        //             draging = false;
        //         }
        //     }).AddTo(this);


        // io.Sequence().AsSwipe().Subscribe(s =>
        // {
        //     Debug.Log($"<color=green>Swipe: {s}</color>");
        // }).AddTo(this);

        // Double Tap
        io.Double(250).Subscribe(e =>
        {
            Debug.Log($"<color=blue>double click/tap ({e})</color>");
            draw.Put(e);
        }).AddTo(this);


        var mw = io as MouseInputObservable;
        if(mw!=null) {
            mw.Wheel.Subscribe(e => {
                Debug.Log(e.wheel);
                cube.transform.Translate(Vector3.forward * e.wheel);
            }).AddTo(this);
        }
    }
}
