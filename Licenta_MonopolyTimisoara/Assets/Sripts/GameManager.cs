using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    public float secondsBetweenTurns = 2f;
    [Header("Player Info")]
    [SerializeField] GameObject playerInfoPrefab;
    [SerializeField] Transform playerPanel; // FOR THE playerInfo Prefabs to become parented to
    [SerializeField] List<GameObject> playerTokenList = new List<GameObject>();

    //DEBUG
    //[SerializeField] bool alwaysDoubleRoll = false;
    //[SerializeField] bool forceDiceRolls;
    //[SerializeField] int dice1;
    //[SerializeField] int dice2;

    [Header("Game Over/ Win Info")]
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] TMP_Text winnerNameText;

    [Header("Dice")]
    [SerializeField] Dice _dice1;
    [SerializeField] Dice _dice2;


    // ABOUT THE ROLLING DICE
    List<int> rolledDice = new List<int>();
    bool rolledADouble;
    int doubleRollCount;
    public bool RolledADouble => rolledADouble;
    public void ResetRolledADouble() => rolledADouble = false;
    bool hasRolledDice;
    public bool HasRolledDice => hasRolledDice;

    public bool allwaysDR = true;

    //TAX POOL
    int taxPool = 0;

    //PASS OVER GO TO GET THE MONEY
    public int GetGoMoney => goMoney;
    public List<Player> GetPlayers => playerList;
    public Player GetCurrentPlayer => playerList[currentPlayer];

    //MESSAGE SYSTEM
    public delegate void UpdateMessage(string message);
    public static UpdateMessage OnUpdateMessage;

    //HUMAN INPUT PANEL
    public delegate void ShowHumanPanel(bool activatePanel, bool activateRollDice, bool activateEndTurn, bool hasCommunityJailCard, bool hasChanceJailCard);
    public static ShowHumanPanel OnShowHumanPanel;




    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        gameOverPanel.SetActive(false);
        Initialize();
        currentPlayer = Random.Range(0, playerList.Count);
        CameraSwitcher.instance.SwitchToTopDown();
        StartCoroutine(StartGame());
        OnUpdateMessage.Invoke("BINE ATI VENIT LA MONOPOLY TIMISOARA!<br>DISTRATI-VA!!!");
    }

    IEnumerator StartGame()
    {
        yield return new WaitForSeconds(3f);
        if (playerList[currentPlayer].playerType == Player.PlayerType.AI)
        {
            //RollDice();
            RollPhysicalDice();
        }
        else
        {
            //SHOW UI FOR HUMAN INPUTS
            OnShowHumanPanel.Invoke(true, true, false, false, false);
        }
    }

    void Initialize()
    {
        if (GameSettings.settingsList.Count == 0)
        {
            //Debug.LogError("Start the game from the Main Menu!");
            return;
        }

        foreach (var setting in GameSettings.settingsList)
        {
            Player p1 = new Player();
            p1.name = setting.playerName;
            p1.playerType = (Player.PlayerType)setting.selectedType;

            playerList.Add(p1);

            GameObject infoObject = Instantiate(playerInfoPrefab, playerPanel, false);
            PlayerInfo info = infoObject.GetComponent<PlayerInfo>();

            GameObject newToken = Instantiate(playerTokenList[setting.selectedColor], gameBoard.route[0].transform.position, Quaternion.identity);
            p1.Initialize(gameBoard.route[0], startMoney, info, newToken);
        }

        /*for (int i = 0; i < playerList.Count; i++)
        {
            GameObject infoObject = Instantiate(playerInfoPrefab, playerPanel, false);
            PlayerInfo info = infoObject.GetComponent<PlayerInfo>();
            //random token
            int RandomIndex = Random.Range(0, playerList.Count);
            //instantiate
            GameObject newToken = Instantiate(playerTokenList[RandomIndex], gameBoard.route[0].transform.position, Quaternion.identity);
            playerList[i].Initialize(gameBoard.route[0], startMoney, info, newToken);
        }
        */

        playerList[currentPlayer].ActivateSelector(true);

        if (playerList[currentPlayer].playerType == Player.PlayerType.HUMAN)
        {
            bool jail1 = playerList[currentPlayer].HasCommunityJailFreeCard;
            bool jail2 = playerList[currentPlayer].HasChanceJailFreeCard;
            OnShowHumanPanel.Invoke(true, true, false, jail1, jail2);
        }
        else 
        {
            bool jail1 = playerList[currentPlayer].HasCommunityJailFreeCard;
            bool jail2 = playerList[currentPlayer].HasChanceJailFreeCard;
            OnShowHumanPanel.Invoke(false, false, false, jail1, jail2);
        }
    }

    void CheckForJailFree()
    {
        //JAIL FREE CARD
        if (playerList[currentPlayer].IsInJail && playerList[currentPlayer].playerType == Player.PlayerType.AI)
        {
            if (playerList[currentPlayer].HasChanceJailFreeCard)
            {
                playerList[currentPlayer].UseChanceJailFreeCard();
            }
            else if (playerList[currentPlayer].HasCommunityJailFreeCard)
            {
                playerList[currentPlayer].UseCommunityJailFreeCard();
            }
        }
    }

    public void RollPhysicalDice()
    {
        CheckForJailFree();
        rolledDice.Clear();
        CameraSwitcher.instance.SwitchToDice();
        bool jail1 = playerList[currentPlayer].HasCommunityJailFreeCard;
        bool jail2 = playerList[currentPlayer].HasChanceJailFreeCard;
        if (playerList[currentPlayer].playerType == Player.PlayerType.HUMAN)
        {
            OnShowHumanPanel.Invoke(true, false, false, jail1, jail2);
        }
        _dice1.RollDice();
        _dice2.RollDice();
    }

    public void ReportDiceRolled(int diceValue)
    {
        rolledDice.Add(diceValue);
        if (rolledDice.Count == 2)
        {
            RollDice();
        }
    }

    public void RollDice() // PRESS BUTTON FROM HUMAN - OR AUTO FROM AI
    {
        bool allowedToMove = true;
        hasRolledDice = true;

       
        // RESET LAST ROLL
        //rolledDice = new int[2];

        // ANY ROLL DICE AND STORE THEM
        //rolledDice[0] = Random.Range(1, 7);
        //rolledDice[1] = Random.Range(1, 7);

        //DEBUG
        //if (alwaysDoubleRoll)
        //{
        //    rolledDice[0] = 1;
        //    rolledDice[1] = 1;
        //}
        //if (forceDiceRolls)
        //{
        //    rolledDice[0] = dice1;
        //    rolledDice[1] = dice2;
        //}

        //Debug.Log("Rolled dice are: " + rolledDice[0] + " & " + rolledDice[1]);

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
                OnUpdateMessage.Invoke(playerList[currentPlayer].name + " <color=green>poate iesi din inchisoare pentru ca a dat o dubla!</color>");
                // MOVE THE PLAYER
            }
            else if (playerList[currentPlayer].NumTurnsInJail >= maxTurnsInJail)
            {
                // WE HAVE BEEN LONG ENOUGH HERE
                playerList[currentPlayer].SetOutOfJail();
                OnUpdateMessage.Invoke(playerList[currentPlayer].name + " <color=green>poate iesi din inchisoare pentru ca au trecut cele 3 ture</color>");
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
                    OnUpdateMessage.Invoke(playerList[currentPlayer].name + " a dat de 3 ori dubla <color=red> si trebuie sa mearga la inchisoare! </color>" );
                    playerList[currentPlayer].GoToJail(indexOnBoard);
                    return;
                }
            }
        }

        // CAN WE LEAVE JAIL
        
        // MOVE ANYHOW IF ALLOWED
        if (allowedToMove)
        {
            OnUpdateMessage.Invoke(playerList[currentPlayer].name + " a dat " + rolledDice[0] + " si " + rolledDice[1]);
            StartCoroutine(DelayBeforeMove(rolledDice[0] + rolledDice[1]));
        }
        else
        {
            OnUpdateMessage.Invoke(playerList[currentPlayer].name + " trebuie sa stea la inchisoare");

           StartCoroutine(DelayBetweenSwitchPlayer());
        }

        // SHOW OR HIDE UI
        if (playerList[currentPlayer].playerType == Player.PlayerType.HUMAN)
        {
            bool jail1 = playerList[currentPlayer].HasCommunityJailFreeCard;
            bool jail2 = playerList[currentPlayer].HasChanceJailFreeCard;
            OnShowHumanPanel.Invoke(true, false, false, jail1, jail2);
        }
    }

    IEnumerator DelayBeforeMove(int rolledDice)
    {
        CameraSwitcher.instance.SwitchToPlayer(playerList[currentPlayer].MyToken.transform);
        yield return new WaitForSeconds(secondsBetweenTurns);
        // IF WE ARE ALLOWED TO MOVE WE DO SO
        gameBoard.MovePlayerToken(rolledDice, playerList[currentPlayer]);
        // ELSE WE SWITCH
    }

    IEnumerator DelayBetweenSwitchPlayer()
    {
        yield return new WaitForSeconds(secondsBetweenTurns);
        SwitchPlayer();
    }


    public void SwitchPlayer()
    {
        CameraSwitcher.instance.SwitchToTopDown();
        currentPlayer++;
        hasRolledDice = false;

        // ROLLEDDOUBLE?
        doubleRollCount = 0;

        // OVERFLOW CHECK
        if (currentPlayer >= playerList.Count)
        {
            currentPlayer = 0;
        }

        DeactivateArrows();
        playerList[currentPlayer].ActivateSelector(true);

        // CHECK IF IN JAIL

        // IS PLAYER AI
        if (playerList[currentPlayer].playerType == Player.PlayerType.AI)
        {
            //RollDice();
            RollPhysicalDice();
            OnShowHumanPanel.Invoke(false, false, false, false, false);
        }
        else
        {
            bool jail1 = playerList[currentPlayer].HasCommunityJailFreeCard;
            bool jail2 = playerList[currentPlayer].HasChanceJailFreeCard;
            OnShowHumanPanel.Invoke(true, true, false, jail1, jail2);
        }

        // IF HUMAN - SHOW UI
    }

    public List<int> LastRolledDice()
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

    //---------------------GAME OVER---------------------

    public void RemovePlayer(Player player)
    {
        playerList.Remove(player);
        //CHECK FOR GAME OVER
        CheckForGameOver();
    }

    void CheckForGameOver()
    {
        if (playerList.Count == 1)
        {
            //WE HAVE A WINNER
            //Debug.Log(playerList[0].name + " IS THE WINNER");
            OnUpdateMessage.Invoke(playerList[0].name + " IS THE WINNER");
            //STOP THE GAME LOOP ANYHOW

            //SHOW UI
            gameOverPanel.SetActive(true);
            winnerNameText.text = playerList[0].name;
        }
    }

    //-------------------------UI STUFF----------------------------

    void DeactivateArrows()
    {
        foreach (var player in playerList)
        {
            player.ActivateSelector(false);
        }
    }

    //--------------------------CONTINUE GAME STUFF----------------------

    public void Continue()
    {
        if (playerList.Count > 1)
        {
            Invoke("ContinueGame", secondsBetweenTurns);
        }
    }

    void ContinueGame()
    {
        // IF THE LAST ROLL WAS A DOUBLE
        if (RolledADouble)
        {
            // ROLL AGAIN
            //RollDice();
            RollPhysicalDice();
        }
        else
        {
            // NOT A DOUBLE ROLL
            // SWITCH PLAYER            
                SwitchPlayer();
        }
    }

    //-----------------------------BANKRUPT-----------------------------------

    public void HumanBankrupt()
    {
        playerList[currentPlayer].Bankrupt();
    }

    //---------------------------JAIL FREE CARDS---------------------------HUMAN
    //BUTTONS
    public void UseJail1Card() // CHANCE JAIL CARD
    {
        playerList[currentPlayer].UseCommunityJailFreeCard();
    }

    public void UseJail2Card() // COMMUNITY JAIL CARD
    {
        playerList[currentPlayer].UseChanceJailFreeCard();
    }

}
