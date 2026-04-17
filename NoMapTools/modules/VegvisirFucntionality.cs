using HarmonyLib;
using Jotunn.Managers;
using NoMapTools.common;
using System.Linq;
using UnityEngine;
using Logger = NoMapTools.common.Logger;

namespace NoMapTools.modules {
    internal static class VegvisirFucntionality {

        [HarmonyPatch]
        private static class Patch_Vegvisir_Interact {
            static float InteractTimer = 0;
            static string TargetLocation = "";
            static string DisplayName = "";

            [HarmonyPrefix]
            [HarmonyPatch(typeof(Vegvisir), nameof(Vegvisir.Interact))]
            private static void VegInteract(Vegvisir __instance) {
                InteractTimer = Time.realtimeSinceStartup;
                TargetLocation = __instance.m_locations.First().m_locationName;
                DisplayName = __instance.m_locations.First().m_pinName;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(Minimap), nameof(Minimap.DiscoverLocation))]
            private static bool PrefixMapPin() {
                // If we recently interacted with a vegvesir, skip adding pins
                if (ValConfig.VegvisirGivesMapIcon.Value == false && InteractTimer < Time.realtimeSinceStartup + ValConfig.VegvisirNetworkWaitDuration.Value) {
                    return true;
                }
                return false;
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(Game), nameof(Game.RPC_DiscoverLocationResponse))]
            private static void PrefixLocationTrack(Vector3 pos) {
                if (Player.m_localPlayer == null) { return; }

                if (InteractTimer < Time.realtimeSinceStartup + ValConfig.VegvisirNetworkWaitDuration.Value) {
                    if (Player.m_localPlayer.GetComponent<NoMapLocationTracker>() == null) {
                        NoMapLocationTracker nmtracker = Player.m_localPlayer.gameObject.AddComponent<NoMapLocationTracker>();
                        Logger.LogDebug($"Tracking {TargetLocation} at {pos}");
                        nmtracker.Setup(ValConfig.VegvisirTrackerDuration.Value, pos);
                        Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"You are briefly tracking {DisplayName}");
                    }
                    return;
                }
            }

        }

        public class NoMapLocationTracker : MonoBehaviour {
            float timeRemaining = 0;
            float nextParticleSpawnTimer = 0;
            Vector3 targetPosition = Vector3.zero;
            bool setup = false;
            GameObject sfxfinder = null;
            GameObject vfxfinder = null;

            internal void LoadTrackingEffects() {
                sfxfinder = PrefabManager.Instance.GetPrefab("sfx_WishbonePing_far");
                vfxfinder = NoMapTools.EmbeddedResourceBundle.LoadAsset<GameObject>($"assets/assets/vfx_location_ping.prefab");
            }

            public void Setup(int time, Vector3 targetlocation) {
                LoadTrackingEffects();
                float yground = ZoneSystem.instance.GetGroundHeight(targetlocation);
                targetPosition = new Vector3(targetlocation.x, yground, targetlocation.z);
                timeRemaining = time + Time.realtimeSinceStartup;
                setup = true;
            }

            public void Update() {
                // Nothing to do if not setup
                if (setup == false) { return; }

                if (nextParticleSpawnTimer < Time.realtimeSinceStartup) {
                    Vector3 currentPositionStart = new Vector3() { x = Player.m_localPlayer.transform.position.x, z = Player.m_localPlayer.transform.position.z, y = Player.m_localPlayer.transform.position.y + 1f };
                    if (sfxfinder != null) {
                        GameObject.Instantiate(sfxfinder, currentPositionStart, Quaternion.identity);
                    }

                    GameObject spawnedEffect = GameObject.Instantiate(vfxfinder, currentPositionStart, Quaternion.identity);
                    Vector3 direction = (targetPosition - spawnedEffect.transform.position).normalized;
                    Vector3 force = direction * ValConfig.VegvisirTrackerSpeed.Value;
                    spawnedEffect.GetComponent<Rigidbody>().AddForce(force, ForceMode.VelocityChange);
                    // Todo: make configurable the interval
                    nextParticleSpawnTimer = Time.realtimeSinceStartup + 5f;
                }


                // Destroy once the time is up
                if (timeRemaining < Time.realtimeSinceStartup) {
                    GameObject.Destroy(this);
                }
            }
        }
    }
}
