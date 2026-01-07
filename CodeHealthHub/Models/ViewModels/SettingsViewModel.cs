using System;
using System.ComponentModel.DataAnnotations;

namespace CodeHealthHub.Models.ViewModels;

public class SettingsViewModel
{
    // Regex breakdown: 
    // ^      : Start of string
    // #      : Must start with #
    // [0-9a-fA-F]{6} : Exactly 6 hex characters (0-9, a-f, A-F)
    // $      : End of string
    private const string HexRegex = @"^#([0-9a-fA-F]{6})$";

    [Required]
    [RegularExpression(HexRegex, ErrorMessage = "Please enter a valid 6-digit hex color (e.g., #FFFFFF)")]
    public string DefaultColour { get; set; } = "#4e79a7";

    [Required]
    [RegularExpression(HexRegex, ErrorMessage = "Please enter a valid 6-digit hex color (e.g., #FFFFFF)")]
    public string InfoColour { get; set; } = "#59a14f";

    [Required]
    [RegularExpression(HexRegex, ErrorMessage = "Please enter a valid 6-digit hex color (e.g., #FFFFFF)")]
    public string MinorColour { get; set; } = "#f1ce63";

    [Required]
    [RegularExpression(HexRegex, ErrorMessage = "Please enter a valid 6-digit hex color (e.g., #FFFFFF)")]
    public string MajorColour { get; set; } = "#f28e2b";

    [Required]
    [RegularExpression(HexRegex, ErrorMessage = "Please enter a valid 6-digit hex color (e.g., #FFFFFF)")]
    public string CriticalColour { get; set; } = "#e15759";

    [Required]
    [RegularExpression(HexRegex, ErrorMessage = "Please enter a valid 6-digit hex color (e.g., #FFFFFF)")]
    public string BlockerColour { get; set; } = "#b07aa1";
}
