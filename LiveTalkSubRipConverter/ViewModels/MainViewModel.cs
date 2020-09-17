/*
 * Copyright 2020 FUJITSU SOCIAL SCIENCE LABORATORY LIMITED
 * クラス名　：MainViewModel
 * 概要      ：MainViewModel
*/

using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.ComponentModel.DataAnnotations;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Windows;

namespace LiveTalkSubRipConverter.ViewModels
{
    public class MainViewModel : IDisposable
    {
        private Models.SubRipModel Model = new Models.SubRipModel();
        private CompositeDisposable Disposable { get; } = new CompositeDisposable();

        #region "SubRipModel-Property"
        [Required]       // 必須チェック
        public ReactiveProperty<string> FileName { get; }
        [Required]       // 必須チェック
        public ReactiveProperty<int> OffsetSec { get; }
        public ReactiveProperty<string> Message { get; }
        #endregion

        /// <summary>
        /// True:連携開始可能
        /// </summary>
        public ReadOnlyReactiveProperty<bool> IsCanStart { get; }

        /// <summary>
        /// True:連携中フラグ
        /// </summary>
        public ReactiveProperty<bool> IsStarted { get; } = new ReactiveProperty<bool>();

        public MainViewModel()
        {
            // プロパティ設定
            this.FileName = this.Model.ToReactivePropertyAsSynchronized((x) => x.FileName)
                .SetValidateAttribute(() => this.FileName)
                .AddTo(this.Disposable);
            this.OffsetSec = this.Model.ToReactivePropertyAsSynchronized((x) => x.OffsetSec)
                .SetValidateAttribute(() => this.OffsetSec)
                .AddTo(this.Disposable);
            this.Message = this.Model.ToReactivePropertyAsSynchronized((x) => x.Message)
                .AddTo(this.Disposable);

            // 3つのステータスがすべてFalseの時だけスタートボタンがクリックできる
            this.IsCanStart = new[]
            {
                this.FileName.ObserveHasErrors,
                this.OffsetSec.ObserveHasErrors,
                this.IsStarted,
            }.CombineLatestValuesAreAllFalse()
             .ToReadOnlyReactiveProperty()
             .AddTo(this.Disposable);

            // コマンド設定
            this.FileOpenCommand = this.IsStarted.Inverse()
                .ToReactiveCommand()
                .WithSubscribe(() => this.FileInput())
                .AddTo(this.Disposable);
            this.StartCommand = this.IsCanStart
                .ToReactiveCommand()
                .WithSubscribe(async () => await this.Start())
                .AddTo(this.Disposable);
            this.ExitCommand.Subscribe((x) =>
            {
                OnClosed();
            }).AddTo(this.Disposable);

            // エラーハンドリング
            this.Model.Threw += (s, e) =>
            {
                MessageBox.Show(e.GetException().Message, "LiveTalk SubRip Converter", MessageBoxButton.OK, MessageBoxImage.Warning);
            };
        }

        /// <summary>
        /// 常時ファイル入力
        /// </summary>
        public ReactiveCommand FileOpenCommand { get; }
        private void FileInput()
        {
            try
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    FilterIndex = 1,
                    Filter = "LiveTalk CSVファイル(*.csv)|*.csv",
                    Title = "ファイル名を指定",
                    CreatePrompt = true,
                    OverwritePrompt = false,
                    DefaultExt = "csv"
                };
                if (string.IsNullOrEmpty(this.FileName.Value))
                {
                    dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                    dialog.FileName = "Output.csv";
                }
                else
                {
                    dialog.InitialDirectory = System.IO.Path.GetDirectoryName(this.FileName.Value);
                    dialog.FileName = System.IO.Path.GetFileName(this.FileName.Value);
                }
                if (dialog.ShowDialog() == true)
                {
                    this.FileName.Value = dialog.FileName;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 字幕送信開始
        /// </summary>
        public ReactiveCommand StartCommand { get; }
        private async Task Start()
        {
            try
            {
                this.IsStarted.Value = true;
                await this.Model.Convert();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                this.IsStarted.Value = false;
            }
        }

        /// <summary>
        /// 画面クローズ
        /// </summary>
        public ReactiveCommand ExitCommand { get; } = new ReactiveCommand();
        public event EventHandler Closed;
        protected virtual void OnClosed()
        {
            this.Closed?.Invoke(this, new EventArgs());
        }

        public void Dispose()
        {
            this.Disposable.Dispose();
        }
    }
}
