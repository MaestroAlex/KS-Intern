using GalaSoft.MvvmLight.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ChatClient.Interface
{
    public interface IFrameNavigationService : INavigationService
    {
        public Frame MainFrame { get; set; }
        object Argument { get; }
    }
}
