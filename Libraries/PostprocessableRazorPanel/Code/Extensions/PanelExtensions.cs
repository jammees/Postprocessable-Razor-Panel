namespace PostprocessPanel;

public static class PanelExtensions
{
	public static bool IsReady( this PostprocessablePanel panel )
	{
		return panel != null && panel.IsReady;
	}
}
