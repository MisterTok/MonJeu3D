using System;
using System.Collections.Generic;

// « Pas japonais » : l'événement Stepping Stones de Forge Master, qui fait gagner les skins.
// Comme dans FM : UNE SEMAINE SUR DEUX (changement le lundi à 22:00, comme les clés), 8 rangées de 2 pierres.
// Une partie coûte 1 jeton ; à chaque rangée on choisit la pierre de gauche ou de droite, l'une des deux casse (50/50).
// Chaque pierre franchie rapporte aussitôt des marteaux et de l'or ; la 6e a 1 % de chances de donner le petit skin
// de l'édition, le bout du chemin donne le grand skin (puis le petit). Une chute renvoie sur la rive.
// Jetons : 8 offerts chaque jour dans la boutique pendant l'édition, packs ; Auto (dès 20 jetons) joue 20 parties d'un coup.
// Débloqué à l'étape 6-8 (FM : âge 6, combat 15).
public static class SteppingStones
{
    public const int Rows = 8;
    public const float Chance = 0.5f;
    public static readonly int[] Hammers = { 2, 4, 8, 15, 25, 40, 60, 100 };            // marteaux en franchissant la rangée i
    public static readonly long[] GoldRef = { 200, 400, 800, 1500, 2500, 4000, 6000, 10000 }; // or « de référence » (1 000 = 1 h hors ligne)
    public const int RareRow = 6;                                                        // 6e pierre : 1 % de chances du petit skin
    public const float RareChance = 0.01f;
    public const int EndGems = 100;                                                      // bout du chemin quand les 2 skins sont déjà obtenus
    public const int FreeTokens = 8;                                                     // offerts dans la boutique, chaque jour de l'édition
    public static readonly int[] PackTokens = { 10, 30 }, PackGems = { 150, 400 };      // packs (gemmes en attendant les achats réels)
    public const int AutoMin = 20, AutoRuns = 20;                                        // Auto : dès 20 jetons, 20 parties d'un coup
    public const int Unlock = 57;                                                        // étape 6-8

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
    static int Day => (int)Math.Floor((Shifted.Date - Epoch).TotalDays);
    static int Week => (int)Math.Floor(Day / 7.0);
    public static bool Active => ((Week % 2) + 2) % 2 == 0;            // une semaine sur deux
    public static int Edition => (int)Math.Floor(Week / 2.0);            // numéro de l'édition (en cours, ou la prochaine pendant la pause)
    public static int NextEdition => Active ? Edition : Edition + 1;
    public static string PackOf(int edition) => Packs[((edition % Packs.Count) + Packs.Count) % Packs.Count];
    public static string SeasonPack => PackOf(NextEdition);              // pack en cours (ou le prochain pendant la pause)
    public static TimeSpan TimeLeft => Epoch.AddDays((Week + 1) * 7) - Shifted;   // fin de l'édition, ou début de la suivante

    // Skins de l'édition, du moins rare (petit skin, 1 % à la 6e pierre) au plus rare (bout du chemin).
    public static List<HeroSkins.Def> SeasonSkins
    {
        get { var l = new List<HeroSkins.Def>(); foreach (var d in HeroSkins.All) if (d.pack == SeasonPack) l.Add(d); l.Sort((a, b) => a.rarity.CompareTo(b.rarity)); return l; }
    }
    public static bool InSeason(string id) { var d = HeroSkins.Find(id); return Active && d != null && d.pack == SeasonPack; }
    public static bool IsRare(string id) { var l = SeasonSkins; return l.Count > 1 && l[0].id == id; }

    public static bool Unlocked => GameState.Reached(Unlock);
    public static bool FreeAvailable => Unlocked && Active && GameState.Data.ssFreeEdition != Day;
    public static long Gold(int row) => GameState.ShopGold(GoldRef[row]);

    // Changement d'édition : la partie en cours est abandonnée (les gains sont déjà donnés).
    static void CheckSeason()
    {
        var s = GameState.Data;
        int ed = Active ? Edition : -1;
        if (s.ssSeason != ed) { s.ssPos = 0; s.ssPending = 0; s.ssSeason = ed; }
    }

    public enum Result { Error, Ok, Fall, End }

    public struct Step
    {
        public Result result;
        public int hammers, gems;
        public long gold;
        public string skin;     // skin gagné (id) ou null
    }

