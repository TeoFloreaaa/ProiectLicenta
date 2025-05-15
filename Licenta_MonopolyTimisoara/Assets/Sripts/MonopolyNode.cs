using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public enum MonopolyNodeType
{
    Property,
    Utility,
    Railroad,
    Tax,
    Chance,
    CommunityChest,
    Go,
    Jail,
    FreeParking,
    GoToJail
}

public class MonopolyNode : MonoBehaviour
{
    public MonopolyNodeType monopolyNodeType;

    public Image propertyColorField;

    [Header("Name")]
    [SerializeField] internal new string name;
    [SerializeField] TMP_Text nameText;

    [Header("Property Price")]
    public int price;
    [SerializeField] TMP_Text priceText;

    [Header("Property Rent")]
    [SerializeField] bool calculateRentAuto;
    [SerializeField] int currentRent;
    [SerializeField] internal int baseRent;
    [SerializeField] internal List<int> rentWithHouses = new List<int>();
    int numberOfHouses;
    public int NumberOfHouses => numberOfHouses;

    [Header("Property Mortgage")]
    [SerializeField] GameObject mortgageImage;
    [SerializeField] GameObject propertyImage;
    [SerializeField] bool isMortgaged;
    [SerializeField] int mortgageValue;

    [Header("Property Owner")]
    [SerializeField] GameObject ownerBar;
    [SerializeField] TMP_Text ownerText;
    Player owner;
    public Player Owner => owner;

    //MESSAGE SYSTEM
    public delegate void UpdateMessage(string message);
    public static UpdateMessage OnUpdateMessage;

    //DRAG A COMMUNITY CARD
    public delegate void DrawCommunityCard(Player player);
    public static DrawCommunityCard OnDrawCommunityCard;

    //DRAG A CHANCE CARD
    public delegate void DrawChanceCard(Player player);
    public static DrawChanceCard OnDrawChanceCard;


    public void SetOwner(Player newOwner)
    {
        owner = newOwner;
    }

    private void OnValidate()
    {
        if (nameText != null)
        {
            nameText.text = name;
        }

        // CALCULATION
        if (calculateRentAuto)
        {
            if (monopolyNodeType == MonopolyNodeType.Property)
            {
                if (baseRent > 0)
                {
                    price = 3 * (baseRent * 10);

                    // MORTGAGE PRICE
                    mortgageValue = price / 2;

                    rentWithHouses.Clear();
                    rentWithHouses.Add(baseRent * 5);
                    rentWithHouses.Add(baseRent * 5 * 3);
                    rentWithHouses.Add(baseRent * 5 * 9);
                    rentWithHouses.Add(baseRent * 5 * 16);
                    rentWithHouses.Add(baseRent * 5 * 25);
                }
                else if (baseRent <= 0)
                {
                    price = 0;
                    baseRent = 0;
                    rentWithHouses.Clear();
                    mortgageValue = 0;
                }
            }
            if (monopolyNodeType == MonopolyNodeType.Utility)
            {
                mortgageValue = price / 2;
            }
            if (monopolyNodeType == MonopolyNodeType.Railroad)
            {
                mortgageValue = price / 2;
            }
        }

        if (priceText != null)
        {
            priceText.text = "$ " + price;
        }

        //UPDATE THE OWNER
        OnOwnerUpdated();
        UnMortgageProperty();       
    }

    public void UpdateColorField(Color color)
    {
        color.a = 1f;
        if(propertyColorField != null)
        { 
            propertyColorField.color = color;        
        }
        
    }


    // MORTGAGE CONTENT
    public int MortgageProperty()
    {
        isMortgaged = true;
        if (mortgageImage != null)
        {
            mortgageImage.SetActive(true);
        }
        if (propertyImage != null)
        {
            propertyImage.SetActive(false);
        }
        return mortgageValue;
    }

    public void UnMortgageProperty()
    {
        isMortgaged = false;
        if (mortgageImage != null)
        {
            mortgageImage.SetActive(false);
        }
        if (propertyImage != null)
        { 
        propertyImage.SetActive(true);
        }
    }

    public bool IsMortgaged => isMortgaged;
    public int MortgageValue => mortgageValue;

    // UPDATE OWNER
    public void OnOwnerUpdated()
    {
        if (ownerBar != null)
        {
            if (owner != null)
            {
                ownerBar.SetActive(true);
                ownerText.text = owner.name;
            }
            else
            {
                ownerBar.SetActive(false);
                ownerText.text = "";
            }
        }
    }

