using System;
using System.Collections.Generic;
using UnityEngine;

// Modèle du jeu : équipement, monnaies, forge, sauvegarde.
[Serializable]
public class Item
{
    public bool valid;
    public int slot;
    public int circle;
    public int level;
    public long atk;
    public long hp;
    public int[] subs;      // statistiques secondaires (index dans GameState.SubNames)
    public float[] subVals; // valeurs (0,05 = 5 %)

    public int SubCount => subs == null ? 0 : subs.Length;
    public string SubLine(int i) => "+" + GameState.FmtPct(subVals[i]) + " " + GameState.SubNames[subs[i]];

    public bool IsAttack => atk > 0;
    public long MainStat => IsAttack ? atk : hp;
    public string StatLabel => IsAttack ? "ATQ" : "PV";
    public string Name => GameState.SlotNames[slot] + " " + GameState.CircleSuffix[circle];
    public long SellValue => (long)Math.Round(GameState.CircleBase(circle) * 0.75 * GameState.LevelMult(level) * (1 + GameState.TV("EquipmentSellPrice")));
}

[Serializable]
public class OwnedPet
{
    public int id;
    public int level = 1;
    public int copies;      // doublons accumulés vers le niveau suivant
    public int[] subs;
    public float[] subVals;
}

[Serializable]
public class OwnedMount
{
    public int id;
    public int level = 1;
    public int copies;
}

[Serializable]
public class OwnedSkill
{
    public int id;
    public int level = 1;
    public int copies;
}

[Serializable]
public class Incubator
{
    public int rarity = -1;   // -1 = vide
    public long endTicks;
}

[Serializable]
public class SaveData
{
    public int version = 1;
    public long gold;
    public long gems = 30;   // comme Forge Master : 30 gemmes au départ
    public int hammers = 60;
    public long lastHammerTicks;
    public int forgeLevel = 1;
    public int nodesPaid;
    public bool upgrading;
    public long upgradeEndTicks;
    public Item[] equipped;
    public Item pending = new Item();
    public long totalForged;
    public long eggshells;          // coquilles : monnaie d'invocation des œufs
    public long winders;            // remontoirs : monnaie d'invocation des montures
    public int mountSummonLevel, mountSummonProgress;
    public System.Collections.Generic.List<OwnedMount> mounts;
    public int equippedMount = -1;
    public bool mountGift;
    public long potions;            // potions rouges (arbre technologique, à venir)
    public long skillTickets;       // tickets d'invocation de compétences
    public System.Collections.Generic.List<OwnedSkill> skills;
    public int[] equippedSkills;    // 3 emplacements (id ou -1)
    public int skillSummonLevel, skillSummonProgress;
    public bool skillGift;
    public long shopDay;            // jour des offres (change à 22:00, comme les clés)
    public bool shopFreeTaken;
    public int[] shopDeals;         // index des 3 offres du jour dans ShopData.Deals
    public bool[] shopDealBought;
    public int[] shopBundleBuys;    // achats du jour par ressource
    public int missionEnergy;
    public long missionDay;
    public int[] missionSquad;      // escouade de chaque mission proposée
    public int[] missionLevel;      // niveau de chaque mission proposée
    public bool[] passClaimed;      // paliers du pass de progression déjà récupérés
    public int[] techLevels;        // niveaux de recherche (arbres mis bout à bout)
    public int researchTree = -1, researchNode = -1;
    public long researchEndTicks;
    public long lastSeenTicks;      // dernière sauvegarde (gains hors ligne)
    public bool autoForge;
    public int[] dungeonLevel;      // niveau atteint dans chaque donjon
    public int[] dungeonKeys;       // clés restantes aujourd'hui
    public long keyDay;             // jour de la dernière recharge des clés (02:00 heure locale)
    public int eggSummonLevel;      // niveau d'invocation (0 = niveau 1)
    public int eggSummonProgress;
    public bool shellGift;
    public int[] eggs;              // œufs en réserve par rareté
    public Incubator[] incubators;
    public int incubatorSlots = ProgressionData.IncubatorStart;
    public System.Collections.Generic.List<OwnedPet> pets;
    public int[] equippedPets;      // 3 emplacements (id ou -1)
    public bool petGift;            // œufs offerts au démarrage
    public int stage;      // étape globale : cercle * 10 + étape (0 à 99)
    public int bestStage;
}

public static class GameState
{
    public const int SlotCount = 8;
    public const int HammerCap = 100;
    public const int HammerRegenSeconds = 15;
    public const long BaseAtk = 10;
    public const long BaseHp = 100;
    const string SaveKey = "forge_enfer_save_v1";

    public static readonly string[] SlotNames =
        { "Lame", "Heaume", "Bouclier", "Gantelets", "Bottes", "Ceinture", "Amulette", "Anneau" };
    // true = l'emplacement donne de l'attaque, false = des points de vie.
    static readonly bool[] SlotIsAtk = { true, false, false, true, false, false, true, true };
    static readonly double[] SlotWeight = { 1.0, 4.0, 6.0, 0.35, 3.0, 3.5, 0.45, 0.35 };

    public static readonly string[] CircleNames =
        { "Limbes", "Luxure", "Gourmandise", "Avarice", "Colère", "Hérésie", "Violence", "Fraude", "Trahison", "Lucifer" };
    public static readonly string[] CircleSuffix =
        { "des Limbes", "de Luxure", "de Gourmandise", "d'Avarice", "de Colère", "d'Hérésie", "de Violence", "de Fraude", "de Trahison", "de Lucifer" };
    public static readonly Color[] CircleColors =
    {
        new Color(0.62f, 0.60f, 0.58f), // Limbes : cendre
        new Color(0.90f, 0.35f, 0.65f), // Luxure : rose
        new Color(0.50f, 0.78f, 0.25f), // Gourmandise : bile
        new Color(0.95f, 0.75f, 0.22f), // Avarice : or
        new Color(0.92f, 0.22f, 0.15f), // Colère : rouge
        new Color(1.00f, 0.50f, 0.08f), // Hérésie : flamme
        new Color(0.70f, 0.08f, 0.25f), // Violence : sang
        new Color(0.62f, 0.32f, 0.95f), // Fraude : violet
        new Color(0.35f, 0.80f, 1.00f), // Trahison : glace
        new Color(1.00f, 0.95f, 0.75f), // Lucifer : lumière
    };

    public static SaveData Data;
    public static event Action Changed;
    static readonly System.Random rng = new System.Random();

    public static double CircleBase(int c) => 20.0 * Math.Pow(3.2, c);
    public static double LevelMult(int l) => 1.0 + 0.12 * (l - 1);

