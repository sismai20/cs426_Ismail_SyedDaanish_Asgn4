using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class ObjectTimerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private bool trackAllMovableObjects = true;
    [FormerlySerializedAs("target")]
    [SerializeField] private MovableObject singleTarget;
    [FormerlySerializedAs("prefix")]
    [SerializeField] private string timerPrefix = "Time: ";
    [SerializeField] private bool showTimerOnlyWhenRunning = true;
    [SerializeField] private bool hideWhenGoalReached = true;
    [FormerlySerializedAs("goalText")]
    [SerializeField] private string goalLabel = "Goal Reached!";
    [FormerlySerializedAs("refreshTargetsInterval")]
    [SerializeField] private float refreshInterval = 0.5f;

    private MovableObject[] trackedObjects = System.Array.Empty<MovableObject>();
    private float nextRefreshAt;

    private void Awake()
    {
        if (timerText == null)
        {
            timerText = GetComponent<TMP_Text>();
        }
    }

    private void Update()
    {
        if (timerText == null) return;

        if (trackAllMovableObjects)
        {
            UpdateAllObjectTimers();
            return;
        }

        UpdateSingleObjectTimer();
    }

    private void UpdateAllObjectTimers()
    {
        if (Time.time >= nextRefreshAt || trackedObjects.Length == 0)
        {
            trackedObjects = FindObjectsByType<MovableObject>(FindObjectsSortMode.None);
            nextRefreshAt = Time.time + refreshInterval;
        }

        if (trackedObjects.Length == 0)
        {
            timerText.text = showTimerOnlyWhenRunning ? string.Empty : $"{timerPrefix}--";
            return;
        }

        StringBuilder lines = new StringBuilder();
        bool hasAnyLine = false;

        foreach (MovableObject obj in trackedObjects)
        {
            if (obj == null) continue;

            if (obj.HasReachedGoal())
            {
                if (hideWhenGoalReached) continue;

                AddLine(lines, $"{obj.name}: {goalLabel}", ref hasAnyLine);
                continue;
            }

            if (obj.IsTimerRunning())
            {
                string timerLine = $"{obj.name}: {timerPrefix}{obj.GetSyncedTimeRemaining():F1}";
                AddLine(lines, timerLine, ref hasAnyLine);
                continue;
            }

            if (!showTimerOnlyWhenRunning)
            {
                AddLine(lines, $"{obj.name}: {timerPrefix}--", ref hasAnyLine);
            }
        }

        timerText.text = hasAnyLine ? lines.ToString() : string.Empty;
    }

    private void UpdateSingleObjectTimer()
    {
        if (singleTarget == null)
        {
            singleTarget = FindFirstObjectByType<MovableObject>();
            if (singleTarget == null)
            {
                timerText.text = showTimerOnlyWhenRunning ? string.Empty : $"{timerPrefix}--";
                return;
            }
        }

        if (singleTarget.HasReachedGoal())
        {
            timerText.text = hideWhenGoalReached ? string.Empty : goalLabel;
            return;
        }

        if (!singleTarget.IsTimerRunning())
        {
            timerText.text = showTimerOnlyWhenRunning ? string.Empty : $"{timerPrefix}--";
            return;
        }

        timerText.text = timerPrefix + singleTarget.GetSyncedTimeRemaining().ToString("F1");
    }

    private static void AddLine(StringBuilder textBuilder, string line, ref bool hasAnyLine)
    {
        if (hasAnyLine) textBuilder.AppendLine();
        textBuilder.Append(line);
        hasAnyLine = true;
    }
}
