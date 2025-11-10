using System;

namespace CodeHealthHub.Models;

public class Paging
{
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
}
