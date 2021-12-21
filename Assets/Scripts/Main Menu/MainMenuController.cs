using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject MainMenuCanvas;
    [SerializeField] private GameObject CreateWorldCanvas;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateWorld()
    {
        MainMenuCanvas.SetActive(false);
        CreateWorldCanvas.SetActive(true);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
