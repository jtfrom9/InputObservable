using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointView : MonoBehaviour
{
    public TextMesh textMesh;

    public void SetText(string txt)
    {
        textMesh.text = txt;
    }

    void Start()
    {
        Destroy(gameObject, 3.0f);
    }
}
