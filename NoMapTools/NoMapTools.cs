using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using NoMapTools.common;
using NoMapTools.modules;
using PlayFab.Internal;
using System.Reflection;
using UnityEngine;

namespace NoMapTools
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class NoMapTools : BaseUnityPlugin
    {
        public const string PluginGUID = "MidnightsFX.NoMapTools";
        public const string PluginName = "NoMapTools";
        public const string PluginVersion = "0.0.3";

        public static AssetBundle EmbeddedResourceBundle;

        public ValConfig cfg;
        public static ManualLogSource Log;
        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        public void Awake() {
            Log = this.Logger;
            cfg = new ValConfig(Config);

            EmbeddedResourceBundle = AssetUtils.LoadAssetBundleFromResources("NoMapTools.assets.nomap", typeof(NoMapTools).Assembly);

            Assembly assembly = Assembly.GetExecutingAssembly();
            Harmony harmony = new(PluginGUID);
            harmony.PatchAll(assembly);
            Logger.LogInfo("Whats a map?");
            //Logger.LogInfo($"Asset Names: {string.Join(",\n", EmbeddedResourceBundle.GetAllAssetNames())}");
        }
    }
}