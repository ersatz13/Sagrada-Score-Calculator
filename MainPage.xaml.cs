using System.Collections.ObjectModel;

namespace StainedGlass;

public partial class MainPage : ContentPage
{
	private const string ObjectiveBlank = "Blank";
	private const string ObjectiveShadeVariety = "Shade Variety (sets of one of each value anywhere 5 points)";
	private const string ObjectiveRowColorVariety = "Row Color Variety (rows with no repeated color 6 points)";
	private const string ObjectiveColorVariety = "Color Variety (sets of one of each color anywhere 4 points)";
	private const string ObjectiveColumnShadeVariety = "Column Shade Variety (columns with no repeated values 4 points)";
	private const string ObjectiveColorDiagonals = "Color Diagonals (count of diagonally adjacent same color dice)";
	private const string ObjectiveLightShade = "Light Shade (sets of 1 & 2 values anywhere each die only counts for 1 set, 2 points for each set)";
	private const string ObjectiveMediumShade = "Medium Shade (sets of 3 & 4 values anywhere each die only counts for 1 set, 2 points for each set)";
	private const string ObjectiveRowShadeVariety = "Row Shade Variety (rows with no repeated values, 5 points)";
	private const string ObjectiveDeepShades = "Deep Shades (sets of 5 & 6 values anywhere each die only counts for 1 set, 2 points for each set)";

	private const string PrivateShadesBlue = "Shades of Blue (private sum of values on blue dice)";
	private const string PrivateShadesRed = "Shades of Red (private sum of values on red dice)";
	private const string PrivateShadesGreen = "Shades of Green (private sum of values on green dice)";
	private const string PrivateShadesYellow = "Shades of Yellow (private sum of values on yellow dice)";
	private const string PrivateShadesPurple = "Shades of Purple (private sum of values on purple dice)";

	private string _publicScoreText = "Public objectives: 0";
	private string _privateScoreText = "Private objective: 0";
	private string _favorTokenScoreText = "Favor tokens: 0";
	private string _emptySpacePenaltyText = "Empty spaces: 0";
	private string _totalScoreText = "Total score: 0";

	public ObservableCollection<CellModel> Cells { get; } = new();
	public IList<string> ColorOptions { get; } = new List<string>
	{
		ObjectiveBlank,
		"Red",
		"Yellow",
		"Green",
		"Blue",
		"Purple"
	};
	public IList<string> ValueOptions { get; } = new List<string>
	{
		ObjectiveBlank,
		"1",
		"2",
		"3",
		"4",
		"5",
		"6"
	};
	public IList<string> PublicObjectiveOptions { get; } = new List<string>
	{
		ObjectiveBlank,
		ObjectiveShadeVariety,
		ObjectiveRowColorVariety,
		ObjectiveColorVariety,
		ObjectiveColumnShadeVariety,
		ObjectiveColorDiagonals,
		ObjectiveLightShade,
		ObjectiveMediumShade,
		ObjectiveRowShadeVariety,
		ObjectiveDeepShades
	};
	public IList<string> PrivateObjectiveOptions { get; } = new List<string>
	{
		ObjectiveBlank,
		PrivateShadesBlue,
		PrivateShadesRed,
		PrivateShadesGreen,
		PrivateShadesYellow,
		PrivateShadesPurple
	};

	public string? PublicObjective1 { get; set; }
	public string? PublicObjective2 { get; set; }
	public string? PublicObjective3 { get; set; }
	public string? PrivateObjective { get; set; }
	public string? FavorTokens { get; set; }

	public string PublicScoreText
	{
		get => _publicScoreText;
		set
		{
			if (_publicScoreText == value)
				return;

			_publicScoreText = value;
			OnPropertyChanged();
		}
	}

	public string PrivateScoreText
	{
		get => _privateScoreText;
		set
		{
			if (_privateScoreText == value)
				return;

			_privateScoreText = value;
			OnPropertyChanged();
		}
	}

