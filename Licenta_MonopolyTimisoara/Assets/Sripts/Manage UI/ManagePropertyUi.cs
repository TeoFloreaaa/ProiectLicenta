using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;


public class ManagePropertyUi : MonoBehaviour
{
    [SerializeField] Transform cardHolder; //HORIZONTAL LAYOUT
    [SerializeField] GameObject cardPrefab;
    [SerializeField] Button buyHouseButton, sellHouseButton;
    [SerializeField] TMP_Text buyHousePriceText, sellHousePriceText;
    Player playerReference;
    List<MonopolyNode> nodesInSet = new List<MonopolyNode>();
    List<GameObject> cardsInSet = new List<GameObject>();

    //THIS PROPERTY IS ONLY FOR 1 SPECIFIC CARD SET
    public void SetProperty(List<MonopolyNode> nodes, Player owner)
    {
        playerReference = owner;
        nodesInSet.AddRange(nodes);
        for (int i = 0; i < nodesInSet.Count; i++)
        {
            GameObject newCard = Instantiate(cardPrefab, cardHolder, false);
            ManageCardUI manageCardUi = newCard.GetComponent<ManageCardUI>();
            cardsInSet.Add(newCard);
            manageCardUi.SetCard(nodesInSet[i], owner, this);
        }
        var (list, allsame) = MonopolyBoard.Instance.PlayerHasAllNodesOfSet(nodesInSet[0]);
        buyHouseButton.interactable = allsame && CheckIfBuyAllowed();
        sellHouseButton.interactable = CheckIfSellAllowed(); ;

        buyHousePriceText.text = "<color=red>-$</color>" + nodesInSet[0].houseCost;
        sellHousePriceText.text = "<color=green>+$</color>" + nodesInSet[0].houseCost;
    }

    public void BuyHouseButton()
    {
        if(!CheckIfBuyAllowed())
        {
            //ERROR MESSAGE
            string message = "O propietate sau mai multe sunt ipotecate, nu poti sa construiesti case!";
            ManageUI.instance.UpdateSystemMessage(message);

            return;
        }
        if (playerReference.CanAffordHouse(nodesInSet[0].houseCost))
        {
            playerReference.BuildHouseOrHotelEvenly(nodesInSet);
            //UPDATE MONEY TEXT - IN MANAGE UI
            string message = "Ai construit o casa!";
            ManageUI.instance.UpdateSystemMessage(message);
            UpdateHouseVisuals();
        }
        else
        {
            string message = "Fonduri insuficiente!";
            ManageUI.instance.UpdateSystemMessage(message);
            //CANT AFFORD HOUSE - SYSTEM MESSAGE FOR THE PLAYER
        }

        sellHouseButton.interactable = CheckIfSellAllowed();
        ManageUI.instance.UpdateMoneyText();
    }

    public void SellHouseButton()
    {
        //MAYBE CHECK IF THERE IS AT LEAST 1 HOUSE TO SELL
        playerReference.SellHouseEvenly(nodesInSet);
        //UPDATE MONEY TEXT - IN MANAGE UI
        UpdateHouseVisuals();
        sellHouseButton.interactable = CheckIfSellAllowed();
        ManageUI.instance.UpdateMoneyText();
    }

    public bool CheckIfSellAllowed()
    {
        if (nodesInSet.Any(n => n.NumberOfHouses > 0))
        {
            return true;
        }
        return false;
    }

    bool CheckIfBuyAllowed()
    {
        if (nodesInSet.Any(n => n.IsMortgaged == true))
        {
            return false;
        }

        return true;
    }

    public bool CheckIfMortgageAllowed()
    {
        if (nodesInSet.Any(n => n.NumberOfHouses > 0))
        {
            return false;
        }
        return true;
    }

    void UpdateHouseVisuals()
    {
        foreach (var card in cardsInSet)
        {
            card.GetComponent<ManageCardUI>().ShowBuildings();
        }
    }


}
