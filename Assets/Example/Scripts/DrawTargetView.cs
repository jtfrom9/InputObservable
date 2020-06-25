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

    Dictionary<IInputObservable, GameObject> cross = new Dictionary<IInputObservable, GameObject>();

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

    public void NewCross(InputEvent e, Color color)
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(e.position), out hit))
        {
            var view = Instantiate(pointPrefab, hit.point, Quaternion.identity).GetComponent<PointView>();
            view.SetCross(color);
            cross[e.sender] = view.gameObject;
        }
    }

    public void MoveCross(InputEvent e)
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(e.position), out hit))
        {
            if(hit.collider.gameObject!=gameObject) {
                return;
            }
            if (cross.ContainsKey(e.sender))
            {
                cross[e.sender].transform.position = hit.point;
            }
        }
    }

    public void EndCross(InputEvent e)
    {
        if (cross.ContainsKey(e.sender))
        {
            Destroy(cross[e.sender]);
            cross.Remove(e.sender);
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
            dragList[e.sequenceId] = new GameObject($"{e.sender}-{e.sequenceId}");

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
