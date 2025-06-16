public class FgosCompetency
{
    public string Code { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }
    public string Direction { get; set; }
}

public class FgosIndicator
{
    public string IndicatorCode { get; set; }
    public string CompetencyCode { get; set; }
    public string Description { get; set; }
    public string Direction { get; set; }
}

public class ProfStandardFunction
{
    public string Code { get; set; }
    public string Description { get; set; }
    public string StandardName { get; set; }
}

public class ProfStandardAction
{
    public string Code { get; set; }
    public string Description { get; set; }
    public string StandardName { get; set; }
}