    public void PlayerLandedOnNode(Player currentPlayer)
    {
        bool playerIsHuman = currentPlayer.playerType == Player.PlayerType.HUMAN;
        bool continueTurn = true;

        // CHECK FOR NODE TYPE AND ACT
        switch (monopolyNodeType)
        {
            case MonopolyNodeType.Property:
                if (!playerIsHuman) // AI
                {
                    // IF IT OWNED && IF WE NOT ARE OWNER && IS NOT MORTGAGED
                    if (owner != null && owner != currentPlayer && !isMortgaged)
                    {
                        // PAY RENT TO SOMEBODY

                        // CALCULATE THE RENT
                        //Debug.Log("PLAYER MIGHT PAY RENT && OWNER IS: " + owner.name);

                        int rentToPay = CalculatePropertyRent();

                        // PAY THE RENT TO THE OWNER
                        currentPlayer.PayRent(rentToPay, owner);

                        // SHOW A MESSAGE ABOUT WHAT HAPPENED
                        OnUpdateMessage.Invoke(currentPlayer.name + " trebuie sa-i plateasca: " + rentToPay + " lui " + owner.name);

                    }
                    else if (owner == null && currentPlayer.CanAffordNode(price))
                    {
                        // BUY THE NODE
                        OnUpdateMessage.Invoke(currentPlayer.name + " a cumparat" + this.name);
                        currentPlayer.BuyProperty(this);
                        OnOwnerUpdated();

                        // SHOW A MESSAGE ABOUT WHAT HAPPENED
                    }
                    else
                    {
                        //IS UNOWNED AND WE CANT AFFORD IT
                    }
                }
                else // HUMAN
                {
                    // IF IT OWNED && IF WE NOT ARE OWNER && IS NOT MORTGAGED
                    if (owner != null && owner != currentPlayer && !isMortgaged)
                    {
                        // PAY RENT TO SOMEBODY

                        // CALCULATE THE RENT

                        // PAY THE RENT TO THE OWNER

                        // SHOW A MESSAGE ABOUT WHAT HAPPENED
                    }
                    else if (owner == null /* && IF CAN AFFORD */)
                    {
                        //SHOW BUY INTERFACE

                        // SHOW A MESSAGE ABOUT WHAT HAPPENED
                    }
                    else
                    {
                        //IS UNOWNED AND WE CANT AFFORD IT
                    }
                }
                break;
            case MonopolyNodeType.Utility:
                if (!playerIsHuman) // AI
                {
                    // IF IT OWNED && IF WE NOT ARE OWNER && IS NOT MORTGAGED
                    if (owner != null && owner != currentPlayer && !isMortgaged)
                    {
                        // PAY RENT TO SOMEBODY

                        // CALCULATE THE RENT
                        

                        int rentToPay = CalculateUtilityRent();
                        currentRent = rentToPay;
                        // PAY THE RENT TO THE OWNER
                        currentPlayer.PayRent(rentToPay, owner);

                        // SHOW A MESSAGE ABOUT WHAT HAPPENED
                        OnUpdateMessage.Invoke(currentPlayer.name + " trebuie sa-i plateasca: " + rentToPay + " lui " + owner.name);

                    }
                    else if (owner == null && currentPlayer.CanAffordNode(price))
                    {
                        // BUY THE NODE
                        OnUpdateMessage.Invoke(currentPlayer.name + " a cumparat" + this.name);
                        currentPlayer.BuyProperty(this);
                        OnOwnerUpdated();

                        // SHOW A MESSAGE ABOUT WHAT HAPPENED
                    }
                    else
                    {
                        //IS UNOWNED AND WE CANT AFFORD IT
                    }
                }
                else // HUMAN
                {
                    // IF IT OWNED && IF WE NOT ARE OWNER && IS NOT MORTGAGED
                    if (owner != null && owner != currentPlayer && !isMortgaged)
                    {
                        // PAY RENT TO SOMEBODY

                        // CALCULATE THE RENT

                        // PAY THE RENT TO THE OWNER

                        // SHOW A MESSAGE ABOUT WHAT HAPPENED
                    }
                    else if (owner == null /* && IF CAN AFFORD */)
                    {
                        //SHOW BUY INTERFACE

                        // SHOW A MESSAGE ABOUT WHAT HAPPENED
                    }
                    else
                    {
                        //IS UNOWNED AND WE CANT AFFORD IT
                    }
                }
                break;
            case MonopolyNodeType.Railroad:
                if (!playerIsHuman) // AI
                {
                    // IF IT OWNED && IF WE NOT ARE OWNER && IS NOT MORTGAGED
                    if (owner != null && owner != currentPlayer && !isMortgaged)
                    {
                        // PAY RENT TO SOMEBODY

                        // CALCULATE THE RENT
                        
                        int rentToPay = CalculateRailroadRent();
                        currentRent = rentToPay;
                        // PAY THE RENT TO THE OWNER
                        currentPlayer.PayRent(rentToPay, owner);

                        // SHOW A MESSAGE ABOUT WHAT HAPPENED
                        OnUpdateMessage.Invoke(currentPlayer.name + " trebuie sa-i plateasca: " + rentToPay + " lui " + owner.name);

                    }
                    else if (owner == null && currentPlayer.CanAffordNode(price))
                    {
                        // BUY THE NODE
                        OnUpdateMessage.Invoke(currentPlayer.name + " a cumparat" + this.name);
                        currentPlayer.BuyProperty(this);
                        OnOwnerUpdated();
                        // SHOW A MESSAGE ABOUT WHAT HAPPENED
                    }
                    else
                    {
                        //IS UNOWNED AND WE CANT AFFORD IT
                    }
                }
                else // HUMAN
                {
                    // IF IT OWNED && IF WE NOT ARE OWNER && IS NOT MORTGAGED
                    if (owner != null && owner != currentPlayer && !isMortgaged)
                    {
                        // PAY RENT TO SOMEBODY

                        // CALCULATE THE RENT

                        // PAY THE RENT TO THE OWNER

                        // SHOW A MESSAGE ABOUT WHAT HAPPENED
                    }
                    else if (owner == null /* && IF CAN AFFORD */)
                    {
                        //SHOW BUY INTERFACE

                        // SHOW A MESSAGE ABOUT WHAT HAPPENED
                    }
                    else
                    {
                        //IS UNOWNED AND WE CANT AFFORD IT
                    }
                }
                break;
            case MonopolyNodeType.Tax:
                GameManager.instance.AddTaxToPool(price);
                currentPlayer.PayMoney(price);
                //SHOW A MASSAGE
                OnUpdateMessage.Invoke(currentPlayer.name + " trebuie sa plateasca " + price + " taxa");
                break;
            case MonopolyNodeType.FreeParking:
                int tax = GameManager.instance.GetTaxPool();
                currentPlayer.CollectMoney(tax);
                //SHOW A MASSAGE
                OnUpdateMessage.Invoke(currentPlayer.name + " <color=green>a colectat </color>" + tax + " din taxe");
                break;
            case MonopolyNodeType.GoToJail:
                currentPlayer.GoToJail(30);
                OnUpdateMessage.Invoke(currentPlayer.name + " <color=rad>trebuie sa mearga la inchisoare</color>");
                continueTurn = false;
                break;
            case MonopolyNodeType.Chance:
                break;
            case MonopolyNodeType.CommunityChest:
                OnDrawCommunityCard.Invoke(currentPlayer);
                continueTurn = false;
                break;
        }
        //STOP HERE IF NEEDED
        if(!continueTurn)
        {
            return;
        }

        if (!playerIsHuman)
        {
            Invoke("ContinueGame", GameManager.instance.secondsBetweenTurns);
        }
        else
        {
            // SHOW UI
        }
    }

