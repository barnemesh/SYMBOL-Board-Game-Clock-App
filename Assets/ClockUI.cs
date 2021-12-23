using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ClockUI : MonoBehaviour
{
    [SerializeField]
    private float timerTimeSeconds = 90;

    [SerializeField]
    private UnityEvent myEvent;

    [SerializeField]
    private float maxIndicator = 1.0f;

    [SerializeField]
    private float currentTime;

    [SerializeField]
    private Image radialIndicator;

    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private AudioClip gongSound;

    [SerializeField]
    private AudioClip tickingSound;

    [SerializeField]
    private TMP_InputField inputField;

    [SerializeField]
    private TextMeshProUGUI digitalCounter;

    private enum State
    {
        Running,
        Ready,
        Paused,
        Ended
    }

    private State _state = State.Ready;
    private float _queuedTimerTime;

    private void Start()
    {
        radialIndicator.fillAmount = 0;
        audioSource.clip = tickingSound;
        audioSource.loop = true;
        _queuedTimerTime = timerTimeSeconds;
        digitalCounter.text = timerTimeSeconds.ToString("N0");
    }

    // Update is called once per frame
    void Update()
    {
        if (_state == State.Ready)
        {
            digitalCounter.text = timerTimeSeconds.ToString("N0");
        }

        if (_state != State.Running) return;

        currentTime += Time.deltaTime / timerTimeSeconds;
        digitalCounter.text = ((1 - currentTime) * timerTimeSeconds).ToString("N0");
        radialIndicator.fillAmount = currentTime;
        audioSource.pitch = 1 + 2 * currentTime;

        if (currentTime < 1)
            return;
        audioSource.clip = gongSound;
        audioSource.pitch = 1;
        audioSource.loop = false;
        audioSource.Play();

        radialIndicator.fillAmount = maxIndicator;
        myEvent.Invoke();
        _state = State.Ended;
    }

    public void OnClick()
    {
        print(_state);
        switch (_state)
        {
            case State.Running:
                _state = State.Paused;
                audioSource.Stop();
                break;
            case State.Paused:
            case State.Ready:
                _state = State.Running;
                audioSource.Play();
                break;
            case State.Ended:
                ResetTimer();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void ResetTimer()
    {
        _state = State.Ready;
        digitalCounter.text = timerTimeSeconds.ToString("N0");
        timerTimeSeconds = _queuedTimerTime;
        radialIndicator.fillAmount = 0;
        currentTime = 0;
        audioSource.clip = tickingSound;
        audioSource.loop = true;
        audioSource.pitch = 1;
        audioSource.Stop();
    }

    public void UpdateTime()
    {
        float.TryParse(inputField.text, out var result);
        _queuedTimerTime = Mathf.Clamp(result , 1, 999);
        inputField.text = _queuedTimerTime.ToString("N0");

        if (_state == State.Running) return;

        timerTimeSeconds = _queuedTimerTime;
    }

    public void UpdateInputField(float timeDelta)
    {
        float.TryParse(inputField.text, out var result);
        result = Mathf.Clamp(result + timeDelta, 1, 999);
        inputField.text = result.ToString("N0");
    }
}