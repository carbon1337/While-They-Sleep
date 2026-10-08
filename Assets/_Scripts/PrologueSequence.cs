using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PrologueSequence : MonoBehaviour
{

    public UnityEvent[] events;
    public int currentEventID = 0;

    public bool canAdvance = false;
    public float minWaitTime = 3f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartNextEvent();
    }

    // Update is called once per frame
    void Update()
    {
        if(Keyboard.current.spaceKey.wasPressedThisFrame && canAdvance)
        {
            StartNextEvent();
        }
    }

    public void StartNextEvent()
    {
        canAdvance = false;

        events[currentEventID].Invoke();

        if(currentEventID < events.Length)
        {
            currentEventID++;
        }

        StartCoroutine(WaitToAdvance());
    }

    private IEnumerator WaitToAdvance()
    {
        yield return new WaitForSeconds(minWaitTime);
        canAdvance = true;
    }
}
