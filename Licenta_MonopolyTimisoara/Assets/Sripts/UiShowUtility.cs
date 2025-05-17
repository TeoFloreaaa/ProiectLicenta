using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using static MonopolyNode;

public class UiShowUtility : MonoBehaviour
{
    MonopolyNode nodeReference;
    Player playerReference;

    [Header("Buy Utility UI")]
    [SerializeField] GameObject utilityUiPanel;
    [SerializeField] TMP_Text utilityNameText;
    [Space]
    [SerializeField] TMP_Text mortgagePriceText;
    [Space]
    [SerializeField] Button buyUtilityButton;
    [Space]
    [SerializeField] TMP_Text utilityPriceText;
    [SerializeField] TMP_Text playerMoneyText;

    void OnEnable()
    {
        MonopolyNode.OnShowUtilityBuyPanel += ShowUtilityBuyPanel;
    }

    void OnDisable()
    {
        MonopolyNode.OnShowUtilityBuyPanel -= ShowUtilityBuyPanel;
    }


    private void Start()
    {
        utilityUiPanel.SetActive(false);
    }

    void ShowUtilityBuyPanel(MonopolyNode node, Player currentPlayer)
    {
        nodeReference = node;
        playerReference = currentPlayer;
        //TOP PANEL CONTENT
        utilityNameText.text = node.name;
        //colorField.color = node.propertyColorField.color;

        //CENTER OF THE CARD
      
        //COST OF BUILDINGS
        mortgagePriceText.text = "$ " + node.MortgageValue;

        //BOTTOM BAR
        utilityPriceText.text = "Pret: $ " + node.price;
        playerMoneyText.text = "Banii tai: $ " + currentPlayer.ReadMoney;

        //Buy Property Button
        if (currentPlayer.CanAffordNode(node.price))
        {
            buyUtilityButton.interactable = true;
        }
        else
        {
            buyUtilityButton.interactable = false;
        }

        utilityUiPanel.SetActive(true);
    }

    public void BuyUtilityButton() // THIS IS CALLED FROM THE BUY BUTTON
    {
        //TELL THE PLAYER TO BUY THIS PROPERTY
        playerReference.BuyProperty(nodeReference);

        //MAYBE CLOSE THE PROPERTY CARD OR

        //MAKE THE BUTTON NOT INTERACTABLE ANYMORE
        buyUtilityButton.interactable = false;
    }

    public void CloseUtilityButton() // THIS IS CALLED FROM THE BUY BUTTON
    {
        //CLOSE THE PANEL
        utilityUiPanel.SetActive(false);

        //CLEAR NODEREFERENCE
        nodeReference = null;
        playerReference = null;
    }

}