    void ContinueGame()
    {
        // IF THE LAST ROLL WAS A DOUBLE
        if (GameManager.instance.RolledADouble)
        {
            // ROLL AGAIN
            GameManager.instance.RollDice();
        }
        else
        {
            // NOT A DOUBLE ROLL
            // SWITCH PLAYER
            GameManager.instance.SwitchPlayer();
        }

    }

    int CalculatePropertyRent()
    {
        switch (numberOfHouses)
        {
            case 0:
                // CHECK IF OWNER HAS THE FULL SET OF THIS NODES
                var (list, allSame) = MonopolyBoard.Instance.PlayerHasAllNodesOfSet(this);
                if (allSame)
                {
                    currentRent = baseRent * 2;
                }
                else
                {
                    currentRent = baseRent;
                }
                break;
            case 1:
                currentRent = rentWithHouses[0];
                break;
            case 2:
                currentRent = rentWithHouses[1];
                break;
            case 3:
                currentRent = rentWithHouses[2];
                break;
            case 4:
                currentRent = rentWithHouses[3];
                break;
            case 5: //HOTEL
                currentRent = rentWithHouses[4];
                break;
        }
        return currentRent;
    }

    int CalculateUtilityRent()
    {
        int result = 0;
        int[] lastRolledDice = GameManager.instance.LastRolledDice();
        var (list, allSame) = MonopolyBoard.Instance.PlayerHasAllNodesOfSet(this);
        if (allSame)
        {
            result = (lastRolledDice[0] + lastRolledDice[1]) * 10;
        }
        else
        {
            result = (lastRolledDice[0] + lastRolledDice[1]) * 4;
        }      
        return result;
    }

    int CalculateRailroadRent()
    {
        int result = 0;
        var (list, allSame) = MonopolyBoard.Instance.PlayerHasAllNodesOfSet(this);

        int amount = 0;
        foreach (var item in list)
        {
            amount += (item.owner == this.owner) ? 1 : 0;
        }

        result = baseRent * (int)Mathf.Pow(2, amount-1);

        return result;
    }

}