    // Saute sur la pierre « side » (0 = gauche, 1 = droite) de la rangée suivante. ssPending garde les côtés franchis (bits).
    public static Step Jump(int side, out string message)
    {
        CheckSeason();
        var s = GameState.Data;
        message = null;
        if (!Unlocked) { message = GameState.UnlockText(Unlock); return new Step(); }
        if (!Active) { message = "Le Pas japonais revient dans " + GameState.FmtTime((long)TimeLeft.TotalSeconds); return new Step(); }
        if (s.ssPos == 0)
        {
            if (s.ssTokens <= 0) { message = "Plus de jetons : la boutique en offre chaque jour"; return new Step(); }
            s.ssTokens--;
            s.ssPending = 0;
        }
        var st = Resolve(side);
        GameState.Notify();
        return st;
    }

    static Step Resolve(int side)
    {
        var s = GameState.Data;
        var st = new Step();
        if (UnityEngine.Random.value >= Chance)
        {
            st.result = Result.Fall;
            s.ssPos = 0;
            return st;
        }
        int row = s.ssPos;
        if (side == 1) s.ssPending |= 1 << row;
        st.hammers = Hammers[row];
        st.gold = Gold(row);
        s.hammers += st.hammers;
        s.gold += st.gold;
        s.ssPos++;
        st.result = Result.Ok;
        var l = SeasonSkins;
        if (s.ssPos == RareRow && l.Count > 1 && !HeroSkins.Owned(l[0].id) && UnityEngine.Random.value < RareChance)
            st.skin = Give(l[0].id);
        if (s.ssPos >= Rows)
        {
            st.result = Result.End;
            string id = null;
            for (int i = l.Count - 1; i >= 0 && id == null; i--) if (!HeroSkins.Owned(l[i].id)) id = l[i].id;
            if (id != null) st.skin = Give(id);
            else { st.gems = EndGems; s.gems += EndGems; }
            s.ssPos = 0;
        }
        return st;
    }

    static string Give(string id)
    {
        var s = GameState.Data;
        s.skinsOwned.Add(id);
        s.skinEquipped = id;
        return id;
    }

    // Auto : joue AutoRuns parties d'un coup (pierres au hasard) et renvoie le résumé.
    public static string AutoSweep()
    {
        CheckSeason();
        var s = GameState.Data;
        if (!Active) return "Le Pas japonais revient dans " + GameState.FmtTime((long)TimeLeft.TotalSeconds);
        if (s.ssPos != 0) return "Termine d'abord la partie en cours";
        if (s.ssTokens < AutoMin) return "Auto : il faut " + AutoMin + " jetons";
        int ham = 0, best = 0, gems = 0; long gold = 0;
        var skins = new List<string>();
        for (int r = 0; r < AutoRuns; r++)
        {
            s.ssTokens--; s.ssPending = 0;
            int reached = 0;
            while (true)
            {
                var st = Resolve(UnityEngine.Random.Range(0, 2));
                if (st.result == Result.Fall) break;
                reached++;
                ham += st.hammers; gold += st.gold; gems += st.gems;
                if (st.skin != null) skins.Add(st.skin);
                if (st.result == Result.End) break;
            }
            if (reached > best) best = reached;
        }
        s.ssPos = 0; s.ssPending = 0;
        GameState.Notify();
        string msg = "Auto x" + AutoRuns + " : meilleure pierre " + best + "/" + Rows + "  ·  +" + ham + " marteaux  +" + GameState.Fmt(gold) + " or";
        if (gems > 0) msg += "  +" + gems + " gemmes";
        foreach (var id in skins) msg += "\nNOUVEAU SKIN : " + HeroSkins.Find(id).name + " !";
        return msg;
    }

    // Boutique : 8 jetons offerts chaque jour de l'édition (ssFreeEdition garde le jour), packs payants.
    public static string ClaimFree()
    {
        if (!Unlocked) return GameState.UnlockText(Unlock);
        if (!Active) return "Le Pas japonais revient dans " + GameState.FmtTime((long)TimeLeft.TotalSeconds);
        if (!FreeAvailable) return "Déjà pris aujourd'hui (nouveaux jetons à 22:00)";
        GameState.Data.ssFreeEdition = Day;
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

    public static void Tick() => CheckSeason();
}
