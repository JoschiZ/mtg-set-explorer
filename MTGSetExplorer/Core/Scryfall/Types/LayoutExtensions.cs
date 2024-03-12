namespace MTGSetExplorer.Core.Scryfall.Types;

public static class LayoutExtensions
{
    public static bool HasMultipleFaces(this Layout layout)
    {
        return layout switch
        {
            Layout.Flip => true,
            Layout.Split => true,
            Layout.Transform => true,
            Layout.DoubleFacedToken => true,
            _ => false
        };
    }
}