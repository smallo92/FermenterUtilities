using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Reflection;

namespace FermenterUtilities
{
    [BepInPlugin("smallo.mods.fermenterutilities", "Fermenter Utilities", "1.0.0")]
    [HarmonyPatch]
    class FermenterUtilitiesPlugin : BaseUnityPlugin
    {
        private static ConfigEntry<bool> enableMod;
        private static ConfigEntry<bool> showPercentage;
        private static ConfigEntry<bool> showColourPercentage;
        private static ConfigEntry<bool> customFermentTime;
        private static ConfigEntry<bool> noCover;
        private static ConfigEntry<int> showPercentageDecimal;
        private static ConfigEntry<int> newFermentTime;

        void Awake()
        {
            enableMod = Config.Bind("1 - Global", "Enable Mod", true, "Enable or disable this mod");
            if (!enableMod.Value) return;

            showPercentage = Config.Bind("2 - Percentage", "Show Percentage", true, "Shows the fermentation progress as a percentage when you hover over the fermenter");
            showColourPercentage = Config.Bind("2 - Percentage", "Show Colour Percentage", true, "Makes it so the percentage changes colour depending on the progress");
            showPercentageDecimal = Config.Bind("2 - Percentage", "Decimal Places", 0, "Amount of decimal places to display on the percentage if you want more detail");

            customFermentTime = Config.Bind("3 - Time", "Custom Time", false, "Enables the custom time for fermentation");
            newFermentTime = Config.Bind("3 - Time", "Fermentation Time", 5, "The amount of minutes fermentation takes (Default 40)");

            noCover = Config.Bind("4 - Cover", "Work Without Cover", false, "Allow the Fermenter to work without any cover");

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
        }
        private static string GetColour(double percentage)
        {
            if (!showColourPercentage.Value) return "white";

            string colour = "red";
            if (percentage >= 25 && percentage <= 50) colour = "orange";
            if (percentage >= 50 && percentage <= 75) colour = "yellow";
            if (percentage >= 75 && percentage <= 100) colour = "lime";

            return colour;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Fermenter), "Awake")]
        public static void FermenterAwake_Patch(Fermenter __instance)
        {
            if (customFermentTime.Value) __instance.m_fermentationDuration = newFermentTime.Value * 60;
            if (noCover.Value) __instance.m_updateCoverTimer = -100f;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Fermenter), "GetHoverText")]
        public static string FermenterGetHoverText_Patch(string __result, Fermenter __instance)
        {
            if (__instance == null) return __result;

            if (showPercentage.Value && __instance.GetStatus() == Fermenter.Status.Fermenting)
            {
                string replaceString = Localization.instance.Localize("$piece_fermenter_fermenting");
                double percentage = __instance.GetFermentationTime() / __instance.m_fermentationDuration * 100;
                string colour = GetColour(percentage);

                string newString = $"<color={colour}>{decimal.Round((decimal)percentage, showPercentageDecimal.Value, MidpointRounding.AwayFromZero)}%</color>";

                return __result.Replace(replaceString, newString);
            }

            return __result;
        }
    }
}