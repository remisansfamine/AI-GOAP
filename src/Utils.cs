
internal class Utils
{
    public static void TimedWrite(string text, int sleepTime = 10)
    {
        foreach (char c in text)
        {
            Thread.Sleep(sleepTime);
            Console.Write(c);
        }
    }

    public static void DrawProgressBar(string stepDescription, int progress, int total)
    {
        Console.Write(stepDescription);

        int totalChunks = 30;

        int startOffset = stepDescription.Length;

        //draw empty progress bar
        Console.CursorLeft = startOffset;
        Console.Write("["); //start
        Console.CursorLeft = startOffset + totalChunks + 1;
        Console.Write("]"); //end

        Console.CursorLeft = startOffset + 1;

        double pctComplete = Convert.ToDouble(progress) / total;
        int numChunksComplete = Convert.ToInt16(totalChunks * pctComplete);

        // Draw completed chunks
        Console.BackgroundColor = ConsoleColor.Green;
        Console.Write("".PadRight(numChunksComplete));

        //draw incomplete chunks
        Console.BackgroundColor = ConsoleColor.Gray;
        Console.Write("".PadRight(totalChunks - numChunksComplete));
        Console.BackgroundColor = ConsoleColor.Black;

        // Draw totals
        Console.CursorLeft = totalChunks + 5;

        Console.CursorLeft = 0;
    }
}
