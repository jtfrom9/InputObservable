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
        public static IInputObservable DefaultInputObservable(this MonoBehaviour behaviour, int id, EventSystem eventSystem)
        {
#if UNITY_EDITOR || UNITY_WEBGL
            return new MouseInputObservable(behaviour, id, eventSystem);
#elif UNITY_ANDROID || UNITY_IOS
            return new TouchInputObservable(behaviour, id, eventSystem);
#endif
        }

        public static IInputObservable DefaultInputObservable(this MonoBehaviour behaviour, int id = 0)
        {
            return DefaultInputObservable(behaviour, id, EventSystem.current);
        }
    }

    public static class IInputObservableExtension
    {
        public static IObservable<InputEvent> Any(this IInputObservable io)
        {
            return Observable.Merge(io.Begin, io.Move, io.End);
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

        public static IObservable<IList<TimeInterval<InputEvent>>> MoveThrottleAnyTimeIntervalSequence(this IInputObservable io, double interval)
        {
            return Observable.Merge(io.Begin,
                io.Move.ThrottleFirst(TimeSpan.FromMilliseconds(interval)),
                io.End)
                .TimeInterval()
                .Buffer(io.End);
        }

        public static IObservable<IList<TimeInterval<InputEvent>>> TakeBeforeEndTimeInterval(this IInputObservable io, int count)
        {
            return io.Any().TakeUntil(io.End.DelayFrame(1))
                .TimeInterval()
                .TakeLast(count)
                .Buffer(count)
                .RepeatUntilDestroy(io.gameObject);
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

        public static Vector3 ToEulerAngle(this Vector2 diff,Vector2 max_degree, Vector2Int screen)
        {
            return diff.ToEulerAngle(max_degree.x / screen.x, max_degree.y / screen.y);
        }

        public static Vector3 ToEulerAngle(this Vector2 diff, Vector2 max_degree)
        {
            return diff.ToEulerAngle(max_degree.x / Screen.width, max_degree.y / Screen.width);
        }

        public static IObservable<Vector3> ToEulerAngle(this IInputObservable io, float horizontal_ratio, float vertical_ratio)
        {
            return io.Any().TakeUntil(io.End.DelayFrame(1)).Buffer(2, 1)
                .Where(events => events.Count > 1)
                .RepeatUntilDestroy(io.gameObject)
                .Select(events =>
                {
                    var diffx = events[1].position.x - events[0].position.x;
                    var diffy = events[1].position.y - events[0].position.y;
                    return ToEulerAngle(new Vector2 { x = diffx, y = diffy }, horizontal_ratio, vertical_ratio);
                });
        }

        public static IObservable<Vector3> ToEulerAngle(this IInputObservable io, Vector2 max_degree, Vector2Int screen)
        {
            return io.ToEulerAngle(max_degree.x / screen.x, max_degree.y / screen.y);
        }

        public static IObservable<Vector3> ToEulerAngle(this IInputObservable io, Vector2 max_degree)
        {
            return io.ToEulerAngle(max_degree.x / Screen.width, max_degree.y / Screen.width);
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

    }
}