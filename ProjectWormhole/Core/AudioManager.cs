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

    public class AudioManager
    {
        private static AudioManager? _instance;
        public static AudioManager Instance => _instance ??= new AudioManager();

        private IWavePlayer? musicPlayer;
        private AudioFileReader? musicReader;
        private IWavePlayer? sfxPlayer;
        private AudioFileReader? sfxReader;

        private AudioManager()
        {
            // Initialize audio output devices
            try
            {
                musicPlayer = new WaveOutEvent();
                sfxPlayer = new WaveOutEvent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to initialize audio players: {ex.Message}");
            }
        }

        public void PlayBackgroundMusic(string filename, bool loop = true)
        {
            try
            {
                StopBackgroundMusic();

                string musicPath = Path.Combine(Directory.GetCurrentDirectory(), "audio", "music", filename);
                if (!File.Exists(musicPath))
                {
                    return;
                }

                musicReader = new AudioFileReader(musicPath);

                if (loop)
                {
                    var loopStream = new LoopStream(musicReader);
                    musicPlayer?.Init(loopStream);
                }
                else
                {
                    musicPlayer?.Init(musicReader);
                }

                // Apply volume
                UpdateMusicVolume();
                musicPlayer?.Play();

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
                musicPlayer?.Stop();
                musicReader?.Dispose();
                musicReader = null;
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
                string sfxPath = Path.Combine(Directory.GetCurrentDirectory(), "audio", "sfx", filename);
                if (!File.Exists(sfxPath))
                {
                    return;
                }

                // Stop any currently playing SFX
                sfxPlayer?.Stop();
                sfxReader?.Dispose();

                sfxReader = new AudioFileReader(sfxPath);
                sfxReader.Volume = CalculateEffectiveVolume(AudioType.SFX);
                
                sfxPlayer?.Init(sfxReader);
                sfxPlayer?.Play();
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to play SFX: {ex.Message}");
            }
        }

        public void UpdateMusicVolume()
        {
            if (musicReader != null)
            {
                musicReader.Volume = CalculateEffectiveVolume(AudioType.Music);
            }
        }

        public void UpdateSfxVolume()
        {
            if (sfxReader != null)
            {
                sfxReader.Volume = CalculateEffectiveVolume(AudioType.SFX);
            }
        }

        private float CalculateEffectiveVolume(AudioType type)
        {
            float masterVolume = Settings.Instance.MasterVolume;
            float typeVolume = type == AudioType.Music ? Settings.Instance.MusicVolume : Settings.Instance.SfxVolume;
            
            return Math.Clamp(masterVolume * typeVolume, 0.0f, 1.0f);
        }

        public void Dispose()
        {
            StopBackgroundMusic();
            sfxPlayer?.Stop();
            sfxReader?.Dispose();
            musicPlayer?.Dispose();
            sfxPlayer?.Dispose();
        }
    }

    // Helper class for looping audio
    public class LoopStream : WaveStream
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
