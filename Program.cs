using System;
using System.Collections.Generic;
using System.Linq;
using System.IO; // Required for file operations

namespace Lab_Week_1_Friday
{
    // Alias for the map data type required by the IPathFinder interface
    using Grid = System.Int32[,];

    internal class FridayLabGame
    {
        // --- CONSTANTS ---
        const int roomrows = 12;
        const int roomcols = 12;
        const char EmptyCell = '.';
        const char PlayerCell = 'P';
        const char WaterCell = '2'; // User's custom char for Water
        const char WoodCell = '1';  // User's custom char for Wood
        const char WallCell = '#';
        const char ExitCell = 'E';
        const char PathCell = '*';
        const int WallCount = 15;
        const int WaterCount = 10;
        const int WoodCount = 10;

        // Constants for the Pathfinding Grid (1 for Wall, 0 for traversable)
        const int intMapWallValue = 1;
        const int intMapEmptyValue = 0;

        // --- CONSTANTS for Map Saving/Loading Format ---
        // Save format: 0=Wall, 1=Open Ground, 2=Wood, 3=Water
        const int MapValueOpenGround = 1;
        const int MapValueWood = 2;
        const int MapValueWater = 3;
        const int MapValueWall = 0;
        // ------------------------------------------

        static void Main(string[] args)
        {
            // Initial map creation (only done at program start)
            char[,] room = InitializeCharGrid(roomrows, roomcols, EmptyCell, WallCell, WaterCell, WoodCell, WallCount, WaterCount, WoodCount);

            int playerRow = roomrows / 2;
            int playerCol = roomcols / 2;

            // Exit is initialized to -1, indicating it is NOT set by default
            int exitRow = -1;
            int exitCol = -1;

            room[playerRow, playerCol] = PlayerCell;

            bool programOver = false;
            while (!programOver)
            {
                Console.Clear();
                Console.WriteLine("Main Menu:");
                Console.WriteLine("1. Play Game");
                Console.WriteLine("2. Edit Grid");
                Console.WriteLine("3. Load Map from Desktop");
                Console.WriteLine("Q. Quit");
                Console.Write("Choose an option: ");

                string choice = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

                if (choice == "1")
                {
                    // CHECK: Require user to place the exit before starting the game.
                    if (exitRow == -1 || exitCol == -1)
                    {
                        Console.WriteLine("\nError: The Exit ('E') must be placed in the editor or loaded before playing the game.");
                        Console.WriteLine("Press Enter to continue...");
                        Console.ReadLine();
                        continue;
                    }

                    GameLoop(room, PlayerCell, EmptyCell, WallCell, ExitCell, ref playerRow, ref playerCol, exitRow, exitCol);
                }
                else if (choice == "2")
                {
                    EditorLoop(room, WallCell, EmptyCell, PlayerCell, ExitCell, ref playerRow, ref playerCol, ref exitRow, ref exitCol);
                }
                else if (choice == "3") // LOGIC FOR LOADING MAP
                {
                    Console.Write("Enter map file name (e.g., map1.txt): ");
                    string filename = Console.ReadLine() ?? "";

                    // Attempt to load the map and update the game variables
                    LoadMapFromFile(filename, ref room, ref playerRow, ref playerCol, ref exitRow, ref exitCol);

                    Console.WriteLine("Press Enter to return to main menu.");
                    Console.ReadLine();
                }
                else if (choice == "Q")
                {
                    programOver = true;
                }
            }
        }

