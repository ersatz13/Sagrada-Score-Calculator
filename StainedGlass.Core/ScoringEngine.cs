using System.Diagnostics.CodeAnalysis;

namespace StainedGlass.Core;

public static class ScoringEngine
{
	public const int Rows = 4;
	public const int Columns = 5;
	public const string ObjectiveBlank = "Blank";
	public const string ObjectiveShadeVariety = "Shade Variety (sets of one of each value anywhere 5 points)";
	public const string ObjectiveRowColorVariety = "Row Color Variety (rows with no repeated color 6 points)";
	public const string ObjectiveColorVariety = "Color Variety (sets of one of each color anywhere 4 points)";
	public const string ObjectiveColumnColorVariety = "Column Color Variety (columns with no repeated color 5 points)";
	public const string ObjectiveColumnShadeVariety = "Column Shade Variety (columns with no repeated values 4 points)";
	public const string ObjectiveColorDiagonals = "Color Diagonals (count of diagonally adjacent same color dice)";
	public const string ObjectiveLightShade = "Light Shade (sets of 1 & 2 values anywhere each die only counts for 1 set, 2 points for each set)";
	public const string ObjectiveMediumShade = "Medium Shade (sets of 3 & 4 values anywhere each die only counts for 1 set, 2 points for each set)";
	public const string ObjectiveRowShadeVariety = "Row Shade Variety (rows with no repeated values, 5 points)";
	public const string ObjectiveDeepShades = "Deep Shades (sets of 5 & 6 values anywhere each die only counts for 1 set, 2 points for each set)";

	public const string PrivateShadesBlue = "Shades of Blue (private sum of values on blue dice)";
	public const string PrivateShadesRed = "Shades of Red (private sum of values on red dice)";
	public const string PrivateShadesGreen = "Shades of Green (private sum of values on green dice)";
	public const string PrivateShadesYellow = "Shades of Yellow (private sum of values on yellow dice)";
	public const string PrivateShadesPurple = "Shades of Purple (private sum of values on purple dice)";

	public static int CalculatePublicObjectives(
		string? objective1,
		string? objective2,
		string? objective3,
		DieCell?[,] grid)
	{
		ValidateGrid(grid);
		return new[] { objective1, objective2, objective3 }.Distinct().Sum(objective => ScorePublicObjective(objective, grid));
	}

	public static int CalculatePrivateObjective(string? objective, DieCell?[,] grid)
	{
		ValidateGrid(grid);
		if (string.IsNullOrWhiteSpace(objective) || objective == ObjectiveBlank)
			return 0;

		var targetColor = objective switch
		{
			PrivateShadesBlue => "Blue",
			PrivateShadesRed => "Red",
			PrivateShadesGreen => "Green",
			PrivateShadesYellow => "Yellow",
			PrivateShadesPurple => "Purple",
			_ => null
		};

		if (targetColor is null)
			return 0;

		var sum = 0;

		foreach (var cell in grid)
		{
			if (!IsCompleteDie(cell))
				continue;

			if (string.Equals(cell.Color, targetColor, StringComparison.OrdinalIgnoreCase))
				sum += cell.Value!.Value;
		}

		return sum;
	}

	public static int CountEmptySpaces(DieCell?[,] grid)
	{
		ValidateGrid(grid);
		var empty = 0;

		foreach (var cell in grid)
		{
			if (!IsCompleteDie(cell))
				empty++;
		}

		return empty;
	}

	public static bool IsCompleteDie([NotNullWhen(true)] DieCell? cell) =>
		cell?.Value is >= 1 and <= 6 &&
		new[] { "Red", "Yellow", "Green", "Blue", "Purple" }.Contains(cell.Color, StringComparer.OrdinalIgnoreCase);

	private static void ValidateGrid(DieCell?[,] grid)
	{
		ArgumentNullException.ThrowIfNull(grid);
		if (grid.GetLength(0) != Rows || grid.GetLength(1) != Columns)
			throw new ArgumentException("A Sagrada window must have 4 rows and 5 columns.", nameof(grid));
	}

	public static string? GetInputError(DieCell?[,] grid, params string?[] objectives)
	{
		ValidateGrid(grid);
		foreach (var cell in grid)
			if (cell is not null && !IsCompleteDie(cell))
				return "Select both a value and a color for each die, or clear both for an empty space.";
		var selected = objectives.Where(o => !string.IsNullOrWhiteSpace(o) && o != ObjectiveBlank).ToArray();
		if (selected.Distinct().Count() != selected.Length)
			return "Choose three different public objectives.";
		return null;
	}

	public static int ParseFavorTokens(string? value)
	{
		if (string.IsNullOrWhiteSpace(value))
			return 0;

		if (!int.TryParse(value, out var parsed))
			return 0;

		return parsed < 0 ? 0 : parsed;
	}

