using System.Collections.Generic;
using UnityEngine;

public enum NodeType { Malkhut, Yesod, HodNezach, Tiferet, GevurahHesed, BinahHochmah, DaatSecret, Keter }

[System.Serializable]
public class MapNode
{
    public string nodeName;
    public NodeType type;
    public int sephiraNumber; // 10 down to 1 (or 0 for Da'at)
    
    [System.NonSerialized] 
    public List<MapNode> connectedNextNodes = new List<MapNode>();
    
    public bool isCompleted;
    public bool isUnlocked;
    public Vector2 uiPosition; 
}

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    [System.NonSerialized]
    public List<MapNode> allNodes = new List<MapNode>();
    
    [System.NonSerialized]
    public MapNode currentNode;

    // Track the sequence of Sephirot numbers visited in this run
    public List<int> visitedPath = new List<int>();
    
    public bool daatUnlocked = false;

    // Static flag so it survives scene loads and forces generation to unlock everything for testing
    public static bool debugUnlockAllPending = false;

    void Awake()
    {
        if (Instance == null) 
        { 
            Instance = this; 
            DontDestroyOnLoad(gameObject); 
        }
        else 
        { 
            Destroy(gameObject); 
        }
    }

    void Start()
    {
        GenerateKabbalahMap();
    }

    public void GenerateKabbalahMap()
    {
        allNodes.Clear();
        visitedPath.Clear();

        // 1. Create nodes with their corresponding Sephirot numbers
        MapNode malkuth   = new MapNode { nodeName = "10. Malkuth", type = NodeType.Malkhut, sephiraNumber = 10, isUnlocked = true, uiPosition = new Vector2(0, -360) };
        MapNode yesod     = new MapNode { nodeName = "9. Yesod",    type = NodeType.Yesod,   sephiraNumber = 9,  isUnlocked = false, uiPosition = new Vector2(0, -280) };
        MapNode hod       = new MapNode { nodeName = "8. Hod",      type = NodeType.HodNezach, sephiraNumber = 8, uiPosition = new Vector2(-140, -180) };
        MapNode netzach   = new MapNode { nodeName = "7. Netzach",  type = NodeType.HodNezach, sephiraNumber = 7, uiPosition = new Vector2(140, -180) };
        MapNode tiphareth = new MapNode { nodeName = "6. Tiphareth", type = NodeType.Tiferet, sephiraNumber = 6, uiPosition = new Vector2(0, -80) };
        MapNode geburah   = new MapNode { nodeName = "5. Geburah",  type = NodeType.GevurahHesed, sephiraNumber = 5, uiPosition = new Vector2(-140, 20) };
        MapNode chesed    = new MapNode { nodeName = "4. Chesed",   type = NodeType.GevurahHesed, sephiraNumber = 4, uiPosition = new Vector2(140, 20) };
        
        // Secret Abyss Node (0)
        MapNode daat      = new MapNode { nodeName = "11. Da'at (Abyss)", type = NodeType.DaatSecret, sephiraNumber = 0, uiPosition = new Vector2(0, 120) };
        
        MapNode binah     = new MapNode { nodeName = "3. Binah",    type = NodeType.BinahHochmah, sephiraNumber = 3, uiPosition = new Vector2(-140, 220) };
        MapNode chokhmah  = new MapNode { nodeName = "2. Chokhmah", type = NodeType.BinahHochmah, sephiraNumber = 2, uiPosition = new Vector2(140, 220) };
        MapNode kether    = new MapNode { nodeName = "1. Kether",   type = NodeType.Keter,   sephiraNumber = 1, uiPosition = new Vector2(0, 320) };

        // 2. Link Pathworking (Zig-zag lightning flash path)
        malkuth.connectedNextNodes.Add(yesod);
        
        yesod.connectedNextNodes.Add(hod);
        yesod.connectedNextNodes.Add(netzach);
        
        hod.connectedNextNodes.Add(netzach); 
        hod.connectedNextNodes.Add(tiphareth);
        
        netzach.connectedNextNodes.Add(tiphareth);
        
        tiphareth.connectedNextNodes.Add(geburah);
        tiphareth.connectedNextNodes.Add(chesed);

        geburah.connectedNextNodes.Add(chesed); 
        geburah.connectedNextNodes.Add(binah);

        chesed.connectedNextNodes.Add(binah);
        chesed.connectedNextNodes.Add(chokhmah);

        binah.connectedNextNodes.Add(chokhmah); 
        binah.connectedNextNodes.Add(kether);

        chokhmah.connectedNextNodes.Add(kether);

        // Secret Abyss route if unlocked via the exact lightning path or debug flag
        if (daatUnlocked || debugUnlockAllPending)
        {
            if (!kether.connectedNextNodes.Contains(daat))
            {
                kether.connectedNextNodes.Add(daat);
            }
        }

        allNodes.AddRange(new MapNode[] { malkuth, yesod, hod, netzach, tiphareth, geburah, chesed, daat, binah, chokhmah, kether });
        
        // 3. Handle Debug Unlock vs Normal Start State
        if (debugUnlockAllPending)
        {
            daatUnlocked = true;
            foreach (var node in allNodes)
            {
                node.isUnlocked = true;
            }
            currentNode = null;
            Debug.Log("DEBUG: All nodes forced unlocked during map generation.");
        }
        else
        {
            // Leave currentNode null at the start so Malkuth is green/selectable instead of yellow
            currentNode = null; 
        }
    }

    public void SelectNode(MapNode nextNode)
    {
        // Prevent re-entering already completed (blue) nodes unless debugging
        if (nextNode.isCompleted && !debugUnlockAllPending)
        {
            return;
        }

        // If debug unlock is active or it's the start of a normal run picking Malkuth
        if (debugUnlockAllPending || (currentNode == null && nextNode.isUnlocked))
        {
            currentNode = nextNode;
            visitedPath.Add(nextNode.sephiraNumber);
            LoadSceneForNodeType(nextNode.type);
            return;
        }

        // Standard movement logic for subsequent nodes
        if (currentNode != null && (currentNode.connectedNextNodes.Contains(nextNode) || nextNode.isUnlocked))
        {
            currentNode = nextNode;
            visitedPath.Add(nextNode.sephiraNumber); 
            LoadSceneForNodeType(nextNode.type);
        }
    }

    // Call this when all enemies are cleared in a combat room
    public void CompleteCurrentNode()
    {
        if (currentNode != null)
        {
            currentNode.isCompleted = true;
            currentNode.isUnlocked = false; // Lock the completed node

            // Automatically unlock all connected next nodes so the player can progress
            foreach (var nextNode in currentNode.connectedNextNodes)
            {
                if (!nextNode.isCompleted) // Only unlock nodes that haven't been beaten yet
                {
                    nextNode.isUnlocked = true;
                }
            }

            // When Kether (1) is completed, check if the player followed 10 -> 1 in order
            if (currentNode.type == NodeType.Keter || currentNode.sephiraNumber == 1)
            {
                if (CheckLightningPathCondition())
                {
                    UnlockDaatNode();
                }
            }
        }
    }

    private void UnlockDaatNode()
    {
        daatUnlocked = true;

        MapNode kether = allNodes.Find(n => n.type == NodeType.Keter || n.sephiraNumber == 1);
        MapNode daat = allNodes.Find(n => n.type == NodeType.DaatSecret);

        if (daat != null && !daat.isCompleted)
        {
            daat.isUnlocked = true;
        }

        if (kether != null && daat != null && !kether.connectedNextNodes.Contains(daat))
        {
            kether.connectedNextNodes.Add(daat);
        }
    }

    public bool CheckLightningPathCondition()
    {
        int[] lightningPath = new int[] { 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 };

        if (visitedPath.Count < lightningPath.Length) return false;

        for (int i = 0; i < lightningPath.Length; i++)
        {
            if (visitedPath[i] != lightningPath[i]) return false;
        }

        daatUnlocked = true;
        return true;
    }

    void LoadSceneForNodeType(NodeType type)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("CombatRoomScene"); 
    }

    public void ResetRunPath()
    {
        debugUnlockAllPending = false;
        visitedPath.Clear();
        daatUnlocked = false;
        GenerateKabbalahMap();
    }

    public void UnlockAllNodesForTesting()
    {
        debugUnlockAllPending = true;
        GenerateKabbalahMap();
    }
}