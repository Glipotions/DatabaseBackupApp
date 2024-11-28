using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace DatabaseBackupApp.Wpf.Behaviors
{
    public class PasswordBehavior : Behavior<PasswordBox>
    {
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register("Password", typeof(string), typeof(PasswordBehavior), 
                new PropertyMetadata(string.Empty, OnPasswordChanged));

        private bool _isUpdating;

        public string Password
        {
            get { return (string)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PasswordChanged += OnPasswordBoxPasswordChanged;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PasswordChanged -= OnPasswordBoxPasswordChanged;
            base.OnDetaching();
        }

        private static void OnPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (PasswordBehavior)d;
            if (!behavior._isUpdating)
            {
                behavior.AssociatedObject.Password = (string)e.NewValue;
            }
        }

        private void OnPasswordBoxPasswordChanged(object sender, RoutedEventArgs e)
        {
            _isUpdating = true;
            Password = AssociatedObject.Password;
            _isUpdating = false;
        }
    }
}
