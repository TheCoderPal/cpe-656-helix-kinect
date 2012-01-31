﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

using Iava.Audio;
using Iava.Core;

using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

namespace Iava.Test.Audio
{
    /// <summary>
    /// Tests the AudioRecognizer class.
    /// </summary>
    [TestClass]
    public class AudioRecognizerTest
    {
        public AudioRecognizerTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        
        /// <summary>
        /// Called before each test is run.
        /// </summary>
        [TestInitialize()]
        public void MyTestInitialize() 
        {

        }
        
        /// <summary>
        /// Called after each test is run.
        /// </summary>
        [TestCleanup()]
        public void MyTestCleanup() 
        {
            recognizerCallbackInvoked = false;

            if (recognizer != null)
            {
                recognizer.Stop();
            }

            recognizer = null;
        }
        
        #endregion

        /// <summary>
        /// Tests the Start method.
        /// </summary>
        [TestMethod]
        public void StartTest()
        {
            recognizer = new AudioRecognizer(string.Empty);

            Assert.AreEqual<RecognizerStatus>(RecognizerStatus.NotReady, recognizer.Status);
            recognizer.Started += RecognizerStatusCallback;

            try
            {
                recognizer.Start();
                Thread.Sleep(2000);
                Assert.AreEqual<RecognizerStatus>(RecognizerStatus.Running, recognizer.Status);

                // Ensure the OnStarted callback was invoked
                Assert.IsTrue(recognizerCallbackInvoked, "OnStarted callback was not invoked.");

                recognizer.Start();
                Assert.AreEqual<RecognizerStatus>(RecognizerStatus.Running, recognizer.Status);
            }
            finally
            {
                if (recognizer != null)
                {
                    recognizer.Started -= RecognizerStatusCallback;
                }
            }
        }

        /// <summary>
        /// Tests the Stop method.
        /// </summary>
        [TestMethod]
        public void StopTest()
        {
            recognizer = new AudioRecognizer(string.Empty);

            Assert.AreEqual<RecognizerStatus>(RecognizerStatus.NotReady, recognizer.Status);
            recognizer.Stopped += RecognizerStatusCallback;

            try
            {
                recognizer.Start();
                Thread.Sleep(2000);
                Assert.AreEqual<RecognizerStatus>(RecognizerStatus.Running, recognizer.Status);

                recognizer.Stop();
                Thread.Sleep(1000);
                Assert.AreEqual<RecognizerStatus>(RecognizerStatus.Ready, recognizer.Status);
                Assert.IsTrue(recognizerCallbackInvoked, "OnStopped callback was not invoked.");

                // Start and stop immediately after one another and ensure it can be started again
                recognizer.Start();
                recognizer.Stop();
                Assert.AreEqual<RecognizerStatus>(RecognizerStatus.Ready, recognizer.Status);
                recognizer.Start();
                Thread.Sleep(2000);
                Assert.AreEqual<RecognizerStatus>(RecognizerStatus.Running, recognizer.Status);
            }
            finally
            {
                if (recognizer != null)
                {
                    recognizer.Stopped -= RecognizerStatusCallback;
                }
            }
        }

        /// <summary>
        /// Tests the Subscribe method.
        /// </summary>
        [TestMethod]
        public void SubscribeTest()
        {
            const string commandString = "Test Callback";

            // Create a mock speech engine and set it up
            var mockEngine = SetupMockSpeechRecognitionEngine();

            recognizer = new AudioRecognizer(mockEngine.Object);

            int methodCallbackCount = 0;
            bool callback1Invoked = false;
            recognizer.Subscribe(commandString, (eventArgs) => 
                {
                    Assert.AreEqual(commandString, eventArgs.Command, "Command string returned did not match expected value.");
                    methodCallbackCount++;
                    callback1Invoked = true;

                    if (methodCallbackCount > 1)
                    {
                        Assert.Fail("Audio callback method was called when with a confidence value less than the threshold.");
                    }
                });

            // Test for exception throwing on invalid parameters entered
            try
            {
                recognizer.Subscribe(string.Empty, (eventArgs) => { });
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ArgumentException));
            }

