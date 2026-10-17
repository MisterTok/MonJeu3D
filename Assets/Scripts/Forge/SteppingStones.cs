using System;
using System.Collections.Generic;

// « Pas japonais » : l'événement Stepping Stones de Forge Master, qui fait gagner les skins.
// Comme dans FM, il n'a lieu qu'UNE SEMAINE SUR DEUX (changement le lundi à 22:00, comme les clés) ; pendant sa semaine,
// la boutique offre 8 jetons gratuits et vend des packs de jetons.
// Un saut coûte 1 jeton ; chaque pierre atteinte ajoute des éclats au butin en jeu, de plus en plus risqué.
// On encaisse quand on veut ; tomber dans la lave fait perdre le butin. La 6e pierre encaisse tout (jackpot).
// Chaque édition met à l'honneur un pack : ses 2 skins s'obtiennent avec les éclats. Débloqué à l'étape 6-8 (FM : âge 6, combat 15).
public static class SteppingStones
{
    public const int Stones = 6;
    public static readonly float[] Chance = { 1f, 0.9f, 0.8f, 0.7f, 0.6f, 0.5f };   // réussite du saut vers la pierre i
    public static readonly int[] Loot = { 3, 5, 8, 12, 18, 30 };                       // éclats ajoutés en atteignant la pierre i
    public const int FreeTokens = 8;                                                     // offerts dans la boutique, une fois par édition
    public static readonly int[] PackTokens = { 10, 30 }, PackGems = { 150, 400 };      // packs (gemmes en attendant les achats réels)
    public const int AutoCashAt = 4;                                                     // le mode Auto encaisse à la 4e pierre
    public const int Unlock = 57;                                                        // étape 6-8
    public static int SkinCost(HeroSkins.Def d) => d.rarity >= 5 ? 80 : 40;

    static readonly DateTime Epoch = new DateTime(2026, 1, 5);   // un lundi : semaine 0 = semaine d'événement
    static List<string> packs;
    static List<string> Packs
    {
        get
        {
            if (packs == null) { packs = new List<string>(); foreach (var d in HeroSkins.All) if (!packs.Contains(d.pack)) packs.Add(d.pack); }
            return packs;
        }
    }

    static DateTime Shifted => DateTime.Now.AddHours(2);   // la journée change à 22:00
    static int Week => (int)Math.Floor((Shifted.Date - Epoch).TotalDays / 7.0);
    public static bool Active => ((Week % 2) + 2) % 2 == 0;            // une semaine sur deux
    public static int Edition => (int)Math.Floor(Week / 2.0);            // numéro de l'édition (en cours, ou la prochaine pendant la pause)
    public static int NextEdition => Active ? Edition : Edition + 1;
    public static string PackOf(int edition) => Packs[((edition % Packs.Count) + Packs.Count) % Packs.Count];
    public static string SeasonPack => PackOf(NextEdition);              // pack en cours (ou le prochain pendant la pause)
    public static TimeSpan TimeLeft => Epoch.AddDays((Week + 1) * 7) - Shifted;   // fin de l'édition, ou début de la suivante

    public static List<HeroSkins.Def> SeasonSkins
    {
        get { var l = new List<HeroSkins.Def>(); foreach (var d in HeroSkins.All) if (d.pack == SeasonPack) l.Add(d); return l; }
    }
    public static bool InSeason(string id) { var d = HeroSkins.Find(id); return Active && d != null && d.pack == SeasonPack; }

    public static bool Unlocked => GameState.Reached(Unlock);
    public static bool FreeAvailable => Unlocked && Active && GameState.Data.ssFreeEdition != Edition;

    // À la fin d'une édition, le butin en jeu est encaissé (rien n'est perdu).
    static void CheckSeason()
    {
        var s = GameState.Data;
        int ed = Active ? Edition : -1;
        if (s.ssSeason != ed) { s.ssEclats += s.ssPending; s.ssPos = 0; s.ssPending = 0; s.ssSeason = ed; }
    }

    public enum Result { Error, Ok, Fall, Jackpot }

    public static Result Jump(out string message)
    {
        CheckSeason();
        var s = GameState.Data;
        message = null;
        if (!Unlocked) { message = GameState.UnlockText(Unlock); return Result.Error; }
        if (!Active) { message = "Le Pas japonais revient dans " + GameState.FmtTime((long)TimeLeft.TotalSeconds); return Result.Error; }
        if (s.ssTokens <= 0) { message = "Plus de jetons : la boutique en offre et en vend"; return Result.Error; }
        s.ssTokens--;
        if (UnityEngine.Random.value < Chance[s.ssPos])
        {
            s.ssPending += Loot[s.ssPos];
            s.ssPos++;
            if (s.ssPos >= Stones)
            {
                message = "Jackpot ! +" + s.ssPending + " éclats";
                s.ssEclats += s.ssPending; s.ssPending = 0; s.ssPos = 0;
                GameState.Notify();
                return Result.Jackpot;
            }
            GameState.Notify();
            return Result.Ok;
        }
        message = s.ssPending > 0 ? "Plouf ! " + s.ssPending + " éclats perdus dans la lave" : "Plouf ! Tombé dans la lave";
        s.ssPending = 0; s.ssPos = 0;
        GameState.Notify();
        return Result.Fall;
    }

    public static int CashOut()
    {
        CheckSeason();
        var s = GameState.Data;
        int n = s.ssPending;
        s.ssEclats += n; s.ssPending = 0; s.ssPos = 0;
        GameState.Notify();
        return n;
    }

    // Boutique : 8 jetons offerts par édition, packs payants.
    public static string ClaimFree()
    {
        if (!Unlocked) return GameState.UnlockText(Unlock);
        if (!Active) return "Le Pas japonais revient dans " + GameState.FmtTime((long)TimeLeft.TotalSeconds);
        if (!FreeAvailable) return "Déjà pris pour cette édition";
        GameState.Data.ssFreeEdition = Edition;
        GameState.Data.ssTokens += FreeTokens;
        GameState.Notify();
        return null;
    }

    public static string BuyPack(int i)
    {
        var s = GameState.Data;
        if (!Unlocked) return GameState.UnlockText(Unlock);
        if (!Active) return "Le Pas japonais revient dans " + GameState.FmtTime((long)TimeLeft.TotalSeconds);
        if (s.gems < PackGems[i]) return "Pas assez de gemmes";
        s.gems -= PackGems[i]; s.ssTokens += PackTokens[i];
        GameState.Notify();
        return null;
    }

    public static string Claim(string id)
    {
        CheckSeason();
        var d = HeroSkins.Find(id);
        var s = GameState.Data;
        if (d == null) return "Skin inconnu";
        if (HeroSkins.Owned(id)) return "Déjà obtenu";
        if (!InSeason(id)) return "Ce skin reviendra lors d'une autre édition du Pas japonais";
        int cost = SkinCost(d);
        if (s.ssEclats < cost) return "Il faut " + cost + " éclats";
        s.ssEclats -= cost;
        s.skinsOwned.Add(id);
        s.skinEquipped = id;
        GameState.Notify();
        return null;
    }

    public static void Tick() => CheckSeason();
}
