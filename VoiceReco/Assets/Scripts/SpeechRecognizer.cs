using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

public enum ActionType { None, Turn };
public enum ObjectType { None, Cube };
public delegate void performActionOnObject(GameObject o);

public class SpeechRecognizer : MonoBehaviour {

    public GameObject target;
    public string[] keywords = new string[]{"turn"};
    public ConfidenceLevel confidenceLevel = ConfidenceLevel.Medium;

    protected PhraseRecognizer phraseRecognizer;
    protected string actionWord = "none";
    protected string objectWord = "none";
    protected ActionType action = ActionType.None;
    protected ObjectType obj    = ObjectType.None; 
    performActionOnObject performAction;

	// Use this for initialization
	void Start () {
        if (keywords != null && PhraseRecognitionSystem.isSupported)
        {
            Debug.Log(PhraseRecognitionSystem.isSupported);
            phraseRecognizer = new KeywordRecognizer(keywords, confidenceLevel);
            phraseRecognizer.OnPhraseRecognized += onPhraseRecognized;
            phraseRecognizer.Start();
            Debug.Log(PhraseRecognitionSystem.Status);
        } else
        {
            if (!PhraseRecognitionSystem.isSupported)
            {
                Debug.Log("Phrase Recognition seems to be not supported by your system. Please use Windows 10 and change your language settings to English.");
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void onPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        actionWord = args.text;
        //object = args.text....

        processActionName();
        processObjectName();

        if (performAction != null && target != null)
        {
            performAction(target);
        }
    }

    #region PhraseProcessor

    private void processActionName()
    {
        switch (actionWord)
        {
            case "turn":
                action = ActionType.Turn;
                performAction = rotateObject;
                break;
            default:
                action = ActionType.None;
                performAction = null;
                break;
        }
    }

    private void processObjectName()
    {
        switch (objectWord)
        {
            case "cube":
                obj = ObjectType.Cube;
                //target = cube
                break;
            default:
                obj = ObjectType.None;
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

    #endregion

    private void OnApplicationQuit()
    {
        if(phraseRecognizer != null && phraseRecognizer.IsRunning)
        {
            phraseRecognizer.Stop();
        }
    }
}
