using System.Collections.ObjectModel;
using StainedGlass.Core;

namespace StainedGlass;

public partial class MainPage : ContentPage
{
	private string _publicScoreText = "Public objectives: 0";
	private string _privateScoreText = "Private objective: 0";
	private string _favorTokenScoreText = "Favor tokens: 0";
	private string _emptySpacePenaltyText = "Empty spaces: 0";
	private string _totalScoreText = "Total score: 0";

	public ObservableCollection<CellModel> Cells { get; } = new();
	public IList<string> ColorOptions { get; } = new List<string>
	{
		ScoringEngine.ObjectiveBlank,
		"Red",
		"Yellow",
		"Green",
		"Blue",
		"Purple"
	};
	public IList<string> ValueOptions { get; } = new List<string>
	{
		ScoringEngine.ObjectiveBlank,
		"1",
		"2",
		"3",
		"4",
		"5",
		"6"
	};
	public IList<string> PublicObjectiveOptions { get; } = new List<string>
	{
		ScoringEngine.ObjectiveBlank,
		ScoringEngine.ObjectiveShadeVariety,
		ScoringEngine.ObjectiveRowColorVariety,
		ScoringEngine.ObjectiveColorVariety,
		ScoringEngine.ObjectiveColumnShadeVariety,
		ScoringEngine.ObjectiveColorDiagonals,
		ScoringEngine.ObjectiveLightShade,
		ScoringEngine.ObjectiveMediumShade,
		ScoringEngine.ObjectiveRowShadeVariety,
		ScoringEngine.ObjectiveDeepShades
	};
	public IList<string> PrivateObjectiveOptions { get; } = new List<string>
	{
		ScoringEngine.ObjectiveBlank,
		ScoringEngine.PrivateShadesBlue,
		ScoringEngine.PrivateShadesRed,
		ScoringEngine.PrivateShadesGreen,
		ScoringEngine.PrivateShadesYellow,
		ScoringEngine.PrivateShadesPurple
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
		var publicScore = ScoringEngine.CalculatePublicObjectives(PublicObjective1, PublicObjective2, PublicObjective3, grid);
		var privateScore = ScoringEngine.CalculatePrivateObjective(PrivateObjective, grid);
		var favorScore = ScoringEngine.ParseFavorTokens(FavorTokens);
		var emptyPenalty = ScoringEngine.CountEmptySpaces(grid);
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
		if (string.IsNullOrWhiteSpace(color) || color == ScoringEngine.ObjectiveBlank)
			return null;

		return color;
	}
}

public sealed class CellModel
{
	public string? Value { get; set; }
	public string? Color { get; set; }
}
