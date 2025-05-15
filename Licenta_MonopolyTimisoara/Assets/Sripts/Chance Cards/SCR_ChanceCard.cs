using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Chance Card", menuName = "Monopoly/Cards/Chance")]
public class SCR_ChanceCard : ScriptableObject
{
    public string textOnCard; // Description
    public int rewardMoney;   // GET MONEY
    public int penalityMoney; // PAY MONEY
    public int moveToBoardIndex = -1;
    public bool payToPlayer;

    [Header("MoveToLocations")]
    public bool nextRailroad;
    public bool nextUtility;
    public int moveStepsBackwards;

    [Header("Jail content")]
    public bool goToJail;
    public bool jailFreeCard;
    [Header("Repair streets")]
    public bool streetRepairs;
    public int streetRepairsHousePrice = 25;
    public int streetRepairsHotelPrice = 100;
}
