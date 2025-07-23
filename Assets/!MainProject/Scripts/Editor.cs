using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
[CustomEditor(typeof(WordSearchGame))]
public class WordSearchEditor : Editor
{
    private string newWord = "";
    private List<string> tempWordList = new List<string>();
    private bool showWordList = true;
    private const string saveKey = "WordSearchWordList";
    
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();
        
        WordSearchGame wordSearch = (WordSearchGame)target;
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Gerenciador de Palavras", EditorStyles.boldLabel);
        
        // Interface para adicionar palavras
        EditorGUILayout.BeginHorizontal();
        newWord = EditorGUILayout.TextField("Nova Palavra", newWord).ToUpper();
        GUI.enabled = !string.IsNullOrEmpty(newWord);
        if (GUILayout.Button("Adicionar", GUILayout.Width(100)))
        {
            if (!tempWordList.Contains(newWord))
            {
                tempWordList.Add(newWord);
                newWord = "";
                SaveWordList();
            }
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
        
        // Lista de palavras atual
        showWordList = EditorGUILayout.Foldout(showWordList, "Lista de Palavras");
        if (showWordList)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            if (tempWordList.Count == 0)
            {
                EditorGUILayout.LabelField("Nenhuma palavra adicionada.");
            }
            
            for (int i = 0; i < tempWordList.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(tempWordList[i]);
                if (GUILayout.Button("X", GUILayout.Width(30)))
                {
                    tempWordList.RemoveAt(i);
                    SaveWordList();
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        // Botões para importar/exportar palavra
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Importar Lista"))
        {
            ImportWordList();
        }
        
        if (GUILayout.Button("Exportar Lista"))
        {
            ExportWordList();
        }
        EditorGUILayout.EndHorizontal();
        
        // Botão para limpar a lista
        if (GUILayout.Button("Limpar Lista"))
        {
            if (EditorUtility.DisplayDialog("Confirmar", "Deseja realmente limpar a lista de palavras?", "Sim", "Não"))
            {
                tempWordList.Clear();
                SaveWordList();
            }
        }
    }
    
    private void SaveWordList()
    {
        string json = JsonUtility.ToJson(new WordListData { words = tempWordList.ToArray() });
        EditorPrefs.SetString(saveKey, json);
    }
    
    private void LoadWordList()
    {
        if (EditorPrefs.HasKey(saveKey))
        {
            string json = EditorPrefs.GetString(saveKey);
            WordListData data = JsonUtility.FromJson<WordListData>(json);
            tempWordList = new List<string>(data.words);
        }
    }
    
    private void ImportWordList()
    {
        string path = EditorUtility.OpenFilePanel("Importar Lista de Palavras", "", "txt");
        if (!string.IsNullOrEmpty(path))
        {
            string content = File.ReadAllText(path);
            string[] words = content.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            
            tempWordList.Clear();
            foreach (string word in words)
            {
                string cleanWord = word.Trim().ToUpper();
                if (!string.IsNullOrEmpty(cleanWord) && !tempWordList.Contains(cleanWord))
                {
                    tempWordList.Add(cleanWord);
                }
            }
            
            SaveWordList();
        }
    }
    
    private void ExportWordList()
    {
        string path = EditorUtility.SaveFilePanel("Exportar Lista de Palavras", "", "palavras_caca_palavras.txt", "txt");
        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllLines(path, tempWordList.ToArray());
        }
    }
    
    private void OnEnable()
    {
        LoadWordList();
    }
    
    [System.Serializable]
    private class WordListData
    {
        public string[] words;
    }
}
#endif