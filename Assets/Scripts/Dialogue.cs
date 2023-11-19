using System;

[Serializable]
public class Dialogue
{
    public string[] lines;
    public string[] tutorialLines;   

    public string[] greetingLines;
    public string[] goodbyeLines;
    public string[] tips;   

    //Greeting -> Open Shop -> Goodbye -> Tip
    public string[] regularLines;    
}
