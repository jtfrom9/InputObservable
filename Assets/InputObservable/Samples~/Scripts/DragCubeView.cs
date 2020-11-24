using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragCubeView : MonoBehaviour
{
    public TextMesh textMesh;
    public MeshRenderer meshRenderer;

    public void SetText(string txt)
    {
        textMesh.text = txt;
    }

    public void SetColor(Color color)
    {
        meshRenderer.material.color = color;
    }

    public void DrawLine(Vector3 p)
    {
        var lr = GetComponent<LineRenderer>();
        lr.enabled = true;
        lr.SetPosition(0, transform.position);
        lr.SetPosition(1, p);
        lr.startWidth = 0.01f;
        lr.endWidth = 0.01f;
        // lr.startColor = Color.green;
        // lr.endColor = Color.green;
        lr.material.color = Color.green;
    }

    // void Start()
    // {
    //     Destroy(gameObject, 3.0f);
    // }
}
