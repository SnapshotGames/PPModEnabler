using System.Collections.Generic;
using System;
using Base.Core;
using HarmonyLib;
using PhoenixPoint.Common.Game;

namespace Doorstop
{
	// Support old and new entry points of Doorstop

	static class Loader
	{
		public static void Main(string[] args)
		{
			Entrypoint.Start();
		}
	}

	public class Entrypoint
	{

		public static void Start()
		{
			AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;

		}

		private static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
		{
			if (args.LoadedAssembly.GetName().Name == "Assembly-CSharp")
			{
				// Patch game
				Harmony harmony = new Harmony("PPModEnabler");
				harmony.PatchAll();
			}
		}
	}
}



[HarmonyPatch(typeof(PhoenixGame), "InitMods")]
public class InitMods
{
	static bool Prefix(PhoenixGame __instance, ref IEnumerator<NextUpdate> __result)
	{
		__result = new List<NextUpdate>() { NextUpdate.Never }.GetEnumerator();

		__instance.ModManager.CanUseMods = true;

		// Disable mods if demo
		if (__instance.Platform.IsDemo())
			__instance.ModManager.CanUseMods = false;


		OptionsComponent optionsComponent = __instance.GetComponent<OptionsComponent>();


		if (__instance.ModManager.CanUseMods)
		{
			while (!optionsComponent.InitialOptionsLoaded)
				System.Threading.Thread.Sleep(1);

			if (!__instance.ModManager.Initialize(__instance))
				return false;

			__instance.ModManager.DiscoverMods();

			__instance.ModManager.EnableModsFromStore(optionsComponent.Options);

			__instance.ModManager.SaveModConfig();
		}

		return false;
	}
}
