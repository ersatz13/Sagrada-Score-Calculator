namespace StainedGlass.Core;

public static class ScoringEngine
{
	public const string ObjectiveBlank = "Blank";
	public const string ObjectiveShadeVariety = "Shade Variety (sets of one of each value anywhere 5 points)";
	public const string ObjectiveRowColorVariety = "Row Color Variety (rows with no repeated color 6 points)";
	public const string ObjectiveColorVariety = "Color Variety (sets of one of each color anywhere 4 points)";
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
		return ScorePublicObjective(objective1, grid)
		       + ScorePublicObjective(objective2, grid)
		       + ScorePublicObjective(objective3, grid);
	}

	public static int CalculatePrivateObjective(string? objective, DieCell?[,] grid)
	{
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
			if (cell is null || cell.Value is null || cell.Color is null)
				continue;

			if (string.Equals(cell.Color, targetColor, StringComparison.OrdinalIgnoreCase))
				sum += cell.Value.Value;
		}

		return sum;
	}

	public static int CountEmptySpaces(DieCell?[,] grid)
	{
		var empty = 0;

		foreach (var cell in grid)
		{
			if (cell is null || cell.Value is null || cell.Color is null)
				empty++;
		}

		return empty;
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

		for (var row = 0; row < 5; row++)
		{
			var colors = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			var complete = true;

			for (var col = 0; col < 5; col++)
			{
				var cell = grid[row, col];
				if (cell?.Color is null)
				{
					complete = false;
					break;
				}

				if (!colors.Add(cell.Color))
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
			if (cell?.Color is null)
				continue;

			if (colorCounts.ContainsKey(cell.Color))
				colorCounts[cell.Color]++;
		}

		var sets = colorCounts.Values.Min();
		return sets * 4;
	}

	private static int ScoreColumnShadeVariety(DieCell?[,] grid)
	{
		var score = 0;

		for (var col = 0; col < 5; col++)
		{
			var values = new HashSet<int>();
			var complete = true;

			for (var row = 0; row < 5; row++)
			{
				var cell = grid[row, col];
				if (cell?.Value is null)
				{
					complete = false;
					break;
				}

				if (!values.Add(cell.Value.Value))
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

		for (var row = 0; row < 5; row++)
		{
			for (var col = 0; col < 5; col++)
			{
				var cell = grid[row, col];
				if (cell?.Color is null)
					continue;

				if (HasDiagonalMatch(grid, row, col, cell.Color))
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

				if (newRow < 0 || newRow >= 5 || newCol < 0 || newCol >= 5)
					continue;

				var neighbor = grid[newRow, newCol];
				if (neighbor?.Color is not null && string.Equals(neighbor.Color, color, StringComparison.OrdinalIgnoreCase))
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

		for (var row = 0; row < 5; row++)
		{
			var values = new HashSet<int>();
			var complete = true;

			for (var col = 0; col < 5; col++)
			{
				var cell = grid[row, col];
				if (cell?.Value is null)
				{
					complete = false;
					break;
				}

				if (!values.Add(cell.Value.Value))
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
			if (cell?.Value is null)
				continue;

			var value = cell.Value.Value;
			if (value is >= 1 and <= 6)
				counts[value - 1]++;
		}

		return counts;
	}
}

public sealed record DieCell(int? Value, string? Color);