        // --- GAME LOOP ---
        static void GameLoop(
            char[,] room,
            char PlayerCell,
            char EmptyCell,
            char WallCell,
            char ExitCell,
            ref int playerRow,
            ref int playerCol,
            int exitRow,
            int exitCol)
        {
            bool gameOver = false;
            int moveCount = 0;

            var moveHistory = new LinkedList<Coordinate>();
            char oldCellContent = EmptyCell;

            while (!gameOver)
            {
                Console.Clear();
                Draw(room);

                Console.WriteLine();
                Console.WriteLine($"Current Pos: ({playerRow}, {playerCol}). Exit at: ({exitRow}, {exitCol}). Moves: {moveCount}");
                Console.WriteLine("WASD to move, U to undo. Q to quit.");
                Console.WriteLine("--- Pathfinding: F (DFS), B (BFS), H (Hill Climbing) ---");

                var key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.Q) { gameOver = true; continue; }

                if (key == ConsoleKey.U)
                {
                    if (moveHistory.Count() > 0)
                    {
                        Coordinate last = new Coordinate();
                        moveHistory.GetFront(ref last);
                        moveHistory.popFront();

                        char contentToRestoreUnderPlayer = room[last.Row, last.Col];

                        room[playerRow, playerCol] = oldCellContent;
                        playerRow = last.Row;
                        playerCol = last.Col;
                        room[playerRow, playerCol] = PlayerCell;
                        oldCellContent = contentToRestoreUnderPlayer;

                        if (moveCount > 0) moveCount--;
                    }
                    continue;
                }

                if (key == ConsoleKey.F || key == ConsoleKey.B || key == ConsoleKey.H)
                {
                    if (exitRow < 0 || exitCol < 0)
                    {
                        Console.WriteLine("Error: Exit 'E' must be placed first! Press Enter.");
                        Console.ReadLine();
                        continue;
                    }

                    Algorithm algoType = (key == ConsoleKey.F) ? Algorithm.DepthFirst :
                                         (key == ConsoleKey.B) ? Algorithm.BreadthFirst :
                                         Algorithm.HillClimbing;

                    PathFinderInterface pathfinder = PathFinderFactory.NewPathFinder(algoType);

                    // Uses the simplified grid for pathfinding (1=Wall, 0=Open)
                    Grid intMap = ConvertCharGridToIntGrid(room, WallCell, intMapWallValue, intMapEmptyValue);
                    LinkedList<Coordinate> path = new LinkedList<Coordinate>();
                    Coordinate startCoord = new Coordinate(playerRow, playerCol);
                    Coordinate goalCoord = new Coordinate(exitRow, exitCol);

                    bool found = pathfinder.FindPath(intMap, startCoord, goalCoord, ref path);

                    Console.Clear();

                    if (found)
                    {
                        VisualizePath(room, path, PlayerCell, ExitCell, PathCell);
                        Draw(room);
                        Console.WriteLine($"\nPath using {algoType} found! Path length: {path.Count()} (highlighted with '{PathCell}').");
                    }
                    else
                    {
                        Draw(room);
                        Console.WriteLine($"\nPath using {algoType} failed to find a solution.");
                    }

                    Console.WriteLine("Press Enter to continue and clear the visualization...");
                    Console.ReadLine();

                    CleanupVisualization(room, startCoord, goalCoord, EmptyCell, PlayerCell, ExitCell);
                    continue;
                }

                // --- WASD movement logic ---
                int newPlayerRow = playerRow;
                int newPlayerCol = playerCol;

                if (key == ConsoleKey.W) newPlayerRow--;
                else if (key == ConsoleKey.S) newPlayerRow++;
                else if (key == ConsoleKey.A) newPlayerCol--;
                else if (key == ConsoleKey.D) newPlayerCol++;

                if (IsMoveAllowed(room, newPlayerRow, newPlayerCol, WallCell))
                {
                    char contentOfNewCell = room[newPlayerRow, newPlayerCol];

                    moveHistory.pushFront(new Coordinate(playerRow, playerCol));

                    int oldPlayerRow = playerRow;
                    int oldPlayerCol = playerCol;

                    room[oldPlayerRow, oldPlayerCol] = oldCellContent;
                    playerRow = newPlayerRow;
                    playerCol = newPlayerCol;
                    room[playerRow, playerCol] = PlayerCell;
                    oldCellContent = contentOfNewCell;
                    moveCount++;

                    if (oldCellContent == ExitCell)
                    {
                        Console.Clear();
                        Draw(room);
                        Console.WriteLine($"\nCongratulations! You reached the exit in {moveCount} moves!");
                        Console.WriteLine("Press Enter to return to main menu.");
                        Console.ReadLine();
                        gameOver = true;
                    }
                }
            }
        }

