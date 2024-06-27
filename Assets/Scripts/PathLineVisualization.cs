using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class PathLineVisualization : MonoBehaviour
{
    [SerializeField] private LineRenderer line;
    private NavMeshPath path;
    private Vector3[] calculatedPathAndOffset;
    private Slider slider;
    // Start is called before the first frame update
    private void Start()
    {
        line.enabled = false;
        slider = NavigationManager.Instance.lineYOffsetSlider;
    }

    // Update is called once per frame
    private void Update()
    {
        path = NavigationManager.Instance.path;
        if (path.status != NavMeshPathStatus.PathInvalid)
        {
            line.positionCount = path.corners.Length;
            calculatedPathAndOffset = CalculateLineOffset();
            line.SetPositions(calculatedPathAndOffset);
            line.enabled = true;
        }
        else
        {
            line.enabled = false;
        }
    }
    private Vector3[] CalculateLineOffset()
    {
        if (slider.value == 0)
        {
            return path.corners;
        }
        Vector3[] calculatedPosition = new Vector3[path.corners.Length];
        for (int i = 0; i < path.corners.Length; i++)
        {
            calculatedPosition[i] = path.corners[i] + new Vector3(0, slider.value, 0);
        }
        return calculatedPosition;
    }
}