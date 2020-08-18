using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using InputObservable;

public class ObjectViewer : MonoBehaviour
{
    public GameObject prefab;
    Transform target;

    void Awake()
    {
        target = Instantiate(prefab).transform;
    }

    void Start()
    {
        var touch0 = this.DefaultInputObservable(0, null);
        var touch1 = this.DefaultInputObservable(1, null);

        touch0.Any().Where(e => e.type != InputEventType.Move).Subscribe(e => { Debug.Log(e); }).AddTo(this);
        touch1.Any().Where(e => e.type != InputEventType.Move).Subscribe(e => { Debug.Log(e); }).AddTo(this);

        // Rotate by Touch Operation
        touch0.ToEulerAngle(new Vector2 { x = -180, y = -180 })
            .Where(_ => !touch1.Began)
            .Subscribe(rot =>
            {
                target.Rotate(rot, Space.World);
            }).AddTo(this);

        // Reset
        var orig_rotate = target.rotation;
        var orig_scale = target.localScale;
        touch0.DoubleSequence(200).Subscribe(_ =>
        {
            target.rotation = orig_rotate;
            target.localScale = orig_scale;
        }).AddTo(this);

        // Scale Object by Mouse Wheel
        var camera = Camera.main;
        (touch0 as IMouseWheelObservable)?.Wheel.Subscribe(v =>
        {
            var scale = target.localScale + Vector3.one * v.wheel;
            if(0.1 < scale.x && scale.x < 10)
                target.localScale += Vector3.one * v.wheel;
        }).AddTo(this);

        // Scale Object by Pinch Operation
        RectangleObservable.From(touch0, touch1)
            .PinchSequence()
            .RepeatUntilDestroy(this)
            .Subscribe(diff =>
        {
            if (diff.x > 0 && diff.y > 0)
            {
                var v = Mathf.Max(diff.x / Screen.width, diff.y / Screen.height);
                var scale = target.localScale + Vector3.one * v * 10;
                if (scale.x < 10)
                {
                    target.localScale = scale;
                    Debug.Log($"pinch-out: diff={diff}, v={v}, localScale={target.localScale}");
                }
            }
            else if (diff.x < 0 && diff.y < 0)
            {
                var v = Mathf.Min(diff.x / Screen.width, diff.y / Screen.height);
                var scale = target.localScale + Vector3.one * v * 10;
                if (scale.x > 0.1)
                {
                    target.localScale = scale;
                    Debug.Log($"pinch-in: diff={diff}, v={v}, localScale={target.localScale}");
                }
            }
        }).AddTo(this);
    }
}
