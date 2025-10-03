using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using System.IO; 
using System.Linq;
using System;

public static class Utility{ 

    public static string getCharacterContact(string character, string player_name){
            if (character == "PL"){
                return player_name + " <ee0430@etherean.org>";
            }
            else if (character == "CL"){
                return "Clara Luvinia <cl9834@etherean.org>";
            }
            else if(character == "EA"){
                return "Eric Apostolos <ea3802@etherean.org>";
            }
            else if(character == "AC"){
                return "Aisha Celia <ac5664@etherean.org>";
            }
            return "1"; 
    }

    public static string getCharacterName(string character, string player_name){
        if (character == "PL"){
            return player_name;
        }
        else if (character == "CL"){
            return "Clara Luvinia";
        }
        else if(character == "EA"){
            return "Eric Apostolos";
        }
        else if(character == "AC"){
            return "Aisha Celia";
        }
        return "1"; 
    }

    public static string getCharacterTeamsName(string character){
        if (character == "CL"){
            return "Clara";
        }
        else if(character == "EA"){
            return "Eric";
        }
        else if(character == "TRC"){
            return "The_Real_Clara";
        }
        else if (character == "RD"){
            return "Renes_Damian";
        }
        return "1"; 
    }

    public static string getCharacterTeamsInitials(string character){
        if (character == "CL"){
            return "C";
        }
        else if(character == "EA"){
            return "E";
        }
        else if (character == "TRC"){
            return "T"; 
        }
        else if (character == "RD"){
            return "R";
        }
        return "1"; 

    }

    public static Color getCharacterTeamsColor(string character){
        string hex = "#FFFFFF";
        if (character == "CL"){
            hex = "#708CC3";
        }
        else if(character == "EA"){
            hex = "#617F7D";
        }
        else if (character == "TRC"){
            hex = "#C37E70";
        }
        else if (character == "RD"){
            hex = "#8B8589";
        }
        Color color;
        if (ColorUtility.TryParseHtmlString(hex, out color))
        {
            return color;
        }
        return Color.white;
    }

    public static string InsertStringAtIndex(string original, string insert, int index)
    {
        string firstPart = original.Substring(0, index);
        string secondPart = original.Substring(index);
        return firstPart + insert + secondPart;
    }

    public static void windowReopen(GameObject mainApp, CanvasGroup appCanvasGroup, WindowResizerScript windowResizerScript){
        mainApp.transform.SetSiblingIndex(6); //I should move all of the Apps to one container, and then look at the heirarchy of the container
        if(appCanvasGroup.alpha == 0){
            openCanvasGroup(appCanvasGroup); 
            windowResizerScript.window_is_open = true; 
        }
    }

    public static void windowMinimize(CanvasGroup appCanvasGroup, WindowResizerScript windowResizerScript){
        closeCanvasGroup(appCanvasGroup);
        windowResizerScript.window_is_open = false; 
    }

    public static void windowOpen(GameObject mainApp, CanvasGroup appCanvasGroup, WindowResizerScript windowResizerScript, bool currentlyOpen, Vector2 standardPosition){
        if (!currentlyOpen){
            RectTransform rectTransform = mainApp.GetComponent<RectTransform>();
            rectTransform.pivot = new Vector2(0.5f,0.5f);
            rectTransform.anchoredPosition = standardPosition;
        }
    }

    public static void tbIconOpenOrClose(GameObject mainApp, CanvasGroup appCanvasGroup, WindowResizerScript windowResizerScript){
        if(appCanvasGroup.alpha == 1){
            windowMinimize(appCanvasGroup, windowResizerScript);
        }
        else{
            windowReopen(mainApp, appCanvasGroup, windowResizerScript);
        }
    }

    public static void closeCanvasGroup(CanvasGroup canvasGroup){
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false; 
    }

    public static void openCanvasGroup(CanvasGroup canvasGroup){
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true; 
    }

}