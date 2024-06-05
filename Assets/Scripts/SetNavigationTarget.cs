using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SetNavigationTarget : MonoBehaviour
{
    [SerializeField] private Camera topDownCamera;
    [SerializeField] private GameObject navTargetObj;

    private NavMeshPath path;
    private LineRenderer line;

    private bool lineToggle = false;
    // Start is called before the first frame update
    private void Start()
    {
        path = new NavMeshPath();
        line = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    private void Update()
    {
        if ((Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Began))
        {
            lineToggle = !lineToggle;
        }
        if (lineToggle)
        {
            NavMesh.CalculatePath(transform.position, navTargetObj.transform.position, NavMesh.AllAreas, path);
            line.positionCount = path.corners.Length;
            line.SetPositions(path.corners);
            line.enabled = true;
        }
    }
}
