using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Script
{
    /// <summary>
    ///     Clock Manager and main runner for the app.
    /// </summary>
    public class ClockUI : MonoBehaviour
    {
        #region Private Methods

        /// <summary>
        ///     Set the internal time and the input field text to newTime
        /// </summary>
        /// <param name="newTime"> Time to set</param>
        private void UpdateTimeTo(float newTime)
        {
            _queuedTimerTime = newTime;
            inputField.text = _queuedTimerTime.ToString("N0");

            if (_state == State.Running)
                return;

            timerTimeSeconds = _queuedTimerTime == 0 ? 1 : _queuedTimerTime;
            _percentageOfIncreasePitchTime = Mathf.InverseLerp(0,timerTimeSeconds,timerTimeSeconds - increasePitchTime);
        }
        
        /// <summary>
        ///     Pitch the ticking sound when increasePitchTime is left in the timer.
        /// </summary>
        private void PitchTickingSound()
        {
            audioSource.pitch = currentTime > _percentageOfIncreasePitchTime
                ? Interpolator(1, 3, Mathf.InverseLerp(_percentageOfIncreasePitchTime, 1, currentTime))
                : 1;
            // _backgroundMaterial.SetFloat(TimeScaleProperty, audioSource.pitch);
        }

        #endregion

        #region Static Methods

        /// <summary>
        ///     Interpolate smoothly from a to b at point t.
        /// </summary>
        /// <param name="a"> from</param>
        /// <param name="b"> to</param>
        /// <param name="t"> distance</param>
        /// <returns> The interpolated value.</returns>
        private static float Interpolator(float a, float b, float t)
        {
            // return Mathf.SmoothStep(a, b, t);
            // return a + (b - a) * (3 * Mathf.Pow(t, 4) - 3 * Mathf.Pow(t, 8) + Mathf.Pow(t, 12));
            return a + (b - a) * (6 * Mathf.Pow(t, 5) - 15 * Mathf.Pow(t, 4) + 10 * Mathf.Pow(t, 3));
        }

        #endregion

        #region Inspector

        [SerializeField]
        [Tooltip("Called when timer reach 0.")]
        private UnityEvent onTimeUp;
        
        [SerializeField]
        [Header("Background")]
        [Tooltip("Background image.")]
        private Image backgroundImage;

        [Header("Timer Variables")]
        [SerializeField]
        [Tooltip("Timer time in seconds.")]
        private float timerTimeSeconds = 90;

        [SerializeField]
        [Tooltip("How much time should be left before starting to pitch the time.")]
        private float increasePitchTime = 20f;

        [SerializeField]
        [Tooltip("Percentage of radial indicator to fill.")]
        private float maxIndicator = 1.0f;

        [SerializeField]
        [Tooltip("Percentage of time since timer start.")]
        private float currentTime;

        [Header("UI Objects")]
        [SerializeField]
        [Tooltip("Input field for time")]
        private TMP_InputField inputField;

        [SerializeField]
        [Tooltip("Radial Indicator Image")]
        private Image radialIndicator;

        [SerializeField]
        [Tooltip("Digital output for timer")]
        private TextMeshProUGUI digitalCounter;

        [Header("Self Components")]
        [SerializeField]
        [Tooltip("AudioSource for timer.")]
        private AudioSource audioSource;

        [Header("Audio Clips")]
        [SerializeField]
        [Tooltip("Sound to play when timer ends")]
        private AudioClip gongSound;

        [SerializeField]
        [Tooltip("Sound to play during timer run")]
        private AudioClip tickingSound;

        #endregion

        #region Private Fields

        /// <summary>
        ///     Clock states.
        /// </summary>
        private enum State
        {
            Running,
            Ready,
            Paused,
            Ended
        }

        /// <summary>
        ///     Current state of the timer
        /// </summary>
        private State _state = State.Ready;

        /// <summary>
        ///     Next timer time.
        /// </summary>
        private float _queuedTimerTime;

        /// <summary>
        ///     Percentage of time to reach when staring to pitch sound.
        /// </summary>
        private float _percentageOfIncreasePitchTime;

        /// <summary>
        /// Material used by the background image.
        /// </summary>
        private Material _backgroundMaterial;

        /// <summary>
        /// Property id to change time scale in shader
        /// </summary>
        private static readonly int TimeScaleProperty = Shader.PropertyToID("_TimeScale");

        #endregion

        #region MonoBehaviour

        private void Start()
        {
            _backgroundMaterial = backgroundImage.material;
            _queuedTimerTime = timerTimeSeconds;
            _percentageOfIncreasePitchTime = Mathf.InverseLerp(0,timerTimeSeconds,timerTimeSeconds - increasePitchTime);
            ResetTimer();
        }

        // Update is called once per frame
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                Application.Quit();

            if (_state == State.Ready)
                digitalCounter.text = timerTimeSeconds.ToString("N0");

            if (_state != State.Running)
                return;

            currentTime += Time.deltaTime / timerTimeSeconds;
            digitalCounter.text = ((1 - currentTime) * timerTimeSeconds).ToString("N0");
            radialIndicator.fillAmount = currentTime;
            PitchTickingSound();

            if (currentTime < 1)
                return;

            audioSource.clip = gongSound;
            audioSource.pitch = 1;
            // _backgroundMaterial.SetFloat(TimeScaleProperty, 1);
            audioSource.loop = false;
            audioSource.Play();

            radialIndicator.fillAmount = maxIndicator;
            onTimeUp.Invoke();
            _state = State.Ended;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///     Change Timer state based on previous state.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void OnClick()
        {
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

        /// <summary>
        ///     Set timer state to Ready.
        /// </summary>
        public void ResetTimer()
        {
            _state = State.Ready;
            timerTimeSeconds = _queuedTimerTime == 0 ? 1 : _queuedTimerTime;
            _percentageOfIncreasePitchTime =  Mathf.InverseLerp(0,timerTimeSeconds,timerTimeSeconds - increasePitchTime);
            digitalCounter.text = timerTimeSeconds.ToString("N0");
            radialIndicator.fillAmount = 0;
            currentTime = 0;
            audioSource.clip = tickingSound;
            audioSource.loop = true;
            audioSource.pitch = 1;
            // _backgroundMaterial.SetFloat(TimeScaleProperty, 1);
            audioSource.Stop();
        }

        /// <summary>
        ///     Get new Timer time from Input field
        /// </summary>
        /// <param name="input"> New string to parse time from</param>
        public void ParseNewTime(string input)
        {
            float.TryParse(input, out var result);
            result = Mathf.Clamp(result, 0, 999);

            UpdateTimeTo(result);
        }

        /// <summary>
        ///     Update the time to current time + timeDelta
        /// </summary>
        /// <param name="timeDelta"> Time to add</param>
        public void AddTimeToInputField(float timeDelta)
        {
            float.TryParse(inputField.text, out var result);
            result = Mathf.Clamp(result + timeDelta, 0, 999);

            UpdateTimeTo(result);
        }

        #endregion
    }
}