using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmDestinationManager : MonoBehaviour
{
    [SerializeField] private TMP_Text title;
    [SerializeField] private Image siteImage;
    [SerializeField] private TMP_Text description;
    // Start is called before the first frame update
    void Start()
    {
        Target currentDestination = SearchControl.GetCurrentTarget();
        //title.SetText(currentDestination.targetName);
        if (currentDestination == null)
        {
            title.SetText("Not Found");
            siteImage.sprite = SaveLoadManager.LoadSearchImage("Project Room 2-0");
            description.SetText("Not Found");
        }
        else
        {
            title.SetText(currentDestination.targetName);
            description.SetText(currentDestination.addressInfo);
            siteImage.sprite = SaveLoadManager.LoadSearchImage(currentDestination.imgPath);
        }
    }
}
