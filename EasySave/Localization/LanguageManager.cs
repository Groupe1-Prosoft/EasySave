using System;
using System.Collections.Generic;

namespace EasySave.Localization
{
    /// <summary>
    /// Provides a minimal EN/FR localization layer for console UI texts.
    /// </summary>
    public sealed class LanguageManager
    {
        /// <summary>
        /// Active language code (e.g., "en" or "fr").
        /// </summary>
        private string _currentLanguage;

        /// <summary>
        /// Active translation map for the current language.
        /// </summary>
        private Dictionary<string, string> _translations;

        /// <summary>
        /// English translations keyed by UI token.
        /// </summary>
        private static readonly Dictionary<string, string> EnglishTranslations = new()
        {
            ["Title"] = "EasySave 1.0",
            ["SelectLanguage"] = "Select language: 1. English  2. Français",
            ["InvalidLanguage"] = "Invalid selection. Defaulting to English.",
            ["SelectOption"] = "Select an option:",
            ["ListJobs"] = "1. List backup jobs",
            ["CreateJob"] = "2. Create a backup job",
            ["ExecuteJob"] = "3. Execute a backup job",
            ["ExecuteAllJobs"] = "4. Execute all backup jobs",
            ["DeleteJob"] = "5. Delete a backup job",
            ["Options"] = "6. Options",
            ["Exit"] = "7. Exit",
            ["Separator"] = "-----------------------------------",
            ["YourChoice"] = "Your choice: ",
            ["InvalidOption"] = "Invalid option. Please try again.",
            ["Goodbye"] = "Goodbye!",
            ["ListHeader"] = "--- List of Backup Jobs ---",
            ["NoJobs"] = "No jobs configured yet.",
            ["CreateHeader"] = "--- Create a New Backup Job ---",
            ["EnterJobName"] = "Enter Job Name: ",
            ["EnterSourcePath"] = "Enter Source Path: ",
            ["EnterTargetPath"] = "Enter Target Path: ",
            ["SelectType"] = "Select Type (1. Full / 2. Differential): ",
            ["JobCreated"] = "Job created & saved successfully!",
            ["JobLimitReached"] = "Error: Could not add job.",
            ["ExecuteHeader"] = "--- Execute a Backup Job ---",
            ["DeleteHeader"] = "--- Delete a Backup Job ---",
            ["EnterJobNumber"] = "Enter job number: ",
            ["JobNotFound"] = "Job not found.",
            ["JobDeleted"] = "Job deleted successfully.",
            ["BackupFinished"] = "Backup finished!",
            ["PressEnterReturn"] = "Press Enter to return to menu...",
            ["ConfirmDelete"] = "Are you sure you want to delete this job? (y/n): ",
            ["ExecutingJob"] = "Executing: {0}",
            ["JobExecuted"] = "Job {0} completed.",
            ["ValidationError"] = "Validation error: {0}",
            ["SourceNotFound"] = "Source directory not found: {0}",
            ["Processing"] = "Processing: {0} ({1} files)...",
            ["FileCopied"] = " -> {0} copied.",
            ["CopyError"] = "Copy error: {0}",
            ["ExecutionControls"] = "Controls: [P] Pause/Resume, [S] Stop",
            ["BackupPaused"] = "Backup paused.",
            ["BackupResumed"] = "Backup resumed.",
            ["BackupStopped"] = "Backup stopped.",
            ["BackupFailed"] = "Backup failed.",
            ["BtnPause"] = "Pause",
            ["BtnResume"] = "Resume",
            ["BtnStop"] = "Stop",

            ["NavHome"] = "Home",
            ["NavLogs"] = "Logs",
            ["NavSettings"] = "Settings",
            ["HomeTitle"] = "Backup Jobs",
            ["HomeNewJob"] = "Create New Job",
            ["HomeJobList"] = "Job List",
            ["HomeNoJobs"] = "No backup jobs configured yet.",
            ["LabelName"] = "Name",
            ["LabelType"] = "Type",
            ["LabelSource"] = "Source",
            ["LabelTarget"] = "Target",
            ["TypeFull"] = "Full",
            ["TypeDifferential"] = "Differential",
            ["BtnCreate"] = "Create",
            ["BtnBrowse"] = "Browse",
            ["BtnExecuteAll"] = "Execute All",
            ["BtnRefresh"] = "Refresh",
            ["BtnSave"] = "Save",
            ["JobSuccess"] = "Operation completed successfully!",
            ["JobError"] = "Operation failed.",
            ["JobExecuting"] = "Executing...",
            ["SettingsTitle"] = "Settings",
            ["SettingsCryptoPath"] = "CryptoSoft Path",
            ["SettingsExtensions"] = "Extensions to Encrypt",
            ["SettingsBusinessSoft"] = "Business Software Name",
            ["SettingsLogFormat"] = "Log Format",
            ["SettingsSaved"] = "Settings saved!",
            ["LogsTitle"] = "Daily Logs",
            ["LogsNoLogs"] = "No logs found.",
        };

