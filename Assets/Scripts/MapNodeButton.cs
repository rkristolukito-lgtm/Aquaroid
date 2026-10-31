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

        bool isSelectable = MapManager.Instance.currentNode.connectedNextNodes.Contains(node) || node.isUnlocked;
        
        if (buttonComponent != null)
        {
            buttonComponent.interactable = isSelectable;
            buttonComponent.onClick.RemoveAllListeners();
            buttonComponent.onClick.AddListener(OnNodeClicked);
        }

        if (nodeImage != null)
        {
            if (node == MapManager.Instance.currentNode) 
                nodeImage.color = Color.yellow; 
            else if (isSelectable) 
                nodeImage.color = Color.green;  
            else 
                nodeImage.color = Color.gray;   
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