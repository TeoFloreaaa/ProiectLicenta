using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ManageUI : MonoBehaviour
{
    public static ManageUI instance;

    [SerializeField] GameObject managePanel; //TO SHOW AND HIDE
    [SerializeField] Transform propertyGrid; //TO PARENT PROPERTY SETS TO IT
    [SerializeField] GameObject propertySetPrefab; //
    [SerializeField] TMP_Text yourMoneyText;
    [SerializeField] TMP_Text systemMessageText;

    Player playerReference;
    List<GameObject> propertyPrefabs = new List<GameObject>();

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        managePanel.SetActive(false);
    }

    public void OpenManager() //CALL FROM MANAGE BUTTON
    {
        playerReference = GameManager.instance.GetCurrentPlayer;

        CreateProperties();

        managePanel.SetActive(true);

        UpdateMoneyText();
    }

    public void CloseManager()
    {
        managePanel.SetActive(false);
        ClearProperties();
    }

    void ClearProperties()
    {
        for (int i = propertyPrefabs.Count - 1; i >= 0; i--)
        {
            Destroy(propertyPrefabs[i]);
        }
        propertyPrefabs.Clear();
    }

    void CreateProperties()
    {

        //GET ALL NODES AS NODE SETS
        List<MonopolyNode> processedSet = null;

        foreach (var node in playerReference.GetMonopolyNodes)
        {
            var (list, allSame) = MonopolyBoard.Instance.PlayerHasAllNodesOfSet(node);
            List<MonopolyNode> nodeSet = new List<MonopolyNode>();
            nodeSet.AddRange(list);


            if (nodeSet != null && list != processedSet)
            {
                //UPDATE PROCESSED FIRST
                processedSet = list;

                nodeSet.RemoveAll(n => n.Owner != playerReference);
                //CREATE PREFAB WITH ALL NODES OWNED BY THE PLAYER
                GameObject newPropertySet = Instantiate(propertySetPrefab, propertyGrid, false);
                newPropertySet.GetComponent<ManagePropertyUi>().SetProperty(nodeSet, playerReference);
                propertyPrefabs.Add(newPropertySet);
            }
        }
    }

    public void UpdateMoneyText()
    {
        string showMoney = (playerReference.ReadMoney >= 0)
            ? "<color=green>$" + playerReference.ReadMoney + "</color>"
            : "<color=red>$" + playerReference.ReadMoney + "</color>";

        yourMoneyText.text = "<color=black>Banii tai:</color> " + showMoney;
    }

    public void UpdateSystemMessage(string message)
    {
        systemMessageText.text = message;
    }

    public void AutoHandleFunds() //CALL FROM BUTTON
    {
        if (playerReference.ReadMoney > 0)
        {
            UpdateSystemMessage("Ai deja destui bani!");
            return;
        }

        playerReference.HandleInsufficientFunds(Mathf.Abs(playerReference.ReadMoney));

        //UPDATE THE UI
        ClearProperties();
        CreateProperties();
        UpdateMoneyText();  
    }


}
