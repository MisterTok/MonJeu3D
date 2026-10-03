using System;
using System.Collections.Generic;

// Noms et descriptions françaises des recherches de l'arbre technologique.
public static class TechText
{
    // type → (nom, description avec {0} = valeur cumulée, effet disponible ?)
    static readonly Dictionary<string, (string name, string desc, bool active)> T = new Dictionary<string, (string, string, bool)>
    {
        { "ForgeTimerSpeed", ("Forge rapide", "+{0} de vitesse d'amélioration de la forge", true) },
        { "ForgeUpgradeCost", ("Forge économe", "-{0} sur le coût d'amélioration de la forge", true) },
        { "EquipmentSellPrice", ("Marchandage", "+{0} sur le prix de revente des pièces", true) },
        { "HammerThiefHammerReward", ("Traque du voleur", "+{0} de marteaux au Voleur de marteau", true) },
        { "HammerThiefCoinReward", ("Rançon du voleur", "+{0} d'or au Voleur de marteau", true) },
        { "AutoForge", ("Forge automatique", "Débloque et accélère la forge automatique", true) },
        { "FreeForgeChance", ("Frappe gratuite", "{0} de chance de forger sans marteau", true) },
        { "MaxOfflineReward", ("Longue absence", "+{0} de durée maximale des gains hors ligne", true) },
        { "CoinOfflineReward", ("Or hors ligne", "+{0} d'or gagné hors ligne", true) },
        { "HammerOfflineReward", ("Marteaux hors ligne", "+{0} de marteaux gagnés hors ligne", true) },
        { "WeaponBonus", ("Lames trempées", "+{0} d'ATQ des lames", true) },
        { "HelmetBonus", ("Heaumes renforcés", "+{0} de PV des heaumes", true) },
        { "BodyBonus", ("Boucliers renforcés", "+{0} de PV des boucliers", true) },
        { "GloveBonus", ("Gantelets aiguisés", "+{0} d'ATQ des gantelets", true) },
        { "ShoeBonus", ("Bottes ferrées", "+{0} de PV des bottes", true) },
        { "BeltBonus", ("Ceintures cloutées", "+{0} de PV des ceintures", true) },
        { "NecklaceBonus", ("Amulettes maudites", "+{0} d'ATQ des amulettes", true) },
        { "RingBonus", ("Anneaux maudits", "+{0} d'ATQ des anneaux", true) },
        { "WeaponLevelUp", ("Maîtrise des lames", "+{0} niveaux max des lames forgées", true) },
        { "HelmetLevelUp", ("Maîtrise des heaumes", "+{0} niveaux max des heaumes forgés", true) },
        { "BodyLevelUp", ("Maîtrise des boucliers", "+{0} niveaux max des boucliers forgés", true) },
        { "GloveLevelUp", ("Maîtrise des gantelets", "+{0} niveaux max des gantelets forgés", true) },
        { "ShoeLevelUp", ("Maîtrise des bottes", "+{0} niveaux max des bottes forgées", true) },
        { "BeltLevelUp", ("Maîtrise des ceintures", "+{0} niveaux max des ceintures forgées", true) },
        { "NecklaceLevelUp", ("Maîtrise des amulettes", "+{0} niveaux max des amulettes forgées", true) },
        { "RingLevelUp", ("Maîtrise des anneaux", "+{0} niveaux max des anneaux forgés", true) },
        { "MountDamage", ("Montures : dégâts", "+{0} de dégâts des montures", false) },
        { "MountHealth", ("Montures : vie", "+{0} de vie des montures", false) },
        { "MountSummonCost", ("Montures : invocation", "-{0} sur le coût d'invocation des montures", false) },
        { "ExtraMountChance", ("Monture bonus", "{0} de chance d'une monture en plus", false) },
        { "TechResearchTimer", ("Recherche rapide", "+{0} de vitesse de recherche", true) },
        { "TechNodeUpgradeCost", ("Recherche économe", "-{0} sur le coût des recherches", true) },
        { "SkillDamage", ("Compétences : puissance", "+{0} de dégâts et de vie des compétences", false) },
        { "SkillPassiveDamage", ("Compétences : dégâts passifs", "+{0} de dégâts passifs des compétences", false) },
        { "SkillPassiveHealth", ("Compétences : vie passive", "+{0} de vie passive des compétences", false) },
        { "SkillSummonCost", ("Compétences : invocation", "-{0} sur le coût d'invocation des compétences", false) },
        { "PetBonusDamage", ("Compagnons féroces", "+{0} de dégâts des compagnons", true) },
        { "PetBonusHealth", ("Compagnons robustes", "+{0} de vie des compagnons", true) },
        { "CommonEggTimer", ("Couvaison : Commune", "+{0} de vitesse d'éclosion des œufs communs", true) },
        { "RareEggTimer", ("Couvaison : Rare", "+{0} de vitesse d'éclosion des œufs rares", true) },
        { "EpicEggTimer", ("Couvaison : Épique", "+{0} de vitesse d'éclosion des œufs épiques", true) },
        { "LegendaryEggTimer", ("Couvaison : Légendaire", "+{0} de vitesse d'éclosion des œufs légendaires", true) },
        { "UltimateEggTimer", ("Couvaison : Ultime", "+{0} de vitesse d'éclosion des œufs ultimes", true) },
        { "MythicEggTimer", ("Couvaison : Mythique", "+{0} de vitesse d'éclosion des œufs mythiques", true) },
        { "ExtraEggChance", ("Œuf bonus", "{0} de chance d'un œuf en plus à chaque invocation", true) },
        { "GhostTownSkillBonus", ("Pillage de la crypte", "+{0} de tickets à la Crypte des grimoires", true) },
        { "ZombieRushTechPotions", ("Pillage du chaudron", "+{0} de potions au Chaudron des potions", true) },
    };

    public static string Name(int type) => T.TryGetValue(TechData.Types[type], out var v) ? v.name : TechData.Types[type];
    public static bool Active(int type) => !T.TryGetValue(TechData.Types[type], out var v) || v.active;

    // Valeur affichée : pourcentage, sauf pour les niveaux max (entier) et la forge automatique.
    public static string Format(int type, float value)
    {
        string t = TechData.Types[type];
        if (t.EndsWith("LevelUp")) return ((int)Math.Round(value)).ToString();
        return (value * 100f).ToString("0.#") + " %";
    }

    public static string Describe(int type, float value)
    {
        if (!T.TryGetValue(TechData.Types[type], out var v)) return TechData.Types[type];
        string d = v.desc.Contains("{0}") ? string.Format(v.desc, Format(type, value)) : v.desc;
        return v.active ? d : d + " (effet à venir)";
    }
}
