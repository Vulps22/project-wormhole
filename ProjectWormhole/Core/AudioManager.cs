using System;
using System.IO;
using NAudio.Wave;

namespace ProjectWormhole.Core
{
    public enum AudioType
    {
        Music,
        SFX
    }

    // Abstraction for settings to enable testing
    public interface ISettingsProvider
    {
        float MasterVolume { get; }
        float MusicVolume { get; }
        float SfxVolume { get; }
    }

    public class DefaultSettingsProvider : ISettingsProvider
    {
        public float MasterVolume => Settings.Instance.MasterVolume;
        public float MusicVolume => Settings.Instance.MusicVolume;
        public float SfxVolume => Settings.Instance.SfxVolume;
    }

    // Abstraction for NAudio engine to enable testing
    public interface IAudioEngine : IDisposable
    {
        void Init(IWaveProvider provider);
        void Play();
        void Stop();
        float Volume { get; set; }
        PlaybackState PlaybackState { get; }
    }

    public interface IWaveProvider
    {
        WaveFormat WaveFormat { get; }
    }

    public interface IAudioFileReader : IWaveProvider, IDisposable
    {
        float Volume { get; set; }
        long Length { get; }
        long Position { get; set; }
        int Read(byte[] buffer, int offset, int count);
    }

    // Factory for creating audio file readers
    public interface IAudioFileReaderFactory
    {
        IAudioFileReader CreateAudioFileReader(string filePath);
        IWaveProvider CreateLoopStream(IAudioFileReader reader);
    }

    public class DefaultAudioFileReaderFactory : IAudioFileReaderFactory
    {
        public IAudioFileReader CreateAudioFileReader(string filePath)
        {
            return new NAudioFileReaderWrapper(filePath);
        }

        public IWaveProvider CreateLoopStream(IAudioFileReader reader)
        {
            if (reader is NAudioFileReaderWrapper wrapper)
            {
                return new LoopStream((WaveStream)wrapper);
            }
            throw new ArgumentException("Cannot create loop stream from this reader type");
        }
    }

    public class NAudioEngine : IAudioEngine
    {
        private readonly IWavePlayer _wavePlayer;
        public NAudioEngine() { _wavePlayer = new WaveOutEvent(); }
        public void Init(IWaveProvider provider)
        {
            if (provider is IWaveProvider wp)
            {
                // This is a bit of a hack, but we need to get the underlying WaveStream
                // In a real scenario, you might have a more robust abstraction
                _wavePlayer.Init(provider as NAudio.Wave.IWaveProvider);
            }
        }
        public void Play() => _wavePlayer.Play();
        public void Stop() => _wavePlayer.Stop();
        public void Dispose() => _wavePlayer.Dispose();
        public float Volume
        {
            get => _wavePlayer.Volume;
            set => _wavePlayer.Volume = value;
        }
        public PlaybackState PlaybackState => _wavePlayer.PlaybackState;
    }

    public class NAudioFileReaderWrapper : IAudioFileReader
    {
        private readonly AudioFileReader _reader;
        public NAudioFileReaderWrapper(string filePath) { _reader = new AudioFileReader(filePath); }
        public WaveFormat WaveFormat => _reader.WaveFormat;
        public float Volume { get => _reader.Volume; set => _reader.Volume = value; }
        public long Length => _reader.Length;
        public long Position { get => _reader.Position; set => _reader.Position = value; }
        public int Read(byte[] buffer, int offset, int count) => _reader.Read(buffer, offset, count);
        public void Dispose() => _reader.Dispose();
        public static implicit operator WaveStream(NAudioFileReaderWrapper wrapper) => wrapper._reader;
    }


    public class AudioManager : IDisposable
    {
        private readonly IAudioEngine _musicPlayer;
        private IAudioFileReader? _musicReader;
        private readonly IAudioEngine _sfxPlayer;
        private IAudioFileReader? _sfxReader;

        private readonly IFileSystem _fileSystem;
        private readonly ISettingsProvider _settings;
        private readonly IAudioFileReaderFactory _audioFileReaderFactory;

        public AudioManager(IAudioEngine musicPlayer, IAudioEngine sfxPlayer, IFileSystem fileSystem, ISettingsProvider settings, IAudioFileReaderFactory audioFileReaderFactory)
        {
            _musicPlayer = musicPlayer;
            _sfxPlayer = sfxPlayer;
            _fileSystem = fileSystem;
            _settings = settings;
            _audioFileReaderFactory = audioFileReaderFactory;
        }

