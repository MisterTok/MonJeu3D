# MonJeu3D — « La Forge de l'Enfer » (clone thématique de Forge Master)

Notes de passation pour Claude. À lire en premier dans toute nouvelle conversation.

## Contexte
- Joueur/dev : Pierre (francophone). Travaille surtout depuis son téléphone (Sunshine/Moonlight vers le PC). Veut faire le moins de manipulations possible : Claude code, compile, teste et vérifie lui-même.
- Jeu mobile Unity 6.3 (6000.3.17f1), URP, Android, portrait. Projet : C:\jeu\MonJeu3D. Dépôt GitHub : MisterTok/MonJeu3D (build Android par GitHub Actions + game-ci).
- Thème : forge de l'enfer, démons, 10 « cercles » (= les 10 âges de Forge Master : Limbes, Luxure, Gourmandise, Avarice, Colère, Hérésie, Violence, Fraude, Trahison, Lucifer).
- Disposition d'écran calquée sur Forge Master : barre du haut (puissance, or, gemmes), grand chemin de combat 3/4, 8 tuiles d'équipement, rangée forge (chances + Auto à gauche, enclume 3D tactile au centre, niveau de forge à droite), barre de navigation : Héros, Donjons, Compagnons, Montures, Compétences, Techno, Missions, Boutique ; bouton « Pass ★ » en haut à gauche du chemin.

## Données de référence
- C:\jeu\_ref\fm_configs : 70 JSON de configuration du jeu original (dépôt GitHub Tymotey/fm, version 2026-05-15). Source principale pour l'équilibrage.
- Tableur Forge_Master.xlsx fourni par Pierre (forge, invocations, tech…). Ses captures (déc. 2025) priment quand elles diffèrent (coûts tech : 40/56/78…, compagnons, montures en %).
- Garder nos noms/visuels propres (pas de copie conforme).

