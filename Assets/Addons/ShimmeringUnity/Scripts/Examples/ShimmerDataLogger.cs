using System.Collections.Generic;
using ShimmerAPI;
using UnityEngine;
using System.IO;
using System;

namespace ShimmeringUnity
{
    /// <summary>
    /// Example of logging data from a shimmer device
    /// </summary>
    public class ShimmerDataLogger : MonoBehaviour
    {

        private string logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "shimmer_log.txt");
        private int checkpoint_num = 0;

        [SerializeField]
        [Tooltip("Reference to the shimmer device.")]
        private ShimmerDevice shimmerDevice;

        [System.Serializable]
        public class Signal
        {
            [SerializeField]
            [Tooltip("The signal's name. More info in the signals section of the readme.")]
            private ShimmerConfig.SignalName name;

            public ShimmerConfig.SignalName Name => name;

            [SerializeField]
            [Tooltip("The signal's format.")]
            private ShimmerConfig.SignalFormat format;

            public ShimmerConfig.SignalFormat Format => format;

            [SerializeField]
            [Tooltip("The units the signal's value is displayed in, set to \"Automatic\" for default.")]
            private ShimmerConfig.SignalUnits unit;

            public ShimmerConfig.SignalUnits Unit => unit;

            [SerializeField]
            [Tooltip("The value output of this signal (only for debug purposes).")]
            private string value;

            public string Value
            {
                set => this.value = value;
            }
        }

        [SerializeField]
        [Tooltip("List of signals to record from this device.")]
        private List<Signal> signals = new List<Signal>();

        private void OnEnable()
        {
            //Listen to the data recieved event when enabled
            shimmerDevice?.OnDataRecieved.AddListener(OnDataRecieved);
        }

        private void OnDisable()
        {
            //Stop listening to the data recieved event when disabled
            shimmerDevice?.OnDataRecieved.RemoveListener(OnDataRecieved);
        }

        /// <summary>
        /// Event listener for the shimmer device's data recieved event
        /// </summary>
        /// <param name="device"></param>
        /// <param name="objectCluster"></param>
        private void OnDataRecieved(ShimmerDevice device, ObjectCluster objectCluster)
        {
            foreach (var signal in signals)
            {
                //Get the data
                SensorData data = signal.Unit == ShimmerConfig.SignalUnits.Automatic ?
                    objectCluster.GetData(
                        ShimmerConfig.NAME_DICT[signal.Name],
                        ShimmerConfig.FORMAT_DICT[signal.Format]) :
                    objectCluster.GetData(
                        ShimmerConfig.NAME_DICT[signal.Name],
                        ShimmerConfig.FORMAT_DICT[signal.Format],
                        ShimmerConfig.UNIT_DICT[signal.Unit]);

                //If data is null, early out
                if (data == null)
                {
                    signal.Value = "NULL";
                    return;
                }

                //Write data back into the signal for debugging
                signal.Value = $"{data.Data} {data.Unit}";

                //This is where you can do something with the data...

                LogDataToFile(signal.Name.ToString(), $"{data.Data} {data.Unit}");
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Checkpoint " + checkpoint_num.ToString());
                LogDataToFile("Checkpoint: ", checkpoint_num.ToString());
                checkpoint_num += 1;
            }
        }

        /// <summary>
        /// Logs data to a file.
        /// </summary>
        /// <param name="signalName">Name of the signal</param>
        /// <param name="value">Value of the signal</param>
        private void LogDataToFile(string signalName, string value)
        {
            try
            {
                using (StreamWriter sw = File.AppendText(logFilePath))
                {
                    sw.WriteLine($"{signalName}: {value}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error writing to log file: {ex.Message}");
            }
        }
    }
}
