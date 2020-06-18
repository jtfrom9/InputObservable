using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UniRx;

namespace InputObservable
{
    public static class IInputObservableMonoBehaviourExtension
    {
        public static IInputObservable DefaultInputObservable(this MonoBehaviour behaviour)
        {
#if UNITY_EDITOR || UNITY_WEBGL
            return new MouseInputObservable(behaviour, 0, EventSystem.current);
#elif UNITY_ANDROID || UNITY_IOS
            return new TouchInputObservable(behaviour, 0, EventSystem.current);
#endif
        }
    }

    public static class IInputObservableExtension
    {
        public static IObservable<InputEvent> Any(this IInputObservable io)
        {
            return Observable.Merge(io.Begin, io.Move, io.End);
        }

        public static IObservable<IList<InputEvent>> Sequence(this IInputObservable io)
        {
            return io.Any().Buffer(io.End);
        }

        public static IObservable<IList<TimeInterval<InputEvent>>> SequenceTimeInterval(this IInputObservable io)
        {
            return io.Any().TimeInterval().Buffer(io.End);
        }

        public static IObservable<InputEvent> MoveThrottle(this IInputObservable io, double interval)
        {
            return io.Move.ThrottleFirst(TimeSpan.FromMilliseconds(interval));
        }

        public static IObservable<IList<InputEvent>> MoveThrottleAny(this IInputObservable io, double interval)
        {
            return Observable.Merge(io.Begin,
                io.Move.ThrottleFirst(TimeSpan.FromMilliseconds(interval)),
                io.End)
                .Buffer(io.End);
        }

        public static IObservable<IList<TimeInterval<InputEvent>>> MoveThrottleAnyTimeInterval(this IInputObservable io, double interval)
        {
            return Observable.Merge(io.Begin,
                io.Move.ThrottleFirst(TimeSpan.FromMilliseconds(interval)),
                io.End)
                .TimeInterval()
                .Buffer(io.End);
        }

        public static IObservable<InputEvent> Double(this IInputObservable io, double interval)
        {
            return io.Begin.TimeInterval()
                .Buffer(2, 1)
                .Where(events => events[0].Interval.TotalMilliseconds > interval && events[1].Interval.TotalMilliseconds <= interval)
                .Select(events => events[1].Value)
                .Publish().RefCount();
        }

        public static IObservable<InputEvent> Long(this IInputObservable io, double interval)
        {
            // return io.Begin.Throttle(TimeSpan.FromMilliseconds(interval));
            return io.Begin.SelectMany(e =>
                Observable.Interval(TimeSpan.FromMilliseconds(interval))
                    .Select(_ => e)
                    .TakeUntil(io.End))
                .First()
                .RepeatUntilDestroy(io.gameObject);
        }

        public static IObservable<IList<Vector2>> Verocity(this IInputObservable io, double interval)
        {
            return io.MoveThrottleAnyTimeInterval(interval).Select(sequence =>
            {
                var verocities = new List<Vector2>();
                var prev = sequence.First();
                for (int i = 1; i < sequence.Count; i++)
                {
                    var t = sequence[i];
                    // var diff = t.Interval.TotalMilliseconds - prev.Interval.TotalMilliseconds;
                    var diff = t.Interval.TotalMilliseconds;
                    verocities.Add(new Vector2()
                    {
                        x = (float)((t.Value.position.x - prev.Value.position.x) / diff),
                        y = (float)((t.Value.position.y - prev.Value.position.y) / diff),
                    });
                    // Debug.Log($"diff={diff}, {t.Value.position}@{t.Interval.TotalMilliseconds} - {prev.Value.position}@{t.Interval.TotalMilliseconds}");
                    prev = t;
                }
                return verocities;
            });
        }
    }

    public static class IInputObservableListExtension
    {
        public static IObservable<IList<Vector2>> BeginEndVerocity(this IObservable<IList<InputEvent>> seqio)
        {
            return seqio.Select(sequence =>
            {
                var begin = sequence.First();
                var end = sequence.Last();
                var diffx = end.position.x - begin.position.x;
                var diffy = end.position.y - begin.position.y;
                var ret = new List<Vector2>() { begin.position };
                if (Math.Abs(diffx) >= 0.1f || Math.Abs(diffy) >= 0.1f)
                {
                    ret.Add(Vector2.right * diffx + Vector2.up * diffy);
                }
                return ret;
            });
        }

        // public static IObservable<IList<Vector2>> Verocity(this IObservable<IList<TimeInterval<InputEvent>>> sequenceTimeInterval)
        // {
        //     sequenceTimeInterval.Select(sequence => {
        //         var verocities = new List<Vector2>();
        //         foreach(var t in sequence) {
        //             var x = t.Interval;
        //             var id = t.Value.id;
        //         }
        //     });
        // }
    }
}
