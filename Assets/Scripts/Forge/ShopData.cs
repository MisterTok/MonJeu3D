// Boutique : cadeau du jour, 3 offres du jour (DailyDealLibrary du jeu de référence) et ressources à l'unité.
// Tout se paie en gemmes gagnées en jeu (pas d'achat réel pour l'instant).
public static class ShopData
{
    // Monnaies que la boutique sait donner.
    public const int Gold = 0, Gems = 1, Hammers = 2, Shells = 3, Winders = 4, Tickets = 5, Potions = 6,
        KeyHammer = 7, KeyEgg = 8, KeyPotion = 9, KeySkill = 10;

    public static readonly string[] CurrencyNames =
        { "or", "gemmes", "marteaux", "coquilles", "remontoirs", "tickets", "potions rouges",
          "clé Voleur de marteau", "clé Couvoir infernal", "clé Chaudron des potions", "clé Crypte des grimoires" };
    public static readonly UnityEngine.Color[] CurrencyColors =
    {
        new UnityEngine.Color(1f, 0.82f, 0.3f), new UnityEngine.Color(0.5f, 0.9f, 1f), new UnityEngine.Color(0.9f, 0.9f, 0.9f),
        new UnityEngine.Color(0.95f, 0.9f, 0.75f), new UnityEngine.Color(0.8f, 0.55f, 1f), new UnityEngine.Color(0.5f, 1f, 0.55f),
        new UnityEngine.Color(1f, 0.4f, 0.4f), new UnityEngine.Color(1f, 0.75f, 0.3f), new UnityEngine.Color(0.85f, 0.85f, 0.7f),
        new UnityEngine.Color(1f, 0.35f, 0.3f), new UnityEngine.Color(0.45f, 0.95f, 0.5f),
    };

    // L'or de référence (1 000 à 150 000) est converti en heures de gains hors ligne : 1 000 pièces = 1 heure.
    public const double GoldRefPerHour = 1000;

    public class Deal
    {
        public string name; public UnityEngine.Color color; public int[][] sizes; // sizes[taille] = { monnaie, quantité, monnaie, quantité, ... }
        public Deal(string n, UnityEngine.Color c, params int[][] s) { name = n; color = c; sizes = s; }
    }

    static UnityEngine.Color C(float r, float g, float b) => new UnityEngine.Color(r, g, b);

    // 4 tailles par offre (la taille dépend du cercle atteint).
    public static readonly Deal[] Deals =
    {
        new Deal("Trousseau du geôlier", C(0.95f, 0.75f, 0.25f),
            new[] { KeyHammer, 1, KeySkill, 1, KeyEgg, 1, KeyPotion, 1, Winders, 60 },
            new[] { KeyHammer, 1, KeySkill, 1, KeyEgg, 1, KeyPotion, 1, Winders, 125 },
            new[] { KeyHammer, 2, KeySkill, 2, KeyEgg, 2, KeyPotion, 2, Winders, 250 },
            new[] { KeyHammer, 5, KeySkill, 5, KeyEgg, 5, KeyPotion, 5, Winders, 625 }),
        new Deal("Butin des Enfers", C(1f, 0.55f, 0.2f),
            new[] { Gold, 1000, Tickets, 200, Potions, 50, Shells, 150, Hammers, 50, Winders, 62 },
            new[] { Gold, 20000, Tickets, 400, Potions, 100, Shells, 170, Hammers, 400, Winders, 125 },
            new[] { Gold, 50000, Tickets, 1500, Potions, 500, Shells, 500, Hammers, 500, Winders, 250 },
            new[] { Gold, 150000, Tickets, 2500, Potions, 1400, Shells, 1150, Hammers, 2500, Winders, 625 }),
        new Deal("Nid de la couveuse", C(0.85f, 0.85f, 0.7f),
            new[] { Shells, 165, Hammers, 100, Gems, 10 },
            new[] { Shells, 660, Hammers, 200, Gems, 20 },
            new[] { Shells, 1650, Hammers, 2000, Gems, 80 },
            new[] { Shells, 4150, Hammers, 4500, Gems, 170 }),
        new Deal("Alambic du savant", C(1f, 0.35f, 0.35f),
            new[] { Potions, 150, Hammers, 300, Gems, 10 },
            new[] { Potions, 600, Hammers, 1000, Gems, 30 },
            new[] { Potions, 1600, Hammers, 2000, Gems, 150 },
            new[] { Potions, 4500, Hammers, 4000, Gems, 200 }),
        new Deal("Grimoire interdit", C(0.45f, 0.95f, 0.5f),
            new[] { Tickets, 400, Hammers, 100, Gems, 10 },
            new[] { Tickets, 1200, Hammers, 300, Gems, 30 },
            new[] { Tickets, 3000, Hammers, 2000, Gems, 90 },
            new[] { Tickets, 9000, Hammers, 3500, Gems, 150 }),
        new Deal("Écurie infernale", C(0.8f, 0.55f, 1f),
            new[] { Winders, 90, Hammers, 100, Gems, 10 },
            new[] { Winders, 320, Hammers, 500, Gems, 20 },
            new[] { Winders, 940, Hammers, 1000, Gems, 40 },
            new[] { Winders, 2150, Hammers, 4000, Gems, 150 }),
    };
    // Prix en gemmes selon la taille (≈ prix réel du jeu de référence × 30 gemmes par euro).
    public static readonly int[] DealPrice = { 60, 200, 550, 1400 };
    public const int DealsPerDay = 3;

    // Taille des offres selon le meilleur cercle atteint.
    public static int DealSize(int bestCircle) => bestCircle <= 2 ? 0 : bestCircle <= 5 ? 1 : bestCircle <= 8 ? 2 : 3;

    // Cadeau du jour gratuit.
    public static readonly int[] FreeGift = { Gems, 10, Hammers, 30, Shells, 50 };

    // Ressources à l'unité : monnaie, quantité, prix en gemmes, achats par jour.
    public class Bundle
    {
        public int currency, amount, price, perDay;
        public Bundle(int c, int a, int p, int d) { currency = c; amount = a; price = p; perDay = d; }
    }
    public static readonly Bundle[] Bundles =
    {
        new Bundle(Hammers, 100, 30, 5),
        new Bundle(Gold, 2000, 40, 3),     // or : converti en heures de gains hors ligne (2 h)
        new Bundle(Shells, 300, 60, 3),
        new Bundle(Winders, 150, 60, 3),
        new Bundle(Tickets, 200, 50, 3),
        new Bundle(Potions, 100, 50, 3),
    };

    // Packs de gemmes (achats réels) : affichés pour plus tard, non achetables.
    public static readonly int[] GemPacks = { 60, 220, 800, 3300 };
    public static readonly string[] GemPackPrices = { "1,99 €", "6,99 €", "24,99 €", "99,99 €" };
}