            try
            {
                recognizer.Subscribe(commandString, null);
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ArgumentException));
            }
            
            recognizer.Start();
            Thread.Sleep(100);  // Allow the Kinect to initialize

            // Sync the callback first then raise spoken event
            mockEngine.Raise(m => m.SpeechRecognized += null, new IavaSpeechRecognizedEventArgs("Blah Blah blah", AudioRecognizer.AudioConfidenceThreshold + 0.01f));
            mockEngine.Raise(m => m.SpeechRecognized += null, new IavaSpeechRecognizedEventArgs("IAVA", AudioRecognizer.AudioConfidenceThreshold + 0.01f));
            Thread.Sleep(50);
            mockEngine.Raise(m => m.SpeechRecognized += null, new IavaSpeechRecognizedEventArgs(commandString, AudioRecognizer.AudioConfidenceThreshold + 0.01f));
            mockEngine.Raise(m => m.SpeechRecognized += null, new IavaSpeechRecognizedEventArgs("Blah Blah blah", AudioRecognizer.AudioConfidenceThreshold + 0.01f));
            Assert.IsTrue(callback1Invoked);

            // Callback with the same command but with confidence below the threshold level to ensure the audio callback method is not called
            mockEngine.Raise(m => m.SpeechRecognized += null, new IavaSpeechRecognizedEventArgs(commandString, AudioRecognizer.AudioConfidenceThreshold - 0.01f));

            // Test that an exception is not propagated when an audio callback throws an exception
            const string commandString2 = "Command String 2";
            bool callback2Invoked = false;
            recognizer.Subscribe(commandString2, (eventArgs) => 
                {
                    Assert.AreEqual(commandString2, eventArgs.Command, "Command string returned did not match expected value.");
                    callback2Invoked = true;
                    throw new Exception ("Kaboom!");
                });
            Thread.Sleep(100); // Restart occurred, allow time to setup

            mockEngine.Raise(m => m.SpeechRecognized += null, new IavaSpeechRecognizedEventArgs(commandString2, 0.95f));
            Assert.IsTrue(callback2Invoked);
        }

        /// <summary>
        /// Tests the Unsubscribe method.
        /// </summary>
        [TestMethod]
        public void UnsubscribeTest()
        {
            const string commandString = "Test Callback";

            // Create a mock speech engine and set it up
            var mockEngine = SetupMockSpeechRecognitionEngine();

            recognizer = new AudioRecognizer(mockEngine.Object);

            // Raise the speech recognized event, unsubscribe the event, and try the command again
            bool callback1Invoked = false;
            recognizer.Subscribe(commandString, (eventArgs) =>
                {
                    Assert.AreEqual(commandString, eventArgs.Command, "Command string returned did not match expected value.");
                    callback1Invoked = true;
                });

            recognizer.Start();
            Thread.Sleep(100);  // Allow the Kinect to initialize

            mockEngine.Raise(m => m.SpeechRecognized += null, new IavaSpeechRecognizedEventArgs("IAVA", AudioRecognizer.AudioConfidenceThreshold + 0.01f));
            Thread.Sleep(50);

            mockEngine.Raise(m => m.SpeechRecognized += null, new IavaSpeechRecognizedEventArgs(commandString, AudioRecognizer.AudioConfidenceThreshold + 0.01f));
            Assert.IsTrue(callback1Invoked);
            callback1Invoked = false;

            recognizer.Unsubscribe(commandString);
            Thread.Sleep(100);  // Restart occurred, allow time to setup

            mockEngine.Raise(m => m.SpeechRecognized += null, new IavaSpeechRecognizedEventArgs(commandString, AudioRecognizer.AudioConfidenceThreshold + 0.01f));
            Assert.IsFalse(callback1Invoked, "Audio callback method was called when the command was unsubscribed.");
        }

        /// <summary>
        /// Tests the syncing and unsyncing of the recognizer.
        /// </summary>
        [TestMethod]
        public void SyncUnsyncTest()
        {
            const string commandString = "Test Callback";

            // TODO: Make the sync timeout value configurable.  That way this test will not run for a long time

            // Create a mock speech engine and set it up
            var mockEngine = SetupMockSpeechRecognitionEngine();
            recognizer = new AudioRecognizer(mockEngine.Object);

            bool syncedCallbackInvoked = false;
            recognizer.Synced += 
                (sender, args) => 
                {
                    syncedCallbackInvoked = true;
                };

            bool unsyncedCallbackInvoked = false;
            recognizer.Unsynced +=
                (sender, args) =>
                {
                    unsyncedCallbackInvoked = true;
                };

            // Sync the recognizer, call a command, wait until un-sync and re-call the same command.
            // Ensure the callback is not called twice.

            bool callback1Invoked = false;
            recognizer.Subscribe(commandString, (eventArgs) =>
            {
                Assert.AreEqual(commandString, eventArgs.Command, "Command string returned did not match expected value.");
                callback1Invoked = true;
            });

            recognizer.Start();
            Thread.Sleep(100);  // Allow the Kinect to initialize

            mockEngine.Raise(m => m.SpeechRecognized += null, new IavaSpeechRecognizedEventArgs("IAVA", AudioRecognizer.AudioConfidenceThreshold + 0.01f));
            Thread.Sleep(50);
            Assert.IsTrue(syncedCallbackInvoked);

            mockEngine.Raise(m => m.SpeechRecognized += null, new IavaSpeechRecognizedEventArgs(commandString, AudioRecognizer.AudioConfidenceThreshold + 0.01f));
            Assert.IsTrue(callback1Invoked);
            callback1Invoked = false;

            Thread.Sleep(AudioRecognizer.SyncTimeoutValue + 100);
            Assert.IsTrue(unsyncedCallbackInvoked);

            mockEngine.Raise(m => m.SpeechRecognized += null, new IavaSpeechRecognizedEventArgs(commandString, AudioRecognizer.AudioConfidenceThreshold + 0.01f));
            Assert.IsFalse(callback1Invoked);
        }

        #region Private Methods And Attributes

        /// <summary>
        /// Recognizer object under test.
        /// </summary>
        private AudioRecognizer recognizer;

        /// <summary>
        /// Used to determine if the recognizer status ballback was invoked.
        /// </summary>
        private bool recognizerCallbackInvoked;

        /// <summary>
        /// Called when the recognizer's status changes.
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void RecognizerStatusCallback(object sender, EventArgs e)
        {
            recognizerCallbackInvoked = true;
        }

        /// <summary>
        /// Creates and sets up a mock speech recongition engine.
        /// </summary>
        /// <returns>Mock engine</returns>
        private Mock<ISpeechRecognitionEngine> SetupMockSpeechRecognitionEngine()
        {
            var mockEngine = new Mock<ISpeechRecognitionEngine>(MockBehavior.Strict);

            // Setup the methods that are going to be called
            mockEngine.Setup(m => m.LoadGrammar(It.IsAny<Grammar>()));
            mockEngine.Setup(m => m.SetInputToAudioStream(It.IsAny<Stream>(), It.IsAny<SpeechAudioFormatInfo>()));
            mockEngine.Setup(m => m.RecognizeAsync(RecognizeMode.Multiple));
            mockEngine.Setup(m => m.RecognizeAsyncStop());

            return mockEngine;
        }

        #endregion
    }
}
