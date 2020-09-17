/*
 * Copyright 2020 FUJITSU SOCIAL SCIENCE LABORATORY LIMITED
 * クラス名　：SubRipModel
 * 概要      ：LiveTalkのCSVファイルを読み込み、SubRip形式のファイルに出力する
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LiveTalkSubRipConverter.Models
{
    /// <summary>
    /// SubRip形式の例
    /// 1
    /// 00:02:17,440 --> 00:02:20,375
    /// Senator, we're making
    /// our final approach into Coruscant.
    /// 
    /// 2
    /// 00:02:20,476 --> 00:02:22,501
    /// Very good, Lieutenant.
    /// </summary>
    /// <remarks>
    /// 時間制限：1 秒当たり4 文字（必要に応じて最大5文字）。
    /// 行幅制限：1 行当たり最大13文字（行数は最大2行まで）。
    /// </remarks>
    internal class SubRipModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 連携ファイル名
        /// </summary>
        private string _FileName = string.Empty;
        public string FileName
        {
            get { return this._FileName; }
            internal set
            {
                if (this._FileName != value)
                {
                    this._FileName = value;
                    OnPropertyChanged();
                    Common.Config.SetConfig("FileName", value);
                }
            }
        }

        /// <summary>
        /// オフセット秒数
        /// </summary>
        private int _OffsetSec = 0;
        public int OffsetSec
        {
            get { return this._OffsetSec; }
            internal set
            {
                if (this._OffsetSec != value)
                {
                    this._OffsetSec = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 処理中メッセージ
        /// </summary>
        private string _Message = string.Empty;
        public string Message
        {
            get { return this._Message; }
            internal set
            {
                if (this._Message != value)
                {
                    this._Message = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// CSVファイルからSRTファイルに変換する
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <remarks>字幕表示終了時刻は次の字幕表示開始時刻 or 文字数÷5秒の短い方</remarks>
        internal async Task Convert()
        {
            var source = this.FileName;
            var destination = Path.ChangeExtension(this.FileName, ".srt");

            try
            {
                // ファイルからの入力は非同期に実施する
                await Task.Run(() =>
                {
                    long seqNo = 1;
                    var reg = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
                    var srts = new List<TSrt>();
                    var baseDateTime = DateTime.MinValue;

                    try
                    {
                        //　CSV入力
                        using (var fs = new System.IO.FileStream(source, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            using (var sr = new StreamReader(fs, Encoding.UTF8))
                            {
                                // ファイルの終わりまで入力する
                                while (!sr.EndOfStream)
                                {
                                    var s = sr.ReadLine();
                                    var items = reg.Split(s);
                                    var messageTime = DateTime.Parse(items[0].Substring(1, items[0].Length - 2));
                                    var name = items[1].Substring(1, items[1].Length - 2);
                                    var message = items[2].Substring(1, items[2].Length - 2);
                                    var translateText = items[3].Substring(1, items[3].Length - 2);

                                    this.Message = $"Read CSV File : SeqNo={seqNo}";
                                    srts.Add(new TSrt()
                                    {
                                        SeqNo = seqNo++,
                                        StartTime = messageTime,
                                        EndedTime = messageTime.AddSeconds(Math.Max(Math.Ceiling((Double)message.Length / 5), 3)),
                                        Subtitle = message,
                                    });
                                }
                                sr.Close();
                            }
                        }

                        // 字幕加工
                        for (var index = 0; index <= srts.Count - 2; index++)
                        {
                            this.Message = $"Check lines : SeqNo={srts[index].SeqNo}";
                            if (srts[index].EndedTime > srts[index + 1].StartTime)
                            {
                                srts[index].EndedTime = srts[index + 1].StartTime;
                            }
                        }

                        // ファイル出力
                        if (srts.Count > 0)
                        {
                            baseDateTime = srts[0].StartTime.AddSeconds(-this.OffsetSec);
                        }
                        using (var fs = new System.IO.FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            using (var sw = new StreamWriter(fs, Encoding.UTF8))
                            {
                                foreach (var srt in srts)
                                {
                                    var writedata = $"{srt.SeqNo}{Environment.NewLine}{srt.StartTime.Subtract(baseDateTime):hh\\:mm\\:ss\\,fff} --> {srt.EndedTime.Subtract(baseDateTime):hh\\:mm\\:ss\\,fff}{Environment.NewLine}{srt.Subtitle}{Environment.NewLine}";
                                    sw.WriteLine(writedata);
                                    this.Message = $"Write SRT File : SeqNo={srt.SeqNo}";
                                }
                                sw.Close();
                            }
                        }

                        this.Message = "End of file";
                    }
                    catch (Exception ex)
                    {
                        OnThrew(ex);
                    }
                });
            }
            catch { }
        }

        private class TSrt
        {
            public long SeqNo { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndedTime { get; set; }
            public string Subtitle { get; set; }
        }

        /// <summary>
        /// エラーメッセージ
        /// </summary>
        public event ErrorEventHandler Threw;
        protected virtual void OnThrew(Exception ex)
        {
            this.Threw?.Invoke(this, new ErrorEventArgs(ex));
        }

        /// <summary>
        /// プロパティ変更通知
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] String propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