    // ---------- Sauvegarde ----------
    public static void Load()
    {
        Data = null;
        string json = PlayerPrefs.GetString(SaveKey, "");
        if (!string.IsNullOrEmpty(json))
        {
            try { Data = JsonUtility.FromJson<SaveData>(json); }
            catch (Exception e) { Debug.LogWarning("Sauvegarde illisible, nouvelle partie : " + e.Message); }
        }
        if (Data == null)
        {
            Data = new SaveData();
            Data.lastHammerTicks = DateTime.UtcNow.Ticks;
        }
        if (Data.equipped == null || Data.equipped.Length != SlotCount)
        {
            var old = Data.equipped;
            Data.equipped = new Item[SlotCount];
            for (int i = 0; i < SlotCount; i++)
                Data.equipped[i] = (old != null && i < old.Length && old[i] != null) ? old[i] : new Item();
        }
        for (int i = 0; i < SlotCount; i++) if (Data.equipped[i] == null) Data.equipped[i] = new Item();
        if (Data.pending == null) Data.pending = new Item();
        if (Data.eggs == null || Data.eggs.Length != 6) Data.eggs = new int[6];
        if (Data.incubators == null || Data.incubators.Length != ProgressionData.IncubatorMax)
        {
            Data.incubators = new Incubator[ProgressionData.IncubatorMax];
            for (int i = 0; i < Data.incubators.Length; i++) Data.incubators[i] = new Incubator();
        }
        if (Data.incubatorSlots < ProgressionData.IncubatorStart) Data.incubatorSlots = ProgressionData.IncubatorStart;
        if (Data.pets == null) Data.pets = new System.Collections.Generic.List<OwnedPet>();
        if (Data.equippedPets == null || Data.equippedPets.Length != 3) Data.equippedPets = new[] { -1, -1, -1 };
        if (!Data.petGift)
        {
            // Cadeau de bienvenue pour découvrir les compagnons.
            Data.petGift = true;
            Data.eggs[0] += 2;
            Data.eggs[1] += 1;
        }
        if (Data.techLevels == null || Data.techLevels.Length != TechTotalNodes)
        {
            var old = Data.techLevels;
            Data.techLevels = new int[TechTotalNodes];
            if (old != null) Array.Copy(old, Data.techLevels, Math.Min(old.Length, TechTotalNodes));
        }
        RecomputeTech();
        ComputeOffline();
        if (Data.dungeonLevel == null || Data.dungeonLevel.Length != DungeonCount) Data.dungeonLevel = new int[DungeonCount];
        if (Data.dungeonKeys == null || Data.dungeonKeys.Length != DungeonCount) { Data.dungeonKeys = new int[DungeonCount]; Data.keyDay = 0; }
        RefreshKeys();
        RefreshShop();
        RefreshMissions();
        if (Data.passClaimed == null || Data.passClaimed.Length != MissionData.PassStage.Length)
        {
            var old = Data.passClaimed;
            Data.passClaimed = new bool[MissionData.PassStage.Length];
            if (old != null) Array.Copy(old, Data.passClaimed, Math.Min(old.Length, Data.passClaimed.Length));
        }
        if (Data.mounts == null) Data.mounts = new List<OwnedMount>();
        if (!Data.mountGift)
        {
            // Remontoirs offerts pour découvrir les montures.
            Data.mountGift = true;
            Data.winders += 500;
        }
        if (Data.skills == null) Data.skills = new List<OwnedSkill>();
        if (Data.equippedSkills == null || Data.equippedSkills.Length != 3) Data.equippedSkills = new[] { -1, -1, -1 };
        if (!Data.skillGift)
        {
            // Tickets offerts pour découvrir les compétences (une invocation x5).
            Data.skillGift = true;
            Data.skillTickets += SkillData.SummonCost * SkillData.SummonSmall;
        }
        if (!Data.shellGift)
        {
            // Coquilles offertes pour découvrir l'invocation d'œufs.
            Data.shellGift = true;
            Data.eggshells += 300;
        }
        Tick();
    }

