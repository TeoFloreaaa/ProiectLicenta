using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class TradingSystem : MonoBehaviour
{
    //MESSAGE SYSTEM
    public delegate void UpdateMessage(string message);
    public static UpdateMessage OnUpdateMessage;

    [SerializeField] GameObject cardPrefab;
    [SerializeField] GameObject tradePanel;
    [SerializeField] GameObject resultPanel;
    [SerializeField] TMP_Text resultMessageText;

    [Header("LEFT SIDE")]
    [SerializeField] TMP_Text leftOffererNameText;
    [SerializeField] Transform leftCardGrid;
    [SerializeField] ToggleGroup leftToggleGroup; //TO TOGGLE THE CARD SELECTION
    [SerializeField] TMP_Text leftYourMoneyText;
    [SerializeField] TMP_Text leftOfferMoney;
    [SerializeField] Slider leftMoneySlider;

    List<GameObject> leftCardPrefabList = new List<GameObject>();

    Player leftPlayerReference;

    [Header("MIDDLE")]
    [SerializeField] Transform buttonGrid;
    [SerializeField] GameObject playerButtonPrefab;
    List<GameObject> playerButtonList = new List<GameObject>();

    [Header("RIGHT SIDE")]
    [SerializeField] TMP_Text rightOffererNameText;
    [SerializeField] Transform rightCardGrid;
    [SerializeField] ToggleGroup rightToggleGroup; //TO TOGGLE THE CARD SELECTION
    [SerializeField] TMP_Text rightYourMoneyText;
    [SerializeField] TMP_Text rightOfferMoney;
    [SerializeField] Slider rightMoneySlider;

    [Header("Trade Offer Panel")]
    [SerializeField] GameObject tradeOfferPanel;
    [SerializeField] TMP_Text leftMessageText, rightMessageText, leftMoneyText, rightMoneyText;
    [SerializeField] GameObject leftCard, rightCard;
    [SerializeField] Image leftColorField, rightColorField;
    [SerializeField] Image leftPropImage, rightPropImage;
    [SerializeField] Sprite houseSprite, railroadSprite, utilitySprite;
    [SerializeField] TMP_Text leftPropertyPrice, rightPropertyPrice;

    //STORE THE OFFER FOR HUMAN
    Player currentPlayer, nodeOwner;
    MonopolyNode requestedNode, offeredNode;
    int requestedMoney, offeredMoney;


    List<GameObject> rightCardPrefabList = new List<GameObject>();

    Player rightPlayerReference;

    public static TradingSystem instace;

    private void Start()
    {
        tradePanel.SetActive(false);
        resultPanel.SetActive(false);
        tradeOfferPanel.SetActive(false);
    }

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
        //CONTINUE IF NOTHING WAS FOUND
        if(requestedNode == null) 
        {
            currentPlayer.ChangeState(Player.AiStates.IDLE);
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
            ShowTradeOfferPanel(currentPlayer, nodeOwner, requestedNode, offeredNode, offeredMoney, requestedMoney);
        }

    }

    //--------------------CONSIDER TRADE OFFER-------------------------------AI

    void ConsiderTradeOffer(Player currentPlayer, Player nodeOwner, MonopolyNode requestedNode, MonopolyNode offeredNode, int offeredMoney, int requestedMoney)
    {
        int valueOfTheTrade = (CalculateValueOfNode(requestedNode) + requestedMoney) - (CalculateValueOfNode(offeredNode) + offeredMoney);
        
        //SELL A NODE FOR MONEY ONLY
        if (requestedNode == null && offeredNode != null && requestedMoney <= nodeOwner.ReadMoney / 3 && !MonopolyBoard.Instance.PlayerHasAllNodesOfSet(requestedNode).allSame)
        {
            Trade(currentPlayer, nodeOwner, requestedNode, offeredNode, offeredMoney, requestedMoney);
            if (currentPlayer.playerType != Player.PlayerType.AI)
            {
                TradeResult(true);
            }
            return;
        }

        if (valueOfTheTrade <= 0 && !MonopolyBoard.Instance.PlayerHasAllNodesOfSet(requestedNode).allSame)
        {
            //TRADE THE NODE IS VALID
            Trade(currentPlayer, nodeOwner, requestedNode, offeredNode, offeredMoney, requestedMoney);
            if (currentPlayer.playerType != Player.PlayerType.AI)
            {
                TradeResult(true);
            }
        }
        else
        {
            if (currentPlayer.playerType != Player.PlayerType.AI)
            {
                TradeResult(false);
            }
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
        //HIDE UI FOR HUMAN ONLY
        CloseTradePanel();

        if (currentPlayer.playerType == Player.PlayerType.AI)
        {
            currentPlayer.ChangeState(Player.AiStates.IDLE);
        }

    }

    //-----------------------------USER INTERFACE CONTENT-----------------------------HUMAN

    public void CloseTradePanel()
    {
        tradePanel.SetActive(false);
        ClearAll();
    }

    public void OpenTradePanel()
    {
        leftPlayerReference = GameManager.instance.GetCurrentPlayer;
        rightOffererNameText.text = "Selexteaza un jucator: ";

        CreateLeftPanel();

        CreateMiddleButtons();
    }

    //-----------------------------CURRENT PLAYER-----------------------------HUMAN

    public void CreateLeftPanel()
    {

        leftOffererNameText.text = leftPlayerReference.name;

        List<MonopolyNode> referenceNode = leftPlayerReference.GetMonopolyNodes;

        for (int i = 0; i < referenceNode.Count; i++)
        {
            GameObject tradeCard = Instantiate(cardPrefab, leftCardGrid, false);
            //SET UP THE ACTUAL CARD CONTENT
            tradeCard.GetComponent<TradePropertyCard>().SetTradeCard(referenceNode[i], leftToggleGroup);


            leftCardPrefabList.Add(tradeCard);
        }

        leftYourMoneyText.text = "Your Money: " + leftPlayerReference.ReadMoney;
        //SET UP THE MONEY SLIDER AND TEXT

        leftMoneySlider.maxValue = leftPlayerReference.ReadMoney;
        leftMoneySlider.minValue = 0;
        UpdateLeftSlider(leftMoneySlider.minValue);
        //leftMoneySlider.onValueChanged.AddListener(UpdateLeftSlider);
        //RESET OLD CONTENT

        tradePanel.SetActive(true);


    }

    public void UpdateLeftSlider(float value)
    {
        leftOfferMoney.text = "Oferta: $" + leftMoneySlider.value;
    }

    //-----------------------------SELECTED PLAYER-----------------------------HUMAN

    public void ShowRightPlayer(Player player)
    {
        //RESET THE CURERNT CONTENT
        rightPlayerReference = player;
        //SHOW RIGHT PLAYER OF ABOVE PLAYER
        ClearRightPanel();
        //UPDATE THE MOBNEY AND THE SLIDER
        rightOffererNameText.text = rightPlayerReference.name;

        List<MonopolyNode> referenceNode = rightPlayerReference.GetMonopolyNodes;

        for (int i = 0; i < referenceNode.Count; i++)
        {
            GameObject tradeCard = Instantiate(cardPrefab, rightCardGrid, false);
            //SET UP THE ACTUAL CARD CONTENT
            tradeCard.GetComponent<TradePropertyCard>().SetTradeCard(referenceNode[i], rightToggleGroup);

            rightCardPrefabList.Add(tradeCard);
        }

        rightYourMoneyText.text = "Your Money: " + rightPlayerReference.ReadMoney;
        //SET UP THE MONEY SLIDER AND TEXT
        rightMoneySlider.maxValue = rightPlayerReference.ReadMoney;
        rightMoneySlider.value = 0;
        UpdateLeftSlider(rightMoneySlider.value);
    }

    public void UpdateRightSlider(float value)
    {
        rightOfferMoney.text = "Requested Money: $ " + rightMoneySlider.value;
    }

    //-----------------------------SET UP MIDDLE-------------------------------HUMAN
    
    void CreateMiddleButtons()
    {
        //CLEAR CONTENT
        for (int i = playerButtonList.Count - 1; i >= 0; i--)
        {
            Destroy(playerButtonList[i]);
        }
        playerButtonList.Clear();

        //LOOP THROUGH ALL PLAYER
        List<Player> allPlayers = new List<Player>();
        allPlayers.AddRange(GameManager.instance.GetPlayers);
        allPlayers.Remove(leftPlayerReference);

        // AND THE BUTTONS FOR THEM
        foreach (var player in allPlayers)
        {
            GameObject newPlayerButton = Instantiate(playerButtonPrefab, buttonGrid, false);
            newPlayerButton.GetComponent<TradePlayerButton>().SetPlayer(player);

            playerButtonList.Add(newPlayerButton);
        }
    }

    //----------------------------CLEAR CONTENT---------------------------------HUMAN

    void ClearAll() //IF WE OPEN OR CLOSE TRADE SYSTEM
    {
        rightOffererNameText.text = "Selexteaza un jucator";
        rightYourMoneyText.text = "Banii tai: $0";
        rightMoneySlider.maxValue = 0;
        rightMoneySlider.value = 0;
        UpdateRightSlider(rightMoneySlider.value);

        //CLEAR MIDDEL BUTTONS
        for (int i = playerButtonList.Count - 1; i >= 0; i--)
        {
            Destroy(playerButtonList[i]);
        }
        playerButtonList.Clear();

        //CLEAR LEFT CARD CONTENT
        for (int i = leftCardPrefabList.Count - 1; i >= 0; i--)
        {
            Destroy(leftCardPrefabList[i]);
        }
        leftCardPrefabList.Clear();

        //CLEAR RIGHT CARD CONTENT
        for (int i = rightCardPrefabList.Count - 1; i >= 0; i--)
        {
            Destroy(rightCardPrefabList[i]);
        }
        rightCardPrefabList.Clear();
    }

    void ClearRightPanel()
    {
        for (int i = rightCardPrefabList.Count - 1; i >= 0; i--)
        {
            Destroy(rightCardPrefabList[i]);
        }
        rightCardPrefabList.Clear();

        //RESET THE SLIDER
        //SET UP THE MONEY SLIDER AND TEXT
        rightMoneySlider.maxValue = rightPlayerReference.ReadMoney;
        rightMoneySlider.value = 0;
        UpdateLeftSlider(rightMoneySlider.value);
    }

    //-----------------------------MAKE TRADE OFFER-----------------------------HUMAN

    public void MakeOfferButton()
    {
        MonopolyNode requestedNode = null;
        MonopolyNode offeredNode = null;

        if (rightPlayerReference == null)
        {
            //ERROR
            return;
        }

        //LEFT SELECTED NODE
        Toggle offeredToggle = leftToggleGroup.ActiveToggles().FirstOrDefault();
        if (offeredToggle != null)
        {
            offeredNode = offeredToggle.GetComponentInParent<TradePropertyCard>().Node();
        }

        //RIGHT SELECTED NODE
        Toggle requestedToggle = rightToggleGroup.ActiveToggles().FirstOrDefault();
        if (requestedToggle != null)
        {
            requestedNode = requestedToggle.GetComponentInParent<TradePropertyCard>().Node();
        }


        MakeTradeOffer(leftPlayerReference, rightPlayerReference, requestedNode, offeredNode, (int)leftMoneySlider.value, (int)rightMoneySlider.value);
    }

    //-----------------------TRADE RESULT----------------------------------------------------

    void TradeResult(bool accepted)
    {
        if (accepted)
        {
            resultMessageText.text = rightPlayerReference.name + "<b><color=green> a acceptat</color></b>" + " schimbul";
        }
        else
        {
            resultMessageText.text = rightPlayerReference.name + "<b><color=red> nu a acceptat</color></b>" + " schimbul";
        }

        resultPanel.SetActive(true);
    }

    //---------------------------------------TRADE OFFER PANEL-------------------------------

    void ShowTradeOfferPanel(Player _currentPlayer, Player _nodeOwner, MonopolyNode _requestedNode, MonopolyNode _offeredNode, int _offeredMoney, int _requestedMoney)
    {
        //FILL THE ACTUAL OFFER CONTENT
        currentPlayer = _currentPlayer;
        nodeOwner = _nodeOwner;
        requestedNode = _requestedNode;
        offeredNode = _offeredNode;
        requestedMoney = _requestedMoney;
        offeredMoney = _offeredMoney;

        //SHOW PANEL CONTENT
        tradeOfferPanel.SetActive(true);
        leftMessageText.text = currentPlayer.name + " offers:";
        rightMessageText.text = "For " + nodeOwner.name + "'s:";
        leftMoneyText.text = "-$" + offeredMoney;
        rightMoneyText.text = "-$" + requestedMoney;
        leftCard.SetActive(offeredNode != null ? true : false);
        rightCard.SetActive(requestedNode != null ? true : false);

        if (leftCard.activeInHierarchy)
        {
            leftColorField.color = (offeredNode.propertyColorField != null) ? offeredNode.propertyColorField.color : Color.black;
            switch (offeredNode.monopolyNodeType)
            {
                case MonopolyNodeType.Property:
                    leftPropImage.sprite = houseSprite;
                    leftPropImage.color = Color.blue;
                    break;
                case MonopolyNodeType.Railroad:
                    leftPropImage.sprite = railroadSprite;
                    leftPropImage.color = Color.white;
                    break;
                case MonopolyNodeType.Utility:
                    leftPropImage.sprite = utilitySprite;
                    leftPropImage.color = Color.black;
                    break;
            }
            leftPropertyPrice.text = "$"+offeredNode.price;
        }
        if (rightCard.activeInHierarchy)
        {
            rightColorField.color = (requestedNode.propertyColorField != null) ? requestedNode.propertyColorField.color : Color.black;
            switch (requestedNode.monopolyNodeType)
            {
                case MonopolyNodeType.Property:
                    rightPropImage.sprite = houseSprite;
                    rightPropImage.color = Color.blue;
                    break;
                case MonopolyNodeType.Railroad:
                    rightPropImage.sprite = railroadSprite;
                    rightPropImage.color = Color.white;
                    break;
                case MonopolyNodeType.Utility:
                    rightPropImage.sprite = utilitySprite;
                    rightPropImage.color = Color.black;
                    break;
            }
            rightPropertyPrice.text = "$" + requestedNode.price;
        }
    }

    public void AcceptOffer()
    {
        Trade(currentPlayer, nodeOwner, requestedNode, offeredNode, offeredMoney, requestedMoney);
        ResetOffer();
    }

    public void RejectOffer()
    {
        currentPlayer.ChangeState(Player.AiStates.IDLE);
        ResetOffer();
    }

    void ResetOffer()
    {
        currentPlayer = null;
        nodeOwner = null;
        requestedNode = null;
        offeredNode = null;
        requestedMoney = 0;
        offeredMoney = 0;
    }

}
