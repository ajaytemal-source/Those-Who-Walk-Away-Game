using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using TMPro;

using Terminal_Nodes; 

public class TerminalManager : MonoBehaviour
{

    public CanvasGroup Terminal_CanvasGroup; 
    public GameObject Terminal_CentralObject; 
    public RectTransform contentWindow;
    public AutoScroll autoScroll;

    public AudioSource keyboard_sfx;
    public AudioSource adminGlitch_sfx; 

    public WindowResizerScript windowResizerScript;
    public FileNavigatorManager fileNavigatorManager; 
    public GlobalGameEventScript globalGameEventScript;

    public Button minimzeButton; 
    public Button exitButton; 

    public bool currently_open = false; 

    public Dictionary<string, List<TERM_InputOutput>> terminal_dict = new(); 

    public GameObject terminalInputPrefab; 
    public GameObject terminalOutputPrefab; 
    public TMP_InputField currentInput; 
    public Color errorMessageColor; 
    bool inputFound = false; 

    public int currentlyExpectedInput = 0;
    public int currentInvalidNext = 0; 
    string currentInvalidOutput = "Command '${input}' not recognized.";  

    public List<TerminalTrigger> terminalTriggerList = new(); 
    
    // Start is called before the first frame update
    void Start()
    {
        keyboard_sfx.Play();
        keyboard_sfx.Pause();
        close(); 
        currentInput.onSubmit.AddListener(checkTerminalInput);
        currentInput.ActivateInputField();
        currentInput.onValueChanged.AddListener(HandleInput);
        terminalInputPrefab.GetComponentInChildren<TMP_InputField>().onSubmit.AddListener(checkTerminalInput);
        terminalInputPrefab.GetComponentInChildren<TMP_InputField>().onValueChanged.AddListener(HandleInput);
        exitButton.onClick.AddListener(close);
        GameEvents.TerminalCommandEvent.Subscribe(recieveCommandTrigger); 

    }

    // Update is called once per frame
    void Update()
    {
        if(!currently_open){
            if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKey(KeyCode.T) && Input.GetKey(KeyCode.P)){
                open();
            }
        }
    }

    public void addTerminalKeys(string tkeys_to_parse_file){
        TextAsset terminal_json = Resources.Load<TextAsset>(tkeys_to_parse_file);
        ParseTerminalKeys(terminal_json);
    }

    public void ParseTerminalKeys(TextAsset json_file){
        TERM_InputOutputCollection nodeList = JsonUtility.FromJson<TERM_InputOutputCollection>(json_file.text);
        foreach (TERM_InputOutput node in nodeList.nodes){
            if (!terminal_dict.ContainsKey(node.valid_input)){
                terminal_dict[node.valid_input] = new List<TERM_InputOutput>();
            }
            terminal_dict[node.valid_input].Add(node); 
        }
    }

    void checkTerminalInput(string input){
        StartCoroutine(checkTerminalInputCoroutine(input)); 
    }

    IEnumerator checkTerminalInputCoroutine(string input){
        currentInput.interactable = false; 
        if (terminal_dict.ContainsKey(input)){
            foreach (TERM_InputOutput prompt in terminal_dict[input]){
                if (prompt.id == currentlyExpectedInput){
                    currentlyExpectedInput = prompt.valid_next;
                    currentInvalidOutput = prompt.next_invalid_output;
                    if(prompt.valid_color != null){
                        Color color;
                        ColorUtility.TryParseHtmlString(prompt.valid_color, out color);
                        terminalOutputPrefab.GetComponent<TextMeshProUGUI>().color = color; 
                    }
                    terminalOutputPrefab.GetComponent<TextMeshProUGUI>().text = prompt.valid_output; 
                    if(prompt.valid_trigger != TerminalTrigger.NoTrigger){
                        GameEvents.TerminalCommandEvent.Raise(prompt.valid_trigger);
                    }
                    yield return new WaitForSeconds(prompt.output_delay);
                    inputFound = true; 
                    break;
                }
            }
            if (!inputFound){
                terminalOutputPrefab.GetComponent<TextMeshProUGUI>().color = errorMessageColor;
                terminalOutputPrefab.GetComponent<TextMeshProUGUI>().text = currentInvalidOutput.Replace("${input}", input); 
            }
        }
        else{
            terminalOutputPrefab.GetComponent<TextMeshProUGUI>().color = errorMessageColor;
            terminalOutputPrefab.GetComponent<TextMeshProUGUI>().text = currentInvalidOutput.Replace("${input}", input); 
        }
        yield return new WaitForSeconds(0.05f); 
        Instantiate(terminalOutputPrefab, contentWindow);
        yield return new WaitForSeconds(0.05f);
        currentInput = Instantiate(terminalInputPrefab, contentWindow).GetComponentInChildren<TMP_InputField>(); 
        autoScroll.scrollVertically(contentWindow.GetComponentInParent<ScrollRect>()); 
        currentInput.onSubmit.AddListener(checkTerminalInput);
        currentInput.onValueChanged.AddListener(HandleInput);
        currentInput.ActivateInputField();
        terminalOutputPrefab.GetComponent<TextMeshProUGUI>().color = Color.white;
    }

    public void close(){
        currently_open = false; 
        Utility.windowMinimize(Terminal_CanvasGroup, windowResizerScript);
    }

    void open(){
        keyboard_sfx.Play();
        keyboard_sfx.Pause();
        recieveCommandTrigger(TerminalTrigger.openTerminal); 
        currently_open = true; 
        Utility.windowOpen(Terminal_CentralObject, Terminal_CanvasGroup, windowResizerScript, currently_open, new Vector2(-10f, 30f));
        Utility.windowReopen(Terminal_CentralObject, Terminal_CanvasGroup, windowResizerScript);
        currentInput.ActivateInputField();
    }

    private void HandleInput(string _){
        StartCoroutine(playKeyboardSound());
    }

    IEnumerator playKeyboardSound(){
        keyboard_sfx.UnPause();
        yield return new WaitForSeconds(0.1f);
        keyboard_sfx.Pause();
    }

    void recieveCommandTrigger(TerminalTrigger trigger){
        StartCoroutine(recieveCommandTriggerIE(trigger)); 
    }

    IEnumerator recieveCommandTriggerIE(TerminalTrigger trigger){ 
        if (!terminalTriggerList.Contains(trigger)){
            terminalTriggerList.Add(trigger); 
            if (!globalGameEventScript.ConversationStallList.Contains(ConversationStall.EricViaET)){
                yield return new WaitUntil(() => globalGameEventScript.ConversationStallList.Contains(ConversationStall.EricViaET));
            }
            globalGameEventScript.ConversationStallList.Remove(ConversationStall.EricViaET);
            if (trigger == TerminalTrigger.rightPassword){
                StartCoroutine(playGlitchSfx()); 
            }
        }
    }

    IEnumerator playGlitchSfx(){
        adminGlitch_sfx.Play();
        yield return new WaitForSeconds(2f);
        adminGlitch_sfx.Stop();
        fileNavigatorManager.unlockAdminFolder();
    }

    public void resetApp(){
        currentlyExpectedInput = 0;
        currentInvalidNext = 0;
        close(); 
        terminalTriggerList.Clear(); 
        for (int i = contentWindow.childCount - 3; i >= 0; i--){
            Destroy(contentWindow.GetChild(i).gameObject);
        }
        for (int i = 2; i < contentWindow.childCount; i++){
            GameObject.Destroy(contentWindow.GetChild(i).gameObject);
        }
        StopAllCoroutines(); 
    }
}
