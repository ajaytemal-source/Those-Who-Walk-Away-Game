using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using System;
using TMPro;  
using SaveConversation; 

public class SaveFileManager : MonoBehaviour
{ 
    public GlobalGameEventScript globalGameManager; 
    public UpdateTimeScript updateTimeManager; 
    public EmailDialougeManager emailDialogueManager;
    public TeamsDialogueManager teamsDialogueManager;
    public FileNavigatorManager fileNavigatorManager; 
    public TextMeshProUGUI saveTimeUpdate; 

    public GameObject saveNotifPrefab; 
    public RectTransform notifContainer; 
    public int ericScreenshotIndex = -1; 
    public string ericScreenshotTime = ""; 

    string savePath; 
    List<ConvoSave> saved_convos = new List<ConvoSave>();
    public List<ConvoSave> unsaved_convos = new List<ConvoSave>();
    List<FileSaveInfo> saved_files = new List<FileSaveInfo>();

    void Awake(){
        savePath = Path.Combine(Application.persistentDataPath, "savefile.json");
        saveNotifPrefab.GetComponent<DownloadNotificationScript>().notifContainer = notifContainer; 
    }

    public void LoadSaveFile()
    {
        if (!File.Exists(savePath)) return;

        ConvoSaveFile saveFile = LoadJson(savePath);
        if (saveFile == null) return;

        LoadGameState(saveFile);
        LoadConversations(saveFile);
        LoadFileSaves(saveFile);
        HandleScreenshots(saveFile);
        UpdateSaveTimeUI(saveFile.save_time);
    }

    private ConvoSaveFile LoadJson(string path)
    {
        try
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<ConvoSaveFile>(json);
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    private void LoadGameState(ConvoSaveFile saveFile)
    {
        globalGameManager.current_save = saveFile.save_stage;
        globalGameManager.setPlayerName(saveFile.player_name);

        // System.Globalization.DateTimeStyles.RoundtripKind supports JSON parsing of string values into a dateTime type.
        updateTimeManager.customTime = DateTime.Parse(saveFile.save_time, null, System.Globalization.DateTimeStyles.RoundtripKind);

        globalGameManager.TriggerList = saveFile.triggers.Select(i => (Trigger)i).ToList();
        ericScreenshotIndex = saveFile.screenshot_index;
        ericScreenshotTime = saveFile.screenshot_time;
    }

    private void LoadConversations(ConvoSaveFile saveFile)
    {
        foreach (ConvoSave convo in saveFile.conversations)
        {
            if (convo.thread == Thread.None){ //Thread.None value indicates 'null' Teams thread --> indicates it must be an email conversation. (Might change for clarity later)
                emailDialogueManager.loadConversation(convo);
            }
            else{
                teamsDialogueManager.loadConversation(convo);
            }
            saved_convos.Add(convo);
        }
    }

    private void LoadFileSaves(ConvoSaveFile saveFile)
    {
        fileNavigatorManager.addFilesToSave = false;

        foreach (FileSaveInfo file in saveFile.fileSaves)
        {
            saved_files.Add(file);
            foreach (int directory in file.directories)
            {
                fileNavigatorManager.AddFileById(file.id, directory);
            }
        }

        fileNavigatorManager.addFilesToSave = true;
    }

    private void HandleScreenshots(ConvoSaveFile saveFile)
    {
        if (saveFile.screenshot_index != -1){
            teamsDialogueManager.sendEricScreenshot(saveFile.screenshot_index);
        }
    }

    private void UpdateSaveTimeUI(string saveTime)
    {
        DateTime parsedTime = DateTime.Parse(saveTime, null, System.Globalization.DateTimeStyles.RoundtripKind);
        saveTimeUpdate.text = $"Last Save at {parsedTime:h:mm tt}";
    }

    public void WriteSaveFile(){
        saved_convos.AddRange(unsaved_convos);

        foreach (int key in fileNavigatorManager.unsaved_files.Keys)
        {
            FileSaveInfo new_file = new FileSaveInfo
            {
                id = key,
                directories = fileNavigatorManager.unsaved_files[key].ToArray()
            };
            saved_files.Add(new_file);
        }

        ConvoSaveFile newSave = new ConvoSaveFile{
            save_stage = globalGameManager.current_save,
            save_time = updateTimeManager.customTime.ToString("o"),
            player_name = globalGameManager.player_name,
            triggers = (globalGameManager.TriggerList.ToArray()).Select(i => (int)i).ToArray(),
            screenshot_index = ericScreenshotIndex,
            screenshot_time = ericScreenshotTime, 
            fileSaves = saved_files.ToArray(),
            conversations = saved_convos.ToArray()
        };

        fileNavigatorManager.unsaved_files.Clear(); 
        unsaved_convos.Clear(); 

        string json = JsonUtility.ToJson(newSave, true);
        File.WriteAllText(savePath, json);
        saveTimeUpdate.text = "Last Save at " + updateTimeManager.customTime.ToString("h:mm tt");
        Instantiate(saveNotifPrefab, notifContainer); 
    }

    public void deleteSaveFile(){
        File.Delete(savePath);

        fileNavigatorManager.unsaved_files.Clear();
        saved_convos.Clear();
        unsaved_convos.Clear();
        saved_files.Clear(); 
        
        globalGameManager.current_save = 0; 
        ericScreenshotIndex = -1; 
        ericScreenshotTime = "";  
    }

}

