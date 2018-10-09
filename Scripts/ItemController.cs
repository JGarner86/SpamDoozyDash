using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ItemController : MonoBehaviour {

    public static int Count;
    [SerializeField] ParticleSystem particle;
    
    
    Renderer spamRend;

    private void Start()
    {
        particle = transform.parent.GetComponentInChildren<ParticleSystem>();
        spamRend = GetComponent<Renderer>();
    }

    public void GotItem()
    {
        Count++;
        //print($"Spam: {Count.ToString()}");
        GameManager.SpamText.text = $"{Count}/90";
        spamRend.enabled = false;
        particle.Play();
        StartCoroutine(WaitForParticle());
    }

    IEnumerator WaitForParticle()
    {
        while (particle.isEmitting)
        {
            yield return null;
        }
        Destroy(transform.parent.parent.gameObject);
    }
   
}
