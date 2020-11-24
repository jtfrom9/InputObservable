using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shapes;

public class PointView : MonoBehaviour
{
    public TextMesh textMesh;
    MeshRenderer meshRenderer;
    bool vector = false;
    public LineInfo vecInfo;
    bool cross = false;
    LineInfo line1;
    LineInfo line2;

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

    public void SetCross(Color color)
    {
        cross = true;
        var X = transform.position.x;
        var Y = transform.position.y;
        var Z = transform.position.z;
        line1 = new LineInfo()
        {
            startPos = new Vector3(-100, Y, Z),
            endPos = new Vector3(100, Y, Z),
            width = 0.03f,
            fillColor = color,
            forward = Camera.main.transform.forward,
        };
        line2 = new LineInfo()
        {
            startPos = new Vector3(X, -100, Z),
            endPos = new Vector3(X, 100, Z),
            width = 0.03f,
            fillColor = color,
            forward = Camera.main.transform.forward,
        };
    }

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    void Start()
    {
        if(!cross)
            Destroy(gameObject, 3.0f);
    }

    void Update()
    {
        if(vector) {
            LineSegment.Draw(vecInfo);
        }
        if(cross) {
            line1.startPos.y = transform.position.y;
            line1.endPos.y = transform.position.y;
            LineSegment.Draw(line1);
            line2.startPos.x = transform.position.x;
            line2.endPos.x = transform.position.x;
            LineSegment.Draw(line2);
        }
    }
}
