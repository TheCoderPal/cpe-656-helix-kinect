﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.Generic;
using GestureRecorder.Data;
using Iava.Audio;
using Iava.Input.Camera;
using System.Linq;
using GestureRecorder.Controls;

namespace GestureRecorder {
    /// <summary>
    /// Interaction logic for CreateGestureWindow.xaml
    /// </summary>
    public partial class CreateGestureWindow : Window {

        #region Constructors

        /// <summary>
        /// Default Constructor
        /// </summary>
        public CreateGestureWindow() {
            InitializeComponent();
            //this.Gesture = new tempuri.org.GestureDefinition.xsd.Gesture();
            this.Gesture = new GestureWrapper();
            // Subscribe to the Camera events we are interested in...
            Camera.ImageFrameReady += OnCameraImageFrameReady;
            Camera.SkeletonFrameReady += OnCameraSkeletonFrameReady;

            // Set up the Audio Recognizer...
            AudioRecognizer = new AudioRecognizer();
            AudioRecognizer.SyncCommand = "IAVA";
            AudioRecognizer.Subscribe("Capture", CaptureCallback);
            AudioRecognizer.Subscribe("Snapshot", CaptureCallback);
            AudioRecognizer.Start();
        }

        #endregion Constructors

        #region Private Properties

        /// <summary>
        /// Captures audio commands to run the current window
        /// </summary>
        private AudioRecognizer AudioRecognizer { get; set; }

        /// <summary>
        /// Number of segments in the current gesture
        /// </summary>
        private int GestureSegmentCount { get; set; }

        /// <summary>
        /// Gets the gesture that is about to be saved. 
        /// </summary>
        private GestureWrapper Gesture
        {
            get;
            set;
        }

        #endregion Private Properties

        #region Private Methods

        #region Audio Callbacks

        /// <summary>
        /// Occurs when the 'Capture' command was received.
        /// </summary>
        /// <param name="e">Audio event args</param>
        private void CaptureCallback(AudioEventArgs e) {
            OnSnapshotClick(null, null);
        }

        #endregion Audio Callbacks

        #region Event Handlers

        /// <summary>
        /// Displays the raw camera image from the Kinect sensor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCameraImageFrameReady(object sender, ImageFrameReadyEventArgs e) {
            PlanarImage Image = e.ImageFrame.Image;

            VideoFeed.Source = BitmapSource.Create(
                Image.Width, Image.Height, 96, 96, PixelFormats.Bgr32, null,
                Image.Bits, Image.Width * Image.BytesPerPixel);
        }

        /// <summary>
        /// Displays the full skeleton image from the Kinect sensor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCameraSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e) {
            // If we don't have a canvas to draw on, there's nothing for us to do...
            if (_activeSkeletonCanvas == null) { return; }

            _activeSkeletonCanvas.SkeletonFrame = e.SkeletonFrame;
        }

        /// <summary>
        /// Raises when the cancel button has been clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCancelClick(object sender, RoutedEventArgs e) {
            this.Close();
        }

        /// <summary>
        /// Tells the selected joint to whether it should be tracked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnJointCheck(object sender, RoutedEventArgs e) {
            System.Windows.Controls.Primitives.ToggleButton temp = sender as System.Windows.Controls.Primitives.ToggleButton;

            Gesture.Segments.ForEach(x => x.BodyParts[(JointID)temp.Tag].Tracking = (bool)temp.IsChecked);
        }

        /// <summary>
        /// Saves the defined gesture to file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNextClick(object sender, RoutedEventArgs e) {
            // TODO: Add save gesture code...
        }

        /// <summary>
        /// Freezes the current gesture segment and loads the next segment canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSnapshotClick(object sender, RoutedEventArgs e) {
            //ToDo: Get the joints and add them to a new segment to add to the current gesture.
            tempuri.org.GestureDefinition.xsd.Gesture.SegmentLocalType gestureSegment = new tempuri.org.GestureDefinition.xsd.Gesture.SegmentLocalType();
            /*
            GestureSegment segment = new GestureSegment(_activeSkeletonCanvas.Skeleton);
            segment.SetTrackingJoints(JointID.AnkleLeft, JointID.AnkleRight);         
            AddSkeletonCanvas();
            */
            // Add the new set of segments of the body to the gesture that will be saved to a file.
            this.Gesture.Segment.Add(gestureSegment);
        }

        /// <summary>
        /// Performs the last of the setup operations once the Window has finished loading
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowLoaded(object sender, RoutedEventArgs e) {
            AddSkeletonCanvas();
        }

        #endregion Event Handlers

        /// <summary>
        /// Adds a new Gesture Segment Canvas to the DataGrid
        /// </summary>
        private void AddSkeletonCanvas() {
            // We only support a maximum of 10 segments per gesture
            if (GestureSegmentCount > 9) { _activeSkeletonCanvas = null; return; }

            // Create a new Canvas to add to the grid
            SkeletonCanvas temp = new SkeletonCanvas() { Background = new SolidColorBrush(Color.FromRgb(0, 0, 0)) };

            // Put the canvas in the correct grid position
            Grid.SetColumn(temp, (GestureSegmentCount < 5) ? GestureSegmentCount : GestureSegmentCount - 5);
            Grid.SetRow(temp, GestureSegmentCount / 5);

            // Do the transformation necessary to draw the skeleton
            temp.RenderTransform = new ScaleTransform(1.0, -1.0,
                gridSnapshot.ColumnDefinitions[0].ActualWidth / 2.0,
                gridSnapshot.RowDefinitions[0].ActualHeight / 2.0);

            // Add the canvas to the grid
            gridSnapshot.Children.Add(temp);

            // Set the active canvas segment
            _activeSkeletonCanvas = temp;

            // Increase the number of segments in the gesture
            GestureSegmentCount++;
        }

        #endregion Private Methods

        #region Private Fields

        #region Joints
        private bool m_bHead = false;
        private bool m_bShoulderCenter = false;
        private bool m_bShoulderLeft = false;
        private bool m_bShoulderRight = false;
        private bool m_bElbowLeft = false;
        private bool m_bElbowRight = false;
        private bool m_bWristLeft = false;
        private bool m_bWristRight = false;
        private bool m_bHandLeft = false;
        private bool m_bHandRight = false;
        private bool m_bSpine = false;
        private bool m_bHipCenter = false;
        private bool m_bHipLeft = false;
        private bool m_bHipRight = false;
        private bool m_bKneeLeft = false;
        private bool m_bKneeRight = false;
        private bool m_bAnkleLeft = false;
        private bool m_bAnkleRight = false;
        private bool m_bFootLeft = false;
        private bool m_bFootRight = false;
        #endregion Joints

        /// <summary>
        /// Pointer to the current gesture segment canvas
        /// </summary>
        private SkeletonCanvas _activeSkeletonCanvas = null;

        #endregion Private Fields

        private void Window_Closed(object sender, EventArgs e) {
            AudioRecognizer.Stop();
        }

        
    }
}
