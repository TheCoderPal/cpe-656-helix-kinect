﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace IAVA
{
    interface IRecognizer
    {
        #region Properties
        /// <summary>
        /// Gets the file used for configuration of the recognizer.
        /// </summary>
        public File Configuration
        {
            get;
            protected set;
        }
        /// <summary>
        /// Gets the status of the cognizer.
        /// </summary>
        public RecognizerStatus Status
        {
            get;
            protected set;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Used to initialize the recognizer object using the configuration
        /// file in the parameter of the function.
        /// </summary>
        /// <param name="configurationFile"></param>
        public void Create(File configurationFile);
        /// <summary>
        /// Starts the recognizer.
        /// </summary>
        public void Start();
        /// <summary>
        /// Stops the recognizer.
        /// </summary>
        public void Stop();
        /// <summary>
        /// Used to subscribe the callback delegate with the specific delegate
        /// speficed by the eventName parameter.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="callback"></param>
        public void Subscribe(string eventName, Delegate callback);
        /// <summary>
        /// Used to unsubscribe the callback deledate.
        /// </summary>
        /// <param name="eventName"></param>
        public void Unsubscribe(string eventName);
        #endregion
    }
}