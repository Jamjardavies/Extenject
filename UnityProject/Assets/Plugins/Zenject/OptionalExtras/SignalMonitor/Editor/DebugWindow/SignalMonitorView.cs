using System;
using System.Diagnostics;
using System.Reflection;
using Boo.Lang;
using UnityEditor;
using UnityEngine;

namespace Zenject.SignalMonitor
{
    public class SignalMonitorView : ISignalMonitor, IInitializable, IDisposable, IGuiRenderable
    {
        [Serializable]
        private struct SignalData
        {
            public object Signal { get; set; }
            public DateTime Timestamp { get; set; }
            public TimeSpan RuntimeSeconds { get; set; }
            public StackFrame Caller { get; set; }
            public string File { get; set; }
        }

        private const float TimeWidth = 200;
        private const float SignalNameWidth = 400;
        private const float MethodWidth = 800;

        private GUIStyle m_linkStyle;

        [SerializeField] private readonly List<SignalData> m_firedSignals = new List<SignalData>();
        [SerializeField] private Vector2 m_scrollPos;

        public void SignalFired(object signal)
        {
            // Find the caller.
            StackTrace trace = new StackTrace(true);

            SignalData data = new SignalData
            {
                Signal = signal,
                Timestamp = DateTime.Now,
                RuntimeSeconds = TimeSpan.FromSeconds(Time.realtimeSinceStartup),
                Caller = trace.GetFrame(5), // It's 5 now, this might change if any more methods are added/changed.
            };

            string filename = data.Caller.GetFileName();

            if (!string.IsNullOrEmpty(filename))
            {
                data.File = filename.Substring(filename.IndexOf("Assets", StringComparison.Ordinal));
            }

            m_firedSignals.Add(data);

            // Force scroll to the bottom.
            m_scrollPos.y = float.MaxValue;
        }

        public void Initialize()
        {
            SignalMonitorRegistry.AddSignalMonitor(this);
        }

        public void Dispose()
        {
            SignalMonitorRegistry.RemoveSignalMonitor(this);
        }

        public void GuiRender()
        {
            if (m_linkStyle == null)
            {
                m_linkStyle = new GUIStyle(EditorStyles.linkLabel);
            }

            using (new GUILayout.VerticalScope())
            {
                DrawHeader();

                using (GUILayout.ScrollViewScope scroll = new GUILayout.ScrollViewScope(m_scrollPos))
                {
                    foreach (SignalData signal in m_firedSignals)
                    {
                        DrawSignal(signal);
                    }

                    m_scrollPos = scroll.scrollPosition;
                }
            }
        }

        private static void DrawHeader()
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Time", GUILayout.Width(TimeWidth));
                GUILayout.Label("Signal", GUILayout.Width(SignalNameWidth));
                GUILayout.Label("Method", GUILayout.Width(MethodWidth));
                //GUILayout.Label("Paused", GUILayout.Width(PauseWidth));
            }
        }

        private void DrawSignal(SignalData signal)
        {
            using (new GUILayout.HorizontalScope())
            {
                // ToDo: Add toggle between realtime and runtime.
                GUILayout.Label($"{signal.RuntimeSeconds:g}", GUILayout.Width(TimeWidth));

                MethodBase method = signal.Caller.GetMethod();
                string methodName = $"{method.DeclaringType?.Name}.{method.Name}";

                bool clicked = false;
                
                clicked |= GUILayout.Button(signal.Signal.GetType().Name, m_linkStyle, GUILayout.Width(SignalNameWidth));
                clicked |= GUILayout.Button(methodName, m_linkStyle, GUILayout.Width(MethodWidth));

                if (clicked)
                {
                    OpenFile(signal);
                }
            }
        }

        private static void OpenFile(SignalData signal)
        {
            // Make sure we found a filename.
            if (string.IsNullOrEmpty(signal.File))
            {
                return;
            }

            // Load the asset so we can open to the line.
            MonoScript asset = AssetDatabase.LoadAssetAtPath<MonoScript>(signal.File);

            if (asset == null)
            {
                return;
            }

            // Finally open the line number.
            AssetDatabase.OpenAsset(asset, signal.Caller.GetFileLineNumber());
        }
    }
}