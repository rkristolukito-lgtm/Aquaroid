using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapNodeButton : MonoBehaviour
{
    public MapNode assignedNode;
    public TextMeshProUGUI nodeNameText;
    public Button buttonComponent;
    public Image nodeImage;

    public void SetupButton(MapNode node)
    {
        assignedNode = node;
        
        if (nodeNameText != null) 
            nodeNameText.text = node.nodeName;

        bool isCurrent = (node == MapManager.Instance.currentNode);
        
        // A node is selectable if it's unlocked (or if no current node is selected yet at the start)
        bool isSelectable = node.isUnlocked && !isCurrent;

        if (buttonComponent != null)
        {
            buttonComponent.interactable = isSelectable;
            buttonComponent.onClick.RemoveAllListeners();
            buttonComponent.onClick.AddListener(OnNodeClicked);
        }

        if (nodeImage != null)
        {
            if (isCurrent) 
                nodeImage.color = Color.yellow; // Active room you are on
            else if (node.isCompleted)
                nodeImage.color = Color.blue;   // Beaten nodes
            else if (node.isUnlocked) 
                nodeImage.color = Color.green;  // Unlocked / Debug test available
            else 
                nodeImage.color = Color.gray;   // Locked
        }
    }

    private void OnNodeClicked()
    {
        if (assignedNode != null && MapManager.Instance != null)
        {
            MapManager.Instance.SelectNode(assignedNode);
        }
    }
}