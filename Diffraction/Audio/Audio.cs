using Silk.NET.OpenAL;

namespace Diffraction.Audio;

    internal unsafe class Audio : IDisposable
    {
        private Silk.NET.OpenAL.AL _al;
        private ALContext _context;
        private Device * _device;

        public float Volume
        {
            get
            {
                _al.GetListenerProperty(ListenerFloat.Gain, out var volume);
                return volume;
            }
            set
            {
                _al.SetListenerProperty(ListenerFloat.Gain, value);
            }
        }

        public static Audio Instance { get; private set; }

        public AL AL
        {
            get { return _al; }
        }

        public Audio()
        {
            Instance = this;

            _al = AL.GetApi();
            _context = ALContext.GetApi();
            _device = _context.OpenDevice(null);

            var context = _context.CreateContext(_device, (int*)null);
            _context.MakeContextCurrent(context);

            if (_al.GetError() != AudioError.NoError)
            {
                throw new Exception("Failed to initialize audio");
            }

        }
        public void Init(bool is3d)
        {
            Audio.Instance = this;

        }

        public void Dispose()
        {
            _context.MakeContextCurrent(null);
            _context.DestroyContext(_context.GetCurrentContext());
            _context.CloseDevice(_device);
        }

        public List<AudioFile> AudioFiles = new List<AudioFile>();
        internal void AddAudioFile(string file)
        {
            AudioFiles.Add(new AudioFile(file, false));
        }

        internal void PlayRandom()
        {
            if (AudioFiles.Count > 0)
            {
                AudioFiles[new Random().Next(0, AudioFiles.Count)].Play();
            }
        }

        internal void StopAll()
        {
            foreach (var file in AudioFiles)
            {
                file.Stop();
            }
        }

        public void Exit()
        {
            foreach (var file in AudioFiles)
            {
                file.Dispose();
            }
            
            _al.Dispose();
            
            Dispose();
        }
    }

    public static class ALExtensions
    {
        public static void SetSettings(this AL al, uint source, AudioSettings settings)
        {
            al.SetSourceProperty(source, SourceVector3.Position, settings.Position.X, settings.Position.Y, settings.Position.Z);
            al.SetSourceProperty(source, SourceBoolean.Looping, settings.Loop);
            al.SetSourceProperty(source, SourceFloat.Gain, settings.Volume);
            al.SetSourceProperty(source, SourceFloat.Pitch, settings.Pitch);
        }
    }