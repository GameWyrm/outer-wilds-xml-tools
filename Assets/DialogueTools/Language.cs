using UnityEngine;
using System.Collections.Generic;

public class Language : ScriptableObject
{
    [HideInInspector]
    public Translation translation;
    [HideInInspector]
    public LanguageType type;

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

    private void OnDestroy()
    {
        if ( XMLEditorSettings.Instance.supportedLanguages.Contains(this))
        {
            XMLEditorSettings.Instance.supportedLanguages.Remove(this);
            Debug.Log($"Removed language {name}.");
        }
        else
        {
            Debug.LogError($"{name} was not correctly removed from the supported languages! Double-check that it's not still there.");
        }
    }

    public string GetDialogueValue(string key)
    {
        if (translation == null || translation.DialogueDictionary == null || !translation.DialogueDictionary.ContainsKey(key)) return string.Empty;

        return translation.DialogueDictionary[key];
    }

    public void SetDialogueValue(string key, string value)
    {
        if (translation == null)
        {
            Debug.LogError($"Translation for {name} missing!");
            return;
        }
        if (translation.DialogueDictionary == null) translation.DialogueDictionary = new Dictionary<string, string>();
        if (translation.DialogueDictionary.ContainsKey(key))
        {
            translation.DialogueDictionary[key] = value;
        }
        else
        {
            translation.DialogueDictionary.Add(key, value);
        }
    }

    public string GetShipLogValue(string key)
    {
        if (translation == null || translation.ShipLogDictionary == null || !translation.ShipLogDictionary.ContainsKey(key)) return string.Empty;

        return translation.ShipLogDictionary[key];
    }

    public void SetShipLogValue(string key, string value)
    {
        if (translation == null)
        {
            Debug.LogError($"Translation for {name} missing!");
            return;
        }
        if (translation.ShipLogDictionary == null) translation.ShipLogDictionary = new Dictionary<string, string>();
        if (translation.ShipLogDictionary.ContainsKey(key))
        {
            translation.ShipLogDictionary[key] = value;
        }
        else
        {
            translation.ShipLogDictionary.Add(key, value);
        }
    }

    public static void SyncTranslations(Language targetLanguage, Language sourceLanguage)
    {
        Translation target = targetLanguage.translation;
        Translation source = sourceLanguage.translation;

        if (target.DialogueDictionary == null)
        {
            target.DialogueDictionary = source.DialogueDictionary;
        }
        else
        {
            foreach (var key in source.DialogueDictionary.Keys)
            {
                if (!target.DialogueDictionary.ContainsKey(key))
                {
                    target.DialogueDictionary.Add(key, source.DialogueDictionary[key]);
                }
            }
        }

        if (target.ShipLogDictionary == null)
        {
            target.ShipLogDictionary = source.ShipLogDictionary;
        }
        else
        {
            foreach (var key in source.ShipLogDictionary.Keys)
            {
                if (!target.ShipLogDictionary.ContainsKey(key))
                {
                    target.ShipLogDictionary.Add(key, source.ShipLogDictionary[key]);
                }
            }
        }
    }
}