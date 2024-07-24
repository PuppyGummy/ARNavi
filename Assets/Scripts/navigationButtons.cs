using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NavigationButtons : MonoBehaviour
{
    public enum buildKeys
    {
        Welcome,
        DefaultMainMenu,
        Navigation,
        SelectLangauge,
        Settings,
        ConfirmDestination,
        ExploreMainMenu,
        Information,
        Acknowledgements
    };
    public static void ToMainMenu()
    {
        SceneManager.LoadScene((int)buildKeys.DefaultMainMenu);
    }

    public static void ToExplore()
    {
        SceneManager.LoadScene((int)buildKeys.ExploreMainMenu);
    }

    //This function is used to navigate to the appropriate main menu screen (explore vs default) after navigating away
    public static void BackToMenu()
    {
        //if the previous main menu was the explore, navigate back to the explore
        if (SearchControl.GetFromExplore() == true)
        {
            ToExplore();
        }
        //otherwise, navigate back to the default
        else
        {
            ToMainMenu();
        }
    }
    //go to language selection screen - need to keep track of current user selection if navigating from settings
    public static void ToSelectLanguage()
    {
        SceneManager.LoadScene((int)buildKeys.SelectLangauge);
    }


    //go to settings screen
    public static void ToSettings()
    {
        SceneManager.LoadScene((int)buildKeys.Settings);
    }


    //go to ar navigation screen when search destination is confirmed
    //check that a location has been selected  
    public static void ToNavigation()
    {
        SceneManager.LoadScene((int)buildKeys.Navigation);
    }

    public static void ToConfirmDestination()
    {
        SceneManager.LoadScene((int)buildKeys.ConfirmDestination);
    }

    public static void ToInfo()
    {
        PlayerPrefs.SetInt("previousScene", SceneManager.GetActiveScene().buildIndex);
        SceneManager.LoadScene((int)buildKeys.Information);
    }

    public static void ToLastScene()
    {
        SceneManager.LoadScene(PlayerPrefs.GetInt("previousScene"));
    }

}
