using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using YourModName.Server.Patches;

namespace YourModName.Server
{
    [BepInPlugin("com.yourname.yourmod.server", "Your Mod Name Server", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        private static string ModPath;

        private void Awake()
        {
            Log = Logger;
            ModPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            
            Log.LogInfo("[YourMod] Server mod loading...");

            try
            {
                // Load configuration
                DatabaseLoader.Initialize();
                
                // Apply Harmony patches to inject into SPT's database
                DatabasePatch.Enable();
                
                Log.LogInfo("[YourMod] Server mod loaded successfully!");
            }
            catch (Exception ex)
            {
                Log.LogError($"[YourMod] Failed to load: {ex}");
            }
        }

        public static string GetModPath()
        {
            return ModPath;
        }
    }
}
