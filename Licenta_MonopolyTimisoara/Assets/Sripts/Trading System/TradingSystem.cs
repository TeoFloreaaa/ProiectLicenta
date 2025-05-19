using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TradingSystem : MonoBehaviour
{
    //MESSAGE SYSTEM
    public delegate void UpdateMessage(string message);
    public static UpdateMessage OnUpdateMessage;

    public static TradingSystem instace;

    private void Awake()
    {
        instace = this;
    }

    //--------------------FIND MISSING PROPERTY'S IN SET---------------------AI

    public void FindMissingProperty(Player currentPlayer)
    {
        List<MonopolyNode> processedSet = null;
        MonopolyNode requestedNode = null;
        foreach (var node in currentPlayer.GetMonopolyNodes)
        {
            var (list, allSame) = MonopolyBoard.Instance.PlayerHasAllNodesOfSet(node);
            List<MonopolyNode> nodeSet = new List<MonopolyNode>();
            nodeSet.AddRange(list);
            //CHECK IF ALL HAVE BEEN PURCHASED
            bool notAllPurchased = list.Any(n => n.Owner == null);

            //AI OWNS THIS FULL SET ALREADY
            if (allSame || processedSet == list || notAllPurchased)
            {
                processedSet = list;
                continue;
            }
            //FIND THE OWNED BY OTHER PLAYER
            //BUT CHECK IF WE HAVE MORE THAN THE AVERAGE
            if (list.Count == 2)
            {
                requestedNode = list.Find(n => n.Owner != currentPlayer && n.Owner != null);
                if (requestedNode != null)
                {
                    //MAKE OFFER TO THE OWNER
                    MakeTradeDecision(currentPlayer, requestedNode.Owner, requestedNode);
                    break;
                }
            }
            if (list.Count >= 3)
            {
                int hasMostOfSet = list.Count(n => n.Owner == currentPlayer);
                if (hasMostOfSet >= 2)
                {
                    requestedNode = list.Find(n => n.Owner != currentPlayer && n.Owner != null);
                    //MAKE OFFER TO OWNER OF THE NODE
                    MakeTradeDecision(currentPlayer, requestedNode.Owner, requestedNode);
                    break;
                }
            }

        }
    }


    //--------------------MAKE TRADE OFFER-----------------------------------

    void MakeTradeDecision(Player currentPlayer, Player nodeOwner, MonopolyNode requestedNode)
    {
        //TRADE WITH MONEY IF POSSIBLE
        if (currentPlayer.ReadMoney >= CalculateValueOfNode(requestedNode))
        {
            //TRADE WITH MONEY ONLY

            //MAKE TRADE OFFER
            MakeTradeOffer(currentPlayer, nodeOwner, requestedNode, null, CalculateValueOfNode(requestedNode), 0);
            return;
        }

        //FIND ALL INCOMPLETE SET AND EXCLUDE THE SET WITH THE REQUESTED NODE
        foreach (var node in currentPlayer.GetMonopolyNodes)
        {
            var (checkedSet, allSame) = MonopolyBoard.Instance.PlayerHasAllNodesOfSet(node);
            
            if (checkedSet.Contains(requestedNode))
            {
                //STOP CHECKING HERE
                continue;
            }
            if (checkedSet.Count(n => n.Owner == currentPlayer) == 1) // VALID NODE CHECK
            {
                if (CalculateValueOfNode(node) + currentPlayer.ReadMoney >= requestedNode.price)
                {
                    int diference = CalculateValueOfNode(requestedNode) - CalculateValueOfNode(node);
                    if (diference > 0)
                    {
                        //VALID TRADE POSSIBLE
                        MakeTradeOffer(currentPlayer, nodeOwner, requestedNode, node, diference, 0);
                        //MAKE TRADE OFFER
                    }
                    else
                    {
                        //VALID TRADE POSSIBLE
                        MakeTradeOffer(currentPlayer, nodeOwner, requestedNode, node, 0, Mathf.Abs(diference));
                    }
                    break;

                }
            }

        }

        //FIND OUT IF ONLY ONE NODE OF THE FOUND SET IS OWNED

        //CALCULATE THE VALUE OF THAT NODE AND SEE IF WITH ENOUGH MONEY IT COULD BE AFFORDABLE

        //IF SO... MAKE TRADE OFFER
    }

    //----------------------------MAKE TRADE OFFER-----------------------------

    void MakeTradeOffer(Player currentPlayer, Player nodeOwner, MonopolyNode requestedNode, MonopolyNode offeredNode, int offeredMoney, int requestedMoney)
    {
        if (nodeOwner.playerType == Player.PlayerType.AI)
        {
            ConsiderTradeOffer(currentPlayer, nodeOwner, requestedNode, offeredNode, offeredMoney, requestedMoney);
        }
        else if (nodeOwner.playerType == Player.PlayerType.HUMAN)
        {
            //SHOW UI
        }

    }

    //--------------------CONSIDER TRADE OFFER-------------------------------AI

    void ConsiderTradeOffer(Player currentPlayer, Player nodeOwner, MonopolyNode requestedNode, MonopolyNode offeredNode, int offeredMoney, int requestedMoney)
    {
        int valueOfTheTrade = (CalculateValueOfNode(requestedNode) + requestedMoney) - (CalculateValueOfNode(offeredNode) + offeredMoney);
        
        //SELL A NODE FOR MONEY ONLY
        if (requestedNode == null && offeredNode != null && requestedMoney <= nodeOwner.ReadMoney / 3)
        {
            Trade(currentPlayer, nodeOwner, requestedNode, offeredNode, offeredMoney, requestedMoney);
            return;
        }

        if (valueOfTheTrade <= 0)
        {
            //TRADE THE NODE IS VALID
            Trade(currentPlayer, nodeOwner, requestedNode, offeredNode, offeredMoney, requestedMoney);
        }
        else
        {
            //DEBUG LINE OR TELL PLAYER THATS REJECTED
            Debug.Log("AI REJECTED TRADE OFFER");
        }

    }

    //--------------------CALCULATE THE VALUE OF NODE------------------------AI

    int CalculateValueOfNode(MonopolyNode requestedNode)
    {
        int value = 0;
        if (requestedNode != null)
        {
            if (requestedNode.monopolyNodeType == MonopolyNodeType.Property)
            {
                value = requestedNode.price + requestedNode.NumberOfHouses * requestedNode.houseCost;
            }
            else
            {
                value = requestedNode.price;
            }
            return value;
        }
        return 0;
    }


    //--------------------TRADE THE NODE-------------------------------------

    void Trade(Player currentPlayer, Player nodeOwner, MonopolyNode requestedNode, MonopolyNode offeredNode, int offeredMoney, int requestedMoney)
    {
        //CURRENTPLAYER NEEDS TO
        if (requestedNode != null)
        {
            currentPlayer.PayMoney(offeredMoney);
            requestedNode.ChangeOwner(currentPlayer);
            //NODE OWNER
            nodeOwner.CollectMoney(offeredMoney);
            nodeOwner.PayMoney(requestedMoney);

            if (offeredNode != null)
            {
                offeredNode.ChangeOwner(nodeOwner);
            }

            //SHOW A MESSAGE FOR THE UI
            string offeredNodeName = (offeredNode != null) ? " & " + offeredNode.name : "";
            OnUpdateMessage.Invoke(currentPlayer.name + " a facut schimb " + requestedNode.name + " pentru $" + offeredMoney + offeredNodeName + " cu " + nodeOwner.name);
        }
        else if (offeredNode != null && requestedNode == null)
        {
            currentPlayer.CollectMoney(requestedMoney);
            nodeOwner.PayMoney(requestedMoney);
            offeredNode.ChangeOwner(nodeOwner);
            OnUpdateMessage.Invoke(currentPlayer.name + " a vandut " + requestedNode.name + " lui " + nodeOwner.name + " pentru $" + requestedMoney);
        }

        //SHOW A MESSAGE FOR THE UI
        

    }

    
}
