using UnityEngine;
using UnityEngine.AI;

public class MascotController : MonoBehaviour
{
    [SerializeField] private Transform mascotTransform;
    [SerializeField] private float mascotDistanceAhead = 2.0f;
    [SerializeField] private float moveOnDistance = 1.0f;
    [SerializeField] private float smoothingFactor = 0.1f;

    private NavMeshPath path;
    private GameObject userIndicator;
    private NavMeshAgent agent;
    private Vector3[] pathOffset;
    private Animator animator;

    public static MascotController Instance;
    private void Awake()
    {
        // Singleton pattern
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        GameObject.DontDestroyOnLoad(this.gameObject);
    }
    void Start()
    {
        userIndicator = NavigationManager.Instance.userIndicator;
        agent = mascotTransform.GetComponent<NavMeshAgent>();
        animator = mascotTransform.GetComponent<Animator>();
    }

    void Update()
    {
        path = NavigationManager.Instance.path;
        if (path.status == NavMeshPathStatus.PathInvalid)
        {
            mascotTransform.gameObject.SetActive(false);
        }
        else
        {
            mascotTransform.gameObject.SetActive(true);
            AddOffsetToPath();
            UpdateMascotPosition();
        }
    }

    void UpdateMascotPosition()
    {
        Vector3 nextCorner = SelectNextNavigationPointWithinDistance();
        Vector3 direction = (nextCorner - userIndicator.transform.position).normalized;
        Vector3 nextPosition = userIndicator.transform.position + direction * mascotDistanceAhead;
        agent.SetDestination(nextPosition);
        // if mascot is moving, play walk animation
        if (agent.velocity.magnitude > 0)
        {
            animator.Play("Walk");
        }
        else
        {
            animator.Play("Idle_A");
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
    private Vector3 SelectNextNavigationPointWithinDistance()
    {
        for (int i = 0; i < pathOffset.Length; i++)
        {
            float currentDistance = Vector3.Distance(userIndicator.transform.position, pathOffset[i]);
            if (currentDistance > moveOnDistance)
            {
                return pathOffset[i];
            }
        }
        return NavigationManager.Instance.targetPosition;
    }
    public void SetMascotStartingPosition()
    {
        mascotTransform.position = userIndicator.transform.position;
    }
}