	private static int ScorePublicObjective(string? objective, DieCell?[,] grid)
	{
		if (string.IsNullOrWhiteSpace(objective) || objective == ObjectiveBlank)
			return 0;

		return objective switch
		{
			ObjectiveShadeVariety => ScoreShadeVariety(grid),
			ObjectiveRowColorVariety => ScoreRowColorVariety(grid),
			ObjectiveColorVariety => ScoreColorVariety(grid),
			ObjectiveColumnColorVariety => ScoreColumnColorVariety(grid),
			ObjectiveColumnShadeVariety => ScoreColumnShadeVariety(grid),
			ObjectiveColorDiagonals => ScoreColorDiagonals(grid),
			ObjectiveLightShade => ScoreShadePair(grid, 1, 2),
			ObjectiveMediumShade => ScoreShadePair(grid, 3, 4),
			ObjectiveRowShadeVariety => ScoreRowShadeVariety(grid),
			ObjectiveDeepShades => ScoreShadePair(grid, 5, 6),
			_ => 0
		};
	}

	private static int ScoreShadeVariety(DieCell?[,] grid)
	{
		var counts = CountValues(grid);
		var sets = counts.Min();
		return sets * 5;
	}

	private static int ScoreRowColorVariety(DieCell?[,] grid)
	{
		var score = 0;

		for (var row = 0; row < Rows; row++)
		{
			var colors = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			var complete = true;

			for (var col = 0; col < Columns; col++)
			{
				var cell = grid[row, col];
				if (!IsCompleteDie(cell))
				{
					complete = false;
					break;
				}

				if (!colors.Add(cell.Color!))
				{
					complete = false;
					break;
				}
			}

			if (complete)
				score += 6;
		}

		return score;
	}

	private static int ScoreColorVariety(DieCell?[,] grid)
	{
		var colorCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
		{
			["Red"] = 0,
			["Yellow"] = 0,
			["Green"] = 0,
			["Blue"] = 0,
			["Purple"] = 0
		};

		foreach (var cell in grid)
		{
			if (!IsCompleteDie(cell))
				continue;

			if (colorCounts.ContainsKey(cell.Color!))
				colorCounts[cell.Color!]++;
		}

		var sets = colorCounts.Values.Min();
		return sets * 4;
	}

	private static int ScoreColumnColorVariety(DieCell?[,] grid)
	{
		var score = 0;
		for (var col = 0; col < Columns; col++)
		{
			var colors = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			for (var row = 0; row < Rows; row++)
			{
				var cell = grid[row, col];
				if (!IsCompleteDie(cell) || !colors.Add(cell.Color!))
					break;
				if (row == Rows - 1)
					score += 5;
			}
		}
		return score;
	}

	private static int ScoreColumnShadeVariety(DieCell?[,] grid)
	{
		var score = 0;

		for (var col = 0; col < Columns; col++)
		{
			var values = new HashSet<int>();
			var complete = true;

			for (var row = 0; row < Rows; row++)
			{
				var cell = grid[row, col];
				if (!IsCompleteDie(cell))
				{
					complete = false;
					break;
				}

				if (!values.Add(cell.Value!.Value))
				{
					complete = false;
					break;
				}
			}

			if (complete)
				score += 4;
		}

		return score;
	}

	private static int ScoreColorDiagonals(DieCell?[,] grid)
	{
		var score = 0;

		for (var row = 0; row < Rows; row++)
		{
			for (var col = 0; col < Columns; col++)
			{
				var cell = grid[row, col];
				if (!IsCompleteDie(cell))
					continue;

				if (HasDiagonalMatch(grid, row, col, cell.Color!))
					score++;
			}
		}

		return score;
	}

	private static bool HasDiagonalMatch(DieCell?[,] grid, int row, int col, string color)
	{
		for (var rowOffset = -1; rowOffset <= 1; rowOffset += 2)
		{
			for (var colOffset = -1; colOffset <= 1; colOffset += 2)
			{
				var newRow = row + rowOffset;
				var newCol = col + colOffset;

				if (newRow < 0 || newRow >= Rows || newCol < 0 || newCol >= Columns)
					continue;

				var neighbor = grid[newRow, newCol];
				if (IsCompleteDie(neighbor) && string.Equals(neighbor.Color, color, StringComparison.OrdinalIgnoreCase))
					return true;
			}
		}

		return false;
	}

	private static int ScoreShadePair(DieCell?[,] grid, int first, int second)
	{
		var counts = CountValues(grid);
		var sets = Math.Min(counts[first - 1], counts[second - 1]);
		return sets * 2;
	}

	private static int ScoreRowShadeVariety(DieCell?[,] grid)
	{
		var score = 0;

		for (var row = 0; row < Rows; row++)
		{
			var values = new HashSet<int>();
			var complete = true;

			for (var col = 0; col < Columns; col++)
			{
				var cell = grid[row, col];
				if (!IsCompleteDie(cell))
				{
					complete = false;
					break;
				}

				if (!values.Add(cell.Value!.Value))
				{
					complete = false;
					break;
				}
			}

			if (complete)
				score += 5;
		}

		return score;
	}

	private static int[] CountValues(DieCell?[,] grid)
	{
		var counts = new int[6];

		foreach (var cell in grid)
		{
			if (!IsCompleteDie(cell))
				continue;

			var value = cell.Value!.Value;
			if (value is >= 1 and <= 6)
				counts[value - 1]++;
		}

		return counts;
	}
}

public sealed record DieCell(int? Value, string? Color);