    public static void Save()
    {
        if (Data == null) return;
        Data.lastSeenTicks = DateTime.UtcNow.Ticks;
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(Data));
        PlayerPrefs.Save();
    }

    public static void ResetAll()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        Load();
        Notify();
    }

    static void Notify() { Save(); Changed?.Invoke(); }

    // ---------- Temps réel : marteaux et amélioration ----------
    public static void Tick()
    {
        long now = DateTime.UtcNow.Ticks;
        bool changed = false;

        if (Data.hammers >= HammerCap) Data.lastHammerTicks = now;
        else
        {
            long step = TimeSpan.TicksPerSecond * HammerRegenSeconds;
            long gained = (now - Data.lastHammerTicks) / step;
            if (gained > 0)
            {
                Data.hammers = (int)Math.Min(HammerCap, Data.hammers + gained);
                Data.lastHammerTicks += gained * step;
                changed = true;
            }
        }

        if (RefreshKeys()) changed = true;
        if (RefreshShop()) changed = true;
        if (RefreshMissions()) changed = true;
        if (Data.researchTree >= 0 && now >= Data.researchEndTicks) { FinishResearch(); changed = true; }
        if (Data.upgrading && now >= Data.upgradeEndTicks) { FinishUpgrade(); return; }
        if (changed) Notify();
    }

    public static int SecondsToNextHammer()
    {
        if (Data.hammers >= HammerCap) return 0;
        long elapsed = (DateTime.UtcNow.Ticks - Data.lastHammerTicks) / TimeSpan.TicksPerSecond;
        return (int)Math.Max(0, HammerRegenSeconds - elapsed);
    }

    // ---------- Statistiques secondaires ----------
    public const int SubCritChance = 0, SubCritDamage = 1, SubBlock = 2, SubRegen = 3, SubLifeSteal = 4,
        SubDouble = 5, SubDamage = 6, SubMelee = 7, SubAttackSpeed = 8, SubHealth = 9;
    public static readonly string[] SubNames =
        { "Chance de critique", "Dégâts critiques", "Blocage", "Régénération", "Vol de vie",
          "Double frappe", "Dégâts", "Dégâts en mêlée", "Vitesse d'attaque", "Santé" };
    // Fourchettes issues de la configuration de référence (min 1 %, max variable).
    static readonly float[] SubMax = { 0.12f, 0.8f, 0.05f, 0.04f, 0.2f, 0.2f, 0.15f, 0.5f, 0.4f, 0.15f };
    const float SubMin = 0.01f;

    // Nombre de bonus selon le cercle : aucun aux Limbes, 1 du cercle 2 au 5, 2 à partir du cercle 6.
    public static int SubCountFor(int circle) => circle <= 0 ? 0 : circle <= 4 ? 1 : 2;

    static void RollSubs(Item it)
    {
        RollSubsInto(SubCountFor(it.circle), out it.subs, out it.subVals);
    }

    static void RollSubsInto(int n, out int[] subs, out float[] vals)
    {
        subs = new int[n];
        vals = new float[n];
        for (int i = 0; i < n; i++)
        {
            int t;
            do { t = rng.Next(SubNames.Length); } while (System.Array.IndexOf(subs, t, 0, i) >= 0);
            subs[i] = t;
            // Les valeurs hautes sont plus rares (tirage biaisé vers le bas).
            double r = Math.Pow(rng.NextDouble(), 1.6);
            vals[i] = (float)Math.Round(SubMin + (SubMax[t] - SubMin) * r, 3);
        }
    }

    public static float SubTotal(int type)
    {
        float t = 0f;
        foreach (var it in Data.equipped)
            if (it.valid && it.subs != null)
                for (int i = 0; i < it.subs.Length; i++) if (it.subs[i] == type) t += it.subVals[i];
        if (Data.equippedPets != null)
            foreach (int id in Data.equippedPets)
            {
                var p = FindPet(id);
                if (p != null && p.subs != null)
                    for (int i = 0; i < p.subs.Length; i++) if (p.subs[i] == type) t += p.subVals[i];
            }
        return t;
    }

    public const float BaseCritChance = 0.05f, BaseCritMult = 1.5f;
    public const float BaseAttackInterval = 0.85f;
    public static float CritChance => Mathf.Min(1f, BaseCritChance + SubTotal(SubCritChance));
    public static float CritMult => BaseCritMult + SubTotal(SubCritDamage);
    public static float DoubleChance => Mathf.Min(1f, SubTotal(SubDouble));
    public static float BlockChance => Mathf.Min(0.75f, SubTotal(SubBlock));
    public static float LifeSteal => SubTotal(SubLifeSteal);
    public static float RegenPerSecond => SubTotal(SubRegen);
    public static float AttackSpeedBonus => SubTotal(SubAttackSpeed);
    public static float AttackInterval => BaseAttackInterval / (1f + AttackSpeedBonus);

    // ---------- Stats du héros ----------
    public static long TotalAtk()
    {
        double t = BaseAtk + PetsDamage() * (1 + TV("PetBonusDamage")) + SkillPassiveDamage();
        foreach (var it in Data.equipped) if (it.valid) t += it.atk * (1 + TV(SlotTech[it.slot] + "Bonus"));
        return (long)Math.Round(t * (1.0 + SubTotal(SubDamage) + SubTotal(SubMelee)) * (1.0 + MountDamageBonus()));
    }

    public static long TotalHp()
    {
        double t = BaseHp + PetsHealth() * (1 + TV("PetBonusHealth")) + SkillPassiveHealth();
        foreach (var it in Data.equipped) if (it.valid) t += it.hp * (1 + TV(SlotTech[it.slot] + "Bonus"));
        return (long)Math.Round(t * (1.0 + SubTotal(SubHealth)) * (1.0 + MountHealthBonus()));
    }

    // Puissance : dégâts par seconde estimés (critiques, double frappe, vitesse) et vie.
    public static long Power()
    {
        double dps = TotalAtk() * (1 + CritChance * (CritMult - 1)) * (1 + DoubleChance) * (1 + AttackSpeedBonus) + SkillsDps();
        double surv = TotalHp() * (1 + BlockChance) * (1 + LifeSteal + RegenPerSecond * 5);
        return (long)Math.Round(dps * 10 + surv);
    }

    // Puissance qu'aurait le héros avec cette pièce équipée (comparaison).
    public static long PowerIfEquipped(Item it)
    {
        var old = Data.equipped[it.slot];
        Data.equipped[it.slot] = it;
        long p = Power();
        Data.equipped[it.slot] = old;
        return p;
    }

    // ---------- Forge ----------
    public static bool CanForge => Data.hammers > 0 && !Data.pending.valid;

    public static float[] CurrentOdds() => ForgeData.CircleOdds[Mathf.Clamp(Data.forgeLevel, 1, ForgeData.MaxLevel) - 1];

    static int RollCircle()
    {
        float[] odds = CurrentOdds();
        double total = 0; foreach (var o in odds) total += o;
        double r = rng.NextDouble() * total;
        for (int i = 0; i < odds.Length; i++)
        {
            r -= odds[i];
            if (r < 0) return i;
        }
        for (int i = odds.Length - 1; i >= 0; i--) if (odds[i] > 0) return i;
        return 0;
    }

    public static Item MakeItem(int slot, int circle, int level)
    {
        double v = CircleBase(circle) * LevelMult(level) * SlotWeight[slot];
        var it = new Item { valid = true, slot = slot, circle = circle, level = level };
        if (SlotIsAtk[slot]) it.atk = Math.Max(1, (long)Math.Round(v));
        else it.hp = Math.Max(1, (long)Math.Round(v * 5));
        RollSubs(it);
        return it;
    }

    public static bool LastForgeFree;

    // Forge une pièce : consomme 1 marteau, la pièce attend la décision du joueur.
    public static Item Forge()
    {
        if (!CanForge) return null;
        LastForgeFree = rng.NextDouble() < TV("FreeForgeChance");
        if (!LastForgeFree)
        {
            Data.hammers--;
            if (Data.hammers == HammerCap - 1) Data.lastHammerTicks = DateTime.UtcNow.Ticks;
        }
        int circle = RollCircle();
        int slot = rng.Next(SlotCount);
        int maxLevel = 4 + Data.forgeLevel * 2 + (int)Math.Round(TV(SlotTech[slot] + "LevelUp"));
        int level = rng.Next(Math.Max(1, maxLevel - 8), maxLevel + 1);
        Data.pending = MakeItem(slot, circle, level);
        Data.totalForged++;
        Notify();
        return Data.pending;
    }

    // Équipe la pièce en attente ; l'ancienne pièce est revendue automatiquement.
    public static long EquipPending()
    {
        if (!Data.pending.valid) return 0;
        var old = Data.equipped[Data.pending.slot];
        long gold = old.valid ? old.SellValue : 0;
        Data.gold += gold;
        Data.equipped[Data.pending.slot] = Data.pending;
        Data.pending = new Item();
        Notify();
        return gold;
    }

    public static long SellPending()
    {
        if (!Data.pending.valid) return 0;
        long gold = Data.pending.SellValue;
        Data.gold += gold;
        Data.pending = new Item();
        Notify();
        return gold;
    }

    // ---------- Amélioration de la forge ----------
    public static bool IsMaxLevel => Data.forgeLevel >= ForgeData.MaxLevel;
    static int NextIdx => Data.forgeLevel; // index des tableaux = niveau visé - 1
    public static long NodeCost => IsMaxLevel ? 0 : (long)Math.Round(ForgeData.NodeCost[NextIdx] * (1 - TV("ForgeUpgradeCost")));
    public static int NodesNeeded => IsMaxLevel ? 0 : Math.Max(1, ForgeData.Nodes[NextIdx]);
    public static long UpgradeDuration => IsMaxLevel ? 0 : (long)Math.Round(ForgeData.UpgradeSeconds[NextIdx] / (1 + TV("ForgeTimerSpeed")));

    public static bool PayNode()
    {
        if (IsMaxLevel || Data.upgrading || Data.gold < NodeCost) return false;
        Data.gold -= NodeCost;
        Data.nodesPaid++;
        if (Data.nodesPaid >= NodesNeeded)
        {
            Data.upgrading = true;
            Data.upgradeEndTicks = DateTime.UtcNow.Ticks + UpgradeDuration * TimeSpan.TicksPerSecond;
        }
        Notify();
        return true;
    }

    public static long UpgradeSecondsLeft()
    {
        if (!Data.upgrading) return 0;
        return Math.Max(0, (Data.upgradeEndTicks - DateTime.UtcNow.Ticks) / TimeSpan.TicksPerSecond);
    }

    public static int SpeedUpGemCost()
    {
        if (!Data.upgrading || IsMaxLevel) return 0;
        double total = Math.Max(1, UpgradeDuration);
        double frac = UpgradeSecondsLeft() / total;
        return Math.Max(1, (int)Math.Ceiling(ForgeData.GemCost[NextIdx] * frac));
    }

    public static bool SpeedUp()
    {
        int cost = SpeedUpGemCost();
        if (!Data.upgrading || Data.gems < cost) return false;
        Data.gems -= cost;
        FinishUpgrade();
        return true;
    }

    public static event Action ForgeLeveledUp;

    static void FinishUpgrade()
    {
        Data.upgrading = false;
        Data.nodesPaid = 0;
        Data.forgeLevel = Math.Min(ForgeData.MaxLevel, Data.forgeLevel + 1);
        Notify();
        ForgeLeveledUp?.Invoke();
    }

    // ---------- Compagnons ----------
    // Les compagnons sont volontairement moins puissants que dans le jeu de référence (équilibrage avec nos objets).
    public const double PetScale = 0.12;
    public const int PetMaxLevel = 100;
    public static event Action<string> PetMessage;

    public static OwnedPet FindPet(int id)
    {
        if (id < 0 || Data.pets == null) return null;
        foreach (var p in Data.pets) if (p.id == id) return p;
        return null;
    }

    public static double PetDamage(OwnedPet p)
    {
        var d = ProgressionData.Pets[p.id];
        return ProgressionData.PetBaseDamage[d.rarity] * ProgressionData.PetTypeDamage[d.type] * Math.Pow(1.01, p.level - 1) * PetScale;
    }

    public static double PetHealth(OwnedPet p)
    {
        var d = ProgressionData.Pets[p.id];
        return ProgressionData.PetBaseHealth[d.rarity] * ProgressionData.PetTypeHealth[d.type] * Math.Pow(1.01, p.level - 1) * PetScale;
    }

    static double PetsDamage() { double t = 0; if (Data.equippedPets != null) foreach (int id in Data.equippedPets) { var p = FindPet(id); if (p != null) t += PetDamage(p); } return t; }
    static double PetsHealth() { double t = 0; if (Data.equippedPets != null) foreach (int id in Data.equippedPets) { var p = FindPet(id); if (p != null) t += PetHealth(p); } return t; }

    // Doublons nécessaires pour passer du niveau L au niveau L+1.
    public static int CopiesForNext(int level) => level;

    public static bool IsPetEquipped(int id) => Array.IndexOf(Data.equippedPets, id) >= 0;

    public static string TogglePetEquip(int id)
    {
        if (FindPet(id) == null) return "Pas encore obtenu";
        int idx = Array.IndexOf(Data.equippedPets, id);
        if (idx >= 0) { Data.equippedPets[idx] = -1; Notify(); return null; }
        int free = Array.IndexOf(Data.equippedPets, -1);
        if (free < 0) return "3 compagnons maximum : retires-en un d'abord";
        Data.equippedPets[free] = id;
        Notify();
        return null;
    }

    // ---------- Invocation d'œufs (coquilles) ----------
    public static int EggSummonLevelMax => ProgressionData.EggSummonRequired.Length;
    public static float[] EggOdds => ProgressionData.EggSummonOdds[Math.Min(Data.eggSummonLevel, EggSummonLevelMax - 1)];
    public static int EggSummonRequired => ProgressionData.EggSummonRequired[Math.Min(Data.eggSummonLevel, EggSummonLevelMax - 1)];

    static int RollEggFromSummon()
    {
        var odds = EggOdds;
        double total = 0; foreach (var o in odds) total += o;
        double x = rng.NextDouble() * total;
        for (int r = 0; r < odds.Length; r++) { x -= odds[r]; if (x < 0) return r; }
        return 0;
    }

    // Invoque « count » œufs ; renvoie le nombre d'œufs obtenus par rareté (null si pas assez de coquilles).
    public static int[] SummonEggs(int count)
    {
        long cost = (long)ProgressionData.EggSummonCost * count;
        if (Data.eggshells < cost) return null;
        Data.eggshells -= cost;
        var got = new int[6];
        for (int i = 0; i < count; i++)
        {
            int r = RollEggFromSummon();
            int n = rng.NextDouble() < TV("ExtraEggChance") ? 2 : 1;
            got[r] += n;
            Data.eggs[r] = Math.Min(999, Data.eggs[r] + n);
            if (Data.eggSummonLevel < EggSummonLevelMax - 1)
            {
                Data.eggSummonProgress++;
                if (Data.eggSummonProgress >= EggSummonRequired) { Data.eggSummonProgress = 0; Data.eggSummonLevel++; }
            }
        }
        Notify();
        return got;
    }

    public static void AddEggshells(long n) { Data.eggshells += n; Notify(); }

    public static int AddEgg(int rarity)
    {
        Data.eggs[rarity] = Math.Min(99, Data.eggs[rarity] + 1);
        Notify();
        return rarity;
    }

    public static int TotalEggs() { int t = 0; foreach (var e in Data.eggs) t += e; return t; }

    // Place l'œuf de plus haute rareté disponible dans la couveuse.
    public static bool StartIncubation(int slot)
    {
        if (slot >= Data.incubatorSlots) return false;
        var inc = Data.incubators[slot];
        if (inc.rarity >= 0) return false;
        for (int r = 5; r >= 0; r--)
        {
            if (Data.eggs[r] <= 0) continue;
            Data.eggs[r]--;
            inc.rarity = r;
            inc.endTicks = DateTime.UtcNow.Ticks + (long)(ProgressionData.EggHatchSeconds[r] / (1 + TV(EggTimerTech[r])) * TimeSpan.TicksPerSecond);
            Notify();
            return true;
        }
        return false;
    }

    public static long IncubatorSecondsLeft(int slot)
    {
        var inc = Data.incubators[slot];
        if (inc.rarity < 0) return 0;
        return Math.Max(0, (inc.endTicks - DateTime.UtcNow.Ticks) / TimeSpan.TicksPerSecond);
    }

    public static int IncubatorSpeedUpCost(int slot) => Math.Max(1, (int)Math.Ceiling(IncubatorSecondsLeft(slot) * 0.001)); // 0,001 gemme/s (Forge Master)

    public static bool SpeedUpIncubator(int slot)
    {
        int cost = IncubatorSpeedUpCost(slot);
        if (Data.incubators[slot].rarity < 0 || Data.gems < cost) return false;
        Data.gems -= cost;
        Data.incubators[slot].endTicks = DateTime.UtcNow.Ticks;
        Notify();
        return true;
    }

    public static bool BuyIncubator()
    {
        if (Data.incubatorSlots >= ProgressionData.IncubatorMax) return false;
        int cost = ProgressionData.IncubatorCost[Data.incubatorSlots];
        if (Data.gems < cost) return false;
        Data.gems -= cost;
        Data.incubatorSlots++;
        Notify();
        return true;
    }

    // Fait éclore l'œuf : nouveau compagnon, ou doublon qui fait progresser son niveau.
    public static string Hatch(int slot)
    {
        var inc = Data.incubators[slot];
        if (inc.rarity < 0 || IncubatorSecondsLeft(slot) > 0) return null;
        int rarity = inc.rarity;
        inc.rarity = -1;
        var pool = new System.Collections.Generic.List<int>();
        for (int i = 0; i < ProgressionData.Pets.Length; i++) if (ProgressionData.Pets[i].rarity == rarity) pool.Add(i);
        int id = pool[rng.Next(pool.Count)];
        var def = ProgressionData.Pets[id];
        var owned = FindPet(id);
        string msg;
        if (owned == null)
        {
            owned = new OwnedPet { id = id };
            RollSubsInto(ProgressionData.PetSubCount[rarity], out owned.subs, out owned.subVals);
            Data.pets.Add(owned);
            int free = Array.IndexOf(Data.equippedPets, -1);
            if (free >= 0) Data.equippedPets[free] = id;
            msg = "Nouveau compagnon : " + def.name + " (" + ProgressionData.Rarities[rarity] + ") !";
        }
        else
        {
            owned.copies++;
            msg = "Doublon : " + def.name + " +1";
            while (owned.level < PetMaxLevel && owned.copies >= CopiesForNext(owned.level))
            {
                owned.copies -= CopiesForNext(owned.level);
                owned.level++;
                msg = def.name + " passe au niveau " + owned.level + " !";
            }
        }
        Notify();
        PetMessage?.Invoke(msg);
        return msg;
    }

    // ---------- Montures ----------
    public static event Action<string> MountMessage;

    public static OwnedMount FindMount(int id)
    {
        if (id < 0 || Data.mounts == null) return null;
        foreach (var m in Data.mounts) if (m.id == id) return m;
        return null;
    }

    // Bonus en fraction (0,1 = +10 %) : base de la rareté, +1 % par niveau, renforcé par l'arbre technologique.
    public static double MountBonus(OwnedMount m) => ProgressionData.MountBonusPct[ProgressionData.Mounts[m.id].rarity] / 100.0 * Math.Pow(1.01, m.level - 1);
    public static double MountDamageBonus() { var m = FindMount(Data.equippedMount); return m == null ? 0 : MountBonus(m) * (1 + TV("MountDamage")); }
    public static double MountHealthBonus() { var m = FindMount(Data.equippedMount); return m == null ? 0 : MountBonus(m) * (1 + TV("MountHealth")); }

    public static int MountSummonLevelMax => ProgressionData.MountSummonRequired.Length;
    public static float[] MountOdds => ProgressionData.MountSummonOdds[Math.Min(Data.mountSummonLevel, MountSummonLevelMax - 1)];
    public static int MountSummonRequired => ProgressionData.MountSummonRequired[Math.Min(Data.mountSummonLevel, MountSummonLevelMax - 1)];
    public static long MountSummonCost => (long)Math.Ceiling(ProgressionData.MountSummonCost * (1 - TV("MountSummonCost")));

    static int RollMountRarity()
    {
        var odds = MountOdds;
        double total = 0; foreach (var o in odds) total += o;
        double x = rng.NextDouble() * total;
        for (int r = 0; r < odds.Length; r++) { x -= odds[r]; if (x < 0) return r; }
        return 0;
    }

    static string GainMount(int rarity)
    {
        var pool = new List<int>();
        for (int i = 0; i < ProgressionData.Mounts.Length; i++) if (ProgressionData.Mounts[i].rarity == rarity) pool.Add(i);
        int id = pool[rng.Next(pool.Count)];
        var def = ProgressionData.Mounts[id];
        var m = FindMount(id);
        if (m == null)
        {
            m = new OwnedMount { id = id };
            Data.mounts.Add(m);
            var cur = FindMount(Data.equippedMount);
            if (cur == null || MountBonus(m) > MountBonus(cur)) Data.equippedMount = id;
            return "Nouvelle monture : " + def.name + " (" + ProgressionData.Rarities[rarity] + ")";
        }
        m.copies++;
        string msg = "Doublon : " + def.name;
        while (m.level < 100 && m.copies >= m.level) { m.copies -= m.level; m.level++; msg = def.name + " niv. " + m.level; }
        return msg;
    }

    public static string SummonMounts(int count)
    {
        long cost = MountSummonCost * count;
        if (Data.winders < cost) return null;
        Data.winders -= cost;
        var best = -1; string bestMsg = null; int news = 0;
        for (int i = 0; i < count; i++)
        {
            int n = rng.NextDouble() < TV("ExtraMountChance") ? 2 : 1;
            for (int k = 0; k < n; k++)
            {
                int r = RollMountRarity();
                string msg = GainMount(r);
                if (msg.StartsWith("Nouvelle")) news++;
                if (r > best) { best = r; bestMsg = msg; }
            }
            if (Data.mountSummonLevel < MountSummonLevelMax - 1)
            {
                Data.mountSummonProgress++;
                if (Data.mountSummonProgress >= MountSummonRequired) { Data.mountSummonProgress = 0; Data.mountSummonLevel++; }
            }
        }
        Notify();
        string res = count == 1 ? bestMsg : count + " invocations : meilleure = " + bestMsg + (news > 0 ? " (" + news + " nouvelle(s))" : "");
        MountMessage?.Invoke(res);
        return res;
    }

    public static void EquipMount(int id)
    {
        if (FindMount(id) == null) return;
        Data.equippedMount = Data.equippedMount == id ? -1 : id;
        Notify();
    }

    // ---------- Compétences ----------
    // Même échelle que les compagnons : les valeurs de référence sont trop fortes pour nos objets.
    public const double SkillScale = 0.12;
    public static event Action<string> SkillMessage;

    public static OwnedSkill FindSkill(int id)
    {
        if (id < 0 || Data.skills == null) return null;
        foreach (var s in Data.skills) if (s.id == id) return s;
        return null;
    }

    // Valeur d'activation (dégâts par coup / attaque en plus) et vie (soin / PV en plus).
    public static double SkillDamage(OwnedSkill s) => SkillData.Skills[s.id].damage * SkillData.LevelMult(s.level) * SkillScale * (1 + TV("SkillDamage"));
    public static double SkillHealth(OwnedSkill s) => SkillData.Skills[s.id].health * SkillData.LevelMult(s.level) * SkillScale * (1 + TV("SkillDamage"));

    static double PassiveBase(OwnedSkill s)
    {
        int r = SkillData.Skills[s.id].rarity;
        return SkillData.PassiveDamage[r] * (1 + SkillData.PassiveSlope[r] * (s.level - 1)) * SkillScale;
    }
    public static double SkillPassiveDamage(OwnedSkill s) => PassiveBase(s) * (1 + TV("SkillPassiveDamage"));
    public static double SkillPassiveHealth(OwnedSkill s) => PassiveBase(s) * 8 * (1 + TV("SkillPassiveHealth"));

    // Le bonus passif vient de toutes les compétences possédées, équipées ou non.
    public static double SkillPassiveDamage() { double t = 0; if (Data.skills != null) foreach (var s in Data.skills) t += SkillPassiveDamage(s); return t; }
    public static double SkillPassiveHealth() { double t = 0; if (Data.skills != null) foreach (var s in Data.skills) t += SkillPassiveHealth(s); return t; }

    // Dégâts par seconde estimés des compétences équipées (pour la puissance).
    static double SkillsDps()
    {
        double t = 0;
        if (Data.equippedSkills == null) return 0;
        foreach (int id in Data.equippedSkills)
        {
            var s = FindSkill(id);
            if (s == null) continue;
            var d = SkillData.Skills[id];
            double cycle = d.cooldown + d.duration;
            switch (d.kind)
            {
                case SkillData.Strike: t += SkillDamage(s) / cycle; break;
                case SkillData.Volley: t += SkillDamage(s) * 2 / cycle; break;
                case SkillData.Rage:
                case SkillData.Aura: t += SkillDamage(s) * d.duration / cycle; break;
                case SkillData.Drone: t += SkillDamage(s) * (d.duration / 2f) / cycle; break;
            }
        }
        return t;
    }

    public static int SkillCopiesForNext(int level) => SkillData.CopiesForNext[Math.Min(Math.Max(level, 1), SkillData.CopiesForNext.Length) - 1];
    public static bool IsSkillEquipped(int id) => Array.IndexOf(Data.equippedSkills, id) >= 0;

    public static string ToggleSkillEquip(int id)
    {
        if (FindSkill(id) == null) return "Pas encore obtenue";
        int idx = Array.IndexOf(Data.equippedSkills, id);
        if (idx >= 0) { Data.equippedSkills[idx] = -1; Notify(); return null; }
        int free = Array.IndexOf(Data.equippedSkills, -1);
        if (free < 0) return "3 compétences maximum : retires-en une d'abord";
        Data.equippedSkills[free] = id;
        Notify();
        return null;
    }

    public static int SkillSummonLevelMax => SkillData.SummonRequired.Length;
    public static float[] SkillOdds => SkillData.SummonOdds[Math.Min(Data.skillSummonLevel, SkillSummonLevelMax - 1)];
    public static int SkillSummonRequired => SkillData.SummonRequired[Math.Min(Data.skillSummonLevel, SkillSummonLevelMax - 1)];
    public static long SkillSummonCost => (long)Math.Ceiling(SkillData.SummonCost * (1 - TV("SkillSummonCost")));

    static int RollSkillRarity()
    {
        var odds = SkillOdds;
        double total = 0; foreach (var o in odds) total += o;
        double x = rng.NextDouble() * total;
        for (int r = 0; r < odds.Length; r++) { x -= odds[r]; if (x < 0) return r; }
        return 0;
    }

    static string GainSkill(int rarity, out bool isNew)
    {
        var pool = new List<int>();
        for (int i = 0; i < SkillData.Skills.Length; i++) if (SkillData.Skills[i].rarity == rarity) pool.Add(i);
        int id = pool[rng.Next(pool.Count)];
        var def = SkillData.Skills[id];
        var s = FindSkill(id);
        isNew = s == null;
        if (s == null)
        {
            s = new OwnedSkill { id = id };
            Data.skills.Add(s);
            int free = Array.IndexOf(Data.equippedSkills, -1);
            if (free >= 0) Data.equippedSkills[free] = id;
            return "Nouvelle compétence : " + def.name + " (" + ProgressionData.Rarities[rarity] + ")";
        }
        s.copies++;
        string msg = "Doublon : " + def.name;
        while (s.level < SkillData.MaxLevel && s.copies >= SkillCopiesForNext(s.level))
        {
            s.copies -= SkillCopiesForNext(s.level);
            s.level++;
            msg = def.name + " niv. " + s.level;
        }
        return msg;
    }

    // Invoque « count » compétences avec des tickets ; renvoie un résumé (null si pas assez de tickets).
    public static string SummonSkills(int count)
    {
        long cost = SkillSummonCost * count;
        if (Data.skillTickets < cost) return null;
        Data.skillTickets -= cost;
        int best = -1, news = 0; string bestMsg = null;
        for (int i = 0; i < count; i++)
        {
            int r = RollSkillRarity();
            string msg = GainSkill(r, out bool isNew);
            if (isNew) news++;
            if (r > best || (r == best && isNew)) { best = r; bestMsg = msg; }
            if (Data.skillSummonLevel < SkillSummonLevelMax - 1)
            {
                Data.skillSummonProgress++;
                if (Data.skillSummonProgress >= SkillSummonRequired) { Data.skillSummonProgress = 0; Data.skillSummonLevel++; }
            }
        }
        Notify();
        string res = count + " invocations : meilleure = " + bestMsg + (news > 0 ? " (" + news + " nouvelle(s))" : "");
        SkillMessage?.Invoke(res);
        return res;
    }

    // Texte de l'effet avec les valeurs du niveau actuel.
    public static string SkillEffectText(int id, OwnedSkill s)
    {
        var d = SkillData.Skills[id];
        double a = s != null ? SkillDamage(s) : d.damage * SkillScale;
        double h = s != null ? SkillHealth(s) : d.health * SkillScale;
        return d.desc.Replace("{a}", Fmt(a)).Replace("{h}", Fmt(h)).Replace("{d}", d.duration.ToString("0"));
    }

    // ---------- Boutique ----------
    public static event Action<string> ShopMessage;
    public static int DealSize => ShopData.DealSize(Math.Min(9, Data.bestStage / StagesPerCircle));

    // Nouvelles offres chaque jour à 22:00. Vrai si elles viennent d'être renouvelées.
    static bool RefreshShop()
    {
        long day = DateTime.Now.AddHours(2).Date.Ticks;
        bool valid = Data.shopDeals != null && Data.shopDeals.Length == ShopData.DealsPerDay
            && Data.shopDealBought != null && Data.shopDealBought.Length == ShopData.DealsPerDay
            && Data.shopBundleBuys != null && Data.shopBundleBuys.Length == ShopData.Bundles.Length;
        if (valid && day == Data.shopDay) return false;
        Data.shopDay = day;
        Data.shopFreeTaken = false;
        Data.shopDealBought = new bool[ShopData.DealsPerDay];
        Data.shopBundleBuys = new int[ShopData.Bundles.Length];
        // 3 offres différentes tirées au hasard (graine = jour : stable si on relance le jeu).
        var r = new System.Random((int)(day / TimeSpan.TicksPerDay));
        var pool = new List<int>();
        for (int i = 0; i < ShopData.Deals.Length; i++) pool.Add(i);
        Data.shopDeals = new int[ShopData.DealsPerDay];
        for (int i = 0; i < ShopData.DealsPerDay; i++) { int k = r.Next(pool.Count); Data.shopDeals[i] = pool[k]; pool.RemoveAt(k); }
        return true;
    }

    // Or réel donné pour une quantité « de référence » : 1 000 = 1 heure de gains hors ligne au niveau actuel.
    public static long ShopGold(long refAmount) => (long)Math.Round(refAmount / ShopData.GoldRefPerHour * 3600 * (1 + Data.stage * 0.15) * (1 + TV("CoinOfflineReward")));

    public static long ShopAmount(int currency, long amount) => currency == ShopData.Gold ? ShopGold(amount) : amount;

    static void Give(int currency, long amount)
    {
        switch (currency)
        {
            case ShopData.Gold: Data.gold += ShopGold(amount); break;
            case ShopData.Gems: Data.gems += amount; break;
            case ShopData.Hammers: Data.hammers += (int)amount; break;
            case ShopData.Shells: Data.eggshells += amount; break;
            case ShopData.Winders: Data.winders += amount; break;
            case ShopData.Tickets: Data.skillTickets += amount; break;
            case ShopData.Potions: Data.potions += amount; break;
            case ShopData.KeyHammer: Data.dungeonKeys[DungeonHammer] += (int)amount; break;
            case ShopData.KeyEgg: Data.dungeonKeys[DungeonEgg] += (int)amount; break;
            case ShopData.KeyPotion: Data.dungeonKeys[DungeonPotion] += (int)amount; break;
            case ShopData.KeySkill: Data.dungeonKeys[DungeonSkill] += (int)amount; break;
        }
    }

    static void GiveAll(int[] list) { for (int i = 0; i + 1 < list.Length; i += 2) Give(list[i], list[i + 1]); }

    public static string ContentText(int[] list, string sep)
    {
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i + 1 < list.Length; i += 2)
        {
            if (i > 0) sb.Append(sep);
            int c = list[i];
            string col = "#" + ColorUtility.ToHtmlStringRGB(ShopData.CurrencyColors[c]);
            sb.Append("<color=" + col + ">+" + Fmt(ShopAmount(c, list[i + 1])) + " " + ShopData.CurrencyNames[c] + "</color>");
        }
        return sb.ToString();
    }

    public static int[] DealContent(int slot) => ShopData.Deals[Data.shopDeals[slot]].sizes[DealSize];
    public static int DealPrice => ShopData.DealPrice[DealSize];

    public static string TakeFreeGift()
    {
        if (Data.shopFreeTaken) return "Cadeau déjà pris aujourd'hui : reviens après 22:00";
        Data.shopFreeTaken = true;
        GiveAll(ShopData.FreeGift);
        Notify();
        ShopMessage?.Invoke("Cadeau du jour récupéré !");
        return null;
    }

    public static string BuyDeal(int slot)
    {
        if (Data.shopDealBought[slot]) return "Offre déjà achetée aujourd'hui";
        int price = DealPrice;
        if (Data.gems < price) return "Pas assez de gemmes";
        Data.gems -= price;
        Data.shopDealBought[slot] = true;
        GiveAll(DealContent(slot));
        Notify();
        ShopMessage?.Invoke(ShopData.Deals[Data.shopDeals[slot]].name + " acheté !");
        return null;
    }

    public static string BuyBundle(int i)
    {
        var b = ShopData.Bundles[i];
        if (Data.shopBundleBuys[i] >= b.perDay) return "Limite du jour atteinte";
        if (Data.gems < b.price) return "Pas assez de gemmes";
        Data.gems -= b.price;
        Data.shopBundleBuys[i]++;
        Give(b.currency, b.amount);
        Notify();
        ShopMessage?.Invoke("+" + Fmt(ShopAmount(b.currency, b.amount)) + " " + ShopData.CurrencyNames[b.currency]);
        return null;
    }

    public static TimeSpan TimeToShopRefresh() => TimeToKeyRefresh();

    // ---------- Missions (escouades, 3 énergies par jour) ----------
    public static event Action<string> MissionMessage;

    // Palier de missions débloqué par le niveau atteint au Voleur de marteau.
    public static int MissionTier
    {
        get
        {
            int thief = Data.dungeonLevel[DungeonHammer], t = 0;
            for (int i = 0; i < MissionData.LevelMinThief.Length; i++) if (MissionData.LevelMinThief[i] <= thief) t = i;
            return t;
        }
    }

    static void RollMission(int slot, System.Random r)
    {
        int tier = MissionTier;
        int level = r.Next(MissionData.LevelMin[tier], MissionData.LevelMax[tier] + 1);
        var pool = new List<int>();
        for (int i = 0; i < MissionData.SquadMinLevel.Length; i++) if (MissionData.SquadMinLevel[i] <= level) pool.Add(i);
        Data.missionSquad[slot] = pool[r.Next(pool.Count)];
        Data.missionLevel[slot] = level;
    }

    static void RollAllMissions()
    {
        Data.missionSquad = new int[MissionData.OfferCount];
        Data.missionLevel = new int[MissionData.OfferCount];
        for (int i = 0; i < MissionData.OfferCount; i++) RollMission(i, rng);
    }

    // Énergies rechargées et nouvelle liste chaque jour à 22:00.
    static bool RefreshMissions()
    {
        long day = DateTime.Now.AddHours(2).Date.Ticks;
        bool valid = Data.missionSquad != null && Data.missionSquad.Length == MissionData.OfferCount
            && Data.missionLevel != null && Data.missionLevel.Length == MissionData.OfferCount;
        if (valid && day == Data.missionDay) return false;
        if (day != Data.missionDay) Data.missionEnergy = Math.Max(Data.missionEnergy, MissionData.DailyEnergy);
        Data.missionDay = day;
        RollAllMissions();
        return true;
    }

    public static string RefreshMissionList()
    {
        if (Data.gems < MissionData.RefreshGemCost) return "Pas assez de gemmes";
        Data.gems -= MissionData.RefreshGemCost;
        RollAllMissions();
        Notify();
        return null;
    }

    // Difficulté : le niveau de mission équivaut au Voleur de marteau qui le débloque (même échelle que les donjons).
    static int MissionEquivalentStage(int level)
    {
        int thief = 0;
        for (int i = 0; i < MissionData.LevelMax.Length; i++) if (MissionData.LevelMax[i] >= level) { thief = MissionData.LevelMinThief[i]; break; }
        return 2 + 4 * thief + (level - 1);
    }

    public static int MissionUnits(int slot) => MissionData.SquadUnits[Data.missionSquad[slot]];
    public static double MissionUnitHp(int slot) => 30 * Math.Pow(1.16, MissionEquivalentStage(Data.missionLevel[slot])) * MissionData.SquadHealth[Data.missionSquad[slot]] / 8000.0 * 1.5;
    public static double MissionUnitAtk(int slot) => 5 * Math.Pow(1.16, MissionEquivalentStage(Data.missionLevel[slot])) * MissionData.SquadDamage[Data.missionSquad[slot]] / 1000.0;

    public static int[] MissionReward(int level)
    {
        int i = Math.Max(1, Math.Min(MissionData.MaxLevel, level)) - 1;
        return new[] { ShopData.Gold, (int)(MissionData.RewardGold[i] / MissionData.GoldDivider), ShopData.Tickets, MissionData.RewardTickets[i],
            ShopData.Shells, MissionData.RewardShells[i], ShopData.Potions, MissionData.RewardPotions[i], ShopData.Winders, MissionData.RewardWinders[i] };
    }

    public static bool CanStartMission => Data.missionEnergy > 0;

    // Victoire : récompense, 1 énergie consommée (seulement en cas de réussite) et une nouvelle mission à cette place.
    public static string MissionWon(int slot)
    {
        var reward = MissionReward(Data.missionLevel[slot]);
        string txt = ContentText(reward, "  ");
        GiveAll(reward);
        Data.missionEnergy = Math.Max(0, Data.missionEnergy - 1);
        RollMission(slot, rng);
        Notify();
        return txt;
    }

    // ---------- Pass de progression ----------
    public static bool PassReached(int i) => Data.bestStage > MissionData.PassStage[i];
    public static int PassReadyCount() { int n = 0; for (int i = 0; i < MissionData.PassStage.Length; i++) if (PassReached(i) && !Data.passClaimed[i]) n++; return n; }

    // L'or du pass est converti comme celui des missions (1 000 = 15 min de gains hors ligne).
    public static int[] PassContent(int i)
    {
        var src = MissionData.PassRewards[i];
        var r = (int[])src.Clone();
        for (int k = 0; k + 1 < r.Length; k += 2) if (r[k] == ShopData.Gold) r[k + 1] = (int)(r[k + 1] / MissionData.GoldDivider);
        return r;
    }

    public static string ClaimPass(int i)
    {
        if (Data.passClaimed[i]) return "Déjà récupéré";
        if (!PassReached(i)) return "Franchis d'abord l'étape " + StageName(MissionData.PassStage[i]);
        Data.passClaimed[i] = true;
        GiveAll(PassContent(i));
        Notify();
        return null;
    }

    public static string ClaimAllPass()
    {
        int n = 0;
        for (int i = 0; i < MissionData.PassStage.Length; i++)
            if (PassReached(i) && !Data.passClaimed[i]) { Data.passClaimed[i] = true; GiveAll(PassContent(i)); n++; }
        if (n == 0) return "Rien à récupérer pour l'instant";
        Notify();
        MissionMessage?.Invoke(n + " palier(s) récupéré(s) !");
        return null;
    }

    public static string StageName(int stage)
    {
        stage = Math.Max(0, Math.Min(MaxStage, stage));
        return CircleNames[stage / StagesPerCircle] + " " + (stage / StagesPerCircle + 1) + "-" + (stage % StagesPerCircle + 1);
    }

    // ---------- Arbre technologique ----------
    public static readonly string[] SlotTech = { "Weapon", "Helmet", "Body", "Glove", "Shoe", "Belt", "Necklace", "Ring" };
    static readonly string[] EggTimerTech = { "CommonEggTimer", "RareEggTimer", "EpicEggTimer", "LegendaryEggTimer", "UltimateEggTimer", "MythicEggTimer" };
    public static readonly string[] TreeNames = { "Forge", "Puissance", "Compagnons & Tech" };
    static readonly int[] TreeOffset = { 0, TechData.Trees[0].Length, TechData.Trees[0].Length + TechData.Trees[1].Length };
    public static int TechTotalNodes => TechData.Trees[0].Length + TechData.Trees[1].Length + TechData.Trees[2].Length;
    static readonly Dictionary<string, float> techTotals = new Dictionary<string, float>();
    public static event Action<string> TechDone;
    const double TechGemPerSecond = 0.0023;

    // Valeur cumulée d'un type de recherche (somme des niveaux × valeur par niveau).
    public static float TV(string type) => techTotals.TryGetValue(type, out var v) ? v : 0f;

    static void RecomputeTech()
    {
        techTotals.Clear();
        for (int tr = 0; tr < 3; tr++)
            for (int id = 0; id < TechData.Trees[tr].Length; id++)
            {
                int lvl = TechLevel(tr, id);
                if (lvl <= 0) continue;
                int type = NodeType(tr, id);
                string name = TechData.Types[type];
                techTotals.TryGetValue(name, out float cur);
                techTotals[name] = cur + TechData.Value[type] * lvl;
            }
    }

    public static int TechLevel(int tree, int id) => Data.techLevels[TreeOffset[tree] + id];
    public static int NodeTier(int tree, int id) => TechData.Trees[tree][id][0];
    public static int NodeLayer(int tree, int id) => TechData.Trees[tree][id][1];
    public static int NodeType(int tree, int id) => TechData.Trees[tree][id][2];
    public static int NodeMax(int tree, int id) => TechData.MaxLevel[NodeType(tree, id)];

    public static bool NodeUnlocked(int tree, int id)
    {
        var n = TechData.Trees[tree][id];
        for (int k = 3; k < n.Length; k++) if (TechLevel(tree, n[k]) < 1) return false;
        return true;
    }

    public static bool IsResearching(int tree, int id) => Data.researchTree == tree && Data.researchNode == id;

    public static long ResearchCost(int tree, int id)
    {
        int lvl = Math.Min(TechLevel(tree, id), 4);
        return (long)Math.Ceiling(ProgressionData.TechPotions[NodeTier(tree, id)][lvl] * (1 - TV("TechNodeUpgradeCost")));
    }

    public static long ResearchSeconds(int tree, int id)
    {
        int lvl = Math.Min(TechLevel(tree, id), 4);
        return (long)Math.Round(ProgressionData.TechSeconds[NodeTier(tree, id)][lvl] / (1 + TV("TechResearchTimer")));
    }

    public static string StartResearch(int tree, int id)
    {
        if (Data.researchTree >= 0) return "Une recherche est déjà en cours";
        if (!NodeUnlocked(tree, id)) return "Recherche verrouillée : améliore d'abord les précédentes";
        if (TechLevel(tree, id) >= NodeMax(tree, id)) return "Déjà au maximum";
        long cost = ResearchCost(tree, id);
        if (Data.potions < cost) return "Pas assez de potions rouges (Chaudron des potions)";
        Data.potions -= cost;
        Data.researchTree = tree;
        Data.researchNode = id;
        Data.researchEndTicks = DateTime.UtcNow.Ticks + ResearchSeconds(tree, id) * TimeSpan.TicksPerSecond;
        Notify();
        return null;
    }

    public static long ResearchSecondsLeft() => Data.researchTree < 0 ? 0 : Math.Max(0, (Data.researchEndTicks - DateTime.UtcNow.Ticks) / TimeSpan.TicksPerSecond);
    public static int ResearchSpeedUpCost() => Math.Max(1, (int)Math.Ceiling(ResearchSecondsLeft() * TechGemPerSecond));

    public static bool SpeedUpResearch()
    {
        if (Data.researchTree < 0) return false;
        int cost = ResearchSpeedUpCost();
        if (Data.gems < cost) return false;
        Data.gems -= cost;
        FinishResearch();
        return true;
    }

    static void FinishResearch()
    {
        int tr = Data.researchTree, id = Data.researchNode;
        Data.researchTree = Data.researchNode = -1;
        if (tr < 0) return;
        Data.techLevels[TreeOffset[tr] + id]++;
        RecomputeTech();
        Notify();
        TechDone?.Invoke("Recherche terminée : " + TechText.Name(NodeType(tr, id)) + " niv. " + TechLevel(tr, id));
    }

    // ---------- Forge automatique (débloquée par l'arbre) ----------
    public static bool AutoForgeUnlocked => TV("AutoForge") > 0f;
    public static float AutoForgeInterval => 1.2f / (1f + 0.5f * Math.Max(0f, TV("AutoForge") - 1f));

    // Décide seule : équipe la pièce si elle augmente la puissance, sinon la vend. Renvoie un court résumé.
    public static string AutoResolvePending()
    {
        if (!Data.pending.valid) return null;
        var it = Data.pending;
        if (PowerIfEquipped(it) > Power()) { EquipPending(); return "Auto : " + it.Name + " équipée"; }
        long g = SellPending();
        return "Auto : +" + Fmt(g) + " or";
    }

    // ---------- Gains hors ligne ----------
    public static string OfflineMessage;

    static void ComputeOffline()
    {
        OfflineMessage = null;
        if (Data.lastSeenTicks <= 0) return;
        double secs = (DateTime.UtcNow.Ticks - Data.lastSeenTicks) / (double)TimeSpan.TicksPerSecond;
        if (secs < 600) return;
        double max = 14400 * (1 + TV("MaxOfflineReward"));
        secs = Math.Min(secs, max);
        long gold = (long)Math.Round(secs * 1.0 * (1 + Data.stage * 0.15) * (1 + TV("CoinOfflineReward")));
        int hammers = (int)Math.Round(secs / 60.0 * (1 + TV("HammerOfflineReward")));
        Data.gold += gold;
        Data.hammers += hammers;
        OfflineMessage = "Pendant ton absence (" + FmtTime((long)secs) + ") : +" + Fmt(gold) + " or, +" + hammers + " marteaux";
    }

    // ---------- Donjons ----------
    public const int DungeonCount = 4, DungeonHammer = 0, DungeonEgg = 1, DungeonPotion = 2, DungeonSkill = 3;
    public const int DungeonKeysPerDay = 2, DungeonMaxLevel = 399;
    public static readonly string[] DungeonNames = { "Voleur de marteau", "Couvoir infernal", "Chaudron des potions", "Crypte des grimoires" };
    public static readonly string[] DungeonDesc =
    {
        "Un démon a volé tes marteaux : rattrape-le !",
        "Des nuées de bestioles gardent les coquilles d'œufs.",
        "Les gardiens du chaudron protègent les potions rouges.",
        "Les squelettes de la crypte cachent des tickets de compétences.",
    };
    public static readonly Color[] DungeonColors =
    {
        new Color(0.95f, 0.75f, 0.25f), new Color(0.85f, 0.85f, 0.7f), new Color(0.95f, 0.25f, 0.25f), new Color(0.4f, 0.9f, 0.45f)
    };

    // Les clés reviennent à 2 chaque jour à 22:00 (heure du téléphone). Vrai si une recharge vient d'avoir lieu.
    static bool RefreshKeys()
    {
        if (Data.dungeonKeys == null) return false;
        long day = DateTime.Now.AddHours(2).Date.Ticks;
        if (day == Data.keyDay) return false;
        Data.keyDay = day;
        for (int i = 0; i < DungeonCount; i++) Data.dungeonKeys[i] = Math.Max(Data.dungeonKeys[i], DungeonKeysPerDay);
        return true;
    }

    public static TimeSpan TimeToKeyRefresh()
    {
        var next = DateTime.Now.AddHours(2).Date.AddDays(1).AddHours(-2);
        return next - DateTime.Now;
    }

    // Récompenses : base + augmentation par niveau (valeurs du jeu de référence).
    public static long[] DungeonReward(int type, int level)
    {
        switch (type)
        {
            case DungeonHammer: return new long[] { (long)Math.Round((60 + level) * (1 + TV("HammerThiefHammerReward"))), (long)Math.Round((4000 + 100.0 * level) * (1 + TV("HammerThiefCoinReward"))) };
            case DungeonEgg: return new long[] { (long)Math.Round(200 + 0.65 * level) };
            case DungeonPotion: return new long[] { (long)Math.Round((100 + level) * (1 + TV("ZombieRushTechPotions"))) };
            default: return new long[] { (long)Math.Round((200 + 2.0 * level) * (1 + TV("GhostTownSkillBonus"))) };
        }
    }

    public static string DungeonRewardText(int type, int level)
    {
        var r = DungeonReward(type, level);
        switch (type)
        {
            case DungeonHammer: return "+" + Fmt(r[0]) + " marteaux  +" + Fmt(r[1]) + " or";
            case DungeonEgg: return "+" + Fmt(r[0]) + " coquilles";
            case DungeonPotion: return "+" + Fmt(r[0]) + " potions rouges";
            default: return "+" + Fmt(r[0]) + " tickets de compétence";
        }
    }

    // Difficulté : un niveau de donjon équivaut à avancer de 4 étapes sur le chemin.
    public static int DungeonEquivalentStage(int type) => 2 + 4 * Data.dungeonLevel[type];
    public static double DungeonEnemyHp(int type, double mult) => 30 * Math.Pow(1.16, DungeonEquivalentStage(type)) * mult;
    public static double DungeonEnemyAtk(int type, double mult) => 5 * Math.Pow(1.16, DungeonEquivalentStage(type)) * mult;

    public static bool CanEnterDungeon(int type) => Data.dungeonKeys[type] > 0 && Data.dungeonLevel[type] <= DungeonMaxLevel;

    // Victoire : la clé n'est consommée qu'à la réussite, la récompense tombe et le niveau du donjon augmente.
    public static string DungeonWon(int type)
    {
        int level = Data.dungeonLevel[type];
        var r = DungeonReward(type, level);
        switch (type)
        {
            case DungeonHammer: Data.hammers += (int)r[0]; Data.gold += r[1]; break;
            case DungeonEgg: Data.eggshells += r[0]; break;
            case DungeonPotion: Data.potions += r[0]; break;
            default: Data.skillTickets += r[0]; break;
        }
        Data.dungeonKeys[type] = Math.Max(0, Data.dungeonKeys[type] - 1);
        Data.dungeonLevel[type] = Math.Min(DungeonMaxLevel, level + 1);
        Notify();
        return DungeonRewardText(type, level);
    }

    // ---------- Combat : le chemin des Enfers ----------
    public const int StagesPerCircle = 10;
    public const int MaxStage = 99;
    public static int StageCircle => Mathf.Clamp(Data.stage / StagesPerCircle, 0, 9);
    public static int StageStep => Data.stage % StagesPerCircle;
    public static bool IsBossStage => StageStep == StagesPerCircle - 1;
    public static string StageLabel => CircleNames[StageCircle] + "  " + (StageCircle + 1) + "-" + (StageStep + 1);

    public static double EnemyHp(bool boss) => 30 * Math.Pow(1.16, Data.stage) * (boss ? 8 : 1);
    public static double EnemyAtk(bool boss) => 5 * Math.Pow(1.16, Data.stage) * (boss ? 2.5 : 1);
    public static long KillGold(bool boss) => (long)Math.Round(4 * Math.Pow(1.12, Data.stage) * (boss ? 12 : 1));

    public static event Action StageChanged;

    public static void AddGold(long g) { Data.gold += g; Notify(); }

    public static void WaveCleared()
    {
        Data.hammers = Math.Min(HammerCap * 2, Data.hammers + 1);
        Notify();
    }

    // Étape réussie : gemmes (davantage pour un boss) et passage à l'étape suivante.
    // Coquilles gagnées en battant le boss d'un cercle (récompense de progression, en attendant donjons, ligue et clan).
    public static long BossShellReward => 100 + 50 * StageCircle;
    public static long BossWinderReward => 100 + 50 * StageCircle;
    public static long BossTicketReward => 80 + 40 * StageCircle;

    public static int StageCleared()
    {
        // Comme Forge Master : les étapes ne donnent pas de gemmes (elles viennent du pass, du cadeau du jour et des offres).
        int gems = 0;
        if (IsBossStage && Data.stage >= Data.bestStage) { Data.eggshells += BossShellReward; Data.winders += BossWinderReward; Data.skillTickets += BossTicketReward; }
        Data.stage = Math.Min(MaxStage, Data.stage + 1);
        Data.bestStage = Math.Max(Data.bestStage, Data.stage);
        Notify();
        StageChanged?.Invoke();
        return gems;
    }

    // ---------- Affichage ----------
    public static string Fmt(double v)
    {
        double a = Math.Abs(v);
        if (a < 1000) return ((long)Math.Round(v)).ToString();
        string[] suf = { "k", "M", "B", "T", "aa", "ab", "ac" };
        int i = -1;
        while (a >= 1000 && i < suf.Length - 1) { a /= 1000; v /= 1000; i++; }
        return (a < 10 ? v.ToString("0.##") : a < 100 ? v.ToString("0.#") : v.ToString("0")) + suf[i];
    }

    public static string FmtPct(float v) => (v * 100f).ToString("0.#") + " %";

    public static string FmtTime(long s)
    {
        if (s <= 0) return "0s";
        long d = s / 86400, h = s % 86400 / 3600, m = s % 3600 / 60, sec = s % 60;
        if (d > 0) return d + "j " + h + "h";
        if (h > 0) return h + "h " + m + "m";
        if (m > 0) return m + "m " + sec + "s";
        return sec + "s";
    }
}
