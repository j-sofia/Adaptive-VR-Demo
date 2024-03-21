using ShimmeringUnity;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject Graphs;

    private ShimmerDevice shimmerDevice;
    private ShimmerHeartRateMonitor shimmerHeartRateMonitor;
    private Camera gameCamera;
    private TMP_Text centerPrintText;
    private TMP_Text heartrateText;

    private bool isConnectingCoroutineRunning = false;
    private bool isCalibratingCoroutineRunning = false;

    void Awake()
    {
        gameCamera = GetComponentInChildren<Camera>();

        var shimmerGameObject = GameObject.Find("ShimmerDevice");
        shimmerDevice = shimmerGameObject.GetComponent<ShimmerDevice>();
        shimmerHeartRateMonitor = shimmerGameObject.GetComponent<ShimmerHeartRateMonitor>();


        centerPrintText = gameObject.transform.GetChild(0).GetComponent<TMP_Text>();
        heartrateText = gameObject.transform.GetChild(1).GetComponent<TMP_Text>();

        if (gameCamera != null)
        {
            gameCamera.clearFlags = CameraClearFlags.SolidColor;
            //gameCamera.backgroundColor = Color.blue;
        }
        else
        {
            Debug.LogError($"No camera found among the children of {nameof(GameController)}.");
        }

        if (shimmerDevice == null)
        {
            Debug.LogError($"ShimmerDevice not found by {nameof(GameController)}.");
        }

        if (centerPrintText == null || heartrateText == null) { 
            Debug.LogWarning($"TMP_Text component not found among the children of {nameof(GameController)}.");
        } else
        {
            centerPrintText.text = "";
            heartrateText.text = "";
        }

        shimmerDevice.Connect();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            Graphs.transform.localScale = Graphs.transform.localScale == new Vector3(1, 1, 1) ? new Vector3(0, 0, 0) : new Vector3(1, 1, 1);
        }

        if (shimmerDevice.CurrentState == ShimmerDevice.State.Connecting)
        {
            if (!isConnectingCoroutineRunning)
            {
                StartCoroutine(LoadingText("Connecting", 1));
                isConnectingCoroutineRunning = true;
            }
        }
        else if (shimmerDevice.CurrentState == ShimmerDevice.State.Connected)
        {
            shimmerDevice.StartStreaming();
            isConnectingCoroutineRunning = false;
        }
        else if (shimmerDevice.CurrentState == ShimmerDevice.State.Streaming && shimmerHeartRateMonitor.HeartRate == -1f)
        {
            if (!isCalibratingCoroutineRunning)
            {
                StartCoroutine(LoadingText("Calibrating", 1));
                isCalibratingCoroutineRunning = true;
            }
        }
        else
        {
            isConnectingCoroutineRunning = false;
            if (shimmerHeartRateMonitor.HeartRate != -1f)
            {
                isCalibratingCoroutineRunning = false;
            }
            centerPrintText.text = "";
        }
    }

    IEnumerator LoadingText(string text, float duration)
    {
        float startTime = Time.time;
        WaitForSeconds wait = new WaitForSeconds(1f);

        while (Time.time - startTime < duration)
        {
            for (int i = 0; i < 3; i++)
            {
                StartCoroutine(DisplayCenterPrintText(text + new string('.', i + 1), 1f));
                yield return wait;
            }
        }
        isConnectingCoroutineRunning = false;
        isCalibratingCoroutineRunning = false;
    }

    public IEnumerator DisplayCenterPrintText(string text, float duration)
    {
        centerPrintText.text = text;

        float startTime = Time.time;

        while (Time.time - startTime < duration)
        {
            yield return null;
        }
    }

    public IEnumerator DisplayHeartrateText(string text, float duration)
    {
        heartrateText.text = text;

        float startTime = Time.time;

        while (Time.time - startTime < duration)
        {
            yield return null;
        }
    }
}
