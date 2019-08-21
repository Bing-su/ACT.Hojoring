using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ACT.TTSYukkuri.Yukkuri
{
    public class AqKanji2Koe
    {
        #region Singleton

        private static AqKanji2Koe instance = new AqKanji2Koe();

        public static AqKanji2Koe Instance => instance;

        #endregion Singleton

        private readonly Encoding ShiftJISEncoding = Encoding.GetEncoding("Shift_JIS");

        private string Kanji2KoeDictionaryName => Path.Combine(
            AquesTalk.YukkuriDirectory,
            $@"aq_dic");

        private AqKanji2Koe_Create createDelegate;
        private AqKanji2Koe_Release releaseDelegate;
        private AqKanji2Koe_Convert convertDelegate;
        private AqKanji2Koe_ConvertW convertWDelegate;
        private IntPtr kanji2KoeHandle = IntPtr.Zero;

        private delegate IntPtr AqKanji2Koe_Create(string dic, ref int err);

        private delegate void AqKanji2Koe_Release(IntPtr handle);

        private delegate int AqKanji2Koe_Convert(IntPtr handle, string kanji, [MarshalAs(UnmanagedType.LPArray)] byte[] koe, int koeSize);

        private delegate int AqKanji2Koe_ConvertW(IntPtr handle, byte[] kanji, [MarshalAs(UnmanagedType.LPArray)] byte[] koe, int koeSize);

        public void Load()
        {
            this.createDelegate ??= AqKanji2KoeDLL.AqKanji2Koe_Create;
            this.releaseDelegate ??= AqKanji2KoeDLL.AqKanji2Koe_Release;
            this.convertDelegate ??= AqKanji2KoeDLL.AqKanji2Koe_Convert;
            this.convertWDelegate ??= AqKanji2KoeDLL.AqKanji2Koe_ConvertW;

            if (this.kanji2KoeHandle == IntPtr.Zero)
            {
                // 言語処理モジュールのインスタンスを生成しそのハンドルを取得する
                int err = 0;
                this.kanji2KoeHandle = this.createDelegate.Invoke(
                    this.Kanji2KoeDictionaryName,
                    ref err);
            }
        }

        public void Free()
        {
            this.releaseDelegate?.Invoke(this.kanji2KoeHandle);
            this.kanji2KoeHandle = IntPtr.Zero;

            this.createDelegate = null;
            this.releaseDelegate = null;
            this.convertDelegate = null;
            this.convertWDelegate = null;
        }

        /// <summary>
        /// 漢字混じりのテキストを音声記号列にして返す
        /// </summary>
        /// <param name="kanjiText">
        /// 漢字混じりのテキスト</param>
        /// <returns>
        /// 音声記号列（要するによみがな）</returns>
        public string Convert(
            string kanjiText)
        {
            var phonetic = kanjiText;

            if (this.kanji2KoeHandle != IntPtr.Zero)
            {
                var size = kanjiText.Length * 2 * 16;
                if (size < 256)
                {
                    size = 256;
                }

                var koeBuffer = new byte[size];
                var stat = this.convertDelegate?.Invoke(
                    this.kanji2KoeHandle,
                    phonetic,
                    koeBuffer,
                    size);

                if (stat == 0)
                {
                    var koe = this.ShiftJISEncoding.GetString(koeBuffer).TrimEnd('\0');
                    if (!string.IsNullOrWhiteSpace(koe))
                    {
                        phonetic = koe;
                    }
                }
            }

            return phonetic;
        }

        private readonly Encoding UTF16Encoding = Encoding.GetEncoding("UTF-16");

        /// <summary>
        /// 漢字混じりのテキストを音声記号列にして返す
        /// </summary>
        /// <param name="kanjiText">
        /// 漢字混じりのテキスト</param>
        /// <returns>
        /// 音声記号列（要するによみがな）</returns>
        public string ConvertUTF16(
            string kanjiText)
        {
            var phonetic = kanjiText;

            if (this.kanji2KoeHandle != IntPtr.Zero)
            {
                var size = kanjiText.Length * 2 * 16;
                if (size < 256)
                {
                    size = 256;
                }

                // UTF16でエンコードする
                var inputChars = this.UTF16Encoding.GetBytes(phonetic);

                var koeBuffer = new byte[size];
                var stat = this.convertWDelegate?.Invoke(
                    this.kanji2KoeHandle,
                    inputChars,
                    koeBuffer,
                    size);

                if (stat == 0)
                {
                    var koe = this.UTF16Encoding.GetString(koeBuffer).TrimEnd('\0');
                    if (!string.IsNullOrWhiteSpace(koe))
                    {
                        phonetic = koe;
                    }
                }
            }

            return phonetic;
        }
    }

    public static class AqKanji2KoeDLL
    {
        [DllImport("AqKanji2Koe.dll")]
        public static extern IntPtr AqKanji2Koe_Create(string dic, ref int err);

        [DllImport("AqKanji2Koe.dll")]
        public static extern void AqKanji2Koe_Release(IntPtr handle);

        [DllImport("AqKanji2Koe.dll")]
        public static extern int AqKanji2Koe_Convert(IntPtr handle, string kanji, [MarshalAs(UnmanagedType.LPArray)] byte[] koe, int koeSize);

        [DllImport("AqKanji2Koe.dll")]
        public static extern int AqKanji2Koe_ConvertW(IntPtr handle, byte[] kanji, [MarshalAs(UnmanagedType.LPArray)] byte[] koe, int koeSize);
    }
}
