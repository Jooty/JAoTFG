using System;

public class GConsoleCommand
{
    public string helpText;
    public Func<string, string> method;
    public string description;

    public GConsoleCommand(string description, Func<string, string> method, string helpText = null)
    {
        this.description = description;
        this.method = method;
        this.helpText = helpText;
    }

}
