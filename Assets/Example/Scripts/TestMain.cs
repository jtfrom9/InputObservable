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
    public UICanvasView ui;

    List<IDisposable> disposables = new List<IDisposable>();

    void log(string msg)
    {
        Debug.Log(msg);
    }

    void clear()
    {
        foreach (var d in disposables)
        {
            d.Dispose();
        }
        disposables.Clear();
    }

    void TraceBasic(IInputObservable io)
    {
        disposables.Add(io.Begin.Subscribe(e =>
        {
            log(e.ToString());
            draw.Put(e, Color.red);
        }));
        disposables.Add(io.Move.Subscribe(e =>
        {
            log(e.ToString());
            draw.Put(e, Color.black);
        }));
        disposables.Add(io.End.Subscribe(e =>
        {
            log(e.ToString());
            draw.Put(e, Color.yellow);
        }));
    }

    void TraceLumpPoints(IInputObservable io)
    {
        disposables.Add(io.Any().Buffer(io.End).Subscribe(es => {
            log($"<color=red>Seqeuence {es.Count} points</color>");
            log($"  {es[0]}");
            if (es.Count > 2)
            {
                log($"  {string.Join(",", es.Select((e, i) => (e, i)).Where((e, i) => 1 <= i && i < es.Count - 1).Select(x => x.e.position.ToString()))}");
            }
            log($"  {es[es.Count - 1]}");
            foreach(var e in es) {
                switch (e.type)
                {
                    case InputEventType.Begin:
                        draw.Put(e, Color.red);
                        break;
                    case InputEventType.Move:
                        draw.Put(e, Color.black);
                        break;
                    case InputEventType.End:
                        draw.Put(e, Color.yellow);
                        break;
                }
            }
        }));
    }

    void TraceDoubleTap(IInputObservable io)
    {
        disposables.Add(io.DoubleSequence(250).Subscribe(e =>
       {
           var msg = $"<color=blue>double click/tap ({e})</color>";
           log(msg);
           draw.Put(e, Color.blue);
       }));
    }

    void TraceLongPress(IInputObservable io)
    {
        disposables.Add(io.LongSequence(500).Subscribe(e =>
        {
            log($"<color=green>long press at {e}</color>");
            draw.Put(e, Color.green);
        }));
    }

    void TraceDragAndDrop(IInputObservable io)
    {
        disposables.Add(io.LongSequence(500).Subscribe(begin =>
        {
            log($"<color=green>Drag begin at. {begin}</color>");
            draw.DragBegin(begin, Color.green, begin.ToString());

            disposables.Add(io.MoveThrottle(100).TakeUntil(io.End).Subscribe(drag =>
            {
                log($"Dragging. {drag}");
                draw.Dragging(drag, Color.green, drag.ToString());
            }));
            disposables.Add(io.End.First().Subscribe(drop =>
            {
                log($"<color=green>Drop at. {drop}</color>");
                draw.DragEnd(drop, Color.green, drop.ToString());
            }));
        }));
    }

    void TraceSwipe(IInputObservable io)
    {
        disposables.Add(io.Any().Subscribe(e =>
        {
            draw.Put(e, Color.blue);
        }));
        disposables.Add(io.TakeBeforeEndTimeInterval(4).Verocity().Subscribe(vs =>
        {
            log(string.Join(", ", vs.Select(vi => vi.ToString())));
            foreach(var v in vs) {
                draw.Put(v, Color.magenta);
            }
        }));
        // Swipe Gesture
        // io.Sequence().BeginEndVerocity().Subscribe(vs => {
        //     draw.Put(vs[0]);
        //     if (vs.Count() > 1)
        //     {
        //         draw.Put(vs[0] + vs[1]);
        //     }
        // }).AddTo(this);

        // io.MoveThrottleAnyTimeIntervalSequence(50).Verocity().Subscribe(verocities =>
        // {
        //     foreach(var v in verocities) {
        //         log($"verocity: {v}");
        //     }
        // }).AddTo(this);

        // long lastSeq = 0;
        // io.Any().Buffer(2, 1)
        // .IgnoreSubtle(1.0f)
        // .Where(es => es[0].type!=InputEventType.End).TimeInterval().Subscribe(ts =>
        // {
        //     var events = ts.Value;
        //     if(lastSeq!=events[0].sequenceId)
        //         log("");
        //     log($"{ts.Interval} {events[0]}, {events[1]}");
        //     lastSeq = events[0].sequenceId;
        // });


        // io.Any().TimeInterval().Buffer(4)

        // io.Any().Buffer(2, 1).IgnoreSubtle(1.0f).Subscribe(events =>
        // {
        //     log($"{events[0].id},{events[0].type}, diff={events[1].position - events[0].position}");
        // }).AddTo(this);

        // bool draging = false;
        // var longpress = io.Begin
        //     .SelectMany(e => Observable.Interval(TimeSpan.FromMilliseconds(500)).Select(_ => e))
        //     .TakeUntil(io.End)
        //     .RepeatUntilDestroy(this.gameObject)
        //     .Where(_ => !draging);
        // longpress.Subscribe(e =>
        // {
        //     log($"<color=green>Long Press Begin at. {e}</color>");
        //     draging = true;
        //     draw.DragBegin(e);
        // }).AddTo(this);
        // Observable.Merge(io.Move, io.End).ThrottleFirst(TimeSpan.FromMilliseconds(100))
        //     .Where(_ => draging)
        //     .Subscribe(e =>
        //     {

        //         if (e.type == InputEventType.Move)
        //         {
        //             log($"Dragging. {e}");
        //         }
        //         else
        //         {
        //             log($"<color=green>Drag End at. {e}</color>");
        //             draging = false;
        //         }
        //     }).AddTo(this);


        // io.Sequence().AsSwipe().Subscribe(s =>
        // {
        //     log($"<color=green>Swipe: {s}</color>");
        // }).AddTo(this);
    }

    void Start()
    {
        var io = this.DefaultInputObservable();

        ui.SelectedToggle.Subscribe(type =>
        {
            log(type);
            clear();

            if (type == "Basic")
                TraceBasic(io);
            else if (type == "LumpPoints")
                TraceLumpPoints(io);
            else if (type == "Double")
                TraceDoubleTap(io);
            else if (type == "LongPress")
                TraceLongPress(io);
            else if (type == "DragAndDrop")
                TraceDragAndDrop(io);
            else if (type == "Swipe")
                TraceSwipe(io);
            else
                Debug.LogError($"not found: {type}");
        }).AddTo(this);
    }
}
