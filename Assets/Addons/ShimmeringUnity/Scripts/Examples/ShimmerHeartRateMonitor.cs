using UnityEngine;
using ShimmerAPI;
using ShimmerLibrary;
using System;
using System.Collections.Generic;

namespace ShimmeringUnity
{
    /// <summary>
    /// Basic example of measuring heart rate from the shimmer device
    /// Ensure the ShimmerDevice has internalExpPower enabled and the
    /// INTERNAL_ADC_A13 sensor enabled. Also ensure the correct
    /// sampling rate is set before running the application.
    /// </summary>
    public class ShimmerHeartRateMonitor : MonoBehaviour
    {
        public GameObject GameController;


        [SerializeField]
        private ShimmerDevice shimmerDevice;

        List<GameObject> lineList = new List<GameObject>();
        private DD_DataDiagram m_DataDiagram;
        private float h = 0;

        [SerializeField]
        private int heartRate;
        Filter LPF_PPG;
        Filter HPF_PPG;
        PPGToHRAlgorithm PPGtoHeartRateCalculation;
        int NumberOfHeartBeatsToAverage = 1;
        int TrainingPeriodPPG = 10; //10 second buffer
        double LPF_CORNER_FREQ_HZ = 5;
        double HPF_CORNER_FREQ_HZ = 0.5;

        void AddALine()
        {

            if (null == m_DataDiagram)
                return;

            Color color = Color.HSVToRGB((h += 0.1f) > 1 ? (h - 1) : h, 0.8f, 0.8f);
            GameObject line = m_DataDiagram.AddLine(color.ToString(), color);
            if (null != line)
                lineList.Add(line);
        }

        void Start()
        {
            GameObject dd = GameObject.Find("DataDiagram");
            if (null == dd)
            {
                Debug.LogWarning("can not find a gameobject of DataDiagram");
                return;
            }
            m_DataDiagram = dd.GetComponent<DD_DataDiagram>();

            m_DataDiagram.PreDestroyLineEvent += (s, e) => { lineList.Remove(e.line); };

            AddALine();
            AddALine();
            AddALine();
        }

        private void Awake()
        {
            //Create the heart rate algorithms 
            PPGtoHeartRateCalculation = new PPGToHRAlgorithm(shimmerDevice.SamplingRate, NumberOfHeartBeatsToAverage, TrainingPeriodPPG);
            LPF_PPG = new Filter(Filter.LOW_PASS, shimmerDevice.SamplingRate, new double[] { LPF_CORNER_FREQ_HZ });
            HPF_PPG = new Filter(Filter.HIGH_PASS, shimmerDevice.SamplingRate, new double[] { HPF_CORNER_FREQ_HZ });
        }
        private void OnEnable()
        {
            shimmerDevice.OnDataRecieved.AddListener(OnDataRecieved);
        }

        private void OnDisable()
        {
            shimmerDevice.OnDataRecieved.RemoveListener(OnDataRecieved);
        }

        private void OnDataRecieved(ShimmerDevice device, ObjectCluster objectCluster)
        {
            //Get heart rate data
            SensorData dataPPG = objectCluster.GetData(
                ShimmerConfig.NAME_DICT[ShimmerConfig.SignalName.INTERNAL_ADC_A13],
                ShimmerConfig.FORMAT_DICT[ShimmerConfig.SignalFormat.CAL]
            );
            //Get system  timestamp data
            SensorData dataTS = objectCluster.GetData(
                ShimmerConfig.NAME_DICT[ShimmerConfig.SignalName.SYSTEM_TIMESTAMP],
                ShimmerConfig.FORMAT_DICT[ShimmerConfig.SignalFormat.CAL]
            );

            //Early out if either sensor data is null
            if (dataPPG == null || dataTS == null)
                return;

            //Calculate the heart rate
            double dataFilteredLP = LPF_PPG.filterData(dataPPG.Data);
            double dataFilteredHP = HPF_PPG.filterData(dataFilteredLP);
            heartRate = (int)Math.Round(PPGtoHeartRateCalculation.ppgToHrConversion(dataFilteredHP, dataTS.Data));

            m_DataDiagram.InputPoint(lineList[0], new Vector2(0.1f, heartRate));

            m_DataDiagram.InputPoint(lineList[1], new Vector2(0.1f, (float)dataPPG.Data/1600*100));

            m_DataDiagram.InputPoint(lineList[2], new Vector2(0.1f, (float)dataFilteredHP));

            StartCoroutine(GameController.GetComponent<GameController>().DisplayCenterPrintText("Heartrate: " + heartRate.ToString(), 1f));
        }
    }

}