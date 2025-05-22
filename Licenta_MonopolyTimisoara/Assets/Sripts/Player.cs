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
    public List<MonopolyNode> GetMonopolyNodes => myMonopolyNodes;

    bool hasChanceJailFreeCard, hasCommunityJailFreeCard;
    public bool HasChanceJailFreeCard => hasChanceJailFreeCard;
    public bool HasCommunityJailFreeCard => HasCommunityJailFreeCard;

    // PLAYERINFO
    PlayerInfo myInfo;

    //MESSAGE SYSTEM
    public delegate void UpdateMessage(string message);
    public static UpdateMessage OnUpdateMessage;

    //HUMAN INPUT PANEL
    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn, bool hasCommunityJailCard, bool hasChanceJailCard);
    public static ShowHumanPanel OnShowHumanPanel;

    //AI STATES
    public enum AiStates
    {
        IDLE,
        TRADING
    }

    public AiStates aiState;


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
        if (playerType == PlayerType.HUMAN && GameManager.instance.GetCurrentPlayer == this)
        {
            bool canEndTurn = !GameManager.instance.RolledADouble && ReadMoney >= 0 && GameManager.instance.HasRolledDice;
            bool canRollDice = (GameManager.instance.RolledADouble && ReadMoney >= 0) || (!GameManager.instance.HasRolledDice && ReadMoney >= 0);

            // SHOW UI
            OnShowHumanPanel.Invoke(true, canRollDice, canEndTurn, hasCommunityJailFreeCard, hasCommunityJailFreeCard);
        }

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
        myMonopolyNodes = myMonopolyNodes.OrderBy(_node => _node.price).ToList();
    }

    internal void PayRent(int rentAmount, Player owner)
    {
        // DONT HAVE ENOUGH MONEY
        if (money < rentAmount)
        {
            if (playerType == PlayerType.AI)
            {
                // HANDLE INSUFFICIENT FUNDS > AI
                HandleInsufficientFunds(rentAmount);
            }
            else
            {
                OnShowHumanPanel.Invoke(true, false, false, HasCommunityJailFreeCard, HasCommunityJailFreeCard);
            }
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
            if (playerType == PlayerType.AI)
            {
                // HANDLE INSUFFICIENT FUNDS > AI
                HandleInsufficientFunds(amount);
            }
            else 
            {
                OnShowHumanPanel.Invoke(true, false, false, hasCommunityJailFreeCard, hasCommunityJailFreeCard);
            }
        }

        money -= amount;

        // UPDATE UI
        myInfo.SetPlayerCash(money);

        if (playerType == PlayerType.HUMAN && GameManager.instance.GetCurrentPlayer == this)
        {
            bool canEndTurn = !GameManager.instance.RolledADouble && ReadMoney >= 0 && GameManager.instance.HasRolledDice;
            bool canRollDice = (GameManager.instance.RolledADouble && ReadMoney >= 0) || (!GameManager.instance.HasRolledDice && ReadMoney >= 0);

            // SHOW UI
            OnShowHumanPanel.Invoke(true, canRollDice, canEndTurn, hasCommunityJailFreeCard, hasChanceJailFreeCard);
        }

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

    public void HandleInsufficientFunds(int amountToPay)
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

        if (playerType == PlayerType.AI)
        {
            //AT THIS POINT WE GO BANCKRUPT
            Bankrupt();
        }
    }

    //------------------------BANKRUPT-GAME-OVER------------------------

    public void Bankrupt()
    {
        //TAKE OUT THE PLAYER OF THE GAME

        //SEND A MESSAGE TO MESSAGE SYSTEM
        OnUpdateMessage.Invoke(name + " <color=red>A PIERDUT!</color>");

        //CLEAR ALL WHAT THE PLAYER HAS OWNED
        for (int i = myMonopolyNodes.Count - 1; i >= 0; i--)
        {
            myMonopolyNodes[i].ResetNode();
        }

        if (hasChanceJailFreeCard)
        {
            ChanceField.instance.AddBackJailFreeCard();
        }

        if (hasCommunityJailFreeCard)
        {
            CommunityChest.instance.AddBackJailFreeCard();
        }


        //REMOVE THE PLAYER
        GameManager.instance.RemovePlayer(this);
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

    internal void BuildHouseOrHotelEvenly(List<MonopolyNode> nodesToBuildOn)
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

    internal void SellHouseEvenly(List<MonopolyNode> nodesToSellFrom)
    {
        int minHouses = int.MaxValue;
        bool houseSold = false;
        foreach (var node in nodesToSellFrom)
        {
            minHouses = Mathf.Min(minHouses, node.NumberOfHouses);
        }
        //SELL HOUSE
        for (int i = nodesToSellFrom.Count - 1; i >= 0; i--)
        {
            if (nodesToSellFrom[i].NumberOfHouses > minHouses)
            {
                CollectMoney(nodesToSellFrom[i].SellHouseOrHotel());
                houseSold = true;
                break;
            }
        }
        if (!houseSold)
        {
            CollectMoney(nodesToSellFrom[nodesToSellFrom.Count - 1].SellHouseOrHotel());
        }

    }

    //------------------------HOUSES AND HOTELS - CAN AFFORT AND COUNT------------------------

    public bool CanAffordHouse(int price)
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

    //------------------ADD AND REMOVE PROPERTIES----------------------------------------------

    public void AddProperty(MonopolyNode node)
    {
        myMonopolyNodes.Add(node);
        //SORT ALL NODES BY PRICE
        SortPropertiesByPrice();
    }

    public void RemoveProperty(MonopolyNode node)
    {
        myMonopolyNodes.Remove(node);
        //SORT ALL NODES BY PRICE
        SortPropertiesByPrice();
    }

    //-----------------------------AI STATE MACHINE-------------------------------------------

    public void ChangeState(AiStates state)
    {
        if (playerType == PlayerType.HUMAN)
        {
            return;
        }

        aiState = state;
        switch (aiState)
        {
            case AiStates.IDLE:
            {
                    // CONTINUE THE GAME
                    //ContinueGame();
                    GameManager.instance.Continue();
                }
            break;

            case AiStates.TRADING:
            {
                    // HOLD THE GAME UNTIL CONTINUED
                    TradingSystem.instace.FindMissingProperty(this);
                }
            break;
        }
    }

    public void AddChanceJailFreeCard()
    {
        hasChanceJailFreeCard = true;
    }

    public void AddCommunityJailFreeCard()
    {
        hasCommunityJailFreeCard = true;
    }

    public void UseCommunityJailFreeCard()
    {
        if(!IsInJail)
        { 
            return; 
        }
        hasCommunityJailFreeCard = false;
        SetOutOfJail();
        CommunityChest.instance.AddBackJailFreeCard();
        OnUpdateMessage.Invoke(name + " a folosit cartea IESI GRATIS DIN INCHISOARE");
    }

    public void UseChanceJailFreeCard()
    {
        if (!IsInJail)
        {
            return;
        }
        hasChanceJailFreeCard = false;
        SetOutOfJail();
        ChanceField.instance.AddBackJailFreeCard();
        OnUpdateMessage.Invoke(name + " a folosit cartea IESI GRATIS DIN INCHISOARE");
    }


}
