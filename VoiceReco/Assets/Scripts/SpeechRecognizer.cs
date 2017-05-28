using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Windows.Speech;

public enum ActionType { None, Turn, Color };
public enum ObjectType { None, Cube, Sphere };
public delegate void performActionOnObject(GameObject o);

public class SpeechRecognizer : MonoBehaviour {

    public GameObject target = null;
    public string[] keywords = new string[]{"turn", "color", "cube", "sphere"};
    public ConfidenceLevel confidenceLevel = ConfidenceLevel.Low;

    //
    public GameObject cube;
    public GameObject sphere;
    private bool userLoggedIn = false;
    private bool userAuthenticated = false;
    private string userName = "";
    //

    protected PhraseRecognizer phraseRecognizer;
    protected DictationRecognizer dictationRecognizer;
    protected string currentWord = "none";
    [SerializeField]
    protected ActionType action = ActionType.None;
    [SerializeField]
    protected ObjectType obj    = ObjectType.None; 
    performActionOnObject performAction = null;

	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
        if (!userLoggedIn)
        {
            if (System.IO.File.Exists("login.txt"))
            {
                userLoggedIn = true;
                Debug.Log("Found login file, verifying...");
                readUserFromFile();
                validateUser();
            }
        }
	}

    private void validateUser()
    {
        if (userName.Equals("Patryk") || userName.Equals("Bogna") || userName.Equals("Bartek"))
        {
            userAuthenticated = true;
            Debug.Log("Authenticated user: " + userName);
            initializeSpeechRecognition();
        }
        else
        {
            Debug.Log("Username " + userName + " is not valid");
            userLoggedIn = false;
        }
    }

    private void initializeSpeechRecognition()
    {
        if (keywords != null && PhraseRecognitionSystem.isSupported)
        {
            Debug.Log(PhraseRecognitionSystem.isSupported);
            /* IMPORTANT - only works in 32bit Editor or Build (bug in Unity/Windows that will not be fixed)
            dictationRecognizer = new DictationRecognizer();
            dictationRecognizer.DictationResult     += dictationResultWriteText;
            dictationRecognizer.DictationComplete   += dictationComplete;
            dictationRecognizer.DictationError      += dictationError;
            dictationRecognizer.DictationHypothesis += dictationHypothesis;
            dictationRecognizer.Start(); */
            phraseRecognizer = new KeywordRecognizer(keywords, confidenceLevel);
            phraseRecognizer.OnPhraseRecognized += onPhraseRecognized;
            phraseRecognizer.Start();
            Debug.Log(PhraseRecognitionSystem.Status);
        }
        else
        {
            if (!PhraseRecognitionSystem.isSupported)
            {
                Debug.Log("Phrase Recognition seems to be not supported by your system. Please use Windows 10 and change your language settings to English.");
            }
        }
    }

    private void readUserFromFile()
    {
        var fileStream = new FileStream("login.txt", FileMode.Open, FileAccess.Read);
        string line = "";
        try
        {
            using (StreamReader sr = new StreamReader(fileStream, Encoding.UTF8))
            {
                line = sr.ReadLine();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("The file could not be read:");
            Console.WriteLine(e.Message);
        }
        Debug.Log("Retrieved username: " + line);
        userName = line;
    }

    private void onPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        currentWord = args.text;
        //object = args.text....

        processActionName();
        processObjectName();

        Debug.Log("Current word: " + currentWord +
            "| Current object: " + obj +
            ", current action: " + action);

        if (performAction != null && target != null)
        {
            Debug.Log("Performing action: " + action + " on object: " + obj);
            performAction(target);
            //reset the phrase
            performAction = null;
            action = ActionType.None;
            target = null;
            obj = ObjectType.None;
        }
    }

    #region PhraseProcessor

    private void processActionName()
    {
        switch (currentWord)
        {
            case "turn":
                action = ActionType.Turn;
                performAction = rotateObject;
                break;
            case "color":
                action = ActionType.Color;
                performAction = changeColor;
                break;
            default:
                //action = ActionType.None;
                //performAction = null;
                break;
        }
    }

    private void processObjectName()
    {
        switch (currentWord)
        {
            case "cube":
                obj = ObjectType.Cube;
                target = cube;
                break;
            case "sphere":
                obj = ObjectType.Sphere;
                target = sphere;
                break;
            default:
                //obj = ObjectType.None;
                //target = null;
                break;
        }
    }

    #endregion

    #region ObjectActions

    private void rotateObject(GameObject o)
    {
        o.transform.Rotate(new Vector3(0,0,45.0f));
    }

    private void changeColor(GameObject o)
    {
        o.GetComponent<Renderer>().material.color = Color.red;
    }

    /// For dictation

    private void dictationResultWriteText(string text, ConfidenceLevel confidence)
    {
        Debug.Log("DictationResult: " + text);
    }

    private void dictationComplete(DictationCompletionCause cause)
    {
        Debug.Log("DictationComplete: " + cause);
    }

    private void dictationHypothesis(string text)
    {
        Debug.Log("DictationHypothesis: " + text);
    }

    private void dictationError(string error, int hresult)
    {
        Debug.Log("DictationResult: " + error + ", " + hresult);
    }

    #endregion

    private void OnApplicationQuit()
    {
        if(phraseRecognizer != null && phraseRecognizer.IsRunning)
        {
            phraseRecognizer.Stop();
        }
        if (dictationRecognizer != null)
        {
            dictationRecognizer.Stop();
        }
    }
}
