using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UniRx;
using InputObservable;

public class GyroCameraController : MonoBehaviour
{
    public Text text;
    public Button resetButton;

    void Start()
    {
        // Cursor.visible = false;

        var gyro = new GyroInputObservable(this);

        gyro.EulerAngles.Subscribe(e =>
        {
            transform.rotation = Quaternion.Euler(e);
            text.text = $"Pos: {transform.position}, Rot: {transform.rotation.eulerAngles}";
        }).AddTo(this);

        // reset gyro rotation
        resetButton.OnClickAsObservable()
            .Subscribe(_ => { gyro.Reset(); })
            .AddTo(this);

        // Screen Touch (or Mouse) to gyro input emulation
        var context = this.DefaultInputContext(EventSystem.current);
        var hratio = -90.0f / Screen.width;
        var vratio = -90.0f / Screen.height;
        context.GetObservable(0)
            .Difference()
            .Subscribe(diff => gyro.AddRotate(diff.ToEulerAngle(hratio, vratio)))
            .AddTo(this);
    }
}
