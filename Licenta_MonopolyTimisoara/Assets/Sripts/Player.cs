using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class Player
{
    public enum PlayerType
    {
        HUMAN,
        AI
    }

    public PlayerType playerType;
    public string name;
    int money;
    MonopolyNode currentnode;
    bool isInJail;
    int numTurnsInJail;

    [SerializeField] GameObject myToken;
    [SerializeField] List<MonopolyNode> myMonopolyNodes = new List<MonopolyNode>();

    // PLAYERINFO
    PlayerInfo myInfo;

    //MESSAGE SYSTEM
    public delegate void UpdateMessage(string message);
    public static UpdateMessage OnUpdateMessage;

    // AI
    int aiMoneySavity = 200;

    // RETURN SOME INFOS
    public bool IsInJail => isInJail;
    public GameObject MyToken => myToken;
    public MonopolyNode MyMonopolyNode => currentnode;
    public int ReadMoney => money;

    public void Initialize(MonopolyNode startNode, int startMoney, PlayerInfo info, GameObject token)
    {
        currentnode = startNode;
        money = startMoney;
        myInfo = info;

        myInfo.SetPlayerNameAndCash(name, money);
        myToken = token;

        myInfo.ActivateArrow(false);
    }

    public void SetMyCurrentNode(MonopolyNode newNode) // TURN IS OVER
    {
        currentnode = newNode;

        // PLAYER LANDED ON NODE SO LETS
        newNode.PlayerLandedOnNode(this);
        // IF ITS AI PLAYER
        if (playerType == PlayerType.AI)
        {
            // CHECK IF CAN BUILD HOUSES
            CheckIfPlayerHasASet();
            // CHECK FOR UNMORTGAGE PROPERTIES

            UnMortgageProperties();
            // CHECK IF HE COULD TRADE FOR MISSING PROPERTIES
        }
    }

    public void CollectMoney(int amount)
    {
        money += amount;
        myInfo.SetPlayerCash(money);
    }

    internal bool CanAffordNode(int price)
    {
        return price <= money;
    }

    public void BuyProperty(MonopolyNode node)
    {
        money -= node.price;
        node.SetOwner(this);

        // UPDATE UI
        myInfo.SetPlayerCash(money);

        // SET OWNERSHIP
        myMonopolyNodes.Add(node);

        // SORT ALL NODES BY PRICE
        SortPropertiesByPrice();
    }

    void SortPropertiesByPrice()
    {
        myMonopolyNodes.OrderBy(_node => _node.price).ToList();
    }

    internal void PayRent(int rentAmount, Player owner)
    {
        // DONT HAVE ENOUGH MONEY
        if (money < rentAmount)
        {
            // HANDLE INSUFFICIENT FUNDS > AI
            HandleInsufficientFunds(rentAmount);
        }

        money -= rentAmount;
        owner.CollectMoney(rentAmount);

        // UPDATE UI
        myInfo.SetPlayerCash(money);
    }

    internal void PayMoney(int amount)
    {
        // DONT HAVE ENOUGH MONEY
        if (money < amount)
        {
            // HANDLE INSUFFICIENT FUNDS > AI
            HandleInsufficientFunds(amount);
        }

        money -= amount;

        // UPDATE UI
        myInfo.SetPlayerCash(money);
    }

    //----------------JAIL---------------------

    public void GoToJail(int indexOnBoard)
    {
        isInJail = true;
        // REPOSITION PLAYER
        //myToken.transform.position = MonopolyBoard.Instance.route[10].transform.position;
        //currentnode = MonopolyBoard.Instance.route[10];
        MonopolyBoard.Instance.MovePlayerToken(CalculateDistanceFromJail(indexOnBoard), this);
        GameManager.instance.ResetRolledADouble();
    }

    public void SetOutOfJail()
    {
        isInJail = false;
        numTurnsInJail = 0;
    }

    int CalculateDistanceFromJail(int indexOnBoard)
    {
        int result = 0;
        int indexOfJail = 10;

        if (indexOnBoard > indexOfJail)
        {
            result = (indexOnBoard - indexOfJail) * -1;
        }
        else
        {
            result = (indexOfJail - indexOnBoard);
        }

        return result;
    }

    public int NumTurnsInJail => numTurnsInJail;

    public void IncreaseNumTurnsInJail()
    {
        numTurnsInJail++;
    }

    //--------------------------STREET REPAIRS--------------------------

    public int[] CountHousesAndHotels()
    {
        int houses = 0; //GOES TO INDEX 0
        int hotels = 0; //GOES TO INDEX 1

        foreach (var node in myMonopolyNodes)
        {
            if (node.NumberOfHouses != 5)
            {
                houses += node.NumberOfHouses;
            }
            else
            {
                hotels += 1;
            }
        }

        int[] allBuildings = new int[] { houses, hotels };
        return allBuildings;
    }

    //------------------------HANDLE INSUFFICIENT FUNDS------------------------

    void HandleInsufficientFunds(int amountToPay)
    {
        int housesToSell = 0; // AVAILABLE HOUSES TO SELL
        int allHouses = 0;
        int propertiesToMortgage = 0;
        int allPropertiesToMortgage = 0;

        // COUNT ALL HOUSES
        foreach (var node in myMonopolyNodes)
        {
            allHouses += node.NumberOfHouses;
        }

        // LOOP THROUGH THE PROPERTIES AND TRY TO SELL AS MUCH AS NEEDED
        while (money < amountToPay && allHouses > 0)
        {
            foreach (var node in myMonopolyNodes)
            {
                housesToSell = node.NumberOfHouses;
                if (housesToSell > 0)
                {
                    CollectMoney(node.SellHouseOrHotel());
                    allHouses--;
                    // DO WE NEED MORE MONEY?
                    if (money >= amountToPay)
                    {
                        return;
                    }

                }
            }
        }

        // MORTGAGE
        foreach (var node in myMonopolyNodes)
        {
            allPropertiesToMortgage += (!node.IsMortgaged) ? 1 : 0;
        }

        // LOOP THROUGH THE PROPERTIES AND TRY TO MORTGAGE AS MUCH AS NEEDED
        while (money < amountToPay && allPropertiesToMortgage > 0)
        {
            foreach (var node in myMonopolyNodes)
            {
                propertiesToMortgage = (!node.IsMortgaged) ? 1 : 0;
                if (propertiesToMortgage > 0)
                {
                    CollectMoney(node.MortgageProperty());
                    allPropertiesToMortgage--;

                    // DO WE NEED MORE MONEY?
                    if (money >= amountToPay)
                    {
                        return;
                    }
                }
            }
        }

        //AT THIS POINT WE GO BANCKRUPT
        Bankrupt();
    }


    //------------------------BANKRUPT-GAME-OVER------------------------

    void Bankrupt()
    {
        //TAKE OUT THE PLAYER OF THE GAME

        //SEND A MESSAGE TO MESSAGE SYSTEM
        OnUpdateMessage.Invoke(name + " <color=red>A PIERDUT!</color>");

        //CLEAR ALL WHAT THE PLAYER HAS OWNED
        for (int i = myMonopolyNodes.Count - 1; i >= 0; i--)
        {
            myMonopolyNodes[i].ResetNode();
        }

        //REMOVE THE PLAYER
        GameManager.instance.RemovePlayer(this);
    }


    public void RemoveProperty(MonopolyNode node)
    {
        myMonopolyNodes.Remove(node);
    }


    //------------------------UNMORTGAGE PROPERTY------------------------

    void UnMortgageProperties()
    {
        //FOR AI
        foreach (var node in myMonopolyNodes)
        {
            if (node.IsMortgaged)
            {
                int cost = node.MortgageValue + (int)(node.MortgageValue * 0.1f); //10% Interest

                //CAN WE AFFORT TO UNMORTGAGE
                if (money >= aiMoneySavity + cost)
                {
                    PayMoney(cost);
                    node.UnMortgageProperty();
                }
            }
        }
    }


    //------------------------CHECK IF PLAYER HAS A PROPERTY SET------------------------

    void CheckIfPlayerHasASet()
    {
        //CALL IT ONLY ONCE PER SET
        List<MonopolyNode> processedSet = null;

        foreach (var node in myMonopolyNodes)
        {           
            var (list, allSame) = MonopolyBoard.Instance.PlayerHasAllNodesOfSet(node);
            if (!allSame)
            {
                continue;
            }
            List<MonopolyNode> nodeSet = list;
            if (nodeSet != null && nodeSet != processedSet)
            {
                bool hasMorgdagedNode = nodeSet.Any(node => node.IsMortgaged) ? true : false;
                if (!hasMorgdagedNode)
                {
                    if (nodeSet[0].monopolyNodeType == MonopolyNodeType.Property)
                    {
                        //WE COULD BUILD A HOUSE ON THE SET
                        BuildHouseOrHotelEvenly(nodeSet);
                        //UPDATE PROCESS SET OVER HER
                        processedSet = nodeSet;
                    }
                }
            }
        }
    }

    //------------------------BUILD HOUSES EVENLY ON NODE SETS------------------------

    void BuildHouseOrHotelEvenly(List<MonopolyNode> nodesToBuildOn)
    {
        int minHouses = int.MaxValue;
        int maxHouses = int.MinValue;
        //GET MIN AND MAX NUMBERS OF HOUSE CURRENTLY ON THE PROPERTY
        foreach (var node in nodesToBuildOn)
        {
            int numOfHouses = node.NumberOfHouses;
            if (numOfHouses < minHouses)
            {
                minHouses = numOfHouses;
            }

            if (numOfHouses > maxHouses && numOfHouses < 5)
            {
                maxHouses = numOfHouses;
            }
        }

        //BUY HOUSES ON THE PROPERTIES FOR MAX ALLOWED ON THE PROPERTIES
        foreach (var node in nodesToBuildOn)
        {
            if (node.NumberOfHouses == minHouses && node.NumberOfHouses < 5 && CanAffordHouse(node.houseCost))
            {
                node.BuildHouseOrHotel();
                PayMoney(node.houseCost);
                break;
            }
        }
    }


    //------------------------TRADING SYSTEM------------------------

    //------------------------FIND MISSING PROPERTY'S IN SET------------------------

    //------------------------HOUSES AND HOTELS - CAN AFFORT AND COUNT------------------------

    bool CanAffordHouse(int price)
    {
        if (playerType == PlayerType.AI)
        {
            return (money - aiMoneySavity) >= price;
        }
        //HUMAN 
        return money >= price;
    }

    public void ActivateSelector(bool active)
    {
        myInfo.ActivateArrow(active);
    }

}
