﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using Iava.Input.Camera;

namespace GestureRecorder.Data
{
    [XmlRoot("Gesture", Namespace = "urn:Gestures")]
    public class Gesture
    {
        #region Private Fields

        private List<Snapshot> m_pSnapshots = new List<Snapshot>();

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        /// Gets the list of snapshots for the current Gesture.
        /// </summary>
        [XmlElement("Snapshot")]
        public List<Snapshot> Snapshots
        {
            get
            {
                return this.m_pSnapshots;
            }
            set
            {
                this.m_pSnapshots = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the gesture.
        /// </summary>
        [XmlAttribute("Name")]
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the fudginess of the gesture.
        /// </summary>
        [XmlAttribute("FudgeFactor")]
        public double FudgeFactor
        {
            get;
            set;
        }

        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// Default Constructor
        /// </summary>
        public Gesture()
        {
            //Nothing to do
        }

        #endregion Constructors

        #region Public Methods
        /// <summary>
        /// Sets all the snapshots to track this specified joint.
        /// </summary>
        /// <param name="joint"></param>
        public void SetTrackingJoints(params JointID[] joints)
        {
            foreach (var segment in this.Snapshots)
            {
                segment.SetTrackingJoints(joints);
            }
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// Save the gesture into an xml file.
        /// </summary>
        /// <param name="gesture"></param>
        /// <param name="path"></param>
        public static void Save(Gesture gesture, string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Gesture));
            TextWriter textWriter = new StreamWriter(path);
            serializer.Serialize(textWriter, gesture);
            textWriter.Close();
        }
        /// <summary>
        /// Creates a gesture from the current xml file path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Gesture Load(string path)
        {
            Gesture newGesture = null;
            XmlSerializer deserializer = new XmlSerializer(typeof(Gesture));
            TextReader textReader = new StreamReader(path);
            newGesture = (Gesture)deserializer.Deserialize(textReader);
            textReader.Close();

            return newGesture;
        }
        #endregion
    }
}