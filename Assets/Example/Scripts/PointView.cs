using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shapes;

public class PointView : MonoBehaviour
{
    public TextMesh textMesh;
    MeshRenderer meshRenderer;
    bool vector = false;
    LineInfo vecInfo;

    public void SetText(string txt)
    {
        textMesh.text = txt;
    }

    public void SetColor(Color color)
    {
        meshRenderer.material.color = color;
    }

    public void SetVetor(Vector2 vec, Color color)
    {
        vector = true;
        var width = 0.05f;
        vecInfo = new LineInfo()
        {
            startPos = transform.position,
            endPos = transform.position + Vector3.right * vec.x / 2 + Vector3.up * vec.y / 2,
            width = width,
            fillColor = color,
            forward = Camera.main.transform.forward,
            endArrow = true,
            arrowWidth = width + 0.2f,
            arrowLength = 0.3f,
        };
    }

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    void Start()
    {
        Destroy(gameObject, 3.0f);
    }

    void Update()
    {
        if(vector) {
            LineSegment.Draw(vecInfo);
        }
    }
}
