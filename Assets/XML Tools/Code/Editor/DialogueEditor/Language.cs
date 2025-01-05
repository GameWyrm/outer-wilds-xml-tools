using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Text.RegularExpressions;

namespace XmlTools
{
    public class Language : ScriptableObject
    {
        [SerializeField, HideInInspector]
        public bool hasParsedData;
        [SerializeField, HideInInspector]
        public string parsedData;
        [SerializeField, HideInInspector]
        public string parsedDialogue;
        [SerializeField, HideInInspector]
        public string parsedShipLogs;

        [HideInInspector]
        public LanguageType type;

        [HideInInspector]
        public List<string> tieredDialogueKeys;

        [SerializeField]
        public List<string> dialogueKeys;
        [SerializeField]
        public List<string> dialogueValues;
        [SerializeField]
        public List<string> shipLogKeys;
        [SerializeField]
        public List<string> shipLogValues;

        public static Dictionary<LanguageType, string> GetLanguageFileName = new Dictionary<LanguageType, string>
    {
        {LanguageType.English, "english" },
        {LanguageType.French, "french" },
        {LanguageType.Italian, "italian" },
        {LanguageType.German, "german" },
        {LanguageType.Japanese, "japanese" },
        {LanguageType.Korean, "korean" },
        {LanguageType.Polish, "polish" },
        {LanguageType.Portuguese, "portuguese_br" },
        {LanguageType.Russian, "russian" },
        {LanguageType.Chinese, "chinese_simple" },
        {LanguageType.Spanish, "spanish_la" },
        {LanguageType.Turkish, "turkish" },
        {LanguageType.Custom, "custom" }
    };

        public void BuildTieredDialogueKeys()
        {
            tieredDialogueKeys = new List<string>();
            tieredDialogueKeys.Add("Select...");
            foreach (string key in dialogueKeys)
            {
                if (key == null) continue;
                string newKey = key;
                newKey = Regex.Replace(newKey, @"(?<![A-Z])([A-Z])(?!.*^(?=\S)[A-Z])", m => $"_{m.Value}");
                newKey = newKey.TrimStart('_');
                tieredDialogueKeys.Add(newKey.Replace('_', '/'));
            }
        }

        public Translation GetTranslation(bool dialogue = true, bool shipLogs = true)
        {
            Translation translation = new Translation();
            if (dialogue && dialogueKeys != null && dialogueValues != null)
            {
                translation.DialogueDictionary = new Dictionary<string, string>();
                for (int i = 0; i < dialogueKeys.Count; i++)
                {
                    if (dialogueKeys[i] == null || dialogueKeys[i] == string.Empty) continue;
                    translation.DialogueDictionary.Add(dialogueKeys[i], dialogueValues[i]);
                }
            }
            if (shipLogs && shipLogKeys != null && shipLogValues != null)
            {
                translation.ShipLogDictionary = new Dictionary<string, string>();
                for (int i = 0; i < shipLogKeys.Count; i++)
                {
                    if (shipLogKeys[i] == null || shipLogKeys[i] == string.Empty) continue;
                    translation.ShipLogDictionary.Add(shipLogKeys[i], shipLogValues[i]);
                }
            }
            return translation;
        }

        public string GetDialogueValue(string key)
        {
            if (dialogueKeys == null || !dialogueKeys.Contains(key)) return string.Empty;

            int index = dialogueKeys.IndexOf(key);

            return dialogueValues[index];
        }

        public void SetDialogueValue(string key, string value)
        {
            if (key == string.Empty) return;
            if (dialogueKeys == null)
            {
                dialogueKeys = new List<string>();
                dialogueValues = new List<string>();
            }
            if (dialogueKeys.Contains(key))
            {
                int i = dialogueKeys.IndexOf(key);
                dialogueValues[i] = value;
            }
            else
            {
                dialogueKeys.Add(key);
                dialogueValues.Add(value);
            }
            BuildTieredDialogueKeys();
            EditorUtility.SetDirty(this);
        }

        public bool TryRenameDialogueKey(string oldName, string newName)
        {
            if (dialogueKeys.Contains(newName)) return false;

            if (dialogueKeys.Contains(oldName))
            {
                int i = dialogueKeys.IndexOf(oldName);
                dialogueKeys[i] = newName;
            }
            else
            {
                SetDialogueValue(newName, "");
            }
            BuildTieredDialogueKeys();
            return true;
        }

        public string GetShipLogValue(string key)
        {
            if (shipLogKeys == null || !shipLogKeys.Contains(key)) return string.Empty;

            int index = shipLogKeys.IndexOf(key);

            return shipLogValues[index];
        }

        public void SetShipLogValue(string key, string value)
        {
            if (key == string.Empty) return;
            if (shipLogKeys == null)
            {
                shipLogKeys = new List<string>();
                shipLogValues = new List<string>();
            }
            if (shipLogKeys.Contains(key))
            {
                int i = shipLogKeys.IndexOf(key);
                shipLogValues[i] = value;
            }
            else
            {
                shipLogKeys.Add(key);
                shipLogValues.Add(value);
            }
            BuildTieredDialogueKeys();
            EditorUtility.SetDirty(this);
        }

        public bool TryRenameShipLogKey(string oldName, string newName)
        {
            if (shipLogKeys.Contains(newName)) return false;

            if (shipLogKeys.Contains(oldName))
            {
                int i = shipLogKeys.IndexOf(oldName);
                shipLogKeys[i] = newName;
            }
            else
            {
                SetShipLogValue(newName, "");
            }
            BuildTieredDialogueKeys();
            return true;
        }

        public static void SyncTranslations(Language targetLanguage, Language sourceLanguage)
        {
            if (targetLanguage.dialogueKeys == null)
            {
                targetLanguage.dialogueKeys = new List<string>(sourceLanguage.dialogueKeys);
                targetLanguage.dialogueValues = new List<string>(sourceLanguage.dialogueValues);
            }
            else
            {
                for (int i = 0; i < sourceLanguage.dialogueKeys.Count; i++)
                {
                    var key = sourceLanguage.dialogueKeys[i];
                    var value = sourceLanguage.dialogueValues[i];
                    if (!targetLanguage.dialogueKeys.Contains(key))
                    {
                        targetLanguage.dialogueKeys.Add(key);
                        targetLanguage.dialogueValues.Add(value);
                    }
                }
            }

            if (targetLanguage.shipLogKeys == null)
            {
                targetLanguage.shipLogKeys = new List<string>(sourceLanguage.shipLogKeys);
                targetLanguage.shipLogValues = new List<string>(sourceLanguage.shipLogValues);
            }
            else
            {
                for (int i = 0; i < sourceLanguage.shipLogKeys.Count; i++)
                {
                    var key = sourceLanguage.shipLogKeys[i];
                    var value = sourceLanguage.shipLogValues[i];
                    if (!targetLanguage.shipLogKeys.Contains(key))
                    {
                        targetLanguage.shipLogKeys.Add(key);
                        targetLanguage.shipLogValues.Add(value);
                    }
                }
            }
            targetLanguage.BuildTieredDialogueKeys();
            EditorUtility.SetDirty(targetLanguage);
        }

        public static void UnflagParse()
        {
            foreach (Language lang in XMLEditorSettings.Instance.supportedLanguages)
            {
                lang.hasParsedData = false;
            }
        }
    }
}