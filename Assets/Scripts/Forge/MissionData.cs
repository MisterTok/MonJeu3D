// Missions (adaptées en solo des missions de clan du jeu de référence : MissionBaseConfig, MissionBattleLibrary,
// MissionLevelLibrary, MissionRewardLibrary) et pass de progression (MainGameProgressPassLibrary).
public static class MissionData
{
    public const int DailyEnergy = 3;          // énergies par jour (recharge à 22:00)
    public const int OfferCount = 5;           // missions proposées
    public const int RefreshGemCost = 10;      // nouvelle liste
    public const double LevelMultiplier = 1.524; // vie et dégâts x1,524 par niveau de mission
    public const int MaxLevel = 60;
    public const int MaxShownUnits = 8;        // au-delà, les ennemis sont regroupés (plus forts)
    // L'or de référence est converti comme dans la boutique, mais divisé par 4 (1 000 = 15 min de gains hors ligne).
    public const double GoldDivider = 4;

    public static readonly string[] SquadNames =
    {
        "Hurlement des premiers damnés", "Légion de pierre", "Les jumeaux écorcheurs", "Légion des heaumes", "Les voiles noires",
        "Trois mousquetaires maudits", "Assaut des brigands", "La loi frappe à la porte", "Avant-garde toxique", "Lames étoilées",
        "Invasion des abysses", "La lame colossale", "Brèche infernale", "Forge renégate", "Rebelles du vide",
        "Ordre du trône", "Les deux sorciers", "Descente des cieux", "Sorciers de la nuit", "Venus des Enfers",
    };
    // Modèle d'ennemi (clé de BattleWorld) pour chaque escouade.
    public static readonly string[] SquadModel =
    {
        "Rat", "Skeleton", "Zombie", "Skeleton", "Bat", "Skeleton", "Zombie", "Skeleton", "Slime", "Wasp",
        "Spider", "Dragon", "SnakeAngry", "Skeleton", "Frog", "Zombie", "Dragon", "Bat", "Snake", "Zombie",
    };
    // Pass de progression : étape à franchir et récompenses (piste gratuite + piste « premium » réunies : pas d'achat réel).
    public static readonly int[] PassStage = {2,7,10,13,17,19,20,21,23,25,27,29,30,32,33,37,40,43,47,50,53,57,60,67,70,77,80,84,87,90,97};
    public static readonly int[][] PassRewards =
    {
        new[] { ShopData.Gold, 1000, ShopData.Hammers, 100, ShopData.Gold, 5000, ShopData.Gems, 5 },
        new[] { ShopData.Hammers, 100, ShopData.Gems, 5, ShopData.Shells, 150, ShopData.Gems, 5 },
        new[] { ShopData.Tickets, 220, ShopData.Shells, 150, ShopData.Winders, 50, ShopData.Gems, 5 },
        new[] { ShopData.Tickets, 220, ShopData.Shells, 150, ShopData.Gold, 5000, ShopData.Gems, 5 },
        new[] { ShopData.Shells, 150, ShopData.Hammers, 100, ShopData.Tickets, 220, ShopData.Gems, 5 },
        new[] { ShopData.Potions, 100, ShopData.Hammers, 50, ShopData.Winders, 100, ShopData.Gems, 5 },
        new[] { ShopData.Tickets, 220, ShopData.Shells, 150, ShopData.Hammers, 500, ShopData.Gems, 5 },
        new[] { ShopData.Potions, 100, ShopData.Hammers, 50, ShopData.Winders, 100, ShopData.Gems, 5 },
        new[] { ShopData.Tickets, 220, ShopData.Shells, 150, ShopData.Potions, 400, ShopData.Gems, 5 },
        new[] { ShopData.Potions, 100, ShopData.Hammers, 50, ShopData.Gold, 5000, ShopData.Gems, 5 },
        new[] { ShopData.Tickets, 220, ShopData.Hammers, 100, ShopData.Hammers, 500, ShopData.Gems, 5 },
        new[] { ShopData.Potions, 100, ShopData.Hammers, 50, ShopData.Winders, 100, ShopData.Gems, 5 },
        new[] { ShopData.Tickets, 220, ShopData.Shells, 150, ShopData.Gold, 5000, ShopData.Gems, 5 },
        new[] { ShopData.Potions, 100, ShopData.Hammers, 50, ShopData.Winders, 100, ShopData.Gems, 5 },
        new[] { ShopData.Tickets, 220, ShopData.Shells, 150, ShopData.Hammers, 500, ShopData.Gems, 5 },
        new[] { ShopData.Potions, 100, ShopData.Hammers, 100, ShopData.Gold, 5000, ShopData.Gems, 5 },
        new[] { ShopData.Tickets, 220, ShopData.Shells, 150, ShopData.Winders, 100, ShopData.Gems, 5 },
        new[] { ShopData.Tickets, 220, ShopData.Shells, 150, ShopData.Potions, 400, ShopData.Gems, 5 },
        new[] { ShopData.Winders, 40, ShopData.Hammers, 100, ShopData.Shells, 200, ShopData.Gems, 5 },
        new[] { ShopData.Tickets, 220, ShopData.Shells, 150, ShopData.Hammers, 500, ShopData.Gems, 5 },
        new[] { ShopData.Tickets, 220, ShopData.Shells, 150, ShopData.Potions, 400, ShopData.Gems, 5 },
        new[] { ShopData.Tickets, 220, ShopData.Hammers, 100, ShopData.Potions, 400, ShopData.Gems, 5 },
        new[] { ShopData.Shells, 200, ShopData.Hammers, 100, ShopData.Tickets, 220, ShopData.Gems, 5 },
        new[] { ShopData.Potions, 200, ShopData.Hammers, 100, ShopData.Hammers, 500, ShopData.Gems, 5 },
        new[] { ShopData.Gems, 5, ShopData.Hammers, 100, ShopData.Winders, 200, ShopData.Gems, 5 },
        new[] { ShopData.Winders, 300, ShopData.Hammers, 100, ShopData.Gold, 10000, ShopData.Gems, 5 },
        new[] { ShopData.Tickets, 220, ShopData.Hammers, 100, ShopData.Tickets, 220, ShopData.Gems, 5 },
        new[] { ShopData.Gold, 10000, ShopData.Hammers, 100, ShopData.Potions, 400, ShopData.Gems, 5 },
        new[] { ShopData.Potions, 200, ShopData.Hammers, 100, ShopData.Winders, 400, ShopData.Gems, 5 },
        new[] { ShopData.Gold, 10000, ShopData.Hammers, 100, ShopData.Winders, 500, ShopData.Gems, 5 },
        new[] { ShopData.Tickets, 220, ShopData.Hammers, 100, ShopData.Tickets, 220, ShopData.Gems, 5 },
    };
    // Paliers de niveau des missions : niveau du Voleur de marteau requis, niveaux de mission proposés.
    public static readonly int[] LevelMinThief = {0,7,8,10,12,14,16,19,23,27,32,36,40,45,50,61,71,82,93,104,115,126,136,144,151,158,165,173,179,187,194,201,208,215,222,229,236,243,251,258,265,274,249,286,293,300,306,314,322,329,336,343,350,357,364,371,378,386,392,399};
    public static readonly int[] LevelMin = {1,1,1,1,2,3,3,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49,50,51,52,52,52,53};
    public static readonly int[] LevelMax = {4,5,6,7,7,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49,50,51,52,53,54,55,56,57,58,59,60,60};
    // Récompenses par niveau de mission (1 à 60).
    public static readonly int[] RewardGold = {5500,5555,5611,5667,5723,5781,5838,5897,5956,6015,6075,6136,6198,6260,6322,6385,6449,6514,6579,6645,6711,6778,6846,6914,6984,7053,7124,7195,7267,7340,7413,7487,7562,7638,7714,7791,7869,7948,8027,8108,8189,8271,8353,8437,8521,8606,8693,8779,8867,8956,9045,9136,9227,9320,9413,9507,9602,9698,9795,9893};
    public static readonly int[] RewardTickets = {184,186,188,190,191,193,195,197,199,201,203,205,207,209,212,214,216,218,220,222,225,227,229,231,234,236,238,241,243,246,248,250,253,256,258,261,263,266,269,271,274,277,279,282,285,288,291,294,297,300,303,306,309,312,315,318,321,324,328,331};
    public static readonly int[] RewardShells = {91,92,93,94,95,96,97,98,99,100,101,102,103,104,105,106,107,108,109,110,111,112,113,114,116,117,118,119,120,121,123,124,125,126,128,129,130,132,133,134,135,137,138,140,141,142,144,145,147,148,150,151,153,154,156,157,159,160,162,164};
    public static readonly int[] RewardPotions = {92,93,94,95,96,97,98,99,100,101,102,103,104,105,106,107,108,109,110,111,112,113,115,116,117,118,119,120,122,123,124,125,126,128,129,130,132,133,134,136,137,138,140,141,143,144,145,147,148,150,151,153,154,156,157,159,161,162,164,165};
    public static readonly int[] RewardWinders = {69,70,70,71,72,73,73,74,75,75,76,77,78,79,79,80,81,82,83,83,84,85,86,87,88,88,89,90,91,92,93,94,95,96,97,98,99,100,101,102,103,104,105,106,107,108,109,110,111,112,113,115,116,117,118,119,120,122,123,124};
    // Escouades : niveau minimum, nombre d'ennemis, dégâts et vie de base par ennemi.
    public static readonly int[] SquadMinLevel = {1,1,1,1,1,5,10,15,15,20,20,25,25,30,30,35,35,40,40,40};
    public static readonly int[] SquadUnits = {6,6,2,5,8,3,5,6,5,9,6,1,7,4,6,8,2,20,4,6};
    public static readonly int[] SquadDamage = {1000,1000,3000,1200,750,2000,1200,1000,1200,667,1000,4000,857,1500,1000,750,3000,300,1500,1000};
    public static readonly int[] SquadHealth = {8000,8000,24000,9600,6000,16000,9600,8000,9600,5336,8000,32000,6856,12000,8000,6000,24000,2400,12000,8000};
}