        // Default constructor for backward compatibility
        public AudioManager() : this(new NAudioEngine(), new NAudioEngine(), new FileSystemWrapper(), new DefaultSettingsProvider(), new DefaultAudioFileReaderFactory())
        {
        }

        // Static instance for backward compatibility
        public static AudioManager Instance { get; } = new AudioManager();

        public void PlayBackgroundMusic(string filename, bool loop = true)
        {
            try
            {
                StopBackgroundMusic();

                string musicPath = Path.Combine(_fileSystem.GetCurrentDirectory(), "audio", "music", filename);
                if (!_fileSystem.FileExists(musicPath))
                {
                    return;
                }

                _musicReader = _audioFileReaderFactory.CreateAudioFileReader(musicPath);

                if (loop)
                {
                    var loopStream = _audioFileReaderFactory.CreateLoopStream(_musicReader);
                    _musicPlayer.Init(loopStream);
                }
                else
                {
                    _musicPlayer.Init(_musicReader);
                }

                // Apply volume
                UpdateMusicVolume();
                _musicPlayer.Play();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to play background music: {ex.Message}");
            }
        }

        public void StopBackgroundMusic()
        {
            try
            {
                if (_musicPlayer.PlaybackState == PlaybackState.Playing)
                {
                    _musicPlayer.Stop();
                }
                _musicReader?.Dispose();
                _musicReader = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to stop background music: {ex.Message}");        
            }
        }

        public void PlaySfx(string filename)
        {
            try
            {
                string sfxPath = Path.Combine(_fileSystem.GetCurrentDirectory(), "audio", "sfx", filename);
                if (!_fileSystem.FileExists(sfxPath))
                {
                    return;
                }

                // Stop any currently playing SFX
                if (_sfxPlayer.PlaybackState == PlaybackState.Playing)
                {
                    _sfxPlayer.Stop();
                }
                _sfxReader?.Dispose();

                _sfxReader = _audioFileReaderFactory.CreateAudioFileReader(sfxPath);
                _sfxReader.Volume = CalculateEffectiveVolume(AudioType.SFX);
                
                _sfxPlayer.Init(_sfxReader);
                _sfxPlayer.Play();
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to play SFX: {ex.Message}");
            }
        }

        public void UpdateMusicVolume()
        {
            if (_musicReader != null)
            {
                _musicReader.Volume = CalculateEffectiveVolume(AudioType.Music);
            }
        }

        public void UpdateSfxVolume()
        {
            if (_sfxReader != null)
            {
                _sfxReader.Volume = CalculateEffectiveVolume(AudioType.SFX);
            }
        }

        private float CalculateEffectiveVolume(AudioType type)
        {
            float masterVolume = _settings.MasterVolume;
            float typeVolume = type == AudioType.Music ? _settings.MusicVolume : _settings.SfxVolume;
            
            return Math.Clamp(masterVolume * typeVolume, 0.0f, 1.0f);
        }

        public void Dispose()
        {
            StopBackgroundMusic();
            _sfxPlayer.Stop();
            _sfxReader?.Dispose();
            _musicPlayer.Dispose();
            _sfxPlayer.Dispose();
        }
    }

    // Helper class for looping audio
    public class LoopStream : WaveStream, IWaveProvider
    {
        private WaveStream sourceStream;

        public LoopStream(WaveStream sourceStream)
        {
            this.sourceStream = sourceStream;
            this.EnableLooping = true;
        }

        public bool EnableLooping { get; set; }

        public override WaveFormat WaveFormat => sourceStream.WaveFormat;

        public override long Length => sourceStream.Length;

        public override long Position
        {
            get => sourceStream.Position;
            set => sourceStream.Position = value;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int totalBytesRead = 0;

            while (totalBytesRead < count)
            {
                int bytesRead = sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
                if (bytesRead == 0)
                {
                    if (sourceStream.Position == 0 || !EnableLooping)
                    {
                        // Something wrong or looping disabled
                        break;
                    }
                    // Loop
                    sourceStream.Position = 0;
                }
                totalBytesRead += bytesRead;
            }
            return totalBytesRead;
        }

        protected override void Dispose(bool disposing)
        {
            sourceStream?.Dispose();
            base.Dispose(disposing);
        }
    }
}
