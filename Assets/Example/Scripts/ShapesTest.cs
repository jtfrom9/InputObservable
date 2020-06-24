using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shapes;

public class ShapesTest : MonoBehaviour
{
    public Vector3 forward = Vector3.forward;
    
    [Range(0.05f, 2.0f)]
    public float width = 0.1f;

    void Update()
    {
        LineSegment.Draw(new LineInfo()
        {
            startPos = new Vector3(-1, 0, 0),
            endPos = new Vector3(1, 0, 0),
            fillColor = Color.red,
            // forward = -Camera.main.transform.forward,
            forward = forward,
            width = width,
            // startArrow = true,
            endArrow = true,
            arrowWidth = width + 0.2f,
            arrowLength = 0.3f
        });
    }
}
