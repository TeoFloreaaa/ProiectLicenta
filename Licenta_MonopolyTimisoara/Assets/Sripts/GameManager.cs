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
    [SerializeField] int goMoney = 500;
    [Header("Player Info")]
    [SerializeField] GameObject playerInfoPrefab;
    [SerializeField] Transform playerPanel; // FOR THE playerInfo Prefabs to become parented to
    [SerializeField] List<GameObject> playerTokenList = new List<GameObject>();

    // ABOUT THE ROLLING DICE
    int[] rolledDice;
    bool rolledADouble;
    int doubleRollCount;


    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        Initialize();
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

        // CHECK FOR DOUBLE
        rolledADouble = rolledDice[0] == rolledDice[1];

        // THROW 3 TIMES IN A ROW -> JAIL ANYHOW -> END TURN

        // IS IN JAIL ALREADY

        // CAN WE LEAVE JAIL

        // MOVE ANYHOW IF ALLOWED

        // SHOW OR HIDE UI
    }

    IEnumerator DelayBeforeMove()
    {
        yield return new WaitForSeconds(2f);
        // IF WE ARE ALLOWED TO MOVE WE DO SO

        // ELSE WE SWITCH
    }

}
