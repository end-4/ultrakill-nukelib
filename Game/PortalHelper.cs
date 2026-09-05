using System.Collections.Generic;
using HarmonyLib;
using ULTRAKILL.Portal;
using ULTRAKILL.Portal.Geometry;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NukeLib.Game;

/// <summary>
/// Portal related stuff
/// </summary>
public static class PortalHelper {
    private static readonly HashSet<Portal> _activePortals = [];

    /// <summary>
    /// Read-only collection of all active portals
    /// </summary>
    public static IReadOnlyCollection<Portal> ActivePortals => _activePortals;

    static PortalHelper() {
        // Cleanup on scene unload
        SceneManager.sceneUnloaded += _ => _activePortals.Clear();
    }

    /// <summary>
    /// Adds a portal
    /// </summary>
    /// <param name="portal">The portal to add</param>
    private static void AddPortal(Portal portal) {
        if (portal != null) {
            _activePortals.Add(portal);
        }
    }

    /// <summary>
    /// Removes a portal
    /// </summary>
    /// <param name="portal">The portal to remove</param>
    private static void RemovePortal(Portal portal) {
        if (portal != null) {
            _activePortals.Remove(portal);
        }
    }

    /// <summary>
    /// Get coordinates of 4 corners of a portal counter-clockwise, starting with bottom-left corner
    /// </summary>
    /// <param name="portalTransform">Transform for drawing</param>
    /// <param name="portal">The portal</param>
    /// <param name="p0">Corner 0: bottom-left</param>
    /// <param name="p1">Corner 1: bottom-right</param>
    /// <param name="p2">Corner 2: top-right</param>
    /// <param name="p3">Corner 3: top-left</param>
    /// <param name="offsetDistance">Distance offset away from portal. Good for not drawing/putting anything directly at the portal. You MUST also specify the cameraPos</param>
    /// <param name="cameraPos">Camera position, used for shifting the points towards</param>
    public static void GetPortalCorners(Transform portalTransform, Portal portal,
        out Vector3 p0, out Vector3 p1, out Vector3 p2, out Vector3 p3,
        float offsetDistance, Vector3? cameraPos
    ) {
        var width = 0f;
        var height = 0f;

        if (portal != null && portal.shape is PlaneShape planeShape) {
            width = planeShape.width;
            height = planeShape.height;
        }

        // Halves
        var halfWidth = width * 0.5f;
        var halfHeight = height * 0.5f;

        // Base corners in local coordinates
        p0 = portalTransform.TransformPoint(new Vector3(-halfWidth, -halfHeight, 0f)); // Bottom-Left
        p1 = portalTransform.TransformPoint(new Vector3(halfWidth, -halfHeight, 0f)); // Bottom-Right
        p2 = portalTransform.TransformPoint(new Vector3(halfWidth, halfHeight, 0f)); // Top-Right
        p3 = portalTransform.TransformPoint(new Vector3(-halfWidth, halfHeight, 0f)); // Top-Left

        // Push along normal towards camera if an offset is specified
        if (offsetDistance > 0f && cameraPos != null) {
            Vector3 normal = portalTransform.forward;
            float sign = Vector3.Dot(normal, (cameraPos ?? Vector3.zero) - portalTransform.position) >= 0f ? 1f : -1f;
            Vector3 push = normal * (sign * offsetDistance);
            p0 += push;
            p1 += push;
            p2 += push;
            p3 += push;
        }
    }

    /// <summary>
    /// Get coordinates of 4 corners of a portal counter-clockwise, starting with bottom-left corner
    /// </summary>
    /// <param name="portalTransform">Transform for drawing</param>
    /// <param name="portal">The portal</param>
    /// <param name="p0">Corner 0: bottom-left</param>
    /// <param name="p1">Corner 1: bottom-right</param>
    /// <param name="p2">Corner 2: top-right</param>
    /// <param name="p3">Corner 3: top-left</param>
    public static void GetPortalCorners(Transform portalTransform, Portal portal,
        out Vector3 p0, out Vector3 p1, out Vector3 p2, out Vector3 p3) {
        GetPortalCorners(portalTransform, portal,
            out p0, out p1, out p2, out p3,
            0, null
        );
    }

    [HarmonyPatch(typeof(Portal))]
    internal static class PortalPatches {
        [HarmonyPostfix]
        [HarmonyPatch("OnEnable")]
        private static void OnEnable_Postfix(Portal __instance) {
            PortalHelper.AddPortal(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnDisable")]
        private static void OnDisable_Postfix(Portal __instance) {
            PortalHelper.RemovePortal(__instance);
        }
    }
}
