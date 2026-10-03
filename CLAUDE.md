# MonJeu3D — « La Forge de l'Enfer » (clone thématique de Forge Master)

Notes de passation pour Claude. À lire en premier dans toute nouvelle conversation.

## Contexte
- Joueur/dev : Pierre (francophone). Travaille surtout depuis son téléphone (Sunshine/Moonlight vers le PC). Veut faire le moins de manipulations possible : Claude code, compile, teste et vérifie lui-même.
- Jeu mobile Unity 6.3 (6000.3.17f1), URP, Android, portrait. Projet : C:\jeu\MonJeu3D. Dépôt GitHub : MisterTok/MonJeu3D (build Android par GitHub Actions + game-ci).
- Thème : forge de l'enfer, démons, 10 « cercles » (= les 10 âges de Forge Master : Limbes, Luxure, Gourmandise, Avarice, Colère, Hérésie, Violence, Fraude, Trahison, Lucifer).
- Disposition d'écran calquée sur Forge Master : barre du haut (puissance, or, gemmes), grand chemin de combat 3/4, 8 tuiles d'équipement, rangée forge (chances + Auto à gauche, enclume 3D tactile au centre, niveau de forge à droite), barre de navigation : Héros, Donjons, Compagnons, Montures, Techno, Clan (à venir), Boutique (à venir).

## Données de référence
- C:\jeu\_ref\fm_configs : 70 JSON de configuration du jeu original (dépôt GitHub Tymotey/fm, version 2026-05-15). Source principale pour l'équilibrage.
- Tableur Forge_Master.xlsx fourni par Pierre (forge, invocations, tech…). Ses captures (déc. 2025) priment quand elles diffèrent (coûts tech : 40/56/78…, compagnons, montures en %).
- Garder nos noms/visuels propres (pas de copie conforme).

## Code (Assets/Scripts/Forge) — tout est construit par code au lancement (ForgeGame.Boot, RuntimeInitializeOnLoad)
- GameState.cs : sauvegarde (PlayerPrefs JSON), économie, forge, objets + stats secondaires, compagnons/œufs, montures, donjons, arbre techno (TV(type)), forge auto, gains hors ligne, combat (stats ennemis).
- ForgeData.cs (forge 35 niveaux), ProgressionData.cs (tech coûts/durées, compagnons, œufs/invocation, montures, stats secondaires), TechData.cs (235 nœuds générés), TechText.cs (noms FR).
- ForgeWorld.cs (scène forge 3D, enclume procédurale, révélation de pièce, matériaux/ textures), BattleWorld.cs (chemin de combat, vagues, boss, donjons, compagnons qui suivent, monture chevauchée), ModelLib.cs (chargement FBX + AnimPlayer legacy), ProcGen.cs (textures sol/pavés/lave, maillage enclume).
- ForgeUI.cs + partials ForgeUI.Companions/Dungeons/Tech/Mounts.cs (interface uGUI par code, police LegacyRuntime).
- Editor : ForgeMaterialsSetup (matériaux URP dans Resources/ForgeMats), ForgeModelPostprocessor (FBX Characters/Pets en Legacy), ForgeModelReport.
- Modèles : Resources/ForgeModels (Quaternius CC0, via le dépôt GitHub beep2bleep/FreeAssetsByKenneyNLandQuaternius).

## Fonctionnalités faites
Forge (35 niv., nœuds, minuteur, gemmes), objets 8 emplacements avec vrais modèles (Lame, Heaume, Bouclier), stats secondaires (0/1/2 selon cercle), combat auto 10 cercles × 10 étapes (boss à l'étape 10), compagnons (coquilles → invocation d'œufs → couveuses → éclosion, 25 compagnons, 3 équipés, PetScale 0,12), donjons (4, 2 clés/jour recharge à 22:00, clé consommée seulement en cas de victoire), arbre techno (3 branches, potions rouges, 1 recherche à la fois), forge auto (débloquée par la techno), gains hors ligne, montures (remontoirs, 15 montures, bonus % dégâts/vie, héros chevauche), luminosité relevée.
- Doublons compagnons/montures : montée de niveau automatique (doublons requis = niveau actuel).
- Sources temporaires : boss vaincu la 1re fois → coquilles + remontoirs (100 + 50/cercle).

## À faire ensuite
Compétences (tickets déjà gagnés), boutique, icônes 3D dans les tuiles, ligue classée et guerre de clans (serveur en ligne nécessaire), équilibrage, modèles pour Gantelets/Bottes/Ceinture/Amulette/Anneau.

## Méthode de travail de Claude (important)
- Écrire les fichiers dans le cloud puis device_commit_files (force), et TOUJOURS vérifier le md5 sur le PC (un envoi a déjà été perdu).
- Faire compiler : donner le focus à Unity puis menu Assets > Refresh. Le clavier tactile Windows (textinputhost) passe souvent devant : demander l'accès computer-use à Unity ET à textinputhost.exe, puis cliquer dans Unity. Ne PAS utiliser open_application (relance un 2e Unity qui écrase Editor.log).
- Vérifier la compilation : date de Library/ScriptAssemblies/Assembly-CSharp.dll + `strings Editor.log | grep "error CS"`.
- Tester : Play, puis écrire dans C:\jeu\_dl\shot_request un nom → capture PNG plein écran dans C:\jeu\_dl\ (commandes de test : « forge », « item6 », « discard »). Stager le PNG pour le regarder.
