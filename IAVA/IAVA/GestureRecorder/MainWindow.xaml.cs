﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GestureRecorder {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            //TestGestureWindow window = new TestGestureWindow();
            //Nullable<bool> results = window.ShowDialog();
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            //CreateGestureWindow window = new CreateGestureWindow();
            //Nullable<bool> results = window.ShowDialog();
        }
    }
}
