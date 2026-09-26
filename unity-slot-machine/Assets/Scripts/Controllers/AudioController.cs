using UnityEngine;

namespace SlotMachine.Controllers
{
    /// <summary>
    /// Manages game audio sources and audio clip triggers for slot reels, buttons, wins, and jackpots.
    /// </summary>
    public class AudioController : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioSource _musicSource;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip _spinClip;
        [SerializeField] private AudioClip _reelStopClip;
        [SerializeField] private AudioClip _winClip;
        [SerializeField] private AudioClip _jackpotClip;
        [SerializeField] private AudioClip _bonusTriggerClip;

        public void PlaySpinSound()
        {
            if (_sfxSource != null && _spinClip != null)
                _sfxSource.PlayOneShot(_spinClip);
        }

        public void PlayReelStopSound()
        {
            if (_sfxSource != null && _reelStopClip != null)
                _sfxSource.PlayOneShot(_reelStopClip);
        }

        public void PlayWinSound()
        {
            if (_sfxSource != null && _winClip != null)
                _sfxSource.PlayOneShot(_winClip);
        }

        public void PlayJackpotSound()
        {
            if (_sfxSource != null && _jackpotClip != null)
                _sfxSource.PlayOneShot(_jackpotClip);
        }

        public void PlayBonusTriggerSound()
        {
            if (_sfxSource != null && _bonusTriggerClip != null)
                _sfxSource.PlayOneShot(_bonusTriggerClip);
        }
    }
}
