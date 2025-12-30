using System;

namespace CodeHealthHub.Models.JsonTypes;

public class HistoricalMeasuresSearchResponse
{
    public Paging? Paging { get; set; }

    public List<MeasureHistory>? Measures { get; set; }
}
