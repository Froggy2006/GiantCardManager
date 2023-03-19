using BepInEx;
using BepInEx.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Linq;
using DiskCardGame;
using UnityEngine;
using InscryptionAPI;
using InscryptionAPI.Ascension;
using InscryptionAPI.Encounters;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers;
using InscryptionAPI.Regions;
using InscryptionAPI.Saves;
using InscryptionAPI.Card;
using InscryptionCommunityPatch;
using Pixelplacement;
using UnityEngine.Networking;
using HarmonyLib;
using Sirenix.Serialization.Utilities;
using Random = System.Random;
using TinyJson;
using GiantCardManager.JSON;

namespace GiantCardManager
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("cyantist.inscryption.api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("MADH.inscryption.JSONLoader", BepInDependency.DependencyFlags.HardDependency)]
    public partial class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "Cevin_2006.Inscryption.GiantCardManager";
        public const string PluginName = "GiantCardManager";
        public const string PluginVersion = "0.0.1";
        public static string staticpath;
        public static string Directory;
        public static List<Sprite> art_sprites;
        internal static ManualLogSource Log;
        public static AssetBundle GiantCardBundle;

        private void Awake()
        {
            using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("GiantCardManager.Resources.giantcardbundle"))
            {
                GiantCardBundle = AssetBundle.LoadFromStream(s);
            }

            Plugin.Log = base.Logger;

            /* load json giant cards! c: */
            GiantCardJSON.LoadAllGiantCards(); 
        }

        private static Sprite ConvertToSprite(Texture2D texture)
        {
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        }
        public static GameObject CreateGiantCard(Texture2D texture)
        {

            GameObject GiantCardPortrait;
            Sprite TextureSprite;

            TextureSprite = ConvertToSprite(texture);

            GiantCardPortrait = GiantCardBundle.LoadAsset<GameObject>("GiantCardPortrait");
            GiantCardPortrait.transform.localScale = new(0.4f, 0.25f, 0);
            GiantCardPortrait.transform.Find("Anim").localPosition = new(0f, 0.16f, 0f);
            GiantCardPortrait.GetComponentsInChildren<Transform>(true).ToList().ForEach(x => x.gameObject.layer = LayerMask.NameToLayer("CardOffscreen"));
            GiantCardPortrait.transform.Find("Anim").Find("Body").GetComponent<SpriteRenderer>().sprite = TextureSprite;

            return GiantCardPortrait;

        }

        public void AddTest()
        {
            CardInfo ML = CardManager.New(

                // Card ID Prefix
                modPrefix: "TestTest",

                // Card internal name.
                "ExpertMoonlord",
                // Card display name.
                "Moonlord",
                // Attack.
                1,
                // Health.
                200,
                // Descryption.
                description: "A fanatical leader hell-bent on bringing about the apocalypse by reviving the great Cthulhu through behind-the-scenes scheming."
            )
            .SetCost(0, 0, 0, null)
            .AddAbilities(Ability.AllStrike, Ability.MadeOfStone, Ability.Sharp, Ability.Reach)
            .AddTraits(Trait.Giant)
            .AddSpecialAbilities(SpecialTriggeredAbility.GiantCard)
            .AddAppearances(CardAppearanceBehaviour.Appearance.GiantAnimatedPortrait)
            ;
            CardManager.Add("Terra", ML);
            ML.animatedPortrait = CreateGiantCard(Tools.LoadTexture("Moonlord_Body"));
            Debug.Log(ML.displayedName + " has animated portrait: " + (ML.animatedPortrait != null));
        }
    }
}

public static class Tools
{
    public static Assembly _assembly;
    public static Assembly CurrentAssembly => _assembly ??= Assembly.GetExecutingAssembly();

    public static GameObject Particle;

    public static Texture2D LoadTexture(string name)
    {
        if (name == null)
        {
            return null;
        }
        return TextureHelper.GetImageAsTexture(name + (name.EndsWith(".png") ? "" : ".png"), CurrentAssembly);
    }
    public static int GetActAsInt()
    {
        if (SaveManager.SaveFile.IsPart1)
        {
            return 1;
        }
        if (SaveManager.SaveFile.IsPart2)
        {
            return 2;
        }
        if (SaveManager.SaveFile.IsPart3)
        {
            return 3;
        }
        if (SaveManager.SaveFile.IsGrimora)
        {
            return 4;
        }
        if (SaveManager.SaveFile.IsMagnificus)
        {
            return 5;
        }
        return 0;
    }
}