        /// <summary>
        /// French translations keyed by UI token.
        /// </summary>
        private static readonly Dictionary<string, string> FrenchTranslations = new()
        {
            ["Title"] = "EasySave 1.0",
            ["SelectLanguage"] = "Choisissez la langue : 1. Anglais  2. Français",
            ["InvalidLanguage"] = "Sélection invalide. Anglais choisi par défaut.",
            ["SelectOption"] = "Sélectionnez une option :",
            ["ListJobs"] = "1. Lister les tâches de sauvegarde",
            ["CreateJob"] = "2. Créer une tâche de sauvegarde",
            ["ExecuteJob"] = "3. Exécuter une tâche de sauvegarde",
            ["ExecuteAllJobs"] = "4. Exécuter toutes les tâches de sauvegarde",
            ["DeleteJob"] = "5. Supprimer une tâche de sauvegarde",
            ["Options"] = "6. Paramètres",
            ["Exit"] = "7. Quitter",
            ["Separator"] = "-----------------------------------",
            ["YourChoice"] = "Votre choix : ",
            ["InvalidOption"] = "Option invalide. Veuillez réessayer.",
            ["Goodbye"] = "Au revoir !",
            ["ListHeader"] = "--- Liste des tâches de sauvegarde ---",
            ["NoJobs"] = "Aucune tâche configurée pour l'instant.",
            ["CreateHeader"] = "--- Créer une nouvelle tâche de sauvegarde ---",
            ["EnterJobName"] = "Entrez le nom de la tâche : ",
            ["EnterSourcePath"] = "Entrez le chemin source : ",
            ["EnterTargetPath"] = "Entrez le chemin cible : ",
            ["SelectType"] = "Sélectionnez le type (1. Complète / 2. Différentielle) : ",
            ["JobCreated"] = "Tâche créée et sauvegardée avec succès !",
            ["JobLimitReached"] = "Erreur : Impossible d'ajouter la tâche.",
            ["ExecuteHeader"] = "--- Exécuter une tâche de sauvegarde ---",
            ["DeleteHeader"] = "--- Supprimer une tâche de sauvegarde ---",
            ["EnterJobNumber"] = "Entrez le numéro de la tâche : ",
            ["JobNotFound"] = "Tâche non trouvée.",
            ["JobDeleted"] = "Tâche supprimée avec succès.",
            ["BackupFinished"] = "Sauvegarde terminée !",
            ["PressEnterReturn"] = "Appuyez sur Entrée pour revenir au menu...",
            ["ConfirmDelete"] = "Êtes-vous sûr de vouloir supprimer cette tâche ? (o/n) : ",
            ["ExecutingJob"] = "Exécution : {0}",
            ["JobExecuted"] = "Tâche {0} terminée.",
            ["ValidationError"] = "Erreur de validation : {0}",
            ["SourceNotFound"] = "Répertoire source introuvable : {0}",
            ["Processing"] = "Traitement de : {0} ({1} fichiers)...",
            ["FileCopied"] = " -> {0} copié.",
            ["CopyError"] = "Erreur copie : {0}",
            ["ExecutionControls"] = "Commandes : [P] Pause/Reprise, [S] Stop",
            ["BackupPaused"] = "Sauvegarde en pause.",
            ["BackupResumed"] = "Sauvegarde reprise.",
            ["BackupStopped"] = "Sauvegarde stoppée.",
            ["BackupFailed"] = "Échec de la sauvegarde.",
            ["BtnPause"] = "Pause",
            ["BtnResume"] = "Reprendre",
            ["BtnStop"] = "Stop",

            ["NavHome"] = "Accueil",
            ["NavLogs"] = "Logs",
            ["NavSettings"] = "Paramètres",
            ["HomeTitle"] = "Tâches de sauvegarde",
            ["HomeNewJob"] = "Créer une nouvelle tâche",
            ["HomeJobList"] = "Liste des tâches",
            ["HomeNoJobs"] = "Aucune tâche de sauvegarde configurée.",
            ["LabelName"] = "Nom",
            ["LabelType"] = "Type",
            ["LabelSource"] = "Source",
            ["LabelTarget"] = "Destination",
            ["TypeFull"] = "Complète",
            ["TypeDifferential"] = "Différentielle",
            ["BtnCreate"] = "Créer",
            ["BtnBrowse"] = "Parcourir",
            ["BtnExecuteAll"] = "Tout exécuter",
            ["BtnRefresh"] = "Actualiser",
            ["BtnSave"] = "Enregistrer",
            ["JobSuccess"] = "Opération terminée avec succès !",
            ["JobError"] = "Échec de l'opération.",
            ["JobExecuting"] = "Exécution en cours...",
            ["SettingsTitle"] = "Paramètres",
            ["SettingsCryptoPath"] = "Chemin CryptoSoft",
            ["SettingsExtensions"] = "Extensions à crypter",
            ["SettingsBusinessSoft"] = "Nom du logiciel métier",
            ["SettingsLogFormat"] = "Format des logs",
            ["SettingsSaved"] = "Paramètres enregistrés !",
            ["LogsTitle"] = "Logs journaliers",
            ["LogsNoLogs"] = "Aucun log trouvé.",
        };


