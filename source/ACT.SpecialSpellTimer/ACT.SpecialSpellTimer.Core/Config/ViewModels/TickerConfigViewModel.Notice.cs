using System;
using System.Threading.Tasks;
using System.Windows.Input;
using ACT.SpecialSpellTimer.Config.Models;
using ACT.SpecialSpellTimer.Sound;
using Prism.Commands;

namespace ACT.SpecialSpellTimer.Config.ViewModels
{
    public partial class TickerConfigViewModel
    {
        public static SoundController.WaveFile[] WaveList => SoundController.Instance.EnumlateWave();

        private void ClearSoundTestCommands()
        {
            this.testTTS1Command = null;
            this.testTTS2Command = null;

            this.testWave1Command = null;
            this.testWave2Command = null;

            foreach (var item in new[]
            {
                nameof(this.TestWave1Command),
                nameof(this.TestWave2Command),
                nameof(this.TestTTS1Command),
                nameof(this.TestTTS2Command),
            })
            {
                this.RaisePropertyChanged(item);
            }
        }

        private ICommand CreateTestWaveCommand(
            Func<string> getWave,
            AdvancedNoticeConfig noticeConfig)
            => new DelegateCommand(()
                => this.Model.Play(getWave(), noticeConfig));

        private ICommand CreateTestTTSCommand(
            Func<string> getTTS,
            AdvancedNoticeConfig noticeConfig)
            => new DelegateCommand(()
                => this.Model.Play(getTTS(), noticeConfig));

        private ICommand testWave1Command;
        private ICommand testWave2Command;

        public ICommand TestWave1Command =>
            this.testWave1Command ?? (this.testWave1Command = this.CreateTestWaveCommand(
                () => this.Model.MatchSound,
                this.Model.MatchAdvancedConfig));

        public ICommand TestWave2Command =>
            this.testWave2Command ?? (this.testWave2Command = this.CreateTestWaveCommand(
                () => this.Model.DelaySound,
                this.Model.DelayAdvancedConfig));

        private ICommand testTTS1Command;
        private ICommand testTTS2Command;

        public ICommand TestTTS1Command =>
            this.testTTS1Command ?? (this.testTTS1Command = this.CreateTestTTSCommand(
                () => this.Model.MatchTextToSpeak,
                this.Model.MatchAdvancedConfig));

        public ICommand TestTTS2Command =>
            this.testTTS2Command ?? (this.testTTS2Command = this.CreateTestTTSCommand(
                () => this.Model.DelayTextToSpeak,
                this.Model.DelayAdvancedConfig));

        private ICommand testSequencialTTSCommand;

        public ICommand TestSequencialTTSCommand =>
            this.testSequencialTTSCommand ?? (this.testSequencialTTSCommand = new DelegateCommand(async () =>
            {
                var config = this.Model.MatchAdvancedConfig;

                this.Model.Play("シンクロ再生のテストを開始します。", config);
                await Task.Delay(2 * 1000);

                this.Model.Play("おしらせ1番", config);
                this.Model.Play("おしらせ2番", config);
                this.Model.Play("おしらせ3番", config);
                this.Model.Play("おしらせ4番", config);

                await Task.Delay(3 * 1000);

                this.Model.Play("1番目に登録した通知です", config, true);
                this.Model.Play("2番目に登録した通知です", config, true);
                this.Model.Play("3番目に登録した通知です", config, true);
                this.Model.Play("4番目に登録した通知です", config, true);
            }));
    }
}
