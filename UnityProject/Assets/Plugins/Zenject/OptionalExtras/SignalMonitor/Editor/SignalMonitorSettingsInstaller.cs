using UnityEngine;

namespace Zenject.SignalMonitor
{
    [CreateAssetMenu(fileName = "SignalMonitorSettingsInstaller", menuName = "Installers/SignalMonitorSettingsInstaller")]
    public class SignalMonitorSettingsInstaller : ScriptableObjectInstaller<SignalMonitorSettingsInstaller>
    {
        public override void InstallBindings()
        {
            //Container.BindInstance(EditorGUIUtility.isProSkin ? MpmViewDark : MpmView);
        }
    }
}