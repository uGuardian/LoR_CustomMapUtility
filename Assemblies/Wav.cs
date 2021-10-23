using System.IO;

namespace CustomMapUtility
{
    public class Wav
    {
        private static float bytesToFloat(byte firstByte, byte secondByte)
        {
            var num = (short)(secondByte << 8 | firstByte);
            return num / 32768f;
        }
        private static int bytesToInt(byte[] bytes, int offset = 0)
        {
            var num = 0;
            for (var i = 0; i < 4; i++)
            {
                num |= bytes[offset + i] << i * 8;
            }
            return num;
        }
        private static byte[] GetBytes(string filename)
        {
            return File.ReadAllBytes(filename);
        }
        public float[] LeftChannel { get; internal set; }
        public float[] RightChannel { get; internal set; }
        public int ChannelCount { get; internal set; }
        public int SampleCount { get; internal set; }
        public int Frequency { get; internal set; }
        public Wav(string filename) : this(GetBytes(filename))
        {
        }
        public Wav(byte[] wav)
        {
            ChannelCount = wav[22];
            Frequency = bytesToInt(wav, 24);
            var i = 12;
            while (wav[i] != 100 || wav[i + 1] != 97 || wav[i + 2] != 116 || wav[i + 3] != 97)
            {
                i += 4;
                var num = wav[i] + wav[i + 1] * 256 + wav[i + 2] * 65536 + wav[i + 3] * 16777216;
                i += 4 + num;
            }
            i += 8;
            SampleCount = (wav.Length - i) / 2;
            var flag = ChannelCount == 2;
            var flag2 = flag;
            var flag3 = flag2;
            if (flag3)
            {
                SampleCount /= 2;
            }
            LeftChannel = new float[SampleCount];
            var flag4 = ChannelCount == 2;
            var flag5 = flag4;
            var flag6 = flag5;
            RightChannel = flag6 ? new float[SampleCount] : null;
            var num2 = 0;
            while (i < wav.Length)
            {
                LeftChannel[num2] = bytesToFloat(wav[i], wav[i + 1]);
                i += 2;
                var flag7 = ChannelCount == 2;
                var flag8 = flag7;
                var flag9 = flag8;
                if (flag9)
                {
                    RightChannel[num2] = bytesToFloat(wav[i], wav[i + 1]);
                    i += 2;
                }
                num2++;
            }
        }
        public override string ToString()
        {
            return string.Format("[WAV: LeftChannel={0}, RightChannel={1}, ChannelCount={2}, SampleCount={3}, Frequency={4}]", new object[]
            {
                LeftChannel,
                RightChannel,
                ChannelCount,
                SampleCount,
                Frequency
            });
        }
    }
}