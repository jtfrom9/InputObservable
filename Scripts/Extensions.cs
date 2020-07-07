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

        public static IObservable<InputEvent> Any(this IInputObservable io, double interval)
        {
            return Observable.Merge(io.Begin,
                io.Move.ThrottleFirst(TimeSpan.FromMilliseconds(interval)),
                io.End);
        }

        public static IObservable<InputEvent> MoveThrottle(this IInputObservable io, double interval)
        {
            return io.Move.ThrottleFirst(TimeSpan.FromMilliseconds(interval));
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

        public static IObservable<Vector3> AsRotate(this IInputObservable io, Vector2 maxRotate)
        {
            return io.Any().TakeUntil(io.End.DelayFrame(1)).Buffer(2, 1)
            .Where(events => events.Count > 1)
            .RepeatUntilDestroy(io.gameObject)
            .Select(events =>
            {
                // if (events.Count > 1)
                // {
                //     Debug.Log($"<color=blue>{events[0]}, {events[1]}</color>");
                // }
                // else
                // {
                //     Debug.Log($"<color=blue>{events[0]}");
                // }
                var diffx = events[1].position.x - events[0].position.x;
                var diffy = events[1].position.y - events[0].position.y;
                return new Vector3()
                {
                    x = -maxRotate.y / Screen.height * diffy, // axis X
                    y = maxRotate.x / Screen.width * diffx, // axis Y
                    z = 0
                };
            });
        }

        public static IObservable<Vector3> AsRotate(this IInputObservable io, float horizontal_degree, float vertical_degree)
        {
            return AsRotate(io, new Vector2()
            {
                x = horizontal_degree,
                y = vertical_degree,
            });
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
