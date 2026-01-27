# EasySave - Logiciel de Sauvegarde ProSoft

Bienvenue sur le dépôt officiel du projet **EasySave**.
Ce logiciel est développé dans le cadre du bloc "Programmation Système" (CESI - A3 FISA INFO).

## L'Équipe de Développement
* **Yanis** (Project Lead / Dev)
* **Rayene** (Dev)
* **Fayçal** (Dev)
* **Maxime** (Dev)

## Description du Projet
EasySave est une solution de sauvegarde développée en C# .NET Core pour l'entreprise **ProSoft**.
Le projet suit une évolution en 3 phases majeures :
* **V1.0 :** Application Console, Sauvegardes séquentielles, Logs JSON.
* **V2.0 :** Interface Graphique (WPF/MVVM), Chiffrement (CryptoSoft), Logs XML/JSON.
* **V3.0 :** Sauvegardes parallèles, Priorisation des tâches, Centralisation des logs.

## Stack Technique
* **Langage :** C#
* **Framework :** .NET 8.0
* **IDE :** Visual Studio 2022 (ou supérieur)
* **Gestion de version :** Git & GitHub

## Architecture de la Solution
La solution est divisée en plusieurs projets pour respecter les principes de séparation des responsabilités :
1.  **EasySave (Console App) :** Point d'entrée de l'application, gestion des menus et de l'exécution.
2.  **EasyLog (Class Library / DLL) :** Gestion indépendante des logs (JSON/XML) et des états en temps réel.

## Règles de Développement (Workflow)
Pour garantir la qualité du code et éviter les conflits, nous respectons les règles suivantes :

1.  **Branching :**
    * `main` : Version stable et livrable (ne jamais toucher directement).
    * `develop` : Version de développement commune.
    * `feature/nom-fonctionnalité` : Branche de travail pour chaque tâche (ex: `feature/logs-json`, `feature/interface`).

2.  **Convention :**
    * Pas de noms de personnes dans les branches.
    * Code et commentaires en Anglais (ou Français selon accord interne).
    * Pas de code mort ni de duplication (DRY).

## Installation & Lancement
1.  Cloner le dépôt : `git clone https://github.com/ProSoft-EasySave/EasySave.git`
2.  Ouvrir le fichier `.sln` dans Visual Studio.
3.  Vérifier que le projet de démarrage est bien **EasySave**.
4.  Compiler et lancer (F5).