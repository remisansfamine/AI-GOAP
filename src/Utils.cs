
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
}
