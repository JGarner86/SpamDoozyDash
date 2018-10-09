using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour {

    [SerializeField] TMP_Text spamCountText;
    bool startMenu = true;
    bool endGame = false;
    public static TMP_Text SpamText;
    // Use this for initialization
	void Start () {
        SpamText = spamCountText;
        StartCoroutine(CheckForStartGame());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    IEnumerator CheckForStartGame()
    {
        while (!Input.anyKeyDown)
        {
            yield return null;
        }
        if (startMenu)
        {
            startMenu = false;
        }
        PlayerCarController.startCar = true;
    }

    

}
