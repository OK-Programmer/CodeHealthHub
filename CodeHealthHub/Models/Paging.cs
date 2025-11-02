using System;

namespace CodeHealthHub.Models;

public class Paging
{
    public int pageIndex { get; set; }
    public int pageSize { get; set; }
    public int total { get; set; }
}
