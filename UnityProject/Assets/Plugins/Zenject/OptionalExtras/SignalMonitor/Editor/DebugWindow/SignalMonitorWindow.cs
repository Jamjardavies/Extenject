using UnityEngine;
using UnityEditor;

namespace Zenject.SignalMonitor
{
    public class SignalMonitorWindow : ZenjectEditorWindow
    {
        [MenuItem("Window/Zenject/Signal Monitor")]
        public static SignalMonitorWindow GetOrCreateWindow()
        {
            SignalMonitorWindow window = GetWindow<SignalMonitorWindow>();

            window.titleContent = new GUIContent("Signal Monitor");

            return window;
        }

        public override void InstallBindings()
        {
            //SignalMonitorSettingsInstaller.InstallFromResource(Container);

            Container.BindInstance(this);
            Container.BindInterfacesTo<SignalMonitorView>().AsSingle();
        }
    }
}
