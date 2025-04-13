using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SetPlayerPrefs : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI score_Red;
    [SerializeField] TextMeshProUGUI score_Blue;
    void Start()
    {
        score_Red.text = PlayerPrefs.GetInt("Player1 Spring TEST").ToString();
        score_Blue.text = PlayerPrefs.GetInt("Player2 Spring TEST").ToString();
    }
}
