using Narfox.Logging;
using System.Text;

namespace Narfox.Test.Logging;

public class ConsoleWriter : ILogger
{
    public LogLevel Level { get; set; } = LogLevel.Debug;
    public int PageWidth { get; set; } = 75;
    public int MenuPadding { get; set; } = 8;

    public bool IsAwaitingCancel { get; private set; } = false;

    public ConsoleColor DefaultColor { get; set; } = ConsoleColor.Blue;
    public ConsoleColor TitleColor { get; set; } = ConsoleColor.Cyan;
    public ConsoleColor H1Color { get; set; } = ConsoleColor.DarkCyan;
    public ConsoleColor H2Color { get; set; } = ConsoleColor.DarkCyan;
    public ConsoleColor PromptColor { get; set; } = ConsoleColor.Cyan;
    public ConsoleColor DebugColor { get; set; } = ConsoleColor.DarkGray;
    public ConsoleColor InfoColor { get; set; } = ConsoleColor.Gray;
    public ConsoleColor WarnColor { get; set; } = ConsoleColor.Yellow;
    public ConsoleColor ErrorColor { get; set; } = ConsoleColor.Red;
    public ConsoleColor AlertColor { get; set; } = ConsoleColor.Magenta;


    public ConsoleWriter()
    {
        Console.ForegroundColor = DefaultColor;
        Console.CancelKeyPress += OnConsoleCancelPressed;
    }

    private void OnConsoleCancelPressed(object? sender, ConsoleCancelEventArgs e)
    {
        e.Cancel = true;
        IsAwaitingCancel = true;
        WriteAlert("Cancel request detected...");
    }


    /// <summary>
    /// Writes a message that can be centered within the
    /// page width. Does not wrap the message and can
    /// overflow the page width.
    /// </summary>
    /// <param name="msg">The message to write</param>
    /// <param name="center">Whether to center the message</param>
    public void WriteMessage(string msg, bool center = false)
    {
        if (center)
        {
            var size = msg.Length;
            var pad = (int)((PageWidth - size) / 2f);
            if (pad > 0)
            {
                msg = new string(' ', pad) + msg;
            }
        }

        Console.WriteLine(msg);
    }

    /// <summary>
    /// Clears the screen and sets the cursor to the top
    /// left corner
    /// </summary>
    public void ClearScreen()
    {
        Console.Clear();
    }

    public void SetTextColor(ConsoleColor color)
    {
        Console.ForegroundColor = color;
    }

    public void ResetColor()
    {
        Console.ForegroundColor = DefaultColor;
    }


    /// <summary>
    /// Writes a divider by repeating the provided character
    /// to the edges of the class PageWidth
    /// </summary>
    /// <param name="c">A character to repeat forming a divider</param>
    public void WriteDivider(char c)
    {
        Console.WriteLine(new string(c, PageWidth));
    }

    /// <summary>
    /// Writes a page-wide divider with -
    /// </summary>
    public void WriteSingleDivider()
    {
        WriteDivider('-');
    }

    /// <summary>
    /// Writes a page-wide divider with =
    /// </summary>
    public void WriteDoubleDivider()
    {
        WriteDivider('=');
    }

    /// <summary>
    /// Writes a page-wide divider with *
    /// </summary>
    public void WriteStarDivider()
    {
        WriteDivider('*');
    }

    /// <summary>
    /// Writes a user prompt with special coloring
    /// and returns the first character key they
    /// press
    /// </summary>
    /// <param name="prompt">The prompt message</param>
    /// <returns>The character key the user pressed.</returns>
    public char PromptForCharacter(string prompt)
    {
        SetTextColor(PromptColor);
        WriteMessage(prompt);
        ResetColor();

        var result = Console.ReadKey();
        return result.KeyChar;
    }


    /// <summary>
    /// Writes a title
    /// </summary>
    /// <param name="msg">The title message</param>
    public void WriteTitle(string msg)
    {
        SetTextColor(TitleColor);
        WriteStarDivider();
        WriteMessage(msg.ToUpperInvariant(), true);
        WriteStarDivider();
        ResetColor();
    }

    /// <summary>
    /// Writes a heading
    /// </summary>
    /// <param name="msg">The heading message</param>
    public void WriteH1(string msg)
    {
        SetTextColor(H1Color);
        WriteDoubleDivider();
        WriteMessage(msg.ToUpperInvariant(), true);
        WriteDoubleDivider();
        ResetColor();
    }

    /// <summary>
    /// Writes a lower-priority heading
    /// </summary>
    /// <param name="msg">The heading message</param>
    public void WriteH2(string msg)
    {
        SetTextColor(H2Color);
        WriteMessage(msg.ToUpperInvariant(), true);
        WriteSingleDivider();
        SetTextColor(DefaultColor);
    }
    public void WriteAlert(string msg)
    {
        SetTextColor(AlertColor);
        WriteStarDivider();
        WriteParagraph(msg);
        WriteStarDivider();
        ResetColor();
    }


    /// <summary>
    /// Writes a paragraph that wraps to stay within
    /// the page width
    /// </summary>
    /// <param name="msg">The paragraph message</param>
    /// <param name="center">Whether to center the paragraph</param>
    public void WriteParagraph(string msg, bool center = false)
    {
        var words = msg.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var lines = new List<string>();
        var currentLine = new StringBuilder();

        foreach (var word in words)
        {
            // Check if adding the word would exceed the page width
            if (currentLine.Length + word.Length + 1 > PageWidth)
            {
                // Commit the current line
                lines.Add(currentLine.ToString().TrimEnd());
                currentLine.Clear();
            }

            currentLine.Append(word + " ");
        }

        // Add any remaining content
        if (currentLine.Length > 0)
            lines.Add(currentLine.ToString().TrimEnd());

        // Write each line using WriteMessage
        foreach (var line in lines)
        {
            WriteMessage(line, center);
        }
    }

    public void WriteMenu(string title, Dictionary<char, string> options)
    {
        WriteH1(title);
        foreach (var key in options.Keys)
        {
            var str = $"{key} - {options[key]}";
            if (str.Length + MenuPadding < PageWidth)
            {
                str = new string(' ', MenuPadding) + str;
            }
            WriteMessage(str);
        }
        WriteSingleDivider();
    }




    public void Debug(string msg)
    {
        WriteLog(LogLevel.Debug, msg);
    }

    public void Info(string msg)
    {
        WriteLog(LogLevel.Info, msg);
    }

    public void Warn(string msg)
    {
        WriteLog(LogLevel.Warn, msg);
    }

    public void Error(string msg)
    {
        WriteLog(LogLevel.Error, msg);
    }



    public void Purge() { /* noop */}

    public void Save() { /* noop */}



    void WriteLog(LogLevel level, string msg)
    {
        if (Level <= level)
        {
            var color = DebugColor;
            switch (level)
            {
                case LogLevel.Info: color = InfoColor; break;
                case LogLevel.Warn: color = WarnColor; break;
                case LogLevel.Error: color = ErrorColor; break;
            }

            SetTextColor(color);
            var time = DateTime.Now.ToString("g");
            msg = $"{level.ToString().ToUpperInvariant()} ({time}): {msg}";
            WriteMessage(msg);
            ResetColor();
        }
    }
}
