using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ManageCardUI : MonoBehaviour
{
    [SerializeField] Image colorField;
    [SerializeField] GameObject[] buildings;
    [SerializeField] GameObject mortgageImage;
    [SerializeField] TMP_Text mortgageValueText;
    [SerializeField] Button mortgageButton, unMortgageButton;
    [SerializeField] TMP_Text propertyNameText;

    [SerializeField] Image iconImage;
    [SerializeField] Sprite houseSprite, railroadSprite, utilitySprite;

    Player playerReference;
    MonopolyNode nodeReference;
    ManagePropertyUi propertyReference;

    public void SetCard(MonopolyNode node, Player owner, ManagePropertyUi propertySet)
    {
        nodeReference = node;
        playerReference = owner;
        propertyReference = propertySet;
        //SET COLOR
        if (node.propertyColorField != null)
        {
            colorField.color = node.propertyColorField.color;
        }
        else
        {
            colorField.color = Color.black;
        }


        //SHOW BUILDINGS
        ShowBuildings();

        //SHOW MORTGAGE IMAGE
        mortgageImage.SetActive(node.IsMortgaged);
        //TEXT UPDATE
        mortgageValueText.text = "Valoarea ipotecii: $ " + node.MortgageValue;

        mortgageButton.interactable = !node.IsMortgaged;
        unMortgageButton.interactable = node.IsMortgaged;
        ManageUI.instance.UpdateMoneyText();

        //SET ICON
        switch (nodeReference.monopolyNodeType)
        {
            case MonopolyNodeType.Property:
                iconImage.sprite = houseSprite;
                iconImage.color = Color.blue;
                break;

            case MonopolyNodeType.Railroad:
                iconImage.sprite = railroadSprite;
                iconImage.color = Color.white;
                break;

            case MonopolyNodeType.Utility:
                iconImage.sprite = utilitySprite;
                iconImage.color = Color.black;
                break;
        }

        //SET PROPERTY NAME
        propertyNameText.text = nodeReference.name;
    }

    public void MortgageButton()
    {
        if(!propertyReference.CheckIfMortgageAllowed())
        {
            //ERROR
            string message = "Ai case pe unele terenuri, nu poti sa ipotechezi aceasta propietate acum!";
            ManageUI.instance.UpdateSystemMessage(message);
            return; 
        }
        if (nodeReference.IsMortgaged)
        {
            //ERROR MESSAGE OR SUCH
            string message = "Aceasta propietate e deja ipotecata!";
            ManageUI.instance.UpdateSystemMessage(message);
            return;
        }
        playerReference.CollectMoney(nodeReference.MortgageProperty());
        mortgageImage.SetActive(true);
        mortgageButton.interactable = false;
        unMortgageButton.interactable = true;
        ManageUI.instance.UpdateMoneyText();
    }

    public void UnMortgageButton()
    {
        if (!nodeReference.IsMortgaged)
        {
            //ERROR MESSAGE OR SUCH
            string message = "Ipoteca a fost deja ridicata!";
            ManageUI.instance.UpdateSystemMessage(message);
            return;
        }
        if (playerReference.ReadMoney < nodeReference.MortgageValue)
        {
            //ERROR MESSAGE OR SUCH
            string message = "Fonduri insuficiente! Nu poti sa ridici ipoteca!";
            ManageUI.instance.UpdateSystemMessage(message);
            return;
        }
        playerReference.PayMoney(nodeReference.MortgageValue);
        nodeReference.UnMortgageProperty();
        mortgageImage.SetActive(false);
        mortgageButton.interactable = true;
        unMortgageButton.interactable = false;
        ManageUI.instance.UpdateMoneyText();
    }

    public void ShowBuildings()
    {
        foreach(var icon in buildings)
        {
            icon.SetActive(false);
        }
        //SHOW BUILDINGS
        if (nodeReference.NumberOfHouses < 5)
        {
            for (int i = 0; i < nodeReference.NumberOfHouses; i++)
            {
                buildings[i].SetActive(true);
            }
        }
        else
        {
            buildings[4].SetActive(true);
        }
    }



}
