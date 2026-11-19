using System.Collections.Generic;
using UnityEngine;

public class SnakeBossController : MonoBehaviour
{
    public GameObject segmentPrefab;
    public int totalSegments = 6;
    public float segmentSpacing = 0.8f;
    public float moveSpeed = 4f;

    private List<SnakeSegment> activeSegments = new List<SnakeSegment>();

    // Wandering variables
    private Vector2 currentWanderTarget;
    private float wanderTimer = 0f;
    private float wanderInterval = 3f; // Picks a new direction every 3 seconds

    void Start()
    {
        SpawnSnake();
        PickNewWanderTarget();
    }

    void Update()
    {
        MoveHeadRandomly();
        UpdateSegmentFollow();
        HandleScreenWrap();
    }

    void SpawnSnake()
    {
        for (int i = 0; i < totalSegments; i++)
        {
            Vector3 spawnPos = transform.position - new Vector3(0, i * segmentSpacing, 0);
            GameObject segObj = Instantiate(segmentPrefab, spawnPos, Quaternion.identity);

            SnakeSegment segment = segObj.GetComponent<SnakeSegment>();
            segment.segmentIndex = i;
            segment.bossController = this;

            activeSegments.Add(segment);
        }
    }

    void PickNewWanderTarget()
    {
        // Pick a random point within the screen bounds (or wider arena bounds)
        Vector2 randomViewport = new Vector2(Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.9f));
        currentWanderTarget = Camera.main.ViewportToWorldPoint(randomViewport);
    }

    void MoveHeadRandomly()
    {
        if (activeSegments.Count == 0 || activeSegments[0] == null) return;

        Transform headTransform = activeSegments[0].transform;

        // Update wander timer to pick a new destination periodically
        wanderTimer += Time.deltaTime;
        if (wanderTimer >= wanderInterval || Vector2.Distance(headTransform.position, currentWanderTarget) < 1f)
        {
            PickNewWanderTarget();
            wanderTimer = 0f;
        }

        // Move toward the random wander target
        Vector2 direction = (currentWanderTarget - (Vector2)headTransform.position).normalized;
        headTransform.position += (Vector3)(direction * moveSpeed * Time.deltaTime);

        // Smoothly rotate head to face movement direction
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
        headTransform.rotation = Quaternion.RotateTowards(headTransform.rotation, targetRotation, 180f * Time.deltaTime);
    }

    void UpdateSegmentFollow()
    {
        for (int i = 1; i < activeSegments.Count; i++)
        {
            if (activeSegments[i] == null || activeSegments[i - 1] == null) continue;

            Transform leader = activeSegments[i - 1].transform;
            Transform follower = activeSegments[i].transform;

            Vector3 targetPos = leader.position - (leader.up * segmentSpacing);
            follower.position = Vector3.Lerp(follower.position, targetPos, Time.deltaTime * 15f);
            follower.rotation = Quaternion.Slerp(follower.rotation, leader.rotation, Time.deltaTime * 15f);
        }
    }

    void HandleScreenWrap()
    {
        if (activeSegments.Count == 0 || activeSegments[0] == null) return;

        Transform headTransform = activeSegments[0].transform;
        Vector3 viewportPosition = Camera.main.WorldToViewportPoint(headTransform.position);
        Vector3 moveAdjustment = Vector3.zero;

        bool hasWrapped = false;
        if (viewportPosition.x < 0) { moveAdjustment.x = 1; hasWrapped = true; }
        else if (viewportPosition.x > 1) { moveAdjustment.x = -1; hasWrapped = true; }

        if (viewportPosition.y < 0) { moveAdjustment.y = 1; hasWrapped = true; }
        else if (viewportPosition.y > 1) { moveAdjustment.y = -1; hasWrapped = true; }

        if (hasWrapped)
        {
            Vector3 oldHeadPos = headTransform.position;
            Vector3 newHeadPos = Camera.main.ViewportToWorldPoint(viewportPosition + moveAdjustment);
            Vector3 wrapOffset = newHeadPos - oldHeadPos;

            // Also shift the wander target so the head doesn't snap backward trying to reach an old point
            currentWanderTarget += (Vector2)wrapOffset;

            for (int i = 0; i < activeSegments.Count; i++)
            {
                if (activeSegments[i] != null)
                {
                    activeSegments[i].transform.position += wrapOffset;
                }
            }
        }
    }

    public void OnSegmentDestroyed(SnakeSegment destroyedSegment)
    {
        int index = activeSegments.IndexOf(destroyedSegment);
        if (index != -1)
        {
            activeSegments.RemoveAt(index);

            for (int i = 0; i < activeSegments.Count; i++)
            {
                if (activeSegments[i] != null)
                {
                    activeSegments[i].segmentIndex = i;
                }
            }
        }

        if (activeSegments.Count == 0)
        {
            Debug.Log("Kether Defeated!");

            if (MapManager.Instance != null)
            {
                MapManager.Instance.CompleteCurrentNode();
            }

            GameManager gm = FindFirstObjectByType<GameManager>();
            if (gm != null)
            {
                gm.NotifyEnemyDefeated();
            }
        }
    }
}