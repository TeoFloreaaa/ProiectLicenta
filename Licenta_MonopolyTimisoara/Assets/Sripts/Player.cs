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
    }

    public void SetMyCurrentNode(MonopolyNode newNode) // TURN IS OVER
    {
        currentnode = newNode;

        // PLAYER LANDED ON NODE SO LETS
        newNode.PlayerLandedOnNode(this);
        // IF ITS AI PLAYER

        // CHECK IF CAN BUILD HOUSES

        // CHECK FOR UNMORTGAGE PROPERTIES

        // CHECK IF HE COULD TRADE FOR MISSING PROPERTIES
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

    //------------------------BANKRUPT-GAME-OVER------------------------

    //------------------------UNMORTGAGE PROPERTY------------------------

    //------------------------CHECK IF PLAYER HAS A PROPERTY SET------------------------

    //------------------------BUILD HOUSES EVENLY ON NODE SETS------------------------

    //------------------------TRADING SYSTEM------------------------

    //------------------------FIND MISSING PROPERTY'S IN SET------------------------

    //------------------------HOUSES AND HOTELS - CAN AFFORT AND COUNT------------------------

}
