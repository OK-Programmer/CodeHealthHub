using System;

namespace CodeHealthHub.Models;

public class MeasureSearchResponseComponent
{
    public string key { get; set; }
    public string name { get; set; }
    public string language { get; set; }
    public List<Measure> measures { get; set; }
}