## Code (Assets/Scripts/Forge) — tout est construit par code au lancement (ForgeGame.Boot, RuntimeInitializeOnLoad)
- GameState.cs : sauvegarde (PlayerPrefs JSON), économie, forge, objets + stats secondaires, compagnons/œufs, montures, donjons, arbre techno (TV(type)), forge auto, gains hors ligne, combat (stats ennemis).
- ForgeData.cs (forge 35 niveaux), ProgressionData.cs (tech coûts/durées, compagnons, œufs/invocation, montures, stats secondaires), TechData.cs (235 nœuds générés), TechText.cs (noms FR).
- ForgeWorld.cs (scène forge 3D, enclume procédurale, révélation de pièce, matériaux/ textures), BattleWorld.cs (chemin de combat, vagues, boss, donjons, compagnons qui suivent, monture chevauchée), ModelLib.cs (chargement FBX + AnimPlayer legacy), ProcGen.cs (textures sol/pavés/lave, maillage enclume).
- SkillData.cs (18 compétences, invocation, doublons), SkillFx.cs (effets en particules : projectiles, explosions, auras, onde de choc).
- ShopData.cs (boutique : cadeau du jour, 6 offres du jour du jeu de référence en 4 tailles selon le cercle, ressources à l'unité, packs de gemmes affichés « bientôt »).
- ItemIcons.cs (icônes 3D des tuiles : le modèle est photographié une fois dans un studio caché à y=-300, caméra orthographique, RenderTexture 256 px fond transparent, cache par emplacement+cercle ; GetCreature(modèle, teinte) pour compagnons et montures, posés dans leur animation de repos ; BuildItemModel(item, glowScale) avec lueur réduite).
- MissionData.cs (missions et pass de progression, tables du jeu de référence).
- ForgeUI.Common.cs : style épuré façon Forge Master — Tile (icône + barre de doublons + « Niv. X » + coche équipé, cadre couleur de rareté, MakeTile/SetTile/SetEmptySlot), SummonBar compacte (monnaie, x1/x15, badge « Niv. (?) » qui ouvre les chances), fenêtre de détail ShowDetail/ShowInfo (toucher une tuile = fiche + bouton Équiper/Retirer). Règle : pas de phrases explicatives à l'écran, les infos détaillées vont dans la fiche. Fonds des panneaux opaques ; bouton Pass masqué quand un panneau est ouvert. Attention aux noms de champs : ForgeUI.Tech utilise déjà detailBtn/detailTitle…
- ForgeUI.cs + partials ForgeUI.Companions/Dungeons/Tech/Mounts/Skills/Shop/Missions.cs (interface uGUI par code, police LegacyRuntime).
- Editor : ForgeMaterialsSetup (matériaux URP dans Resources/ForgeMats), ForgeModelPostprocessor (FBX Characters/Pets en Legacy), ForgeModelReport.
- Modèles : Resources/ForgeModels (Quaternius CC0, via le dépôt GitHub beep2bleep/FreeAssetsByKenneyNLandQuaternius).

## Fonctionnalités faites
Forge (35 niv., nœuds, minuteur, gemmes), objets 8 emplacements avec vrais modèles (Lame, Heaume, Bouclier), stats secondaires (0/1/2 selon cercle), combat auto 10 cercles × 10 étapes (boss à l'étape 10), compagnons (coquilles → invocation d'œufs → couveuses → éclosion, 25 compagnons, 3 équipés, PetScale 0,12), donjons (4, 2 clés/jour recharge à 22:00, clé consommée seulement en cas de victoire), arbre techno (3 branches, potions rouges, 1 recherche à la fois), forge auto (débloquée par la techno), gains hors ligne, montures (remontoirs, 15 montures, bonus % dégâts/vie, héros chevauche), luminosité relevée.
- Compétences : tickets (Crypte des grimoires, 1er boss de chaque cercle 80 + 40/cercle, 200 offerts), invocation x5/x25 à 40 tickets (niveaux d'invocation du jeu de référence), 3 équipées qui se lancent seules (boutons ronds en bas à droite du chemin, touche = lancer si prête), bonus passif ATQ/PV de toute la collection, SkillScale 0,12. Types : Strike, Volley, Heal, Rage, Aura, Drone.
- Boutique : tout en gemmes de jeu (pas d'achat réel). Renouvellement à 22:00 comme les clés ; 3 offres tirées au hasard (graine = jour), 1 achat chacune, prix 60/200/550/1400 gemmes selon la taille ; l'or de référence est converti en heures de gains hors ligne (1 000 = 1 h). Ressources : limite d'achats par jour.
- Missions : dans l'original ce sont des missions de CLAN (pas des quêtes du jour, pas de gemmes). Adaptées en solo : 3 énergies/jour (22:00, consommées seulement en cas de victoire), 5 missions (nouvelle liste 10 gemmes), escouades de 1 à 20 ennemis (8 affichés max, regroupés au-delà), niveau 1-60 selon le Voleur de marteau (MissionLevelLibrary), difficulté = étape équivalente du Voleur qui débloque ce niveau. Récompenses MissionRewardLibrary (or ÷4 : 1 000 = 15 min hors ligne). Renforts de clan non faits (serveur).
- Pass de progression : 31 paliers (difficulté normale de l'original, âge a / combat b → étape (a-1)*10 + b/2), pistes gratuite et premium réunies (≈160 gemmes au total), or ÷4. Bouton « Tout récupérer ».
- Doublons compagnons/montures : montée de niveau automatique (doublons requis = niveau actuel).
- Sources temporaires : boss vaincu la 1re fois → coquilles + remontoirs (100 + 50/cercle).

## À faire ensuite
Achats réels (Google Play Billing) et plus de sources de gemmes, épurer de la même façon Donjons, Techno, Boutique, Missions et l'écran principal, ligue classée et guerre de clans (serveur en ligne nécessaire), équilibrage, modèles pour Gantelets/Bottes/Ceinture/Amulette/Anneau.

## Méthode de travail de Claude (important)
- Git : Claude peut committer depuis device_bash (git -c user.name=MisterTok -c user.email=piecassa35@gmail.com) mais PAS pousser (pas d'identifiants GitHub dans la VM, terminaux Windows en clic seulement) : demander à Pierre de faire `git push` ou d'utiliser GitHub Desktop. Les verrous .git demandent la permission de suppression sur le dossier du projet.
- Editor.log : C:\Users\pierr\AppData\Local\Unity\Editor (dossier à demander). Unity 6000.3.17f1 : résoudre l'accès computer-use avec le chemin complet de unity.exe (« Unity » seul pointe sur 6000.4.10f1).
- Écrire les fichiers dans le cloud puis device_commit_files (force), et TOUJOURS vérifier le md5 sur le PC (un envoi a déjà été perdu). Cause trouvée : réutiliser le même chemin dans /mnt/user-data/outputs/ peut renvoyer une ANCIENNE version mise en cache → copier à chaque envoi sous un nom/dossier nouveau (ex. outputs/v4/Fichier_v4.cs).
- Faire compiler : donner le focus à Unity puis menu Assets > Refresh. Le clavier tactile Windows (textinputhost) passe souvent devant : demander l'accès computer-use à Unity ET à textinputhost.exe, puis cliquer dans Unity. Ne PAS utiliser open_application (relance un 2e Unity qui écrase Editor.log).
- Vérifier la compilation : date de Library/ScriptAssemblies/Assembly-CSharp.dll + `strings Editor.log | grep "error CS"`.
- Tester : Play, puis écrire dans C:\jeu\_dl\shot_request un nom → capture PNG plein écran dans C:\jeu\_dl\ (commandes de test : « forge », « item6 », « discard », « tickets » = +5000 tickets, « equipall6 » = équipe les 8 emplacements au cercle 6 — écrase l'équipement de la sauvegarde). Stager le PNG pour le regarder.