        private static readonly Lazy<LanguageManager> _instance = new(() => new LanguageManager());

        /// <summary>
        /// Lock object used to synchronise all read/write access to mutable state.
        /// </summary>
        private readonly object _lock = new();

        /// <summary>
        /// Gets the single shared instance of LanguageManager.
        /// </summary>
        public static LanguageManager Instance => _instance.Value;

        /// <summary>
        /// Gets the current language code (thread-safe).
        /// </summary>
        public string CurrentLanguage
        {
            get { lock (_lock) { return _currentLanguage; } }
        }

        /// <summary>
        /// Private constructor — use Instance instead.
        /// </summary>
        private LanguageManager()
        {
            _currentLanguage = "en";
            _translations = EnglishTranslations;
        }

        /// <summary>
        /// Sets the active language and refreshes translations.
        /// </summary>
        public void SetLanguage(string lang)
        {
            lock (_lock)
            {
                _currentLanguage = lang.ToLower();
                LoadTranslationsUnsafe();
            }
        }

        /// <summary>
        /// Returns the localized value for a key, or the key itself if missing.
        /// </summary>
        public string GetText(string key)
        {
            lock (_lock)
            {
                if (_translations.TryGetValue(key, out string? value))
                    return value;
                return key;
            }
        }

        /// <summary>
        /// Returns a localized value formatted with parameters.
        /// </summary>
        public string GetText(string key, params object[] args)
        {
            string text = GetText(key);
            return string.Format(text, args);
        }

        /// <summary>
        /// Loads the translation dictionary for the current language.
        /// Must be called from within a lock(_lock) block.
        /// </summary>
        public bool LoadTranslations()
        {
            lock (_lock)
            {
                LoadTranslationsUnsafe();
            }
            return true;
        }

        /// <summary>
        /// Inner helper — swaps the dictionary reference.
        /// Caller is responsible for holding _lock before calling this.
        /// </summary>
        private void LoadTranslationsUnsafe()
        {
            _translations = _currentLanguage switch
            {
                "fr" => FrenchTranslations,
                _ => EnglishTranslations
            };
        }
    }
}