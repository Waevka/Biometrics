using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;

public enum ActionType { None, Rotate, Color, Bigger, Smaller, Move };
public enum ObjectType { None, Cube, Sphere, Capsule };
public enum ColorType { None, Red, Yellow, Blue, Random };
public enum DirectionType { None, Up, Down, Left, Right };
public delegate void performActionOnObject(GameObject o);

public class SpeechRecognizer : MonoBehaviour
{

    [SerializeField]
    public GameObject targetObject = null;
    [SerializeField]
    performActionOnObject targetAction = null;

    private string[] actionKeywords = new string[]{
        "rotate", "color", "bigger", "smaller", "move", //actions
        "none", "red", "yellow", "blue", "random", //colors
        "up", "down", "left", "right"}; //directions
    private string[] objectKeywords = new string[]
    {
        "cube", "sphere", "capsule" //objects
    };

    public ConfidenceLevel confidenceLevel = ConfidenceLevel.Low;

    // Link existing objects in scene
    public GameObject cube;
    public GameObject sphere;
    public GameObject capsule;

    public Text usernameText;
    public Text authenticatedText;
    public GameObject disablePanel;
    public GameObject instructions;
    public Text lastCommand;
    //

    private bool userLoggedIn = false;
    private bool userAuthenticated = false;
    private string userName = "";
    //
    protected PhraseRecognizer actionPhraseRecognizer;
    protected PhraseRecognizer objectPhraseRecognizer;
    protected PhraseRecognizer colorPhraseRecognizer;
    protected PhraseRecognizer directionPhraseRecognizer;

    protected DictationRecognizer dictationRecognizer;
    protected string currentWord = "none";
    protected ActionType action = ActionType.None;
    protected ObjectType obj = ObjectType.None;
    protected ColorType col = ColorType.None;
    protected DirectionType dir = DirectionType.None;

    // Use this for initialization
    void Start()
    {
       

    }

