using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using GalaSoft.MvvmLight.Ioc;

namespace ChatClient.Services
{
    public class NavigationServiceEx : INavigationServiceEx
    {
        public event NavigatedEventHandler Navigated;

        public event NavigationFailedEventHandler NavigationFailed;

        private readonly Dictionary<string, Type> pages = new Dictionary<string, Type>();

        private Frame frame;
        private object lastParamUsed;

        public Frame Frame
        {
            get
            {
                if (this.frame == null)
                {
                    this.frame = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive).Content as Frame;
                    this.RegisterFrameEvents();
                }

                return this.frame;
            }

            set
            {
                this.UnregisterFrameEvents();
                this.frame = value;
                this.RegisterFrameEvents();
            }
        }

        public bool CanGoBack => this.Frame.CanGoBack;

        public bool CanGoForward => this.Frame.CanGoForward;

        public bool GoBack()
        {
            if (this.CanGoBack)
            {
                this.Frame.GoBack();
                return true;
            }

            return false;
        }

        public void GoForward() => this.Frame.GoForward();

        public bool Navigate(string pageKey, object parameter = null, NavigationTransitionInfo infoOverride = null)
        {
            Type page;
            lock (this.pages)
            {
                if (!this.pages.TryGetValue(pageKey, out page))
                {
                    throw new ArgumentException(string.Format("ExceptionNavigationServiceExPageNotFound", pageKey), nameof(pageKey));
                }
            }

            if (this.Frame.Content?.GetType() != page || (parameter != null && !parameter.Equals(this.lastParamUsed)))
            {
                var navigationResult = this.Frame.Navigate(SimpleIoc.Default.GetInstance(page), parameter);
                if (navigationResult)
                {
                    this.lastParamUsed = parameter;
                }

                return navigationResult;
            }
            else
            {
                return false;
            }
        }

        public bool Navigate<TViewModel>(object parameter = null, NavigationTransitionInfo infoOverride = null)
        {
            return this.Navigate(typeof(TViewModel).FullName, parameter, infoOverride);
        }

        public void Configure(string key, Type pageType)
        {
            lock (this.pages)
            {
                if (this.pages.ContainsKey(key))
                {
                    throw new ArgumentException(string.Format("ExceptionNavigationServiceExKeyIsInNavigationService", key));
                }

                if (this.pages.Any(p => p.Value == pageType))
                {
                    throw new ArgumentException(string.Format("ExceptionNavigationServiceExTypeAlreadyConfigured", this.pages.First(p => p.Value == pageType).Key));
                }

                this.pages.Add(key, pageType);
            }
        }

        public string GetNameOfRegisteredPage(Type page)
        {
            lock (this.pages)
            {
                if (this.pages.ContainsValue(page))
                {
                    return this.pages.FirstOrDefault(p => p.Value == page).Key;
                }
                else
                {
                    throw new ArgumentException(string.Format("ExceptionNavigationServiceExPageUnknow", page.Name));
                }
            }
        }

        private void RegisterFrameEvents()
        {
            if (this.frame != null)
            {
                this.frame.Navigated += this.Frame_Navigated;
                this.frame.NavigationFailed += this.Frame_NavigationFailed;
            }
        }

        private void UnregisterFrameEvents()
        {
            if (this.frame != null)
            {
                this.frame.Navigated -= this.Frame_Navigated;
                this.frame.NavigationFailed -= this.Frame_NavigationFailed;
            }
        }

        private void Frame_NavigationFailed(object sender, NavigationFailedEventArgs e) => this.NavigationFailed?.Invoke(sender, e);

        private void Frame_Navigated(object sender, NavigationEventArgs e) => this.Navigated?.Invoke(sender, e);
    }
}
