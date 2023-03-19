using System.IO;
using HarmonyLib;
using System.Collections.Generic;
using BepInEx;
using DiskCardGame;
using TinyJson;
using InscryptionAPI.Helpers;
using InscryptionAPI.Card;
using UnityEngine;

#nullable enable
namespace GiantCardManager.JSON;
using Appearance = CardAppearanceBehaviour.Appearance;

[HarmonyPatch]
public static class GiantCardJSON
{
    public static Dictionary<string, GameObject> PrefabCache = new();

    [System.Serializable]
    public class GiantCardList
    {
        public class GiantCardInfo
        {
            public string? name;
            public string? texture;
        }

        public GiantCardInfo[]? giantCards;
    }

    public static void LoadAllGiantCards()
    {
        var jsonFiles = Directory.EnumerateFiles(
                path: Paths.PluginPath,
                searchPattern: "*_giant.json",
                searchOption: SearchOption.AllDirectories);
            
        foreach (string file in jsonFiles)
        {
            string filename = Path.GetFileName(file);
            Plugin.Log.LogDebug($"Loading JSON (giant card) {filename}");

            GiantCardList? giantCardList = JSONParser.FromJson<GiantCardList>(File.ReadAllText(file));

            /* this is a safe nullable check; trust it! c: */
            if (giantCardList?.giantCards == null)
            {
                /* this should be logged as an error. the user should know! */
                Plugin.Log.LogError($"Couldn't load JSON data from file {filename}!");
                continue;
            }

            foreach (var giantCard in giantCardList.giantCards)
            {
                if (giantCard.name == null || giantCard.texture == null) continue;

                CardInfo card = CardLoader.GetCardByName(giantCard.name);
                GiveGiantProperties(card);

                /* load changed prefab! */
                GameObject prefab = Plugin.CreateGiantCard(
                            texture: TextureHelper.GetImageAsTexture(giantCard.texture)
                        );
                /* make sure it doesn't get destroyed between scenes. */
                GameObject.DontDestroyOnLoad(prefab); 
                /* add to cache. c: */
                PrefabCache.Add(card.name, prefab);

                Plugin.Log.LogInfo($"Loaded JSON giant card '{card.name}'!");
            }
        }
    }

    private static void GiveGiantProperties(CardInfo card)
    {
        if (!card.HasTrait(Trait.Giant))
            card.AddTraits(Trait.Giant);

        if (!card.HasSpecialAbility(SpecialTriggeredAbility.GiantCard))
            card.AddSpecialAbilities(SpecialTriggeredAbility.GiantCard);

        if (!card.appearanceBehaviour.Contains(Appearance.GiantAnimatedPortrait))
            card.AddAppearances(CardAppearanceBehaviour.Appearance.GiantAnimatedPortrait);

    }

    /* add the portrait to the card *from* the cache. i had to do this for talking cards too, it's fine. */
    [HarmonyPatch(typeof(CardInfo), nameof(CardInfo.AnimatedPortrait), MethodType.Getter)]
    [HarmonyPostfix]
    private static void AnimatedIconPatch(CardInfo __instance, ref GameObject __result)
    {
        if (!PrefabCache.ContainsKey(__instance.name)) return;

        __instance.animatedPortrait = PrefabCache[__instance.name];
        __result = PrefabCache[__instance.name];
    }
}
