using ChatClient.Interface;
using GalaSoft.MvvmLight.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ChatClient.ViewModel
{
    class ViewModelNavigation : IFrameNavigationService
    {
        public string CurrentPageKey { get; private set; }
        public object Argument { get; private set; }

        private Frame mainFrame;
        public Frame MainFrame
        {
            get => mainFrame;
            set => mainFrame ??= value;
        }
        private Dictionary<string, Uri> pages;
        private Stack<string> history;

        public ViewModelNavigation()
        {
            history = new Stack<string>();
            pages = new Dictionary<string, Uri>();
        }

        public void GoBack()
        {
            if (history.Count > 1)
                mainFrame.Source = pages[history.Pop()];
        }

        public void NavigateTo(string pageKey)
        {
            if (string.IsNullOrWhiteSpace(pageKey))
                return;

            mainFrame.Source = pages[pageKey];
            Argument = null;
        }

        public void NavigateTo(string pageKey, object parameter)
        {
            if (string.IsNullOrWhiteSpace(pageKey))
                return;

            mainFrame.Source = pages[pageKey];
            Argument = parameter;
        }

        public void Register(string key, Uri pagePath)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            if (pages.ContainsKey(key))
                pages[key] = pagePath;
            else
                pages.Add(key, pagePath);
        }
    }
}
