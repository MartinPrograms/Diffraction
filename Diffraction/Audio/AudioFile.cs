using System.Buffers.Binary;
using System.Text;
using Diffraction.Scripting.Globals;
using Silk.NET.OpenAL;
using NAudio.Wave;
namespace Diffraction.Audio;

    internal class AudioFile : IDisposable
    {
        public string Path;
        public string Name;

        public bool Playing { get; private set; }

        public AudioSettings Settings = new AudioSettings();

        short numChannels = -1;
        int sampleRate = -1;
        int byteRate = -1;
        short blockAlign = -1;
        short bitsPerSample = -1;
        private BufferFormat format = 0;       
        uint buffer = 0;
        private uint source = 0;

        private byte[] _data;

        public unsafe AudioFile(string path, bool looping)
        {
            this.Path = path;
            this.Name = System.IO.Path.GetFileNameWithoutExtension(path);
            this.Path = path;
            
            var al = Audio.Instance.AL;
            source = al.GenSource();
            buffer = al.GenBuffer();

            al.SetSourceProperty(source, SourceBoolean.Looping, looping);

            using (var audioFile = new AudioFileReader(Path))
            {
                numChannels = (short)audioFile.WaveFormat.Channels;
                sampleRate = audioFile.WaveFormat.SampleRate;
                byteRate = audioFile.WaveFormat.AverageBytesPerSecond;
                blockAlign = (short)audioFile.WaveFormat.BlockAlign;
                bitsPerSample = (short)audioFile.WaveFormat.BitsPerSample;

                format = numChannels switch
                {
                    1 => bitsPerSample switch
                    {
                        8 => BufferFormat.Mono8,
                        16 => BufferFormat.Mono16,
                        32 => BufferFormat.Mono16,
                        _ => throw new NotSupportedException("Unsupported bit depth")
                    },
                    2 => bitsPerSample switch
                    {
                        8 => BufferFormat.Stereo8,
                        16 => BufferFormat.Stereo16,
                        32 => BufferFormat.Stereo16,
                    },
                    _ => throw new NotSupportedException("Unsupported channel count")
                };
                
                // If bits per sample is 32, we need to convert the data to 16 bit
                // We can do this by making another buffer, and copying the data over while converting it

                var bufferData = new byte[audioFile.Length];
                var bytesRead = audioFile.Read(bufferData, 0, bufferData.Length);
                var length = bytesRead / (bitsPerSample / 8);
                
                if (bitsPerSample == 32)
                {
                    var newBuffer = new byte[length * 2];
                    for (int i = 0; i < length; i++)
                    {
                        var sample = BitConverter.ToSingle(bufferData, i * 4);
                        var sample16 = (short)(sample * short.MaxValue);
                        BinaryPrimitives.WriteInt16LittleEndian(newBuffer.AsSpan(i * 2), sample16);
                    }

                    bufferData = newBuffer;
                }

                unsafe
                {
                    fixed (byte* ptr = bufferData)
                    {
                        al.BufferData(buffer, format, ptr, bufferData.Length, sampleRate);
                    }
                }
                
                _data = bufferData;
            }
            
            
            
            al.SetSourceProperty(source, SourceInteger.Buffer, buffer);
            al.SetSourceProperty(source, SourceFloat.Gain, 1.0f);
            al.SetSourceProperty(source, SourceFloat.Pitch, 1.0f);

            UpdateVolumeGraph();
        }

        public void Play()
        {
            Settings.Pitch = Time.TimeScale;
            Audio.Instance.AL.SetSettings(source, Settings);
            Audio.Instance.AL.SourcePlay(source);
        }

        public void Stop()
        {
            Audio.Instance.AL.SourceStop(source);
            Playing = false;
        }

        public void Dispose()
        {
            Audio.Instance.AL.DeleteSource(source);
            Audio.Instance.AL.DeleteBuffer(buffer);
        }

        public float[] volumeGraph = new float[100];
        internal float Duration;

        private void UpdateVolumeGraph()
        {
            using (var audioFile = new AudioFileReader(Path))
            {
                Duration = (float)audioFile.TotalTime.TotalSeconds;
                var buffer = new float[audioFile.Length];
                var bytesRead = audioFile.Read(buffer, 0, buffer.Length);

                volumeGraph = new float[bytesRead / sizeof(float)];
                for (var i = 0; i < bytesRead / sizeof(float); i++)
                {
                    var volume = Math.Abs(buffer[i]);
                    volumeGraph[i] = volume;
                }
            }
        }

        internal void TogglePlay()
        {
            if (Playing)
            {
                Stop();
            }
            else
            {
                Play();
            }
        }
    }

    public static class AudioFileSettings
    {
        public static List<string> SupportedExtensions = new List<string> { ".wav", ".mp3", ".ogg", ".flac" };
    }