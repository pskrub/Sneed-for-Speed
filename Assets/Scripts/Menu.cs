using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void onClickStartGameButton(){
        SceneManager.LoadScene(1);
    }

    public void OnClickQuitButton() {
        Debug.Log("swag");
        Application.Quit();
    }   

}
