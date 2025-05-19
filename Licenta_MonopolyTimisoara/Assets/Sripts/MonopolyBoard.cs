using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

public class MonopolyBoard : MonoBehaviour
{
    public static MonopolyBoard Instance;

    public List<MonopolyNode> route = new List<MonopolyNode>();

    [System.Serializable]
    public class NodeSet
    {
        public Color setColor = Color.white;
        public List<MonopolyNode> nodesInSetList = new List<MonopolyNode>();
    }

    public List<NodeSet> nodeSetList = new List<NodeSet>();

    private void Awake()
    {
        Instance = this;
    }

    void OnValidate()
    {
        route.Clear();
        foreach (Transform node in transform.GetComponentInChildren<Transform>())
        {
            route.Add(node.GetComponent<MonopolyNode>());
        }

         //UPDATE ALL NODE COLORS
        for (int i = 0; i < nodeSetList.Count; i++)
        {
            for (int j = 0; j < nodeSetList[i].nodesInSetList.Count; j++)
           {
                nodeSetList[i].nodesInSetList[j].UpdateColorField(nodeSetList[i].setColor);
            }
        }

    }

    void OnDrawGizmos()
    {
        if (route.Count > 1)
        {
            for (int i = 0; i < route.Count; i++)
            {
                Vector3 current = route[i].transform.position;
                Vector3 next = (i + 1 < route.Count) ? route[i + 1].transform.position : current;

                Gizmos.color = Color.green;
                Gizmos.DrawLine(current, next);
            }
        }
    }

    public void MovePlayerToken(int steps, Player player)
    {
        StartCoroutine(MovePlayerInSteps(steps, player));
    }

    public void MovePlayerToken(MonopolyNodeType type, Player player)
    {
        int indexOfNextNodeType = -1; // INDEX TO FIND
        int indexOnBoard = route.IndexOf(player.MyMonopolyNode); // WHERE IS THE PLAYER
        int startSearchIndex = (indexOnBoard + 1) % route.Count;
        int nodeSearches = 0; // AMOUNT OF FIELDS SEARCHED

        while (indexOfNextNodeType == -1 && nodeSearches < route.Count) // KEEP SEARCHING
        {
            if (route[startSearchIndex].monopolyNodeType == type) // FOUND THE DESIRED TYPE
            {
                indexOfNextNodeType = startSearchIndex;
            }
            startSearchIndex = (startSearchIndex + 1) % route.Count;
            nodeSearches++;
        }

        if (indexOfNextNodeType == -1) // SECURITY EXIT
        {
            Debug.LogError("NO NODE FOUND");
            return;
        }

        StartCoroutine(MovePlayerInSteps(nodeSearches, player));
    }

    IEnumerator MovePlayerInSteps(int steps, Player player)
    {
        int stepsLeft = steps;
        GameObject tokenToMove = player.MyToken;
        int indexOnBoard = route.IndexOf(player.MyMonopolyNode);
        bool moveOverGo = false;
        bool isMovingForward = steps > 0;

        if (isMovingForward)
        {
            while (stepsLeft > 0)
            {
                indexOnBoard++;

                // IS THIS OVER GO?
                if (indexOnBoard > route.Count - 1)
                {
                    indexOnBoard = 0;
                    moveOverGo = true;
                }

                // GET START AND END POSITIONS
                Vector3 startPos = tokenToMove.transform.position;
                Vector3 endPos = route[indexOnBoard].transform.position;

                // PERFORM THE MOVE
                while (MoveToNextNode(tokenToMove, endPos, 10f))
                {
                    yield return null;
                }

                stepsLeft--;
            }
        }
        else
        {
            while (stepsLeft < 0)
            {
                indexOnBoard--;

                // IS THIS OVER GO?
                if (indexOnBoard < 0)
                {
                    indexOnBoard = route.Count - 1;
                    
                }

                // GET START AND END POSITIONS
                //Vector3 startPos = tokenToMove.transform.position;
                Vector3 endPos = route[indexOnBoard].transform.position;

                // PERFORM THE MOVE
                while (MoveToNextNode(tokenToMove, endPos, 10f))
                {
                    yield return null;
                }

                stepsLeft++;
            }
        }
        // GET GO MONEY
        if (moveOverGo)
        {
            // COLLECT MOBEY ON THE PLAYER
            player.CollectMoney(GameManager.instance.GetGoMoney);
        }

        // SET NEW NODE ON THE CURRENT PLAYER
        player.SetMyCurrentNode(route[indexOnBoard]);
    }

    bool MoveToNextNode(GameObject tokenToMove, Vector3 endPos, float speed)
    {
        return endPos != (tokenToMove.transform.position = Vector3.MoveTowards(tokenToMove.transform.position, endPos, speed * Time.deltaTime));
    }

    public (List<MonopolyNode>, bool) PlayerHasAllNodesOfSet(MonopolyNode node)
    {
        foreach (var nodeSet in nodeSetList)
        {
            if (nodeSet.nodesInSetList.Contains(node))
            {
                // CHECK IFF ALL THE NODES IN SET HAVE THE SAME OWNER
                bool allSame = nodeSet.nodesInSetList.All(_node => _node.Owner == node.Owner);
                return (nodeSet.nodesInSetList, allSame);
            }
        }

        return (null, false);
    }

}
