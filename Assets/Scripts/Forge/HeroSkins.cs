using System.Collections.Generic;

// Skins payants du héros, façon Forge Master : dans chaque pack Craftpix, le personnage le moins beau sert à l'équipement
// (gratuit, CraftpixHero) et les deux plus beaux sont des skins qui remplacent l'apparence du héros et donnent un bonus.
// Prix en gemmes en attendant les achats réels (Google Play Billing).
public static class HeroSkins
{
    public class Def
    {
        public string id, name, pack; public int rarity; public int price;
        public Def(string id, string name, string pack, int rarity) { this.id = id; this.name = name; this.pack = pack; this.rarity = rarity; price = rarity >= 5 ? 1500 : 1000; }
    }

    // Bonus de dégâts et de vie du skin porté (les skins de FM donnent 2 à 12 %).
    public static float Bonus(int rarity) => rarity >= 5 ? 0.08f : 0.05f;

    public static readonly Def[] All =
    {
        new Def("Valkyrie_1", "Valkyrie d'acier", "Valkyries", 4),           new Def("Valkyrie_2", "Valkyrie ailée", "Valkyries", 5),
        new Def("Orc", "Orc brutal", "Peaux-vertes", 4),                       new Def("Ogre", "Ogre glouton", "Peaux-vertes", 5),
        new Def("Golem_1", "Golem de glace", "Golems", 4),                     new Def("Golem_3", "Golem de lave", "Golems", 5),
        new Def("Minotaur_2", "Minotaure sauvage", "Minotaures", 4),           new Def("Minotaur_3", "Minotaure cuirassé", "Minotaures", 5),
        new Def("Skeleton_Warrior_2", "Squelette casqué", "Squelettes", 4),    new Def("Skeleton_Warrior_3", "Squelette viking", "Squelettes", 5),
        new Def("Skeleton_Crusader_1", "Croisé maudit", "Croisés", 4),         new Def("Skeleton_Crusader_3", "Croisé d'os", "Croisés", 5),
        new Def("Reaper_Man_2", "Faucheur des marais", "Faucheurs", 4),       new Def("Reaper_Man_1", "Grand Faucheur", "Faucheurs", 5),
        new Def("Fallen_Angels_1", "Ange déchu", "Anges déchus", 4),           new Def("Fallen_Angels_3", "Roi déchu", "Anges déchus", 5),
        new Def("Forest_Ranger_1", "Rôdeur", "Rôdeurs", 4),                    new Def("Forest_Ranger_3", "Chasseur cornu", "Rôdeurs", 5),
        new Def("Seer_1", "Voyant au turban", "Voyants", 4),                   new Def("Seer_3", "Voyant masqué", "Voyants", 5),
        new Def("Zombie_Villager_3", "Zombie recousu", "Zombies", 4),          new Def("Zombie_Villager_2", "Zombie masqué", "Zombies", 5),
        new Def("Dark_Oracle_1", "Oracle de braise", "Oracles", 4),            new Def("Dark_Oracle_3", "Oracle des étoiles", "Oracles", 5),
        new Def("Necromancer_of_the_Shadow_3", "Nécromancienne pourpre", "Nécromanciennes", 4), new Def("Necromancer_of_the_Shadow_1", "Nécromancienne de givre", "Nécromanciennes", 5),
        new Def("Bloody_Alchemist_2", "Alchimiste écarlate", "Alchimistes", 4), new Def("Bloody_Alchemist_1", "Alchimiste fou", "Alchimistes", 5),
        new Def("Satyr_02", "Satyre des bois", "Satyres", 4),                  new Def("Satyr_01", "Satyre doré", "Satyres", 5),
        new Def("Wraith_03", "Spectre d'ombre", "Spectres", 4),                new Def("Wraith_01", "Spectre de flammes", "Spectres", 5),
        new Def("Golem_03", "Golem d'argile", "Golems de pierre", 4),          new Def("Golem_01", "Golem de granit", "Golems de pierre", 5),
        new Def("Minotaur_01", "Minotaure roux", "Minotaures des plaines", 4), new Def("Minotaur_02", "Minotaure à cornes d'acier", "Minotaures des plaines", 5),
    };

    static Dictionary<string, Def> byId;
    public static Def Find(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        if (byId == null) { byId = new Dictionary<string, Def>(); foreach (var d in All) byId[d.id] = d; }
        return byId.TryGetValue(id, out var r) ? r : null;
    }

    public static string TestView;   // outil de test (« skinview:<id> ») : aperçu non sauvegardé
    // Skin affiché sur le héros : celui porté s'il est possédé (ou l'aperçu de test).
    public static string Shown => !string.IsNullOrEmpty(TestView) ? TestView : Owned(GameState.Data.skinEquipped) ? GameState.Data.skinEquipped : "";

    public static bool Owned(string id) => GameState.Data.skinsOwned != null && GameState.Data.skinsOwned.Contains(id);
    public static float EquippedBonus { get { var d = Find(GameState.Data.skinEquipped); return d != null && Owned(d.id) ? Bonus(d.rarity) : 0f; } }

    // Achat (gemmes) ; renvoie un message d'erreur ou null.
    public static string Buy(string id)
    {
        var d = Find(id);
        if (d == null) return "Skin inconnu";
        if (Owned(id)) return "Déjà possédé";
        if (GameState.Data.gems < d.price) return "Pas assez de gemmes";
        GameState.Data.gems -= d.price;
        GameState.Data.skinsOwned.Add(id);
        GameState.Data.skinEquipped = id;
        GameState.Save();
        GameState.AddEggshells(0);   // prévient l'interface (GameState.Changed)
        return null;
    }

    public static void Toggle(string id)
    {
        if (!Owned(id)) return;
        GameState.Data.skinEquipped = GameState.Data.skinEquipped == id ? "" : id;
        GameState.Save();
        GameState.AddEggshells(0);
    }
}
