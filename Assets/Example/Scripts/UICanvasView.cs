using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Toolkit;

public class UICanvasView : MonoBehaviour
{
    public ToggleGroup toggleGroup;
    public IReadOnlyReactiveProperty<string> SelectedToggle { get; private set; }

    void Start()
    {
        SelectedToggle = toggleGroup.ObserveEveryValueChanged(x => toggleGroup.ActiveToggles().FirstOrDefault())
            .Select(t => t.name)
            .ToReadOnlyReactiveProperty();
    }
}
