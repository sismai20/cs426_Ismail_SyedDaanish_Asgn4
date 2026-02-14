using UnityEngine;
using TMPro;

public class GoalCounterUI : MonoBehaviour
{
    [SerializeField] private TMP_Text counterText;
    [SerializeField] private int totalGoals = 10;
    [SerializeField] private float refreshInterval = 0.5f;

    private int goalsReached = 0;
    private MovableObject[] trackedObjects = System.Array.Empty<MovableObject>();
    private float nextRefreshAt;

    private void Update()
    {
        if (Time.time >= nextRefreshAt || trackedObjects.Length == 0)
        {
            trackedObjects = FindObjectsByType<MovableObject>(FindObjectsSortMode.None);
            nextRefreshAt = Time.time + refreshInterval;
        }

        // count how many have reached their goal
        int count = 0;
        foreach (var obj in trackedObjects)
        {
            if (obj != null && obj.HasReachedGoal())
                count++;
        }

        // only ever go up, not back down when objects despawn
        if (count > goalsReached)
            goalsReached = count;

        if (goalsReached >= totalGoals)
            counterText.text = "You Win!";
        else
            counterText.text = $"{goalsReached}/{totalGoals}";
    }
}