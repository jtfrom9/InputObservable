using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InputObservable;
using Shapes;

public class DrawTargetView : MonoBehaviour
{
    public GameObject pointPrefab;
    public GameObject dragCubePrefab;

    Dictionary<long, GameObject> dragList = new Dictionary<long, GameObject>();
    Vector3 last;

    public void Put(InputEvent e, Color color, string msg = null)
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(e.position), out hit))
        {
            var view = Instantiate(pointPrefab, hit.point, Quaternion.identity).GetComponent<PointView>();
            if (!string.IsNullOrEmpty(msg))
                view.SetText(msg);
            view.SetColor(color);
        }
    }

    public void Put(VerocityInfo v, Color color, string msg = null)
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(v.@event.position), out hit))
        {
            var view = Instantiate(pointPrefab, hit.point, Quaternion.identity).GetComponent<PointView>();
            if (!string.IsNullOrEmpty(msg))
                view.SetText(msg);
            view.SetColor(color);
            view.SetVetor(v.vector, Color.white);
        }
    }

    public void DragBegin(InputEvent e, Color color, string msg = null)
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(e.position), out hit))
        {
            dragList[e.sequenceId] = new GameObject($"{e.sequenceId}");

            var view = Instantiate(dragCubePrefab, hit.point, Quaternion.identity).GetComponent<DragCubeView>();
            Destroy(view.gameObject.GetComponent<LineRenderer>()); // Remove LineRnederer for first point.
            if (!string.IsNullOrEmpty(msg))
                view.SetText(msg);
            view.SetColor(color);
            view.gameObject.name = e.ToString();
            view.gameObject.transform.parent = dragList[e.sequenceId].transform;
            last = hit.point;
        }
    }

    public void Dragging(InputEvent e, Color color, string msg = null)
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(e.position), out hit))
        {
            var view = Instantiate(dragCubePrefab, hit.point, Quaternion.identity).GetComponent<DragCubeView>();
            if (!string.IsNullOrEmpty(msg))
                view.SetText(msg);
            view.SetColor(color);
            view.DrawLine(last);
            view.gameObject.name = e.ToString();
            view.gameObject.transform.parent = dragList[e.sequenceId].transform;
            last = hit.point;
        }
    }

    public void DragEnd(InputEvent e, Color color, string msg = null)
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(e.position), out hit))
        {
            var view = Instantiate(dragCubePrefab, hit.point, Quaternion.identity).GetComponent<DragCubeView>();
            if (!string.IsNullOrEmpty(msg))
                view.SetText(msg);
            view.SetColor(color);
            view.DrawLine(last);
            view.gameObject.name = e.ToString();
            view.gameObject.transform.parent = dragList[e.sequenceId].transform;
            StartCoroutine(DestroyDrag(e.sequenceId));
        }
    }

    IEnumerator DestroyDrag(long id)
    {
        yield return new WaitForSeconds(3);
        Destroy(dragList[id]);
        dragList.Remove(id);
    }
}
