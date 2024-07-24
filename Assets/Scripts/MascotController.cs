using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;


public class MascotController : MonoBehaviour
{
    [SerializeField] private Transform mascotTransform;
    [SerializeField] private float mascotDistanceAhead = 2.0f;
    [SerializeField] private float moveOnDistance = 1.0f;
    [SerializeField] private float smoothingFactor = 0.1f;
    [SerializeField] private float moveTolerance = 0.1f;
    [SerializeField] private float rotationSpeed = 1.5f;
    [SerializeField] private float idelTime = 0.5f;
    [SerializeField] private GameObject mascotsParent;
    [SerializeField] private Button switchMascotButton;

    [SerializeField] private Button openBuddyMenu;
    [SerializeField] private Button buddyMenuSelectedBuddy;
    [SerializeField] private Button switchToSun;
    [SerializeField] private Button switchToWater;
    [SerializeField] private Button switchToLeaf;
    [SerializeField] private Button buddyOffButton;
    [SerializeField] private GameObject buddyMenu;
    [SerializeField] private Sprite leafBuddyFocus;
    [SerializeField] private Sprite waterBuddyFocus;
    [SerializeField] private Sprite sunBuddyFocus;
    [SerializeField] private Sprite buddyOff;
    [SerializeField] private Button showPOIButton;

    [SerializeField] private GameObject menuSlots;


    private List<Transform> mascots = new List<Transform>();
    private NavMeshPath path;
    private GameObject userIndicator;
    private NavMeshAgent agent;
    private Vector3[] pathOffset;
    private Animator animator;
    private Vector3 currentPosition;
    private bool setMascotAtFirstTime = false;
    private float time;
    public bool mascotActive = false;
    private int currentMascotIndex = 0;

