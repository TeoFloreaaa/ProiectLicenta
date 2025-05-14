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
        // RESET LAST ROLL
        rolledDice = new int[2];

        // ANY ROLL DICE AND STORE THEM
        rolledDice[0] = Random.Range(1, 7);
        rolledDice[1] = Random.Range(1, 7);


        Debug.Log("Rolled dice are: " + rolledDice[0] + " & " + rolledDice[1]);

        // CHECK FOR DOUBLE
        rolledADouble = rolledDice[0] == rolledDice[1];

        // THROW 3 TIMES IN A ROW -> JAIL ANYHOW -> END TURN

        // IS IN JAIL ALREADY

        // CAN WE LEAVE JAIL

        // MOVE ANYHOW IF ALLOWED
        StartCoroutine(DelayBeforeMove(rolledDice[0] + rolledDice[1]));
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