        // --- EDITOR HELPER FUNCTIONS ---
        static void EditorLoop(char[,] room, char WallCell, char EmptyCell, char PlayerCell, char ExitCell, ref int playerRow, ref int playerCol, ref int exitRow, ref int exitCol)
        {
            bool editorOver = false;
            while (!editorOver)
            {
                Console.Clear();
                Draw(room);
                Console.WriteLine();
                Console.WriteLine($"Current Start: ({playerRow}, {playerCol}). Current Exit: ({exitRow}, {exitCol})");
                Console.WriteLine("Editor Commands:");
                Console.WriteLine("O row col   - Toggle wall at (row, col)");
                Console.WriteLine("S row col   - Place Start (player) at (row, col)");
                Console.WriteLine("E row col   - Place Exit at (row, col)");
                Console.WriteLine("V filename  - Save Map to File (e.g., V map1.txt)");
                Console.WriteLine("Q           - Quit editor");

                string input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) continue;

                var parts = input.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                string cmd = parts[0].ToUpperInvariant();

                // Handle the Save command (V)
                if (cmd == "V" && parts.Length == 2)
                {
                    string filename = parts[1];
                    SaveMapToFile(filename, room, playerRow, playerCol, exitRow, exitCol);
                    Console.WriteLine($"Map successfully saved to the Desktop as {filename}. Press Enter.");
                    Console.ReadLine();
                }
                // Handle commands O, S, E, Q
                else if (TryParseCommand(input, out cmd, out int a, out int b))
                {
                    if (cmd == "Q")
                    {
                        editorOver = true;
                    }
                    else if (cmd == "O" && IsInBounds(room, a, b))
                    {
                        ToggleWall(room, WallCell, EmptyCell, a, b, PlayerCell, ExitCell);
                    }
                    else if (cmd == "S" && IsInBounds(room, a, b) && IsFreeOfElements(room, a, b, WallCell, ExitCell))
                    {
                        PlaceStart(room, PlayerCell, EmptyCell, ref playerRow, ref playerCol, a, b);
                    }
                    else if (cmd == "E" && IsInBounds(room, a, b) && IsFreeOfElements(room, a, b, WallCell, PlayerCell))
                    {
                        PlaceExit(room, ExitCell, EmptyCell, ref exitRow, ref exitCol, a, b);
                    }
                }
                else
                {
                    Console.WriteLine("Invalid command or coordinate. Press Enter to continue.");
                    Console.ReadLine();
                }
            }
        }

        // --- MAP LOADING AND SAVING FUNCTIONS ---

        // Helper to get the absolute path to the user's Desktop for save/load
        static string GetDefaultMapSavePath(string filename)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            return Path.Combine(desktopPath, filename);
        }

        // Helper to reverse the conversion logic used for saving
        static char ConvertMapIntToChar(int val)
        {
            switch (val)
            {
                case MapValueWall: return WallCell;          // 0 -> #
                case MapValueOpenGround: return EmptyCell;   // 1 -> .
                case MapValueWood: return WoodCell;          // 2 -> 1
                case MapValueWater: return WaterCell;        // 3 -> 2
                default: return EmptyCell;
            }
        }

        static void LoadMapFromFile(string filename, ref char[,] room, ref int playerRow, ref int playerCol, ref int exitRow, ref int exitCol)
        {
            string fullPath = GetDefaultMapSavePath(filename); // Use the same path resolution as saving

            try
            {
                // Read all lines from the file
                string[] lines = File.ReadAllLines(fullPath);

                if (lines.Length < 4)
                {
                    Console.WriteLine("\nError: Map file is too short or corrupted (expected header + grid data).");
                    return;
                }

                int currentLine = 0;

                // 1. Line 1: Dimensions (ROWS COLS)
                string[] dimParts = lines[currentLine++].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (dimParts.Length != 2 || !int.TryParse(dimParts[0], out int newRows) || !int.TryParse(dimParts[1], out int newCols))
                {
                    Console.WriteLine("\nError: Cannot parse map dimensions from line 1.");
                    return;
                }

                // 2. Line 2: Player Start (P_ROW P_COL)
                string[] playerParts = lines[currentLine++].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (playerParts.Length != 2 || !int.TryParse(playerParts[0], out int pRow) || !int.TryParse(playerParts[1], out int pCol))
                {
                    Console.WriteLine("\nError: Cannot parse player start position from line 2.");
                    return;
                }

                // 3. Line 3: Exit Target (E_ROW E_COL)
                string[] exitParts = lines[currentLine++].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (exitParts.Length != 2 || !int.TryParse(exitParts[0], out int eRow) || !int.TryParse(exitParts[1], out int eCol))
                {
                    Console.WriteLine("\nError: Cannot parse exit position from line 3.");
                    return;
                }

                // --- Data is Valid: Reinitialize Grid ---

                char[,] newRoom = new char[newRows, newCols];

                // 4. Remaining Lines: Map Data (Integer Grid)
                for (int r = 0; r < newRows; r++)
                {
                    if (currentLine >= lines.Length)
                    {
                        Console.WriteLine("\nError: Map data incomplete.");
                        return;
                    }

                    string[] cellValues = lines[currentLine++].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (cellValues.Length != newCols)
                    {
                        Console.WriteLine($"\nError: Row {r} has {cellValues.Length} columns, expected {newCols}.");
                        return;
                    }

                    for (int c = 0; c < newCols; c++)
                    {
                        if (!int.TryParse(cellValues[c], out int val))
                        {
                            Console.WriteLine($"\nError: Invalid integer in map data at ({r}, {c}).");
                            return;
                        }

                        // Convert integer value back to game character
                        newRoom[r, c] = ConvertMapIntToChar(val);
                    }
                }

                // --- Update Game State ---

                // Update the main game variables
                room = newRoom;
                playerRow = pRow;
                playerCol = pCol;
                exitRow = eRow;
                exitCol = eCol;

                // Place the Player and Exit on the map, overwriting the underlying terrain
                if (IsInBounds(room, playerRow, playerCol))
                {
                    room[playerRow, playerCol] = PlayerCell;
                }
                if (exitRow != -1 && exitCol != -1 && IsInBounds(room, exitRow, exitCol))
                {
                    room[exitRow, exitCol] = ExitCell;
                }

                Console.WriteLine($"\nSuccessfully loaded map: {newRows}x{newCols}. Player at ({playerRow}, {playerCol}).");

            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"\nError: Map file '{filename}' not found at {fullPath}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nAn unexpected error occurred during loading: {ex.Message}");
            }
        }

        static void SaveMapToFile(string filename, char[,] room, int playerRow, int playerCol, int exitRow, int exitCol)
        {
            // 1. Get the absolute path using the Desktop folder
            string fullPath = GetDefaultMapSavePath(filename);

            // 2. Convert the character map to the required integer format
            int[,] mapToSave = ConvertCharGridToSaveFormat(room);
            int rows = room.GetLength(0);
            int cols = room.GetLength(1);

            try
            {
                // Saves files to desktop
                using (StreamWriter writer = new StreamWriter(fullPath))
                {
                    // Line 1: Dimensions (ROWS COLS)
                    writer.WriteLine($"{rows} {cols}");

                    // Line 2: Player Start (ROW COL)
                    writer.WriteLine($"{playerRow} {playerCol}");

                    // Line 3: Exit Target (ROW COL). Saved as -1 -1 if not set.
                    writer.WriteLine($"{exitRow} {exitCol}");

                    // Remaining lines: The map data
                    for (int r = 0; r < rows; r++)
                    {
                        string rowString = string.Join(" ", Enumerable.Range(0, cols).Select(c => mapToSave[r, c].ToString()));
                        writer.WriteLine(rowString);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError saving map to file {fullPath}: {ex.Message}");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
            }
        }

        // Converts the character grid to the required integer format for saving (0, 1, 2, 3)
        static int[,] ConvertCharGridToSaveFormat(char[,] charGrid)
        {
            int rows = charGrid.GetLength(0);
            int cols = charGrid.GetLength(1);
            int[,] intGrid = new int[rows, cols];

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    char cell = charGrid[r, c];
                    int value;

                    // Map characters to integer values
                    if (cell == WallCell) { value = MapValueWall; }      // '#' -> 0
                    else if (cell == WoodCell) { value = MapValueWood; }  // '1' -> 2
                    else if (cell == WaterCell) { value = MapValueWater; } // '2' -> 3

                    // P and E locations are saved as the underlying terrain (Open Ground, 1)
                    else if (cell == PlayerCell || cell == ExitCell)
                    {
                        value = MapValueOpenGround;
                    }
                    else // Includes EmptyCell ('.')
                    {
                        value = MapValueOpenGround; // '.' -> 1
                    }

                    intGrid[r, c] = value;
                }
            }
            return intGrid;
        }



        static char[,] InitializeCharGrid(int rows, int cols, char empty, char wall, char water, char wood, int wallCount, int waterCount, int woodCount)
        {
            char[,] grid = new char[rows, cols];
            Random rnd = new Random();

            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    grid[r, c] = empty;

            int playerRow = rows / 2;
            int playerCol = cols / 2;

            Action<char, int> placeCells = (cellChar, count) =>
            {
                int placed = 0;
                while (placed < count)
                {
                    int r = rnd.Next(rows);
                    int c = rnd.Next(cols);
                    if (grid[r, c] == empty && !(r == playerRow && c == playerCol))
                    {
                        grid[r, c] = cellChar;
                        placed++;
                    }
                }
            };

            placeCells(wall, wallCount);
            placeCells(water, waterCount);
            placeCells(wood, woodCount);

            return grid;
        }

        // This is the conversion used for PATHFINDING (0 or 1), NOT saving/loading.
        static Grid ConvertCharGridToIntGrid(char[,] charGrid, char wallChar, int wallVal, int emptyVal)
        {
            int rows = charGrid.GetLength(0);
            int cols = charGrid.GetLength(1);

            Grid intGrid = new int[rows, cols];

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    // Pathfinding only cares about walls vs. everything else
                    intGrid[r, c] = (charGrid[r, c] == wallChar) ? wallVal : emptyVal;
                }
            }
            return intGrid;
        }

        static void VisualizePath(char[,] room, LinkedList<Coordinate> path, char PlayerCell, char ExitCell, char PathCell)
        {
            var tempPath = new List<Coordinate>();

            while (path.Count() > 0)
            {
                Coordinate coord = new Coordinate();
                path.GetFront(ref coord);
                tempPath.Add(coord);
                path.popFront();
            }
            tempPath.Reverse();

            foreach (var coord in tempPath)
            {
                if (room[coord.Row, coord.Col] != PlayerCell && room[coord.Row, coord.Col] != ExitCell)
                {
                    room[coord.Row, coord.Col] = PathCell;
                }
            }

            foreach (var coord in tempPath.AsEnumerable().Reverse())
            {
                path.pushFront(coord);
            }
        }

        static void CleanupVisualization(char[,] room, Coordinate start, Coordinate target, char EmptyCell, char PlayerCell, char ExitCell)
        {
            int rows = room.GetLength(0);
            int cols = room.GetLength(1);
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (room[r, c] == PathCell) { room[r, c] = EmptyCell; }
                }
            }
            room[start.Row, start.Col] = PlayerCell;
            if (target.Row >= 0 && target.Col >= 0) room[target.Row, target.Col] = ExitCell;
        }

        static void Draw(char[,] grid)
        {
            int rows = grid.GetLength(0);
            int cols = grid.GetLength(1);
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++) { Console.Write(grid[row, col]); }
                Console.WriteLine();
            }
        }

        static bool IsMoveAllowed(char[,] grid, int row, int col, char wallCell)
        {
            return IsInBounds(grid, row, col) && grid[row, col] != wallCell;
        }

        static bool IsInBounds(char[,] room, int row, int col)
        {
            return row >= 0 && row < room.GetLength(0) && col >= 0 && col < room.GetLength(1);
        }

        static void ToggleWall(char[,] room, char WallCell, char EmptyCell, int row, int col, char PlayerCell, char ExitCell)
        {
            if (room[row, col] == PlayerCell || room[row, col] == ExitCell) return;
            if (room[row, col] == WallCell) room[row, col] = EmptyCell;
            else if (room[row, col] == EmptyCell) room[row, col] = WallCell;
        }

        static void PlaceStart(char[,] room, char PlayerCell, char EmptyCell, ref int playerRow, ref int playerCol, int newRow, int newCol)
        {
            room[playerRow, playerCol] = EmptyCell;
            playerRow = newRow;
            playerCol = newCol;
            room[playerRow, playerCol] = PlayerCell;
        }

        static void PlaceExit(char[,] room, char ExitCell, char EmptyCell, ref int exitRow, ref int exitCol, int newRow, int newCol)
        {
            if (exitRow >= 0 && exitCol >= 0 && room[exitRow, exitCol] == ExitCell) room[exitRow, exitCol] = EmptyCell;
            exitRow = newRow;
            exitCol = newCol;
            room[exitRow, exitCol] = ExitCell;
        }

        static bool IsFreeOfElements(char[,] grid, int row, int col, char WallCell, char FixedCell)
        {
            return grid[row, col] != WallCell && grid[row, col] != FixedCell;
        }

        static bool TryParseCommand(string line, out string cmd, out int a, out int b)
        {
            cmd = "";
            a = b = 0;
            if (string.IsNullOrWhiteSpace(line)) return false;
            var parts = line.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            cmd = parts[0].ToUpperInvariant();
            if (parts.Length == 3 && int.TryParse(parts[1], out a) && int.TryParse(parts[2], out b)) return true;

            return parts.Length == 1;
        }
    }
}