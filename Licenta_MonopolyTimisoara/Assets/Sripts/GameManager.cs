using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] MonopolyBoard gameBoard;
    [SerializeField] List<Player> playerList = new List<Player>();
    [SerializeField] int currentPlayer;
    [Header("Game Settings")]
    [SerializeField] int maxTurnsInJail = 3; // SETTING FOR HOW LONG IN JAIL
    [SerializeField] int startMoney = 1500;
    [SerializeField] int goMoney = 200;
    [Header("Player Info")]
    [SerializeField] GameObject playerInfoPrefab;
    [SerializeField] Transform playerPanel; // FOR THE playerInfo Prefabs to become parented to
    [SerializeField] List<GameObject> playerTokenList = new List<GameObject>();

    // ABOUT THE ROLLING DICE
    int[] rolledDice;
    bool rolledADouble;
    int doubleRollCount;
    public bool RolledADouble => rolledADouble;

    public bool allwaysDR = true;

    //TAX POOL
    int taxPool = 0;

    //PASS OVER GO TO GET THE MONEY
    public int GetGoMoney => goMoney;


    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        Initialize();
        if (playerList[currentPlayer].playerType == Player.PlayerType.AI)
        {
            RollDice();
            
        }
        else
        {
            // SHOW UI FOR HUMAN INPUTS
        }        
    }

    void Initialize()
    {
        for (int i = 0; i < playerList.Count; i++)
        {
            GameObject infoObject = Instantiate(playerInfoPrefab, playerPanel, false);
            PlayerInfo info = infoObject.GetComponent<PlayerInfo>();
            //random token
            int RandomIndex = Random.Range(0, playerList.Count);
            //instantiate
            GameObject newToken = Instantiate(playerTokenList[RandomIndex], gameBoard.route[0].transform.position, Quaternion.identity);
            playerList[i].Initialize(gameBoard.route[0], startMoney, info, newToken);
        }
    }

    public void RollDice() // PRESS BUTTON FROM HUMAN - OR AUTO FROM AI
    {
        bool allowedToMove = true;

        
        // RESET LAST ROLL
        rolledDice = new int[2];

        // ANY ROLL DICE AND STORE THEM
        rolledDice[0] = Random.Range(1, 7);
        rolledDice[1] = Random.Range(1, 7);

        if (allwaysDR)
        {
            rolledDice[0] = 2;
            rolledDice[1] = 2;
        }

        Debug.Log("Rolled dice are: " + rolledDice[0] + " & " + rolledDice[1]);

        // CHECK FOR DOUBLE
        rolledADouble = rolledDice[0] == rolledDice[1];

        // THROW 3 TIMES IN A ROW -> JAIL ANYHOW -> END TURN

        // IS IN JAIL ALREADY
        if (playerList[currentPlayer].IsInJail)
        {
            playerList[currentPlayer].IncreaseNumTurnsInJail();

            if (rolledADouble)
            {
                playerList[currentPlayer].SetOutOfJail();
                doubleRollCount++;
                playerList[currentPlayer].ResetNumTurnsInJail();
                // MOVE THE PLAYER
            }
            else if (playerList[currentPlayer].NumTurnsInJail >= maxTurnsInJail)
            {
                // WE HAVE BEEN LONG ENOUGH HERE
                playerList[currentPlayer].SetOutOfJail();
                // ALLOWED TO LEAVE
            }

            else
            {
                allowedToMove = false;
            }
        }
        else // NOT IN JAIL
        {
            // RESET DOUBLE ROLLS
            if (!rolledADouble)
            {
                doubleRollCount = 0;
            }
            else
            {
                doubleRollCount++;
                if(doubleRollCount >= 3)
                {
                    doubleRollCount = 0;
                    //GO TO JAIL

                    int indexOnBoard = MonopolyBoard.Instance.route.IndexOf(playerList[currentPlayer].MyMonopolyNode);
                    rolledADouble = false;
                    playerList[currentPlayer].GoToJail(indexOnBoard);
                    return;
                }
            }
        }

        // CAN WE LEAVE JAIL
        
        // MOVE ANYHOW IF ALLOWED
        if (allowedToMove)
        {
            StartCoroutine(DelayBeforeMove(rolledDice[0] + rolledDice[1]));
        }
        else
        {
            SwitchPlayer();
        }
        // SHOW OR HIDE UI
    }

    IEnumerator DelayBeforeMove(int rolledDice)
    {
        yield return new WaitForSeconds(2f);
        // IF WE ARE ALLOWED TO MOVE WE DO SO
        gameBoard.MovePlayerToken(rolledDice, playerList[currentPlayer]);
        // ELSE WE SWITCH
    }

    public void SwitchPlayer()
    {
        currentPlayer++;
        // ROLLEDDOUBLE?
        doubleRollCount = 0;
        // OVERFLOW CHECK
        if (currentPlayer >= playerList.Count)
        {
            currentPlayer = 0;
        }

        // CHECK IF IN JAIL

        // IS PLAYER AI
        if (playerList[currentPlayer].playerType == Player.PlayerType.AI)
        {
            RollDice();
        }

        // IF HUMAN - SHOW UI
    }

    public int[] LastRolledDice()
    {
        return rolledDice;
    }

    public void AddTaxToPool(int amount)
    {
        taxPool += amount;
    }

    public int GetTaxPool()
    {
        int currentTaxPool = taxPool;
        taxPool = 0;
        return currentTaxPool;
    }


}
