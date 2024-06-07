namespace SynthSharp;

public class Level
{
    public byte[,] Layout { get; set; }

    public Level(byte[,] layout)
    {
        Layout = layout;
    }
}