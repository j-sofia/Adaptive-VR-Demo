﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoomButtonClickEventArgs : EventArgs {

}

public class DD_ZoomButton : MonoBehaviour {

    private DD_DataDiagram m_DataDiagram;

    struct RTParam {

        public Transform parent;
        public Rect rect;
    }

    RTParam[] RTparams = new RTParam[2];
    int paramSN = 0;

    public delegate void ZoomButtonClickHandle(object sender, ZoomButtonClickEventArgs args);
    public ZoomButtonClickHandle ZoomButtonClickEvent;

    private void Awake() {

        m_DataDiagram = GetComponentInParent<DD_DataDiagram>();
        if(null == m_DataDiagram) {
            Debug.LogWarning(this + "Awake Error : null == m_DataDiagram");
            return;
        }
    }

    void Start()
    {
        if (null == m_DataDiagram)
            return;

        RectTransform rt = m_DataDiagram.GetComponent<RectTransform>();

        if (rt != null)
        {
            if (RTparams[0].parent != null)
            {
                RectTransform parentRectTransform = RTparams[0].parent.GetComponent<RectTransform>();
                if (parentRectTransform != null)
                {
                    RTparams[0].rect = DD_CalcRectTransformHelper.CalcLocalRect(
                        rt.anchorMin,
                        rt.anchorMax,
                        parentRectTransform.rect.size,
                        rt.pivot,
                        rt.anchoredPosition,
                        rt.rect
                    );
                }
                else
                {
                    Debug.LogError("Parent does not have a RectTransform component.");
                }
            }
            else
            {
                Debug.LogError("RTparams[0].parent is null.");
            }
        }
        else
        {
            Debug.LogError("m_DataDiagram does not have a RectTransform component.");
        }

        RTparams[1].parent = GetComponentInParent<Canvas>().transform;
        RTparams[1].rect = new Rect(new Vector2(Screen.width / 10, Screen.height / 10),
            new Vector2(Screen.width * 8 / 10, Screen.height * 8 / 10));

        paramSN = 0;
    }


    // Update is called once per frame
    void Update () {
		
	}

    public void OnZoomButton() {

        if (null == m_DataDiagram)
            return;

        paramSN = (paramSN + 1) % 2;

        m_DataDiagram.transform.SetParent(RTparams[paramSN].parent);
        m_DataDiagram.rect = RTparams[paramSN].rect;

        if (null != ZoomButtonClickEvent)
            ZoomButtonClickEvent(this, new ZoomButtonClickEventArgs());
    }

}
