using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class UiShowPanel : MonoBehaviour
{
    [SerializeField] GameObject humanPanel;
    [SerializeField] Button rollDiceButton;
    [SerializeField] Button endTurnButton;

    void OnEnable()
    {
        GameManager.OnShowHumanPanel += ShowPanel;
        MonopolyNode.OnShowHumanPanel += ShowPanel;
        ChanceField.OnShowHumanPanel += ShowPanel;
        CommunityChest.OnShowHumanPanel += ShowPanel;
        Player.OnShowHumanPanel += ShowPanel;
    }

    void OnDisable()
    {
        GameManager.OnShowHumanPanel -= ShowPanel;
        MonopolyNode.OnShowHumanPanel -= ShowPanel;
        ChanceField.OnShowHumanPanel -= ShowPanel;
        CommunityChest.OnShowHumanPanel -= ShowPanel;
        Player.OnShowHumanPanel -= ShowPanel;
    }

    void ShowPanel(bool showPanel, bool enableRollDice, bool enableEndTurn)
    {
        humanPanel.SetActive(showPanel);
        rollDiceButton.interactable = enableRollDice;
        endTurnButton.interactable = enableEndTurn;
    }
}
