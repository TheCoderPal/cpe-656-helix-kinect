﻿using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Iava.Audio;
using Iava.Input.Camera;
using Iava.Gesture;

namespace GestureRecorder.Controls {
    /// <summary>
    /// Interaction logic for CreateGestureWindow.xaml
    /// </summary>
    public partial class CreateGesture : UserControl {
        /// <summary>
        /// This decides whether or not the button is a next button or finish button.
        /// </summary>
        private bool m_bIsNextButton = true;

        #region Constructors
        /// <summary>
        /// Default Constructor
        /// </summary>
        public CreateGesture() {
            InitializeComponent();
            //this.Gesture = new tempuri.org.GestureDefinition.xsd.Gesture();
            this.Gesture = new IavaGesture();

            // If we are in design mode, do nothing...
            if (DesignerProperties.GetIsInDesignMode(this)) { return; }

            // Subscribe to the Camera events we are interested in...

            try {
                IavaCamera.ImageFrameReady += OnCameraImageFrameReady;
                IavaCamera.SkeletonFrameReady += OnCameraSkeletonFrameReady;
            }

            catch (Exception e) {
            }

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
        private IavaGesture Gesture
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
        private void OnCameraImageFrameReady(object sender, IavaColorImageFrameReadyEventArgs e) {
            if (e.ImageFrame == null) { return; }

            // Set up the writable bitmap...
            if (_firstRun) {
                // This is more efficient than creating a new bitmap every frame
                _colorBitmap = new WriteableBitmap(
                    e.ImageFrame.Width,
                    e.ImageFrame.Height,
                    96.0, // DpiX
                    96.0, // DpiY
                    PixelFormats.Bgr32,
                    null);

                // Set the image source
                VideoFeed.Source = _colorBitmap;

                // Indicate this is not the first run
                _firstRun = false;
            }

            // Draw the image
            _colorBitmap.WritePixels(
                new Int32Rect(0, 0, e.ImageFrame.Width, e.ImageFrame.Height),
                e.ImageFrame.PixelData,
                e.ImageFrame.Width * ((PixelFormats.Bgr32.BitsPerPixel + 7) / 8), // Got this from sample code
                0);
        }

        /// <summary>
        /// Displays the full skeleton image from the Kinect sensor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCameraSkeletonFrameReady(object sender, IavaSkeletonFrameReadyEventArgs e) {
            // If we don't have a canvas to draw on, there's nothing for us to do...
            if (_activeSkeletonCanvas == null) { lblActiveSkeleton.Content = "No Skeleton"; return; }

            _activeSkeletonCanvas.Skeleton = e.SkeletonFrame.ActiveSkeleton;

            // For debugging purposes
            if (e.SkeletonFrame.ActiveSkeleton == null) { lblActiveSkeleton.Content = "No Skeleton"; }

            else { lblActiveSkeleton.Content = ""; }
        }

        /// <summary>
        /// Raises when the cancel button has been clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCancelClick(object sender, RoutedEventArgs e) {
            this.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Tells the selected joint to whether it should be tracked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnJointCheck(object sender, RoutedEventArgs e) {
            System.Windows.Controls.Primitives.ToggleButton temp = sender as System.Windows.Controls.Primitives.ToggleButton;
            // Sets all the snapshots to track this joint.
            this.Gesture.SetTrackingJoints((IavaJointType)(Convert.ToInt32(temp.Tag)));
        }

        /// <summary>
        /// Performs the last of the setup operations once the Control has finished loading
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoaded(object sender, RoutedEventArgs e) {
            AddSkeletonCanvas();
        }

        /// <summary>
        /// Saves the defined gesture to file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNextClick(object sender, RoutedEventArgs e) {
            if (m_bIsNextButton)
            {
                // Get the audio recognizer from adding more snapshots accidentally.
                this.AudioRecognizer.Unsubscribe("Capture");
                this.AudioRecognizer.Unsubscribe("Snapshot");
                // Enable the skeleton
                this.stkSkeletalBody.IsEnabled = true;
                // Disable Snapshot button
                this.btnSnapshot.IsEnabled = false;
                m_bIsNextButton = false;
                this.btnNext.Content = "Finish";
            }
            else
            {
                Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog()
                {
                    DefaultExt = "*.iava",
                    Filter = "IAVA gesture files (*.iava)| *.iava"                    
                };

                Nullable<bool> results = dialog.ShowDialog();
                if (results == true)
                {
                    string sGestureName = System.IO.Path.GetFileNameWithoutExtension(dialog.FileName);
                    // Set the gesture's name
                    // I set the name of the gesture to the filename without the extension.
                    // I thought this would be easier than showing another control or window to ask for a name
                    // T.S.
                    this.Gesture.Name = sGestureName;
                    // Save the content to a file
                    IavaGesture.Save(this.Gesture, dialog.FileName);
                    MessageBox.Show("File Saved", "Saved!");
                }
                m_bIsNextButton = false;
                this.btnNext.Content = "Next";
            }
        }

        /// <summary>
        /// Updates the SkeletonCanvas sizes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnResize(object sender, SizeChangedEventArgs e) {
            foreach (SkeletonCanvas canvas in gridSnapshot.Children) {
                SizeSkeletonCanvas(canvas);
            }

        }

        /// <summary>
        /// Freezes the current gesture segment and loads the next segment canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSnapshotClick(object sender, RoutedEventArgs e) {
            // Turn the new active canvas into a snapshot.
            Snapshot pNewSnapshot = new Snapshot(_activeSkeletonCanvas.Skeleton);
            // Get the new snapshot taken
            AddSkeletonCanvas();
            // Save the snapshot in the gesture for later saving to a file.
            this.Gesture.Snapshots.Add(pNewSnapshot);
        }

        /// <summary>
        /// Performs the last of the tear-down operations on the Control has finished unloading
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUnloaded(object sender, RoutedEventArgs e) {
            AudioRecognizer.Stop();
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
            SizeSkeletonCanvas(temp);

            // Add the canvas to the grid
            gridSnapshot.Children.Add(temp);

            // Set the active canvas segment
            _activeSkeletonCanvas = temp;

            // Increase the number of segments in the gesture
            GestureSegmentCount++;
        }

        /// <summary>
        /// Resizes the SkeletonCanvas
        /// </summary>
        /// <param name="canvas"></param>
        private void SizeSkeletonCanvas(SkeletonCanvas canvas) {
            canvas.RenderTransform = new ScaleTransform(1.0, -1.0,
                gridSnapshot.ColumnDefinitions[0].ActualWidth / 2.0,
                gridSnapshot.RowDefinitions[0].ActualHeight / 2.0);
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

        private WriteableBitmap _colorBitmap = null;

        private bool _firstRun = true;

        #endregion Private Fields
    }
}
