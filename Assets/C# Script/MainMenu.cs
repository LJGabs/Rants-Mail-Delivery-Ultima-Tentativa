using UnityEngine;
using UnityEngine.SceneManagement;

public enum Menu
{
    MainMenu,
    Options, 
    Shop,
}

public class MainMenu : MonoBehaviour
{
    public Menu currentMenu = Menu.MainMenu;

    public GameObject mainMenu, options, shop;

    private void Update()
    {
        switch (currentMenu)
        {
            case Menu.MainMenu:
                mainMenu.SetActive(true);
                options.SetActive(false);
                shop.SetActive(false);
                break;
            case Menu.Options:
                mainMenu.SetActive(false);
                options.SetActive(true);
                shop.SetActive(false);
                break;
            case Menu.Shop:
                mainMenu.SetActive(false);
                options.SetActive(false);
                shop.SetActive(true);
                break;
        }
    }

    public void MenuSelector(string Menu)
    {
        if (Menu == "MainMenu")
        {
            currentMenu = global::Menu.MainMenu;
        }
        if (Menu == "Options")
        {
            currentMenu = global::Menu.Options;
        }
        if (Menu == "Shop")
        {
            currentMenu = global::Menu.Shop;
        }
        if (Menu == "Play")
        {
            SceneManager.LoadScene("Gameplay");
        }
        if (Menu == "Quit")
        {
            Application.Quit();
        }
    }
}

