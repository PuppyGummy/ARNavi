using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class PathArrowVisualization : MonoBehaviour
{
    [SerializeField] private GameObject arrow;
    [SerializeField] private float moveOnDistance = 1.0f;
    private NavMeshPath path;
    private float currentDistance = 0.0f;
    private Vector3[] pathOffset;
    private Vector3 nextNavigationPoint = Vector3.zero;
    private Slider slider;
    private GameObject userIndicator;

    // Start is called before the first frame update
    private void Start()
    {
        arrow.SetActive(false);
        slider = NavigationManager.Instance.lineYOffsetSlider;
        userIndicator = NavigationManager.Instance.userIndicator;
    }

    // Update is called once per frame
    private void Update()
    {
        path = NavigationManager.Instance.path;
        if (path.status != NavMeshPathStatus.PathInvalid)
        {
            arrow.SetActive(true);

            AddOffsetToPath();
            SelectNextNavigationPoint();
            AddArrowOffset();

            arrow.transform.LookAt(nextNavigationPoint);
        }
        else
        {
            arrow.SetActive(false);
        }
    }
    private void AddOffsetToPath()
    {
        pathOffset = new Vector3[path.corners.Length];
        for (int i = 0; i < path.corners.Length; i++)
        {
            pathOffset[i] = new Vector3(path.corners[i].x, userIndicator.transform.position.y, path.corners[i].z);
        }
    }

    private void SelectNextNavigationPoint()
    {
        nextNavigationPoint = SelectNextNavigationPointWithinDistance();
    }

    private Vector3 SelectNextNavigationPointWithinDistance()
    {
        for (int i = 0; i < pathOffset.Length; i++)
        {
            currentDistance = Vector3.Distance(userIndicator.transform.position, pathOffset[i]);
            if (currentDistance > moveOnDistance)
            {
                return pathOffset[i];
            }
        }
        return NavigationManager.Instance.targetPosition;
    }

    private void AddArrowOffset()
    {
        if (slider.value != 0)
        {
            arrow.transform.position = new Vector3(arrow.transform.position.x, slider.value, arrow.transform.position.z);
        }
    }
    public void DisableArrow()
    {
        arrow.SetActive(false);
    }
}
