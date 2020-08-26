using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UniRx;
using UniRx.Diagnostics;

namespace InputObservable
{
    public static class IInputObservableMonoBehaviourExtension
    {
        static InputObservableContext defaultContext = null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void _initDefaultContext() { defaultContext = null; }

        public static InputObservableContext DefaultInputContext(this MonoBehaviour behaviour, EventSystem eventSystem = null)
        {
            if (defaultContext != null)
            {
                throw new InvalidOperationException("already created defaultContext");
            }
#if UNITY_EDITOR || UNITY_WEBGL
            defaultContext = new MouseInputContext(behaviour, eventSystem);
#elif UNITY_ANDROID || UNITY_IOS
            defaultContext = new TouchInputContext(behaviour, eventSystem);
#endif
            return defaultContext;
        }
    }

    public static class IInputObservableExtension
    {
        public static IObservable<InputEvent> Any(this IInputObservable io)
        {
            return Observable.Merge(io.Begin, io.Move, io.End);
        }

        public static IObservable<Unit> Keep(this IInputObservable io, double interval, Func<bool> pred)
        {
            return io.Begin.SelectMany(_ =>
                Observable.Interval(TimeSpan.FromMilliseconds(100))
                    .TakeUntil(io.End)
                    .Where(x => pred())
                    .Select(x => Unit.Default));
        }

        public static IObservable<InputEvent> MoveThrottle(this IInputObservable io, double interval)
        {
            return io.Move.ThrottleFirst(TimeSpan.FromMilliseconds(interval));
        }

        public static IObservable<InputEvent> DoubleSequence(this IInputObservable io, double interval)
        {
            return io.Begin.TimeInterval()
                .Buffer(2, 1)
                .Where(events => events[0].Interval.TotalMilliseconds > interval && events[1].Interval.TotalMilliseconds <= interval)
                .Select(events => events[1].Value);
        }

        public static IObservable<InputEvent> LongSequence(this IInputObservable io, double interval)
        {
            return io.Begin.SelectMany(e =>
                Observable.Interval(TimeSpan.FromMilliseconds(interval))
                    .First()
                    .Select(_ => e)
                    .TakeUntil(io.End));
        }

        public static IObservable<IList<TimeInterval<InputEvent>>> TakeBeforeEndTimeInterval(this IInputObservable io, int count)
        {
            // return io.Any().TakeUntil(io.End.DelayFrame(1))
            //     .TimeInterval()
            //     .TakeLast(count)
            //     .Buffer(count)
            //     .RepeatUntilDestroy(io.Context.gameObject);
            return io.Begin.SelectMany(e =>
                Observable.Merge(Observable.Return(e), io.Any().TakeUntil(io.End.DelayFrame(1)))
                    .TimeInterval()
                    .TakeLast(count)
                    .Buffer(count));
        }

        public static IObservable<IList<VerocityInfo>> Verocity(this IObservable<IList<TimeInterval<InputEvent>>> timeSeqIo)
        {
            return timeSeqIo.Select(sequence =>
            {
                var verocities = new List<VerocityInfo>();
                var prev = sequence.First();
                for (int i = 1; i < sequence.Count; i++)
                {
                    var t = sequence[i];
                    var diff = t.Interval.TotalMilliseconds;
                    verocities.Add(new VerocityInfo()
                    {
                        @event = prev.Value,
                        vector = new Vector2()
                        {
                            x = (float)((t.Value.position.x - prev.Value.position.x) / diff),
                            y = (float)((t.Value.position.y - prev.Value.position.y) / diff),
                        }
                    });
                    prev = t;
                }
                return verocities;
            });
        }

        public static IObservable<IList<VerocityInfo>> Verocity(this IInputObservable io, int count)
        {
            return io.TakeBeforeEndTimeInterval(count).Verocity();
        }

        public static IObservable<Vector2> Difference(this IInputObservable io)
        {
            return io.Begin.SelectMany(e =>
                Observable.Merge(Observable.Return(e), io.Any().TakeUntil(io.End.DelayFrame(1)))
                    .Buffer(2, 1))
                .Where(events => events.Count > 1)
                .Select(events =>
                {
                    return new Vector2
                    {
                        x = events[1].position.x - events[0].position.x,
                        y = events[1].position.y - events[0].position.y
                    };
                });
        }

        public static Vector3 ToEulerAngle(this Vector2 diff, float horizontal_ratio, float vertical_ratio)
        {
            return new Vector3()
            {
                x = -vertical_ratio * diff.y, // axis X
                y = horizontal_ratio * diff.x, // axis Y
                z = 0
            };
        }
    }

    public static class IInputListObservableExtension
    {
        public static IObservable<IList<InputEvent>> IgnoreSubtle(this IObservable<IList<InputEvent>> events, float delta)
        {
            return events.Where(list =>
            {
                return list[1].type == InputEventType.End ||
                    (Mathf.Abs(list[0].position.x - list[1].position.x) > delta &&
                    Mathf.Abs(list[0].position.y - list[1].position.y) > delta);
            });
        }
    }

    public static class IInputTimeListObservableExtension
    {
    }
}
