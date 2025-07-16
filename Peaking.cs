using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace PeakZoomMod
{
    [BepInPlugin("krw.mods.peaking", "PEAKING Zoom Mod", "1.0")]
    public class ZoomMod : BaseUnityPlugin
    {
        private static float normalFov;
        private static float currentFov;

        private void Awake()
        {
            Harmony harmony = new Harmony("krw.mods.peaking");
            harmony.PatchAll();
            Logger.LogInfo("PEAKING Zoom mod loaded!");

            // Initialize currentFov with the real FOV at start
            var camera = Camera.main;
            if (camera != null)
            {
                normalFov = camera.fieldOfView;
                currentFov = normalFov;
                Logger.LogInfo($"Detected starting FOV: {normalFov}");
            }
            else
            {
                // fallback in case Camera.main is null at Awake()
                normalFov = 70f;
                currentFov = normalFov;
            }
        }
    }

[HarmonyPatch]
    public class ZoomPatch
    {
        private static float currentFov;
        private static float zoomFov = 20f;
        private static float minZoom = 5f;
        private static float maxZoom = 100f;
    
        [HarmonyTargetMethod]
        static System.Reflection.MethodBase TargetMethod()
        {
            var type = AccessTools.TypeByName("MainCameraMovement");
            return AccessTools.Method(type, "GetFov");
        }
    
        static bool Prefix(object __instance, ref float __result)
        {
            // Read game’s FOV setting from fovSetting field
            var fovSettingField = AccessTools.Field(__instance.GetType(), "fovSetting");
            var fovSetting = fovSettingField?.GetValue(__instance);
            float baseFov = 70f;
    
            if (fovSetting != null)
            {
                var valueProp = AccessTools.Property(fovSetting.GetType(), "Value");
                if (valueProp != null)
                    baseFov = (float)valueProp.GetValue(fovSetting);
            }
    
            // Apply game logic fallback
            if (baseFov < 60f)
                baseFov = 70f;
    
            bool isZooming = Input.GetKey(KeyCode.C);
    
            if (isZooming)
            {
                float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
                if (Mathf.Abs(scrollDelta) > 0.01f)
                {
                    zoomFov -= scrollDelta * 20f;
                    zoomFov = Mathf.Clamp(zoomFov, minZoom, baseFov);
                }
    
                currentFov = Mathf.Lerp(currentFov, zoomFov, Time.deltaTime * 8f);
            }
            else
            {
                currentFov = Mathf.Lerp(currentFov, baseFov, Time.deltaTime * 5f);
            }
    
            __result = currentFov;
            return false; // skip original
        }
    }
}