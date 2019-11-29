using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ChatClient.Utility
{
    //класс, который добавляет кнопку, для выхода из комнат
    class HamburgerMenuIconLeaveItem : HamburgerMenuIconItem
    {
        public Visibility LeaveButtonVisibility
        {
            get { return (Visibility)GetValue(LeaveButtonVisibilityProperty); }
            set { SetValue(LeaveButtonVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LeaveButtonVisibilityProperty =
            DependencyProperty.Register("LeaveButtonVisibility", typeof(Visibility), 
                typeof(HamburgerMenuIconLeaveItem), new PropertyMetadata()
                                                        { 
                                                            DefaultValue = Visibility.Collapsed
                                                        });


        public ICommand LeaveCommand
        {
            get { return (ICommand)GetValue(LeaveCommandProperty); }
            set { SetValue(LeaveCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LeaveCommandProperty =
            DependencyProperty.Register("LeaveCommand", typeof(ICommand), 
                typeof(HamburgerMenuIconLeaveItem), new PropertyMetadata(null));




        public object LeaveCommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for object.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LeaveCommandParameterProperty =
            DependencyProperty.Register("LeaveCommandParameter", typeof(object), 
                typeof(HamburgerMenuIconLeaveItem), new PropertyMetadata(null));


    }
}
