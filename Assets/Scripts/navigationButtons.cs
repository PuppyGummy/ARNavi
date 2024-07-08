using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NavigationButtons: MonoBehaviour
{


    /*
    should probably be moved to a global enum list, makes stuff easier but need to double check build settings
    Build index key:
    0: Welcome
        should only load if it is the user's first time on the app, no need for navigation button to this screen
    1: Main Menu
    2: Navigation
    3: Select Language
    4: Settings
        not sure if this is needed, currently the only setting is language, but if we are adding acessibility features the settings page may
        be a good place for those
    5: Confirm Destinations
    */



    
    /*
    TODO:
    Implement actual language selection (including framework for adding langauages?)
        maybe not in this file just to keep this clean
    For now, just a button that navigates to main menu
    Merge all navigation into one function, use button variables to add input, use int input to control where the scene transitions to
    */
    public static void ToMainMenu() {
        SceneManager.LoadScene(1);
    }



    //go to language selection screen - need to keep track of current user selection if navigating from settings
    public static void ToSelectLanguage() {
        SceneManager.LoadScene(3);
    }


    //go to settings screen
    public static void ToSettings() {
        SceneManager.LoadScene(3);
    }


    //go to ar navigation screen when search destination is confirmed
    //check that a location has been selected  
    public static void ToNavigation() {
        SceneManager.LoadScene(2);
    }

    public static void ToConfirmDestination()
    {
        SceneManager.LoadScene(5);
    }


}
