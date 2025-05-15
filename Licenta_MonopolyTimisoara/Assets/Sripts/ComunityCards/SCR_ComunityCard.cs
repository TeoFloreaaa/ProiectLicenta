using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Community Card", menuName = "Monopoly/Cards/Community")]
public class SCR_CommunityCard : ScriptableObject
{
    public string textOnCard; // Description
    public int rewardMoney;   // GET MONEY
    public int penalityMoney; // PAY MONEY
    public int moveToBoardIndex = -1;
    public bool collectFromPlayer;
    [Header("Jail content")]
    public bool goToJail;
    public bool jailFreeCard;
    [Header("Repair streets")]
    public bool streetRepairs;
    public int streetRepairsHousePrice;
    public int streetRepairsHotelPrice;
}
