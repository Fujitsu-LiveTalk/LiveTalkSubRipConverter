/*
 * Copyright 2020 FUJITSU SOCIAL SCIENCE LABORATORY LIMITED
 * クラス名　：MainWindow
 * 概要      ：MainWindow
*/
using System.Windows;

namespace LiveTalkSubRipConverter.Views
{
    public partial class MainWindow : Window
    {
        public ViewModels.MainViewModel ViewModel { get; } = App.MainVM;
        
        public MainWindow()
        {
            InitializeComponent();

            this.ViewModel.Closed += (s, args) =>
            {
                this.Close();
            };
        }
    }
}
