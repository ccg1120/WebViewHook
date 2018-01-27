﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class WebEditor : EditorWindow
{

    WebViewHook webView;

    string head = @"<!-- Use this to make custom html placed on head -->";
    string html = @"<h1>Hello World</h1>";
    string css = "body {\n\tbackground-color: white;\n}";
    string js = "/* Javascript Goes Here */";
    int panel = 0;

    class Styles
    {
        public static string template = "<html>\n<head>{3}\n<style>{1}</style>\n<script>{2}</script>\n</head>\n<body>\n{0}\n</body>\n</html>";
        public static GUIContent[] heads = new GUIContent[] { new GUIContent("HTML"), new GUIContent("CSS"), new GUIContent("JS"), new GUIContent("Head") };
        public static GUIStyle[] headStyles = new GUIStyle[] { EditorStyles.miniButtonLeft, EditorStyles.miniButtonMid, EditorStyles.miniButtonMid, EditorStyles.miniButtonRight };
    }

    public string this[int idx]
    {
        get
        {
            switch (idx)
            {
                case 0: return html;
                case 1: return css;
                case 2: return js;
                case 3: return head;
                default: return "";
            }
        }
        set
        {
            switch (idx)
            {
                case 0: html = value; break;
                case 1: css = value; break;
                case 2: js = value; break;
                case 3: head = value; break;
            }
        }
    }

    [MenuItem("Tools/Web Editor %#e")]
    static void Load()
    {
        WebEditor window = GetWindow<WebEditor>();
        window.Show();
    }

    void OnEnable()
    {
        if (!webView)
        {
            // create webView
            webView = CreateInstance<WebViewHook>();
        }
    }

    public void OnBecameInvisible()
    {
        if (webView)
        {
            // signal the browser to unhook
            webView.Detach();
        }
    }

    void OnDestroy()
    {
        //Destroy web view
        DestroyImmediate(webView);
    }

    string Compose()
    {
        return string.Format(Styles.template, html, css, js, head);
    }

    void OnGUI()
    {
        // hook to this window
        if (webView.Hook(this))
            // do the first thing to do
            webView.LoadHTML(Compose());

        var half = position.width / 2;

        // head/body/css/js
        var rect = new Rect(0, 0, (half - 50) / 4, 30);

        EditorGUI.BeginChangeCheck();
        for (int i = 0; i < 4; i++)
        {
            if (GUI.Toggle(rect, i == panel, Styles.heads[i], Styles.headStyles[i]))
                panel = i;
            rect.x += rect.width;
        }

        if (GUI.Button(new Rect(half - 45, 0, 45, 30), "Copy"))
            GUIUtility.systemCopyBuffer = Compose();

        if (EditorGUI.EndChangeCheck())
            // need this so text field can be updated
            GUIUtility.keyboardControl = -1;

        // html text field
        EditorGUI.BeginChangeCheck();
        this[panel] = EditorGUI.TextArea(new Rect(0, 30, half, position.height - 40), this[panel]);
        if (EditorGUI.EndChangeCheck())
            webView.LoadHTML(Compose());

        if (Event.current.type == EventType.Repaint)
        {
            // keep the browser aware with resize
            webView.OnGUI(new Rect(half, 20, half, position.height - (0)));
        }
    }
}

