﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Neo.Gui.Cross.ViewModels.Wallets;

namespace Neo.Gui.Cross.Views.Wallets
{
    public class CertificateRequestView : Window
    {
        public CertificateRequestView()
        {
            InitializeComponent();
            this.DataContext = ViewModelLocator.GetDataContext<CertificateRequestViewModel>();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}