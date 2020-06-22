using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InputObservable;

public class DrawTargetView : MonoBehaviour
{
    public GameObject pointPrefab;
    public GameObject dragCubePrefab;

    Dictionary<long, GameObject> dragList = new Dictionary<long, GameObject>();
    Vector3 last;

    public void Put(InputEvent e)
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(e.position), out hit))
        {
            var view = Instantiate(pointPrefab, hit.point, Quaternion.identity).GetComponent<PointView>();
            view.SetText($"{e}");
        }
    }
    public void Put(Vector2 pos)
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(pos), out hit))
        {
            var view = Instantiate(pointPrefab, hit.point, Quaternion.identity).GetComponent<PointView>();
            view.SetText($"{pos}");
        }
    }

    public void DragBegin(InputEvent e)
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(e.position), out hit))
        {
            var view = Instantiate(dragCubePrefab, hit.point, Quaternion.identity).GetComponent<DragCubeView>();
            view.SetText($"{e}");
            var go = new GameObject($"{e.id}");
            view.gameObject.transform.parent = go.transform;
            dragList[e.id] = go;
            last = hit.point;
        }
    }

    public void Dragging(InputEvent e)
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(e.position), out hit))
        {
            var view = Instantiate(dragCubePrefab, hit.point, Quaternion.identity).GetComponent<DragCubeView>();
            view.SetText($"{e.position}");
            view.DrawLine(last);
            view.gameObject.transform.parent = dragList[e.id].transform;
            last = hit.point;
        }
    }

    public void DragEnd(InputEvent e)
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(e.position), out hit))
        {
            var view = Instantiate(dragCubePrefab, hit.point, Quaternion.identity).GetComponent<DragCubeView>();
            view.SetText($"{e}");
            view.DrawLine(last);
            view.gameObject.transform.parent = dragList[e.id].transform;
            StartCoroutine(DestroyDrag(e.id));
        }
    }

    IEnumerator DestroyDrag(long id)
    {
        yield return new WaitForSeconds(3);
        Destroy(dragList[id]);
        dragList.Remove(id);
    }
}
