using Microsoft.CodeAnalysis.Scripting;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TrackingBugs.Models.ChartModels
{ 
public class PlotlyBarData
{
	public List<PlotlyBar> Data { get; set; }
}


public class PlotlyBar
{
	public string[] X { get; set; }
	public int[] Y { get; set; }
	public string Name { get; set; }
	public string Type { get; set; }
}
}
