using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Keep : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        this.gameObject.SetActive( false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
