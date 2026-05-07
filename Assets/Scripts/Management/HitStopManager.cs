using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitStopManager : MonoBehaviour
{
    public static HitStopManager Instance;

    bool active;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// ======================
    ///     PUBLIC ENTRY
    /// ======================
    public void TriggerHitStop(float freezeTime = 0.03f, float timeScale = 0.05f)
    {
        if (!active)
        {
            StartCoroutine(HitStopRoutine(freezeTime, timeScale));
        }
    }

    // =======================
    //      MAIN ROUTINE
    // =======================
    IEnumerator HitStopRoutine(float freezeTime, float timeScale)
    {
        active = true;

        // Slow/pause game
        Time.timeScale = timeScale;

        // Keep physics synced
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        // Use realtime so coroutine ignores timescale
        yield return new WaitForSecondsRealtime(freezeTime);

        // Restore game speed
        Time.timeScale = 1f;

        Time.fixedDeltaTime = 0.02f;

        active = false;
    }

}
