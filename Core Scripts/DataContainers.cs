using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace IRIS_Dialogue_Nodes{

    [System.Serializable]
    public class RawNodeCollection{  
        public RawNode[] nodes; 
    }   

    [System.Serializable]
    public class RawNode{
        public string type; 
        public int id; 
        public string output; 
        public int next; 
        public DialogueTriggerPairCollection[] dialogueTriggerPairCollection; 
        public ConversationStall stallValue = ConversationStall.None; 
        public ConversationStall removeStall =  ConversationStall.None;
        public bool stallAfterRespond = false;
        public bool isWaitingForTeams = false; 
        public bool otherScriptDepends = false; 
        public bool nextIsChoice = false; 
        public bool is_next_response = false; 
        public int triggerGlitch = -1; 
        public Choice[] choices; 
    }

    [System.Serializable]
    public class Response{
        public string output;
        public int next; 
        public int id;  
        public DialogueTriggerPairCollection[] dialogueTriggerPairCollection; 
        public ConversationStall stallValue = ConversationStall.None;
        public bool stallAfterRespond = false;
        public bool is_next_response = false; 
        public ConversationStall removeStall =  ConversationStall.None;
        public int triggerGlitch = -1; 
    }

    [System.Serializable]
    public class Choice{
        public string output;
        public int next; 
        public Trigger trigger = Trigger.None;   
        public ConversationStall stallValue = ConversationStall.None;
    }

    [System.Serializable]
    public class Choice_Set{
        public int id; 
        public Choice[] choices; 
        public bool isWaitingForTeams = false; 
        public bool otherScriptDepends = false; 
        public bool nextIsChoice = false; 
    }

    [System.Serializable]
    public class DialogueTriggerPair{
        public Trigger trigger; 
        public string dialogue; 
    }

    [System.Serializable]
    public class DialogueTriggerPairCollection{
        public DialogueTriggerPair[] dialogueTriggerPairs; 
    }

}

namespace EMAIL_Dialogue_Nodes{

    [System.Serializable]
    public class RawNodeCollection{  
        public RawNode[] nodes; 
    }   

    [System.Serializable]
    public class RawNode{
        public string type; 
        public int id; 
        public string character; 
        public string subject; 
        public Download download = null;  
        public string output; 
        public int next; 
        public Choice[] choices; 
        public bool has_download = false;  
        public int seconds_between;
        public int attachment;  
        public DialogueTriggerPairCollection[] dialogueTriggerPairCollection; 
        public ConversationStall stallValue = ConversationStall.None; 
        public ConversationStall removeStall =  ConversationStall.None;
        public bool is_next_response = false; 
        public bool stallAfterRespond = false;
        public int triggerGlitch = -1; 
    }  

    [System.Serializable]
    public class Download{
        public int file_id;
        public string file_type; 
        public string file_name; 
        public string file_size; 
    }

    [System.Serializable]
    public class Choice{
        public string output; 
        public int attachment = -1;  
        public int next; 
        public Trigger trigger = Trigger.None;
        public ConversationStall stallValue = ConversationStall.None;
    }

    [System.Serializable]
    public class First_Email{
        public int id; 
        public string character; 
        public string subject;  
        public string output; 
        public bool has_download = false; 
        public Download download = null; 
        public bool is_next_response = false; 
        public DialogueTriggerPairCollection[] dialogueTriggerPairCollection; 
        public ConversationStall stallValue = ConversationStall.None;
        public ConversationStall removeStall =  ConversationStall.None;
        public bool stallAfterRespond = false;
        public int next; 
    }

    [System.Serializable]
    public class Response{
        public int id; 
        public string character; 
        public int seconds_between;   
        public string output; 
        public bool has_download = false; 
        public Download download = null; 
        public bool is_next_response = false; 
        public DialogueTriggerPairCollection[] dialogueTriggerPairCollection; 
        public ConversationStall stallValue = ConversationStall.None;
        public ConversationStall removeStall =  ConversationStall.None;
        public bool stallAfterRespond = false;
        public int triggerGlitch = -1; 
        public int next; 
    }

    [System.Serializable]
    public class Choice_Set{
        public int id; 
        public Choice[] choices;
    }

    [System.Serializable]
    public class DialogueTriggerPair{
        public Trigger trigger; 
        public string dialogue; 
    }

    [System.Serializable]
    public class DialogueTriggerPairCollection{
        public DialogueTriggerPair[] dialogueTriggerPairs; 
    }

}

namespace TEAMS_Dialogue_Nodes{

    [System.Serializable]
    public class RawNode{
        public string type; 
        public int id; 
        public string character; 
        public string[] output;
        public TypingIntervalArray[] typing_intervals; 
        public DialogueTriggerPairCollection[] dialogueTriggerPairCollection; 
        public DialogueTriggerPair[] dialogue_trigger_pairs;
        public ConversationStall stallValue = ConversationStall.None;
        public ConversationStall removeStall =  ConversationStall.None;
        public bool stallAfterRespond = false; 
        public bool isWaitingForIRYS = false; 
        public bool nextIsChoice = false; 
        public bool otherScriptDepends = false; 
        public int next_response_delay = -1;
        public int next; 
        public int triggerGlitch = -1; 
        public Choice[] choices;

    }

