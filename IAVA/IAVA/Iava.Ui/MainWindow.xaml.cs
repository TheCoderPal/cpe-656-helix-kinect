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
using System.Windows.Media.Effects;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Iava.Gesture;
using Iava.Audio;
using Iava.Input.Camera;

namespace Iava.Ui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Iava.Gesture.GestureRecognizer m_pGestureRecognizer;
        private Iava.Audio.AudioRecognizer m_pAudioRecognizer;
        private System.Timers.Timer m_pTimer = new System.Timers.Timer();
        private System.Timers.Timer m_pGestureSycnedTimer = new System.Timers.Timer();
        private System.Timers.Timer m_pAudioSycnedTimer = new System.Timers.Timer();
        private bool m_bGestureSyncedToggle = false;
        private bool m_bAudioSyncedToggle = false;

        public MainWindow()
        {
            InitializeComponent();
            m_pGestureRecognizer = new Gesture.GestureRecognizer(string.Empty);
            m_pAudioRecognizer = new Audio.AudioRecognizer(string.Empty);
            // Events
            m_pAudioRecognizer.StatusChanged += new EventHandler<EventArgs>(m_pAudioRecognizer_StatusChanged);
            m_pGestureRecognizer.StatusChanged += new EventHandler<EventArgs>(m_pGestureRecognizer_StatusChanged);
            m_pAudioRecognizer.Synced += new EventHandler<EventArgs>(m_pAudioRecognizer_Synced);
            m_pGestureRecognizer.Synced += new EventHandler<EventArgs>(m_pGestureRecognizer_Synced);
            // Gesture Callbacks
            m_pGestureRecognizer.Subscribe("Zoom", GestureZoomCallback);
            m_pGestureRecognizer.Subscribe("Left Swipe", GestureLeftSwipeCallback);
            m_pGestureRecognizer.Subscribe("Right Swipe", GestureRightSwipeCallback);
            m_pGestureRecognizer.Subscribe("Up Swipe", GestureUpSwipeCallback);
            m_pGestureRecognizer.Subscribe("Down Swipe", GestureDownSwipeCallback);
            // Audio Callbacks
            m_pAudioRecognizer.Subscribe("Zoom In", ZoomInCallback);
            m_pAudioRecognizer.Subscribe("Zoom Out", ZoomOutCallback);

            m_pGestureRecognizer.Camera.ImageFrameReady += new EventHandler<ImageFrameReadyEventArgs>(Camera_ImageFrameReady);

            m_pGestureSycnedTimer.Elapsed += new System.Timers.ElapsedEventHandler(m_pGestureSycnedTimer_Elapsed);
            m_pAudioSycnedTimer.Elapsed += new System.Timers.ElapsedEventHandler(m_pAudioSycnedTimer_Elapsed);
        }

        #region Gesture Callbacks
        /// <summary>
        /// The callback for when the zoom gesture is detected.
        /// </summary>
        /// <param name="e"></param>
        private void GestureZoomCallback(GestureEventArgs e)
        {
            Window.Dispatcher.Invoke(new Action(() => DisplayStatus(String.Format("Gesture: {0} Detected", e.Name))));
        }
        /// <summary>
        /// The callback for when the Left Swipe gesture is detected.
        /// </summary>
        /// <param name="e"></param>
        private void GestureLeftSwipeCallback(GestureEventArgs e) 
        {
            ESRI.ArcGIS.Client.Geometry.MapPoint center = map1.Extent.GetCenter();
            Point screen = map1.MapToScreen(center);
            screen.X += 300;
            ESRI.ArcGIS.Client.Geometry.MapPoint newCenter = map1.ScreenToMap(screen);
            Window.Dispatcher.Invoke(new Action(() => DisplayStatus(String.Format("Gesture: {0} Detected", e.Name))));
            map1.Dispatcher.Invoke(new Action(() => map1.PanTo(newCenter)));
        }
        /// <summary>
        /// The callback for when the Right Swipe gesture is detected.
        /// </summary>
        /// <param name="e"></param>
        private void GestureRightSwipeCallback(GestureEventArgs e)
        {
            ESRI.ArcGIS.Client.Geometry.MapPoint center = map1.Extent.GetCenter();
            Point screen = map1.MapToScreen(center);
            screen.X -= 300;
            ESRI.ArcGIS.Client.Geometry.MapPoint newCenter = map1.ScreenToMap(screen);
            Window.Dispatcher.Invoke(new Action(() => DisplayStatus(String.Format("Gesture: {0} Detected", e.Name))));
            map1.Dispatcher.Invoke(new Action(() => map1.PanTo(newCenter)));
        }
        /// <summary>
        /// The callback for when the Up Swipe gesture is detected.
        /// </summary>
        /// <param name="e"></param>
        private void GestureUpSwipeCallback(GestureEventArgs e)
        {
            ESRI.ArcGIS.Client.Geometry.MapPoint center = map1.Extent.GetCenter();
            Point screen = map1.MapToScreen(center);
            screen.Y += 300;
            ESRI.ArcGIS.Client.Geometry.MapPoint newCenter = map1.ScreenToMap(screen);
            Window.Dispatcher.Invoke(new Action(() => DisplayStatus(String.Format("Gesture: {0} Detected", e.Name))));
            map1.Dispatcher.Invoke(new Action(() => map1.PanTo(newCenter)));
        }
        /// <summary>
        /// The callback for when the Down Swipe gesture is detected.
        /// </summary>
        /// <param name="e"></param>
        private void GestureDownSwipeCallback(GestureEventArgs e)
        {
            ESRI.ArcGIS.Client.Geometry.MapPoint center = map1.Extent.GetCenter();
            Point screen = map1.MapToScreen(center);
            screen.Y -= 300;
            ESRI.ArcGIS.Client.Geometry.MapPoint newCenter = map1.ScreenToMap(screen);
            Window.Dispatcher.Invoke(new Action(() => DisplayStatus(String.Format("Gesture: {0} Detected", e.Name))));
            map1.Dispatcher.Invoke(new Action(() => map1.PanTo(newCenter)));
        }
        #endregion

        #region Audio Callbacks
        /// <summary>
        /// Occurs when a zoom out command was received.
        /// </summary>
        /// <param name="e">Audio event args</param>
        private void ZoomOutCallback(AudioEventArgs e)
        {
            Window.Dispatcher.Invoke(new Action(() => DisplayStatus(String.Format("Audio: {0} Detected", e.Command))));
            map1.Dispatcher.Invoke(new Action(() => map1.Zoom(0.5)));
        }

        /// <summary>
        /// Occurs when a zoom in command was received.
        /// </summary>
        /// <param name="e">Audio event args</param>
        private void ZoomInCallback(AudioEventArgs e)
        {
            Window.Dispatcher.Invoke(new Action(() => DisplayStatus(String.Format("Audio: {0} Detected", e.Command))));
            map1.Dispatcher.Invoke(new Action(() => map1.Zoom(2.0)));
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Occurs when the map is unloaded.
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event args</param>
        private void OnMapUnloaded(object sender, RoutedEventArgs e)
        {
            m_pAudioRecognizer.Stop();
            m_pGestureRecognizer.Stop();
        }
        /// <summary>
        /// Occurs when the map is loaded.
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event args</param>
        private void OnMapLoaded(object sender, RoutedEventArgs e)
        {
            m_pAudioRecognizer.Start();
            m_pGestureRecognizer.Start();
        } 
        /// <summary>
        /// Raises when the status of the audio recognizer is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_pAudioRecognizer_StatusChanged(object sender, EventArgs e)
        {
            Window.Dispatcher.Invoke(new Action(() => DisplayStatus("Audio Status: " + m_pAudioRecognizer.Status.ToString())));
        }
        /// <summary>
        /// Raises when the status of the gesture recognizer is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_pGestureRecognizer_StatusChanged(object sender, EventArgs e)
        {
            Window.Dispatcher.Invoke(new Action(() => DisplayStatus("Audio Status: " + m_pAudioRecognizer.Status.ToString())));
        }
        /// <summary>
        /// Raises when the audio recognizer is synced.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_pAudioRecognizer_Synced(object sender, EventArgs e)
        {
            this.m_pAudioSycnedTimer.Interval = 1000;
            this.m_pAudioSycnedTimer.Enabled = true;
        }
        /// <summary>
        /// Raises when the gesture recognizer is synced.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_pGestureRecognizer_Synced(object sender, EventArgs e)
        {
            this.m_pGestureSycnedTimer.Interval = 1000;
            this.m_pGestureSycnedTimer.Enabled = true;
        }
        /// <summary>
        /// Raises when the camera frame is ready to be viewed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Camera_ImageFrameReady(object sender, ImageFrameReadyEventArgs e)
        {
            PlanarImage Image = e.ImageFrame.Image;

            kinectVideoFeed.Source = BitmapSource.Create(
                Image.Width, Image.Height, 96, 96, PixelFormats.Bgr32, null,
                Image.Bits, Image.Width * Image.BytesPerPixel);
        }
        /// <summary>
        /// Event used in junction with the display status function.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            popStatus.Dispatcher.Invoke(new Action(() => popStatus.IsOpen = false));
        }
        /// <summary>
        /// Raises when the audio synced timer has been elapsed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_pAudioSycnedTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            m_bAudioSyncedToggle = !m_bAudioSyncedToggle;
            if (m_bAudioSyncedToggle)
            {
                this.audioSync.Dispatcher.Invoke(new Action(() => this.audioSync.Foreground = Brushes.Red));
                //this.audioSync.Dispatcher.Invoke(new Action(() => this.audioSync.FontWeight = FontWeights.Bold));
            }
            else
            {
                this.audioSync.Dispatcher.Invoke(new Action(() => this.audioSync.Foreground = Brushes.Black));
                //this.audioSync.Dispatcher.Invoke(new Action(() => this.audioSync.FontWeight = FontWeights.Normal));
            }
        }
        /// <summary>
        /// Raises when the gesture synced timer has been elasped.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_pGestureSycnedTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            m_bGestureSyncedToggle = !m_bGestureSyncedToggle;
            if (m_bGestureSyncedToggle)
            {
                this.gestureSync.Dispatcher.Invoke(new Action(() => this.gestureSync.Foreground = Brushes.Red));
                //this.gestureSync.Dispatcher.Invoke(new Action(() => this.gestureSync.FontWeight = FontWeights.Bold));
            }
            else
            {
                this.gestureSync.Dispatcher.Invoke(new Action(() => this.gestureSync.Foreground = Brushes.Black));
                //this.gestureSync.Dispatcher.Invoke(new Action(() => this.gestureSync.FontWeight = FontWeights.Normal));
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Displays the pop status control with the auto close.
        /// </summary>
        /// <param name="text"></param>
        private void DisplayStatus(string text)
        {
            if (!popStatus.IsOpen)
            {
                m_pTimer.Interval = 2000; // 2 seconds
                m_pTimer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
                m_pTimer.Enabled = true;
                popStatus.IsOpen = true;
                lblStatus.Content = text;
            }
            else
            {
                m_pTimer.Interval = 2000;
                lblStatus.Content = text;
            }
        }
        #endregion
    }
}
