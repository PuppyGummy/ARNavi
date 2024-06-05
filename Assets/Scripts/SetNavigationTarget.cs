using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class SetNavigationTarget : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown targetListDropdown;
    [SerializeField] private List<Target> targetList = new List<Target>();
    [SerializeField] private Camera topDownCamera;
    [SerializeField] private Vector3 targetPosition = Vector3.zero;

    private NavMeshPath path;
    private LineRenderer line;

    private bool lineToggle = false;
    // Start is called before the first frame update
    private void Start()
    {
        path = new NavMeshPath();
        line = GetComponent<LineRenderer>();
        line.enabled = lineToggle;
    }

    // Update is called once per frame
    private void Update()
    {
        if ((Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Began))
        {
            lineToggle = !lineToggle;
        }
        if (lineToggle && targetPosition != Vector3.zero)
        {
            NavMesh.CalculatePath(transform.position, targetPosition, NavMesh.AllAreas, path);
            line.positionCount = path.corners.Length;
            line.SetPositions(path.corners);
            line.enabled = true;
        }
        else
        {
            line.enabled = false;
        }
    }
    public void SetCurrentNavigationTarget()
    {
        targetPosition = targetList[targetListDropdown.value].positionObj.transform.position;
    }
    public List<Target> GetTargetList()
    {
        return targetList;
    }
}