    private void FixedUpdate()
    {
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

    // Update is called once per frame
    void Update()
    {

        if (userAuthenticated)
        {
            if (targetAction != null && targetObject != null)
            {
                bool actionPerformed = false;
                //check for 3 word command - currently only color and move
                if (action == ActionType.Color && col != ColorType.None)
                {
                    Debug.Log("Performing " + action + " on object: " + obj + " with color " + col);
                    targetAction(targetObject);
                    actionPerformed = true;
                }
                else if (action == ActionType.Move && dir != DirectionType.None)
                {
                    Debug.Log("Performing " + action + " on object: " + obj + " with direction " + dir);
                    targetAction(targetObject);
                    actionPerformed = true;
                }

                if (action != ActionType.Color && action != ActionType.Move)
                {
                    Debug.Log("Performing action: " + action + " on object: " + obj);
                    targetAction(targetObject);
                    actionPerformed = true;
                }

                if (actionPerformed)
                {
                    targetAction = null;
                    targetObject = null;
                    action = ActionType.None;
                    obj = ObjectType.None;
                    col = ColorType.None;
                    dir = DirectionType.None;
                }
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

    private void validateUser()
    {
        if (userName.Equals("Patryk") || userName.Equals("Bogna") || userName.Equals("Bartek") || userName.Equals("Karolina"))
        {
            userAuthenticated = true;
            Debug.Log("Authenticated user: " + userName);
            authenticatedText.text = "Yes";
            usernameText.text = userName;
            disablePanel.SetActive(false);
            instructions.SetActive(true);
            initializeSpeechRecognition();
        }
        else
        {
            Debug.Log("Username " + userName + " is not valid");
            userLoggedIn = false;
            usernameText.text = userName;
        }
    }

    private void initializeSpeechRecognition()
    {
        if (actionKeywords != null && PhraseRecognitionSystem.isSupported)
        {
            Debug.Log(PhraseRecognitionSystem.isSupported);
            /* IMPORTANT - only works in 32bit Editor or Build (bug in Unity/Windows that will not be fixed)
            dictationRecognizer = new DictationRecognizer();
            dictationRecognizer.DictationResult     += dictationResultWriteText;
            dictationRecognizer.DictationComplete   += dictationComplete;
            dictationRecognizer.DictationError      += dictationError;
            dictationRecognizer.DictationHypothesis += dictationHypothesis;
            dictationRecognizer.Start(); */
            actionPhraseRecognizer = new KeywordRecognizer(actionKeywords, confidenceLevel);
            actionPhraseRecognizer.OnPhraseRecognized += onPhraseRecognizedAction;
            actionPhraseRecognizer.Start();

            objectPhraseRecognizer = new KeywordRecognizer(objectKeywords, confidenceLevel);
            objectPhraseRecognizer.OnPhraseRecognized += onPhraseRecognizedObject;
            objectPhraseRecognizer.Start();

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

    private void onPhraseRecognizedAction(PhraseRecognizedEventArgs args)
    {
        currentWord = args.text;
        lastCommand.text = args.text;
        //object = args.text....
        processActionName();
        processColorName();
        processDirectionName();

        Debug.Log("Current word: " + currentWord +
            "| Current object: " + obj +
            ", current action: " + action);
    }

    private void onPhraseRecognizedObject(PhraseRecognizedEventArgs args)
    {
        processObjectName(args.text);
        lastCommand.text = args.text;
        Debug.Log("Current word: " + args.text +
            "| Current object: " + obj +
            ", current action: " + action);
    }

    #region PhraseProcessor

    private void processActionName()
    {
        switch (currentWord)
        {
            case "rotate":
                action = ActionType.Rotate;
                targetAction = rotateObject;
                break;
            case "color":
                action = ActionType.Color;
                targetAction = changeColor;
                break;
            case "bigger":
                action = ActionType.Bigger;
                targetAction = makeBigger;
                break;
            case "smaller":
                action = ActionType.Smaller;
                targetAction = makeSmaller;
                break;
            case "move":
                action = ActionType.Move;
                targetAction = moveObject;
                break;
            default:
                //action = ActionType.None;
                //performAction = null;
                break;
        }
    }

    private void processObjectName(string text)
    {
        switch (text)
        {
            case "cube":
                obj = ObjectType.Cube;
                targetObject = cube;
                break;
            case "sphere":
                obj = ObjectType.Sphere;
                targetObject = sphere;
                break;
            case "capsule":
                obj = ObjectType.Capsule;
                targetObject = capsule;
                break;
            default:
                //obj = ObjectType.None;
                //target = null;
                break;
        }
    }

    private void processColorName()
    {
        switch (currentWord)
        {
            case "none":
                col = ColorType.None;
                break;
            case "red":
                col = ColorType.Red;
                break;
            case "blue":
                col = ColorType.Blue;
                break;
            case "yellow":
                col = ColorType.Yellow;
                break;
            case "random":
                col = ColorType.Random;
                break;
        }

    }

    private void processDirectionName()
    {
        switch (currentWord)
        {
            case "up":
                dir = DirectionType.Up;
                break;
            case "down":
                dir = DirectionType.Down;
                break;
            case "left":
                dir = DirectionType.Left;
                break;
            case "right":
                dir = DirectionType.Right;
                break;
            default:
                break;
        }
    }

    #endregion

    #region ObjectActions

    private void rotateObject(GameObject o)
    {
        o.transform.Rotate(new Vector3(0, 0, 45.0f));
    }

    private void changeColor(GameObject o)
    {
        Color32 c = new Color32();

        switch (col)
        {
            case ColorType.None:
                c = Color.gray;
                break;
            case ColorType.Random:
                c = new Color32(
                    (byte)UnityEngine.Random.Range(0, 255),
                    (byte)UnityEngine.Random.Range(0, 255),
                    (byte)UnityEngine.Random.Range(0, 255),
                    255);
                break;
            case ColorType.Yellow:
                c = Color.yellow;
                break;
            case ColorType.Red:
                c = Color.red;
                break;
            case ColorType.Blue:
                c = Color.blue;
                break;
            default:
                break;
        }

        o.GetComponent<Renderer>().material.color = c;
    }

    private void makeBigger(GameObject o)
    {
        o.transform.localScale = new Vector3(o.transform.localScale.x + 0.25f,
                                             o.transform.localScale.y + 0.25f,
                                             o.transform.localScale.z + 0.25f);
    }

    private void makeSmaller(GameObject o)
    {
        o.transform.localScale = new Vector3(o.transform.localScale.x - 0.25f,
                                             o.transform.localScale.y - 0.25f,
                                             o.transform.localScale.z - 0.25f);
    }

    private void moveObject(GameObject o)
    {
        switch (dir)
        {
            case DirectionType.Up:
                o.transform.Translate(0, 1, 0);
                break;
            case DirectionType.Down:
                o.transform.Translate(0, -1, 0);
                break;
            case DirectionType.Left:
                o.transform.Translate(-1, 0, 0);
                break;
            case DirectionType.Right:
                o.transform.Translate(1, 0, 0);
                break;
            default:
                break;
        }
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
        if (actionPhraseRecognizer != null && actionPhraseRecognizer.IsRunning)
        {
            actionPhraseRecognizer.Stop();
            objectPhraseRecognizer.Stop();
        }
        if (dictationRecognizer != null)
        {
            dictationRecognizer.Stop();
        }
    }
}
