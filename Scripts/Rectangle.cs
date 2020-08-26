using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace InputObservable
{
    public static class RectangleObservable
    {
        // Rect Stream which completes when either one of io1 and io2
        // Client have to Repeat() for permanent stream.
        public static IObservable<Rect> From(IInputObservable io1, IInputObservable io2)
        {
            var end = Observable.Merge(io1.OnEnd, io2.OnEnd);
            return Observable.CombineLatest(io1.Any(), io2.Any())
                .TakeUntil(end)
                .Select(es =>
                {
                    var x = Mathf.Min(es[0].position.x, es[1].position.x);
                    var y = Mathf.Min(es[0].position.y, es[1].position.y);
                    return new Rect(x, y,
                        Mathf.Abs(es[0].position.x - es[1].position.x),
                        Mathf.Abs(es[0].position.y - es[1].position.y));
                })
                .DistinctUntilChanged();
        }
    }

    public static class RectangleObservableExtension
    {
        public static IObservable<Vector2> PinchSequence(this IObservable<Rect> ro)
        {
            return ro.Buffer(2, 1)
                .Where(rects => rects.Count > 1)
                .Select(rects =>
                {
                    var diffh = rects[1].width - rects[0].width;
                    var diffv = rects[1].height - rects[0].height;
                    return new Vector2(diffh, diffv);
                });
        }
    }
}