    [System.Serializable]
    public class RawNodeCollection{  
        public RawNode[] nodes; 
    }

    [System.Serializable]
    public class TypingIntervalArray{
        public int[] intervals;
    }

    [System.Serializable]
    public class Response{
        public int id; 
        public string character; 
        public string[] output;
        public TypingIntervalArray[] typing_intervals; 
        public DialogueTriggerPairCollection[] dialogueTriggerPairCollection; 
        public DialogueTriggerPair[] dialogue_trigger_pairs;
        public ConversationStall stallValue = ConversationStall.None;
        public bool stallAfterRespond = false; 
        public ConversationStall removeStall =  ConversationStall.None;
        public int triggerGlitch = -1; 
        public int next; 
        public int next_response_delay = -1;  //If this is not -1, that means the next points to another response
    }

    [System.Serializable]
    public class Choice_Set{
        public int id; 
        public Choice[] choices; 
        public bool isWaitingForIRYS = false; 
        public bool nextIsChoice = false; 
        public bool otherScriptDepends = false; 
    }

    [System.Serializable]
    public class Choice{
        public string output;
        public int next; //Points to Next Response 
        public Trigger trigger = Trigger.None; 
        public ConversationStall stallValue = ConversationStall.None;
    }

    [System.Serializable]
    public class DialogueTriggerPair{
        public Trigger trigger; 
        public string dialogue; 
        public int message_index; 
    }

    [System.Serializable]
    public class DialogueTriggerPairCollection{
        public DialogueTriggerPair[] dialogueTriggerPairs; 
    }

    //Thread indicator will be written into startConversation call (much simpler)
}

namespace FN_File_Nodes{

    [System.Serializable]
    public class RawNode{
        public string type;
        public int id; 
        public string name; 
        public string date; 
        public string content;
        public string size; 
        public int directory;
        public string password = null;
        public bool isAdmin = false;
        public FileTriggerPair[] file_trigger_pairs;
    }  

    [System.Serializable]
    public class RawNodeCollection{  
        public RawNode[] nodes; 
    }   

    [System.Serializable]
    public class FileTriggerPair{
        public int file; 
        public int trigger; 
    }

    [System.Serializable]
    public class FN_Folder{
        public int id; 
        public string name; 
        public string date; 
        public int directory = -1; 
        public string password = null;
        public bool isAdmin = false; 
        public RectTransform contents; 
        public FileTriggerPair[] file_trigger_pairs; //If file is in folder, trigger is sent out
    }

    [System.Serializable]
    public class FN_File{
        public int id;
        public string name; 
        public string date; 
        public string size;
        public int directory = -1;  
    }

    [System.Serializable]
    public class FN_Token : FN_File{
        //Inherits all necessary components from file class 
    }

    [System.Serializable]
    public class FN_IrysTool : FN_File{
        //Inherits all necessary components from file class 
    }

    [System.Serializable]
    public class FN_Txt : FN_File{
        public string content;
        public GameObject textFileScroll; 
    }

}

namespace Terminal_Nodes{

    [System.Serializable]
    public class RawNode{

    }  

    [System.Serializable]
    public class RawNodeCollection{  
        public RawNode[] nodes; 
    }  

    [System.Serializable]
    public class TERM_InputOutputCollection{
        public TERM_InputOutput[] nodes; 
    }


    [System.Serializable]
    public class TERM_InputOutput{
        public int id; 
        public string valid_input; 
        public string valid_output;
        public string valid_color = null; 
        public string next_invalid_output;
        public int output_delay; 
        public int valid_next; 
        public int invalid_next; 
        public TerminalTrigger valid_trigger = TerminalTrigger.NoTrigger;
    }
}

namespace SaveConversation{
    
    [System.Serializable]
    public class ConvoSaveFile{ 
        public int save_stage; 
        public string save_time; 
        public string player_name;
        public int[] triggers; 
        public int screenshot_index = -1; //When IRYS sends screenshot, record. Check if not -1, and then insert screenshot at index
        public string screenshot_time; 
        public ConvoSave[] conversations; 
        public FileSaveInfo[] fileSaves; 
    }

    [System.Serializable]
    public class ConvoSave{ 
        public string file_name;
        public int convo_id;  
        public Thread thread = Thread.None; //If thread != None, its a Teams Convo, else, its Email 
        public Dialogue_Save[] responses;
        public Dialogue_Save[] choices;
    }

    [System.Serializable]
    public class Dialogue_Save{
        public int id; 
        public int chosen_choice = -1; //If Chosen ID, you know that its a choice  
        public string time;  
    }

    [System.Serializable]
    public class FileSaveInfo{
        public int id; 
        public int[] directories; 
    }
    

}

/*
Notes: 
- if id = 0, first node -> Start with this 
- if next = -1, last node -> signal that conversation has ended 
- Files intended to be downloaded later have a current directory value of -1. When the GGM calls the downloadById function on it, it will get placed in a directory 
*/