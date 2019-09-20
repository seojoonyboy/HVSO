using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Haegin
{
	public class AssetBundlesMenuItems
	{
        const string kSimulationMode = "Haegin/AssetBundles : Simulation Mode";
	
		[MenuItem(kSimulationMode)]
		public static void ToggleSimulationMode ()
		{
			AssetBundleManager.SimulateAssetBundleInEditor = !AssetBundleManager.SimulateAssetBundleInEditor;
		}
	
		[MenuItem(kSimulationMode, true)]
		public static bool ToggleSimulationModeValidate ()
		{
			Menu.SetChecked(kSimulationMode, AssetBundleManager.SimulateAssetBundleInEditor);
			return true;
		}
	}
}