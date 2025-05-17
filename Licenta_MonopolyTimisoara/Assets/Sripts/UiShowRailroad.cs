using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UiShowRailroad : MonoBehaviour
{
    MonopolyNode nodeReference;
    Player playerReference;

    [Header("Buy Railroad UI")]
    [SerializeField] GameObject railroadUiPanel;
    [SerializeField] TMP_Text railroadNameText;
    //[SerializeField] Image colorField;
    [Space]
    [SerializeField] TMP_Text oneRailroadRentText;
    [SerializeField] TMP_Text twoRailroadRentText;
    [SerializeField] TMP_Text threeRailroadRentText;
    [SerializeField] TMP_Text fourRailroadRentText;
    [Space]
    [SerializeField] TMP_Text mortgagePriceText;
    [Space]
    [SerializeField] Button buyRailroadButton;
    [Space]
    [SerializeField] TMP_Text propertyPriceText;
    [SerializeField] TMP_Text playerMoneyText;

    void OnEnable()
    {
        MonopolyNode.OnShowRailroadBuyPanel += ShowRailroadBuyPanel;
    }

    void OnDisable()
    {
        MonopolyNode.OnShowRailroadBuyPanel -= ShowRailroadBuyPanel;
    }


    private void Start()
    {
        railroadUiPanel.SetActive(false);
    }

    void ShowRailroadBuyPanel(MonopolyNode node, Player currentPlayer)
    {
        nodeReference = node;
        playerReference = currentPlayer;
        //TOP PANEL CONTENT
        railroadNameText.text = node.name;
        //colorField.color = node.propertyColorField.color;

        //CENTER OF THE CARD
        oneRailroadRentText.text = "$ " + node.baseRent * (int)Mathf.Pow(2, 0);
        twoRailroadRentText.text = "$ " + node.baseRent * (int)Mathf.Pow(2, 1);
        threeRailroadRentText.text = "$ " + node.baseRent * (int)Mathf.Pow(2, 2);
        fourRailroadRentText.text = "$ " + node.baseRent * (int)Mathf.Pow(2, 3);


        //COST OF BUILDINGS
        mortgagePriceText.text = "$ " + node.MortgageValue;

        //BOTTOM BAR
        propertyPriceText.text = "Pret: $ " + node.price;
        playerMoneyText.text = "Banii tai: $ " + currentPlayer.ReadMoney;

        //Buy Property Button
        if (currentPlayer.CanAffordNode(node.price))
        {
            buyRailroadButton.interactable = true;
        }
        else
        {
            buyRailroadButton.interactable = false;
        }

        railroadUiPanel.SetActive(true);
    }

    public void BuyRailroadButton() // THIS IS CALLED FROM THE BUY BUTTON
    {
        //TELL THE PLAYER TO BUY THIS PROPERTY
        playerReference.BuyProperty(nodeReference);

        //MAYBE CLOSE THE PROPERTY CARD OR

        //MAKE THE BUTTON NOT INTERACTABLE ANYMORE
        buyRailroadButton.interactable = false;
    }

    public void CloseRailroadButton() // THIS IS CALLED FROM THE BUY BUTTON
    {
        //CLOSE THE PANEL
        railroadUiPanel.SetActive(false);

        //CLEAR NODEREFERENCE
        nodeReference = null;
        playerReference = null;
    }

}
