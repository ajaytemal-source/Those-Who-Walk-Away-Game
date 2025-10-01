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
    public EmailDialougeManager emailDialougeManager;
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

    public void LoadSaveFile(){
        if (File.Exists(savePath)){
            string json = File.ReadAllText(savePath);
            ConvoSaveFile SaveFile = JsonUtility.FromJson<ConvoSaveFile>(json);
            globalGameManager.current_save = SaveFile.save_stage; 
            globalGameManager.setPlayerName(SaveFile.player_name); 
            updateTimeManager.customTime = DateTime.Parse(SaveFile.save_time, null, System.Globalization.DateTimeStyles.RoundtripKind); //REQUIRES WRITING INTO JSON AS "o"
            globalGameManager.TriggerList = (SaveFile.triggers.ToList()).Select(i => (Trigger)i).ToList();
            ericScreenshotIndex = SaveFile.screenshot_index;
            ericScreenshotTime = SaveFile.screenshot_time; 
            foreach (ConvoSave convo in SaveFile.conversations){
                if (convo.thread == Thread.None){
                    emailDialougeManager.loadConversation(convo);
                }
                else{
                    teamsDialogueManager.loadConversation(convo);
                }
                saved_convos.Add(convo);
            }
            fileNavigatorManager.addFilesToSave = false; 
            foreach (FileSaveInfo file in SaveFile.fileSaves){
                saved_files.Add(file); 
                foreach (int directory in file.directories){
                    fileNavigatorManager.AddFileById(file.id, directory);
                }
            }
            fileNavigatorManager.addFilesToSave = true; 
            if (ericScreenshotIndex != -1){
                teamsDialogueManager.sendEricScreenshot(ericScreenshotIndex);
            }
            saveTimeUpdate.text = "Last Save at " + updateTimeManager.customTime.ToString("h:mm tt");
        }
    }

    public void WriteSaveFile(){
        saved_convos.AddRange(unsaved_convos);
        foreach (int key in fileNavigatorManager.unsaved_files.Keys){
            FileSaveInfo new_file = new FileSaveInfo{
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