using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapRenderer : MonoBehaviour
{
    public GameObject nodeButtonPrefab; 
    public Transform mapPanelContainer; 

    // Use OnEnable instead of Start so it refreshes every time you return to the map scene
    void OnEnable()
    {
        RenderMap();
    }

    void RenderMap()
    {
        if (MapManager.Instance == null) return;

        foreach (Transform child in mapPanelContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var node in MapManager.Instance.allNodes)
        {
            if (node.type == NodeType.DaatSecret && !MapManager.Instance.daatUnlocked)
                continue;

            GameObject btnObj = Instantiate(nodeButtonPrefab, mapPanelContainer);
            RectTransform rect = btnObj.GetComponent<RectTransform>();
            rect.anchoredPosition = node.uiPosition;

            MapNodeButton nodeButton = btnObj.GetComponent<MapNodeButton>();
            if (nodeButton != null)
            {
                nodeButton.SetupButton(node);
            }
            else
            {
                TextMeshProUGUI txt = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) txt.text = node.nodeName.Split(' ')[0]; 

                Button btn = btnObj.GetComponent<Button>();
                MapNode capturedNode = node;
                btn.onClick.AddListener(() => {
                    MapManager.Instance.SelectNode(capturedNode);
                });
            }
        }
    }
}