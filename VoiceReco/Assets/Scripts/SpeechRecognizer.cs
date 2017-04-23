using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class SpeechRecognizer : MonoBehaviour {
    public GameObject target;

    public string[] keywords = new string[]{"turn"};
    public ConfidenceLevel confidenceLevel = ConfidenceLevel.Medium;
    protected PhraseRecognizer phraseRecognizer;
    protected string word = "none";

	// Use this for initialization
	void Start () {
        if (keywords != null)
        {
            Debug.Log(PhraseRecognitionSystem.isSupported);
            phraseRecognizer = new KeywordRecognizer(keywords, confidenceLevel);
            phraseRecognizer.OnPhraseRecognized += onPhraseRecognized;
            phraseRecognizer.Start();
            Debug.Log(PhraseRecognitionSystem.Status);
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void onPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        word = args.text;
        if(word == "turn")
        {
            turnCube();
        }
    }

    private void turnCube()
    {
        target.transform.Rotate(new Vector3(0,0,45.0f));
    }

    private void OnApplicationQuit()
    {
        if(phraseRecognizer != null && phraseRecognizer.IsRunning)
        {
            phraseRecognizer.Stop();
        }
    }
}
