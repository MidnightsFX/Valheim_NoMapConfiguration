using HarmonyLib;
using Jotunn.Managers;
using NoMapTools.common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger = NoMapTools.common.Logger;

namespace NoMapTools.modules {
    internal static class VegvisirFucntionality {

        internal class TrackedParticle {
            public GameObject go { get; set; }
            public Vector3 vel { get; set; }
        }

        [HarmonyPatch(typeof(Vegvisir))]
        private static class Patch_Vegvisir_Interact {
            [HarmonyPatch(nameof(Vegvisir.Interact))]
            private static bool Prefix(Vegvisir __instance, Humanoid character, ref bool __result) {
                Player player = (Player)character;
                // Skip the interaction
                if (player.GetComponent<NoMapLocationTracker>() == null) {
                    ZoneSystem.instance.FindClosestLocation(__instance.m_locations.First().m_locationName, player.transform.position, out var closest);
                    NoMapLocationTracker nmtracker = player.gameObject.AddComponent<NoMapLocationTracker>();
                    Logger.LogDebug($"Tracking {__instance.m_locations.First().m_locationName} at {closest}");
                    nmtracker.Setup(ValConfig.VegvisirTrackerDuration.Value, closest.m_position);
                    player.Message(MessageHud.MessageType.Center, $"You are briefly tracking {__instance.m_locations.First().m_pinName}");
                }
                __result = false;
                return false;
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
                targetPosition = targetlocation;
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
