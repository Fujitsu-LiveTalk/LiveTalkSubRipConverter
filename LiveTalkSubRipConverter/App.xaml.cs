/*
 * Copyright 2020 FUJITSU SOCIAL SCIENCE LABORATORY LIMITED
 * クラス名　：App
 * 概要      ：App
*/
using System.Windows;

namespace LiveTalkSubRipConverter
{
    public partial class App : Application
    {
        private static ViewModels.MainViewModel _MainVM = null;
        public static ViewModels.MainViewModel MainVM
        {
            get
            {
                if (_MainVM == null)
                {
                    _MainVM = new ViewModels.MainViewModel();
                }
                return _MainVM;
            }
        }
    }
}
