using aMazing;

MazeGenerator generator = new();

//Name of the maze + width + height (depends a bit on resolution of the screen)
generator.GenerateAndSave("maze.txt", 100, 25);

MazeRunner runner = new();
IDictionary<string, double> ret = runner.Run("maze.txt");

foreach (var (key, value) in ret)
{
    Console.WriteLine($"Bot: {key} -> Time in ms: {value / 1000}");
}
Console.ReadLine();