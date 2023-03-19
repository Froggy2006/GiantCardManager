using System.IO;
using BepInEx;
using DiskCardGame;
using TinyJson;
using InscryptionAPI.Helpers;

#nullable enable
namespace GiantCardManager.JSON;

public static class GiantCardJSON
{
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
            Plugin.Log.LogDebug($"Loading JSON (giant cards) {filename}");

            GiantCardList? giantCardInfo = JSONParser.FromJson<GiantCardList>(File.ReadAllText(file));

            /* this is a safe nullable check; trust it! :3 */
            if (giantCardInfo?.giantCards == null)
            {
                Plugin.Log.LogDebug($"Couldn't load JSON data from file {filename}!");
                continue;
            }

            foreach (var giantCard in giantCardInfo.giantCards)
            {
                if (giantCard.name == null || giantCard.texture == null) continue;

                CardInfo card = CardLoader.GetCardByName(giantCard.name);
                card.animatedPortrait = Plugin.CreateGiantCard(
                            texture: TextureHelper.GetImageAsTexture(giantCard.texture)
                        );
                Plugin.Log.LogDebug($"Loaded JSON giant card '{card.name}'!");
            }
        }
    }
}
