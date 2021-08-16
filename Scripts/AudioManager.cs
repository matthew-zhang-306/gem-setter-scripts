using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{

    public float MusicVolume {
        get {
            float db; 
            bgmSource.outputAudioMixerGroup.audioMixer.GetFloat("BGMVolume", out db);
            return DBToVolume(db);
        }
        set {
            PlayerPrefs.SetFloat("MusicVolume", value);
            bgmSource.outputAudioMixerGroup.audioMixer.SetFloat("BGMVolume", VolumeToDB(value));
        }
    }

    public float SoundVolume {
        get {
            float db;
            bgmSource.outputAudioMixerGroup.audioMixer.GetFloat("SFXVolume", out db);
            return DBToVolume(db);
        }
        set {
            PlayerPrefs.SetFloat("SoundVolume", value);
            bgmSource.outputAudioMixerGroup.audioMixer.SetFloat("SFXVolume", VolumeToDB(value));
        }
    }

    private void Start() {
        MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
        SoundVolume = PlayerPrefs.GetFloat("SoundVolume", 1.0f);

        InitSongs();
        InitSounds();
    }

    private void Update() {
        UpdateSongs();
        UpdateSounds();
    }


    // BACKGROUND MUSIC!

    [Header("MUSICS !!!")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource vinylSource;
    [SerializeField] private AudioClip themeSong;
    [SerializeField] private List<SongData> songs;

    public bool Vinyl {
        get { return PlayerPrefs.GetInt("vinyl", 1) > 0; }
        set { 
            if (vinylSource.isPlaying && !value) {
                vinylSource.Stop();
            }
            else if (!vinylSource.isPlaying && value) {
                vinylSource.Play();
            }

            PlayerPrefs.SetInt("vinyl", value ? 1 : 0);
        }
    }

    private int currentSong;
    private int songLoop;
    private float previousTime;

    public bool shouldPlayOnStart = true;
    private bool isPlayingTheme;

    public bool ShouldLoopForever { get; private set; }

    [SerializeField] private int numLoopsUntilFade = 0;
    [SerializeField] private float secondsUntilFade = 0;
    [SerializeField] private float fadeTime = 0;
    [SerializeField] private float silenceTime = 0;
    private Tween songFadeTween;

    public delegate void BGMAction(string songName, string author);
    public static BGMAction OnPlaySong;


    private void InitSongs() {
        for (int s = 0; s < songs.Count; s++) {
            songs[s].id = s + 1;
        }

        currentSong = 0;
        songLoop = 0;

        if (shouldPlayOnStart) {
            PlayCurrentSong();
        }

        // this looks useless but it runs the setter for Vinyl
        Vinyl = Vinyl;
    }

    private void UpdateSongs() {
        if (isPlayingTheme) {
            if (!bgmSource.isPlaying) {
                PlayCurrentSong();
            }
        }
        else {
            if (previousTime > bgmSource.time) {
                // the song has looped.
                songLoop = Mathf.Min(songLoop + 1, numLoopsUntilFade);
            }

            if (!ShouldLoopForever && songFadeTween == null && songLoop >= numLoopsUntilFade && bgmSource.time >= secondsUntilFade) {
                // start fading to another song
                songFadeTween = DOTween.Sequence()
                    .Append(bgmSource.DOFade(0, fadeTime))
                    .Append(bgmSource.DOFade(0, silenceTime))
                    .OnComplete(PlayNextSong);
            }

            previousTime = bgmSource.time;
        }
    }

    private void PlayNextSong() {
        if (ShouldLoopForever) {
            PlaySong(currentSong);
        }
        else {
            PlaySong((currentSong + 1) % songs.Count);
        }
    }

    private void PlaySong(int songId, bool doEvent = true) {
        currentSong = songId;
        songLoop = 0;
        previousTime = 0;

        // reset tween
        bgmSource.volume = 1;
        songFadeTween?.Kill();
        songFadeTween = null;

        bgmSource.clip = songs[currentSong].clip;
        bgmSource.loop = true;
        bgmSource.Play();

        isPlayingTheme = false;
    }

    public void SetLoopForever(bool value) {
        ShouldLoopForever = value;
    }

    public void CycleSong(bool isForward) {
        if (isPlayingTheme) {
            PlayCurrentSong();
            return;
        }

        if (isForward) {
            PlaySong((currentSong + 1) % songs.Count, false);
        } else {
            PlaySong((currentSong + songs.Count - 1) % songs.Count, false);
        }
    }

    public void PlayCurrentSong() {
        PlaySong(currentSong, false);
    }

    public void PlayTheme() {
        // reset tween
        bgmSource.volume = 1;
        songFadeTween?.Kill();
        songFadeTween = null;

        bgmSource.clip = themeSong;
        bgmSource.loop = false;
        bgmSource.Play();

        Debug.Log(bgmSource.clip);

        isPlayingTheme = true;
    }

    public string GetFullSongName() {
        if (isPlayingTheme) {
            return "00 - 1f1n1ty - theme";
        }
        else {
            return songs[currentSong].GetFullName();
        }
    }


    // SOUND EFFECTS!

    [Header("SOUNDS !!!")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private LoopingSoundPlayerData[] loopingSfxSources;


    private void PlaySound(AudioClip clip) {
        sfxSource.PlayOneShot(clip);
    }
    private void PlaySound(AudioClip[] clips) {
        PlaySound(clips[Random.Range(0, clips.Length)]);
    }
    private void PlaySound(OneShotSoundData soundData) {
        if (!soundData.hasPlayed) {
            PlaySound(soundData.clips);
            soundData.hasPlayed = true;
        }
    }
    private void PlaySound(string soundName) {
        PlaySound(soundDatas[soundName]);
    }

    private void PlayLoopingSound(AudioClip clip, int sourceId) {
        LoopingSoundPlayerData soundPlayer = loopingSfxSources[sourceId];
        soundPlayer.fadeTween?.Complete();
        soundPlayer.audioSource.clip = clip;
        soundPlayer.audioSource.Play();
    }

    private void StopLoopingSound(int sourceId) {
        LoopingSoundPlayerData soundPlayer = loopingSfxSources[sourceId];
        soundPlayer.fadeTween = soundPlayer.audioSource.DOFade(0, 0.1f).OnComplete(() => {
            soundPlayer.audioSource.Stop();
            soundPlayer.audioSource.volume = 1;
        });
    }

    private Dictionary<string, OneShotSoundData> soundDatas;

    [SerializeField] private AudioClip[] playerHitWallSounds;
    [SerializeField] private AudioClip[] playerMoveSounds;
    [SerializeField] private AudioClip[] playerWinSounds;
    [SerializeField] private AudioClip[] playerEpicWinSounds;
    [SerializeField] private AudioClip[] playerDieSounds;
    [SerializeField] private AudioClip[] iceAttachSounds;
    [SerializeField] private AudioClip[] iceDetachSounds;
    [SerializeField] private AudioClip[] iceDieSounds;
    [SerializeField] private AudioClip[] switchSounds;
    [SerializeField] private AudioClip[] blockPlaceSounds;
    [SerializeField] private AudioClip[] blockAlterSounds;
    [SerializeField] private AudioClip[] blockDeleteSounds;
    [SerializeField] private AudioClip[] blockButtonHoverSounds;
    [SerializeField] private AudioClip[] blockButtonSelectSounds;
    [SerializeField] private AudioClip[] uiHoverSounds;
    [SerializeField] private AudioClip[] uiPressSounds;
    [SerializeField] private AudioClip[] panelSweepSounds;
    [SerializeField] private AudioClip[] panelSwoopSounds;
    [SerializeField] private AudioClip[] alertSounds;

    private void InitSounds() {
        soundDatas = new Dictionary<string, OneShotSoundData>() {
            {"PlayerHitWallSounds"      , new OneShotSoundData(playerHitWallSounds)},
            {"PlayerWinSounds"          , new OneShotSoundData(playerWinSounds)},
            {"PlayerEpicWinSounds"      , new OneShotSoundData(playerEpicWinSounds)},
            {"PlayerDieSounds"          , new OneShotSoundData(playerDieSounds)},
            {"IceAttachSounds"          , new OneShotSoundData(iceAttachSounds)},
            {"IceDetachSounds"          , new OneShotSoundData(iceDetachSounds)},
            {"IceDieSounds"             , new OneShotSoundData(iceDieSounds)},
            {"SwitchSounds"             , new OneShotSoundData(switchSounds)},
            {"BlockPlaceSounds"         , new OneShotSoundData(blockPlaceSounds)},
            {"BlockAlterSounds"         , new OneShotSoundData(blockAlterSounds)},
            {"BlockDeleteSounds"        , new OneShotSoundData(blockDeleteSounds)},
            {"BlockButtonHoverSounds"   , new OneShotSoundData(blockButtonHoverSounds)},
            {"BlockButtonSelectSounds"  , new OneShotSoundData(blockButtonSelectSounds)},
            {"UIHoverSounds"            , new OneShotSoundData(uiHoverSounds)},
            {"UIPressSounds"            , new OneShotSoundData(uiPressSounds)},
            {"PanelSweepSounds"         , new OneShotSoundData(panelSweepSounds)},
            {"PanelSwoopSounds"         , new OneShotSoundData(panelSwoopSounds)},
            {"AlertSounds"              , new OneShotSoundData(alertSounds)}
        };
    }

    private void UpdateSounds() {
        foreach (OneShotSoundData soundData in soundDatas.Values) {
            soundData.hasPlayed = false;
        }
    }

    private void OnEnable() {
        if (Managers.AudioManager != this) {
            return;
        }
        
        PlayerController.OnHitWall += PlayHitWallSound;
        PlayerController.OnStartMove += PlayPlayerMoveSound;
        PlayerController.OnEndMove += StopPlayerMoveSound;

        PlayerController.OnWin += PlayPlayerWinSound;
        PlayerController.OnDie += PlayPlayerDieSound;

        PlayerController.OnAttach += PlayIceAttachSound;
        PlayerController.OnDetach += PlayIceDetachSound;
        AttachableBox.OnDie += PlayIceDieSound;

        Switcher.OnHitSwitch += PlaySwitchSound;

        WinCredits.OnWinCredits += PlayEpicWinSound;

        DrawTool.BlockAdded += PlayBlockAddedSound;
        DrawTool.BlockAltered += PlayBlockAlteredSound;
        DrawTool.BlockRemoved += PlayBlockRemovedSound;
        LevelBoundsButtons.OnResize += PlayBlockButtonHoverSound;

        UISlideAnimator.OnOpen += PlayPanelSweepSound;
        UISlideAnimator.OnClose += PlayPanelSwoopSound;
    
        AlertManager.OnAlert += PlayAlertSound;
    }

    private void OnDisable() {
        PlayerController.OnHitWall -= PlayHitWallSound;
        PlayerController.OnStartMove -= PlayPlayerMoveSound;
        PlayerController.OnEndMove -= StopPlayerMoveSound;

        PlayerController.OnWin -= PlayPlayerWinSound;
        PlayerController.OnDie -= PlayPlayerDieSound;

        PlayerController.OnAttach -= PlayIceAttachSound;
        PlayerController.OnDetach -= PlayIceDetachSound;
        AttachableBox.OnDie -= PlayIceDieSound;

        Switcher.OnHitSwitch -= PlaySwitchSound;

        WinCredits.OnWinCredits -= PlayEpicWinSound;

        DrawTool.BlockAdded -= PlayBlockAddedSound;
        DrawTool.BlockAltered -= PlayBlockAlteredSound;
        DrawTool.BlockRemoved -= PlayBlockRemovedSound;
        LevelBoundsButtons.OnResize -= PlayBlockButtonHoverSound;
    
        UISlideAnimator.OnOpen -= PlayPanelSweepSound;
        UISlideAnimator.OnClose -= PlayPanelSwoopSound;

        AlertManager.OnAlert -= PlayAlertSound;
    }


    private void PlayPlayerMoveSound(Vector2 _) => PlayLoopingSound(playerMoveSounds[Random.Range(0, playerMoveSounds.Length)], 0);
    private void StopPlayerMoveSound(Vector2 _) => StopLoopingSound(0);
    private void PlayHitWallSound()             => PlaySound("PlayerHitWallSounds"    );
    private void PlayPlayerWinSound()           => PlaySound("PlayerWinSounds"        );
    private void PlayEpicWinSound()             => PlaySound("PlayerEpicWinSounds"    );
    private void PlayPlayerDieSound()           => PlaySound("PlayerDieSounds"        );
    private void PlayIceAttachSound()           => PlaySound("IceAttachSounds"        );
    private void PlayIceDetachSound()           => PlaySound("IceDetachSounds"        );
    private void PlayIceDieSound()              => PlaySound("IceDieSounds"           );
    private void PlaySwitchSound()              => PlaySound("SwitchSounds"           );
    private void PlayBlockAddedSound()          => PlaySound("BlockPlaceSounds"       );
    private void PlayBlockAlteredSound()        => PlaySound("BlockAlterSounds"       );
    private void PlayBlockRemovedSound()        => PlaySound("BlockDeleteSounds"      );
    private void PlayBlockButtonHoverSound()    => PlaySound("BlockButtonHoverSounds" );
    private void PlayBlockButtonSelectSound()   => PlaySound("BlockButtonSelectSounds");
    public void PlayUIHoverSound()              => PlaySound("UIHoverSounds"          );
    public void PlayUIPressSound()              => PlaySound("UIPressSounds"          );
    public void PlayPanelSweepSound()           => PlaySound("PanelSweepSounds"       );
    public void PlayPanelSwoopSound()           => PlaySound("PanelSwoopSounds"       );
    public void PlayAlertSound()                => PlaySound("AlertSounds"            );



    // HELPERS

    [Serializable]
    public class SongData {
        [HideInInspector] public int id;
        public AudioClip clip;
        public float startLoopTime;
        public float endLoopTime;
        public string artist;
        public string name;

        public string GetFullName() {
            return $"0{id} - {artist} - {name}";
        }
    }

    [Serializable]
    public class LoopingSoundPlayerData {
        public AudioSource audioSource;
        [HideInInspector] public Tween fadeTween;
    }

    [Serializable]
    public class OneShotSoundData {
        public AudioClip[] clips;
        public bool hasPlayed;

        public OneShotSoundData(AudioClip[] clips) {
            this.clips = clips;
        }
    }

    private float VolumeToDB(float volume) => volume != 0 ? 40.0f * Mathf.Log10(volume) : -144.0f;
    private float DBToVolume(float db) => Mathf.Pow(10.0f, db / 40.0f);
}