	public string FavorTokenScoreText
	{
		get => _favorTokenScoreText;
		set
		{
			if (_favorTokenScoreText == value)
				return;

			_favorTokenScoreText = value;
			OnPropertyChanged();
		}
	}

	public string EmptySpacePenaltyText
	{
		get => _emptySpacePenaltyText;
		set
		{
			if (_emptySpacePenaltyText == value)
				return;

			_emptySpacePenaltyText = value;
			OnPropertyChanged();
		}
	}

	public string TotalScoreText
	{
		get => _totalScoreText;
		set
		{
			if (_totalScoreText == value)
				return;

			_totalScoreText = value;
			OnPropertyChanged();
		}
	}

	public MainPage()
	{
		InitializeComponent();
		BindingContext = this;

		for (var i = 0; i < 25; i++)
		{
			Cells.Add(new CellModel());
		}
	}

	private async void OnCalculateClicked(object? sender, EventArgs e)
	{
		var grid = BuildGrid();
		var publicScore = CalculatePublicObjectives(grid);
		var privateScore = CalculatePrivateObjective(grid);
		var favorScore = ParseFavorTokens(FavorTokens);
		var emptyPenalty = CountEmptySpaces(grid);
		var totalScore = publicScore + privateScore + favorScore - emptyPenalty;

		PublicScoreText = $"Public objectives: {publicScore}";
		PrivateScoreText = $"Private objective: {privateScore}";
		FavorTokenScoreText = $"Favor tokens: {favorScore}";
		EmptySpacePenaltyText = $"Empty spaces: -{emptyPenalty}";
		TotalScoreText = $"Total score: {totalScore}";

		await DisplayAlertAsync("Final Score", $"You score {totalScore} points.", "OK");
	}

	private DieCell?[,] BuildGrid()
	{
		var grid = new DieCell?[5, 5];

		for (var index = 0; index < Cells.Count; index++)
		{
			var row = index / 5;
			var col = index % 5;
			var cell = Cells[index];

			var value = ParseDieValue(cell.Value);
			var color = NormalizeColor(cell.Color);

			if (value is null && color is null)
			{
				grid[row, col] = null;
				continue;
			}

			grid[row, col] = new DieCell(value, color);
		}

		return grid;
	}

	private static int? ParseDieValue(string? value)
	{
		if (string.IsNullOrWhiteSpace(value))
			return null;

		if (!int.TryParse(value, out var parsed))
			return null;

		return parsed is >= 1 and <= 6 ? parsed : null;
	}

	private static string? NormalizeColor(string? color)
	{
		if (string.IsNullOrWhiteSpace(color) || color == ObjectiveBlank)
			return null;

		return color;
	}

	private static int ParseFavorTokens(string? value)
	{
		if (string.IsNullOrWhiteSpace(value))
			return 0;

		if (!int.TryParse(value, out var parsed))
			return 0;

		return parsed < 0 ? 0 : parsed;
	}

	private static int CountEmptySpaces(DieCell?[,] grid)
	{
		var empty = 0;

		foreach (var cell in grid)
		{
			if (cell is null || cell.Value is null || cell.Color is null)
				empty++;
		}

		return empty;
	}

	private int CalculatePublicObjectives(DieCell?[,] grid)
	{
		var score = 0;

		score += ScorePublicObjective(PublicObjective1, grid);
		score += ScorePublicObjective(PublicObjective2, grid);
		score += ScorePublicObjective(PublicObjective3, grid);

		return score;
	}

	private int ScorePublicObjective(string? objective, DieCell?[,] grid)
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

	private int CalculatePrivateObjective(DieCell?[,] grid)
	{
		if (string.IsNullOrWhiteSpace(PrivateObjective) || PrivateObjective == ObjectiveBlank)
			return 0;

		var targetColor = PrivateObjective switch
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

			if (cell.Color == targetColor)
				sum += cell.Value.Value;
		}

		return sum;
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
			var colors = new HashSet<string>();
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
				if (neighbor?.Color == color)
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

public sealed class CellModel
{
	public string? Value { get; set; }
	public string? Color { get; set; }
}

public sealed record DieCell(int? Value, string? Color);
