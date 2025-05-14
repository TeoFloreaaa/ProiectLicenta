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

    // AI
    int aiMoneySavity = 200;

    // RETURN SOME INFOS
    public bool IsInJail => isInJail;
    public GameObject MyToken => myToken;
    public MonopolyNode MyMonopolyNode => currentnode;

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


}
