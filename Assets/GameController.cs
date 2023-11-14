using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private Camera gameCamera;
    private TMP_Text centerPrintText;

    void Awake()
    {
        // Find the camera among the children of this GameObject
        gameCamera = GetComponentInChildren<Camera>();

        // Find the TMP_Text component among the children
        centerPrintText = GetComponentInChildren<TMP_Text>();

        if (gameCamera != null)
        {
            gameCamera.clearFlags = CameraClearFlags.SolidColor;
            gameCamera.backgroundColor = Color.blue;
        }
        else
        {
            Debug.LogError("No camera found among the children of GameController.");
        }

        if (centerPrintText == null) { 
            Debug.LogWarning("TMP_Text component not found among the children of GameController.");
        }

        StartCoroutine(Calibrate(10));
    }

    void Update()
    {
        // Your game logic can go here
    }

    IEnumerator Calibrate(float duration)
    {
        float startTime = Time.time;
        string text = "Calibrating";
        WaitForSeconds wait = new WaitForSeconds(1f);

        while (Time.time - startTime < duration)
        {
            for (int i = 0; i < 3; i++)
            {
                StartCoroutine(DisplayCenterPrintText(text + new string('.', i + 1), 1f));
                yield return wait;
            }

            //yield return new WaitForSeconds(1.0f);
        }
    }

    public IEnumerator DisplayCenterPrintText(string text, float duration)
    {
        if (centerPrintText != null)
        {
            centerPrintText.text = text;

            float startTime = Time.time;

            while (Time.time - startTime < duration)
            {
                yield return null;
            }
        }
        else
        {
            Debug.LogWarning("TMP_Text component not found among the children of GameController.");
        }
    }

}
