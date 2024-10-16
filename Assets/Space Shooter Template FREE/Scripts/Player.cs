using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// This script defines which sprite the 'Player" uses and its health.
/// </summary>

public class Player : MonoBehaviour
{
    public GameObject destructionFX;
    public FirebaseAuth_manager httpsManager;

    public static Player instance; 

    private void Awake()
    {
        if (instance == null) 
            instance = this;
    }

    private void Start()
    {
        httpsManager = FindFirstObjectByType<FirebaseAuth_manager>();
    }

    //method for damage proceccing by 'Player'
    public void GetDamage(int damage)   
    {
         StartCoroutine(Destruction());
    }    

    //'Player's' destruction procedure
    private IEnumerator Destruction()
    {
        httpsManager.Event_SendData();

        Instantiate(destructionFX, transform.position, Quaternion.identity); //generating destruction visual effect and destroying the 'Player' object
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        gameObject.GetComponent<PlayerShooting>().enabled = false;

        yield return new WaitForSeconds(2);

        SceneManager.LoadScene(0);
    }
}
