    private bool sunOn = true;
    private bool leafOn = false;
    private bool waterOn = false;
    private bool buddyDisabled = false;
    

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
        // GameObject.DontDestroyOnLoad(this.gameObject);
    }
    void Start()
    {
        foreach (Transform child in mascotsParent.transform)
        {
            mascots.Add(child);
        }
        mascotTransform = mascots[currentMascotIndex];
        mascotTransform.gameObject.SetActive(mascotActive);
        userIndicator = NavigationManager.Instance.userIndicator;
        agent = mascotTransform.GetComponent<NavMeshAgent>();
        animator = mascotTransform.GetComponent<Animator>();
        //switchMascotButton.gameObject.SetActive(mascotActive);
        switchMascotButton.gameObject.SetActive(true);
        //set the sun as the selected mascot by default
        sunOn = true;
        leafOn = false;
        waterOn = false;
        buddyDisabled = false;
        bool _fromExplore = SearchControl.GetFromExplore();
        //if not navigating from explore mode, turn off the explore exclusive features
        if (_fromExplore == false)
        {
            openBuddyMenu.gameObject.SetActive(false);
            showPOIButton.gameObject.SetActive(false);

        }

    }

    void Update()
    {
        if (!mascotActive)
        {
            return;
        }
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
    public void ToggleMascot()
    {
        //turn the mascot off
        mascotActive = false;
        mascotTransform.gameObject.SetActive(mascotActive);
        ClearBuddySelection();
        buddyDisabled = true;
        buddyMenu.gameObject.SetActive(false);
        openBuddyMenu.gameObject.SetActive(true);
        buddyMenuSelectedBuddy.image.sprite = buddyOff;
        openBuddyMenu.image.sprite = buddyOff;
        UpdateMenuSlots();
    }
    //opens the menu to select a different mascot or toggle the mascot off
    public void OpenMascotMenu()
    {
        //disable the open mascot menu button
        openBuddyMenu.gameObject.SetActive(false);
        //enable the menu
        buddyMenu.gameObject.SetActive(true);
        //when an option is selected the menu will be disabled and replaced with the correct mascot based on the mascot index
    }
    //Close the mascot selection menu
    public void CloseMascotMenu()
    {
        openBuddyMenu.gameObject.SetActive(true);
        buddyMenu.gameObject.SetActive(false);
    }
    public void UpdateMenuSlots()
    {
        bool sunSet = false;
        bool leafSet = false;
        bool waterSet = false;
        bool offSet = false;

        foreach(Transform slot in menuSlots.transform)
        {
            Debug.Log(slot.name);
        }
        foreach (Transform slot in menuSlots.transform)
        {
         
            bool slotFilled = false;
            //clear menu slot
            foreach (Transform button in slot.transform)
            {
                GameObject.Destroy(button.gameObject);
            }
            if(!slotFilled && !sunOn && !sunSet)
            {
                slotFilled = true;
                sunSet = true;
                Button newButton = Instantiate(switchToSun, slot.transform);
                newButton.GetComponent<Button>().onClick.AddListener(() => SetBuddyToSun());

            }
            if(!slotFilled && !leafOn && !leafSet)
            {
                slotFilled = true;
                leafSet = true;
                Button newButton = Instantiate(switchToLeaf, slot.transform);
                newButton.GetComponent<Button>().onClick.AddListener(() => SetBuddyToLeaf());

            }
            if(!slotFilled && !waterOn && !waterSet)
            {
                slotFilled = true;
                waterSet = true;
                Button newButton = Instantiate(switchToWater, slot.transform);
                newButton.GetComponent<Button>().onClick.AddListener(() => SetBuddyToWater());

            }
            if(!slotFilled && !buddyDisabled && !offSet)
            {
                slotFilled = true;
                offSet = true;
                Button newButton = Instantiate(buddyOffButton, slot.transform);
                newButton.GetComponent<Button>().onClick.AddListener(() => ToggleMascot());

            }
        }
    }
    public void SetBuddyToLeaf()
    {
        mascotActive = true;
        mascotTransform.gameObject.SetActive(mascotActive);
        ClearBuddySelection();
        leafOn = true;
        //deactivate the menu and reactivate the open menu button
        buddyMenu.gameObject.SetActive(false);
        openBuddyMenu.gameObject.SetActive(true);
        //change sprite of open buddy menu to the new sprite
        openBuddyMenu.image.sprite = leafBuddyFocus;
        currentMascotIndex = 0;
        ChangeMascot();
        //change the sprite of the selected buddy in the menu to the new sprite
        buddyMenuSelectedBuddy.image.sprite = leafBuddyFocus;
        UpdateMenuSlots();
    }

    public void SetBuddyToWater()
    {
        mascotActive = true;
        mascotTransform.gameObject.SetActive(mascotActive);
        ClearBuddySelection();
        waterOn = true;
        //deactivate the menu and reactivate the open menu button
        buddyMenu.gameObject.SetActive(false);
        openBuddyMenu.gameObject.SetActive(true);
        //change sprite of open buddy menu to the new sprite
        openBuddyMenu.image.sprite = waterBuddyFocus;
        currentMascotIndex = 1;
        ChangeMascot();
        //change the sprite of the selected buddy in the menu to the new sprite
        buddyMenuSelectedBuddy.image.sprite = waterBuddyFocus;
        UpdateMenuSlots();
    }

    public void SetBuddyToSun()
    {
        mascotActive = true;
        mascotTransform.gameObject.SetActive(mascotActive);
        ClearBuddySelection();
        sunOn = true;
        //deactivate the menu and reactivate the open menu button
        buddyMenu.gameObject.SetActive(false);
        openBuddyMenu.gameObject.SetActive(true);
        //change sprite of open buddy menu to the new sprite
        openBuddyMenu.image.sprite = sunBuddyFocus;
        currentMascotIndex = 0;
        ChangeMascot();
        //change the sprite of the selected buddy in the menu to the new sprite
        buddyMenuSelectedBuddy.image.sprite = sunBuddyFocus;
        UpdateMenuSlots();
    }

    public void ChangeMascot()
    {
        //instead have each of the mascot buttons change the current mascot index to the appropriate value and run the rest of the code as usual
        mascotTransform.gameObject.SetActive(false);
        mascotTransform = mascots[currentMascotIndex];
        mascotTransform.gameObject.SetActive(true);
        setMascotAtFirstTime = false;
        agent = mascotTransform.GetComponent<NavMeshAgent>();
        animator = mascotTransform.GetComponent<Animator>();
    }

    private void ClearBuddySelection()
    {
        sunOn = false;
        leafOn = false;
        waterOn = false;
        buddyDisabled = false;
    }
}
