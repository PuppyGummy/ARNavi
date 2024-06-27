using UnityEngine;
using UnityEngine.AI;

public class MascotController : MonoBehaviour
{
    [SerializeField] private Transform mascotTransform;
    [SerializeField] private float mascotDistanceAhead = 2.0f;
    [SerializeField] private float moveOnDistance = 1.0f;
    [SerializeField] private float smoothingFactor = 0.1f;
    [SerializeField] private float moveTolerance = 0.1f;
    [SerializeField] private float rotationSpeed = 1.5f;
    [SerializeField] private float idelTime = 0.5f;

    private NavMeshPath path;
    private GameObject userIndicator;
    private NavMeshAgent agent;
    private Vector3[] pathOffset;
    private Animator animator;
    private Vector3 currentPosition;
    private bool setMascotAtFirstTime = false;
    private float time;

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
            if (!setMascotAtFirstTime)
            {
                AddOffsetToPath();
                Vector3 nextCorner = SelectNextNavigationPointWithinDistance();
                Vector3 direction = (nextCorner - userIndicator.transform.position).normalized;
                Vector3 nextPosition = userIndicator.transform.position + direction * mascotDistanceAhead;
                mascotTransform.position = nextPosition;
                setMascotAtFirstTime = true;
            }
            if (UserMoved())
            {
                AddOffsetToPath();
                UpdateMascotPosition();
                time = Time.time;
            }
            else
            {
                // if the user is idle for a certain time, make mascot look at the user
                if (Time.time - time > idelTime)
                {
                    // make mascot slowly look at the user
                    mascotTransform.rotation = Quaternion.Slerp(mascotTransform.rotation, Quaternion.LookRotation(userIndicator.transform.position - mascotTransform.position), rotationSpeed * Time.deltaTime);
                }
            }
        }
    }
    private bool UserMoved()
    {
        // if the user moved more than a certain distance, return true
        if (Vector3.Distance(currentPosition, userIndicator.transform.position) > moveTolerance)
        {
            currentPosition = userIndicator.transform.position;
            return true;
        }
        return false;
    }

    private void UpdateMascotPosition()
    {
        Vector3 nextCorner = SelectNextNavigationPointWithinDistance();
        // if the next corner is the target position, set the mascot's destination to the target position
        if (nextCorner == NavigationManager.Instance.targetPosition)
        {
            agent.SetDestination(nextCorner);
        }
        else
        {
            Vector3 direction = (nextCorner - userIndicator.transform.position).normalized;
            Vector3 nextPosition = userIndicator.transform.position + direction * mascotDistanceAhead;
            agent.SetDestination(nextPosition);
        }

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
