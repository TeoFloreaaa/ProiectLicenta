using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class PlayerInfo : MonoBehaviour
{
    [SerializeField] TMP_Text playerNameText;
    [SerializeField] TMP_Text playerCashText;
    [SerializeField] GameObject activePlayerArrow;

    public void SetPlayerName(string newName)
    {
        playerNameText.color = Color.white;
        playerNameText.text = newName;  
    }

    public void SetPlayerCash(int currentCash)
    {
        playerCashText.color = Color.white;
        playerCashText.text = "$ " + currentCash;
    }

    public void SetPlayerNameAndCash(string newName, int currentCash)
    {
        SetPlayerName(newName);
        SetPlayerCash(currentCash);
    }

    public void ActivateArrow(bool active)
    {
        activePlayerArrow.SetActive(active);
    }

}
