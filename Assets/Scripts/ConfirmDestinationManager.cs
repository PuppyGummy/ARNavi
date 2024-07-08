using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ConfirmDestinationManager : MonoBehaviour
{
    [SerializeField] private TMP_Text title;
    // Start is called before the first frame update
    void Start()
    {
        Target currentDestination = SearchControl.GetCurrentTarget();
        title.SetText(currentDestination.targetName);
        
    }


}
