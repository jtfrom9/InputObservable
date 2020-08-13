using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using InputObservable;

public class ObjectViewer : MonoBehaviour
{
    public Transform target;

    void Start()
    {
        // var gio = new GyroInputObservable(this);
        // gio.EulerAngles.Subscribe(e => {
        //     target.rotation = Quaternion.Euler(e);
        // }).AddTo(this);
        var touch0 = this.DefaultInputObservable(0, null);
        var touch1 = this.DefaultInputObservable(1, null);

        touch0.Any().Where(e => e.type != InputEventType.Move).Subscribe(e => { Debug.Log(e); }).AddTo(this);
        touch1.Any().Where(e => e.type != InputEventType.Move).Subscribe(e => { Debug.Log(e); }).AddTo(this);

        touch0.ToEulerAngle(new Vector2 { x = -180, y = -90 })
            .Where(_ => !touch1.Began)
            .Subscribe(rot =>
            {
                // gio.AddRotate(rot);
                target.Rotate(rot);
            }).AddTo(this);

        var orig_rotate = target.rotation;
        var orig_scale = target.localScale;
        touch0.DoubleSequence(200).Subscribe(_ =>
        {
            Debug.Log("reset");
            target.rotation = orig_rotate;
            target.localScale = orig_scale;
        }).AddTo(this);

        var camera = Camera.main;
        (touch0 as IMouseWheelObservable)?.Wheel.Subscribe(v =>
        {
            // camera.transform.Translate(Vector3.forward * v.wheel);
            target.localScale += Vector3.one * v.wheel;
        }).AddTo(this);

        RectangleObservable.From(touch0, touch1)
            .PinchSequence()
            .RepeatUntilDestroy(this)
            .Subscribe(diff =>
        {
            if (diff.x > 0 && diff.y > 0)
            {
                var v = Mathf.Max(diff.x / Screen.width, diff.y / Screen.height);
                target.localScale += Vector3.one * v * 10;
                Debug.Log($"pinch-out: diff={diff}, v={v}, localScale={target.localScale}");
            }
            else if (diff.x < 0 && diff.y < 0)
            {
                var v = Mathf.Min(diff.x / Screen.width, diff.y / Screen.height);
                target.localScale += Vector3.one * v * 10;
                Debug.Log($"pinch-in: diff={diff}, v={v}, localScale={target.localScale}");
            }
        }).AddTo(this);
    }
}
