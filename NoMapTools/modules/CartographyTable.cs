using HarmonyLib;
using Ionic.Zlib;
using NoMapTools.common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NoMapTools.modules {
    internal static class CartographyTable {

        
        [HarmonyPatch(typeof(MapTable))]
        private static class CartographyTableRequiresLargerBase {
            private static readonly int pieceMask = LayerMask.GetMask("piece"); // "piece_nonsolid"
            private static float update_timer = 0f;
            private static float current_update_time = 0f;
            private static int nearby_pieces = 0;

            [HarmonyPatch(nameof(MapTable.GetReadHoverText))]
            [HarmonyPrefix]
            private static bool GetHoverReadText(MapTable __instance, ref string __result) {
                // If we do not meet the piece requirements prevent functionality, and mention
                if (CheckPieceRequirement(__instance.gameObject.transform.position) == false) {
                    __result = Localization.instance.Localize(__instance.m_name + $"\n[<color=yellow><b>$KEY_Use</b></color>] More nearby structures required ({nearby_pieces}/{ValConfig.CartographyTableRequiredPieces.Value})");
                    return false;
                }
                return true;
            }

            [HarmonyPatch(nameof(MapTable.GetWriteHoverText))]
            [HarmonyPrefix]
            private static bool GetHoverWriteText(MapTable __instance, ref string __result) {
                // If we do not meet the piece requirements prevent functionality, and mention
                if (CheckPieceRequirement(__instance.gameObject.transform.position) == false) {
                    __result = Localization.instance.Localize(__instance.m_name + $"\n[<color=yellow><b>$KEY_Use</b></color>] More nearby structures required ({nearby_pieces}/{ValConfig.CartographyTableRequiredPieces.Value})");
                    return false;
                }
                return true;
            }

            [HarmonyPatch(nameof(MapTable.OnRead), argumentTypes: new Type[] { typeof(Switch), typeof(Humanoid), typeof(ItemDrop.ItemData) } )]
            [HarmonyPrefix]
            private static bool Onread(MapTable __instance, ref bool __result) {
                // If we do not meet the piece requirements prevent functionality, and mention
                if (CheckPieceRequirement(__instance.gameObject.transform.position) == false) {
                    __result = false;
                    return false;
                }
                return true;
            }

            [HarmonyPatch(nameof(MapTable.OnWrite), argumentTypes: new Type[] { typeof(Switch), typeof(Humanoid), typeof(ItemDrop.ItemData) })]
            [HarmonyPrefix]
            private static bool OnWrite(MapTable __instance, ref bool __result) {
                // If we do not meet the piece requirements prevent functionality, and mention
                if (CheckPieceRequirement(__instance.gameObject.transform.position) == false) {
                    __result = false;
                    return false;
                }
                return true;
            }

            private static bool CheckPieceRequirement(Vector3 position) {
                current_update_time += UnityEngine.Time.fixedDeltaTime;
                if (update_timer <= current_update_time) {
                    // TODO: configurable duration for delta offset
                    update_timer = current_update_time + 10;
                    Collider[] nearbyPieces = Physics.OverlapSphere(position, ValConfig.CartographyTablePieceRequirementDistance.Value, pieceMask);
                    nearby_pieces = nearbyPieces.Length;
                }
                return nearby_pieces > ValConfig.CartographyTableRequiredPieces.Value;
            }
        }
    }
}
