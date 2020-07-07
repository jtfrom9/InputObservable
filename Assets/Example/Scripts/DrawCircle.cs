using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shapes;

public class DrawCircle : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        Circle.Draw(new CircleInfo()
        {
            center = transform.position,
            radius = 5.0f,
            forward = Vector3.up,
            // fillColor = Color.white
            bordered = true,
            borderColor = Color.white,
            borderWidth = 1.0f
        });
    }
}
