using Avalonia;
using Avalonia.Markup.Xaml;

namespace AluminumFoil.Mac
{
    public class App : Application
    {
        public static GoldLeaf.GoldLeaf GoldLeaf = new GoldLeaf.GoldLeaf(); // Buffalo buffalo Buffalo buffalo buffalo buffalo Buffalo buffalo
        public static TinFoil.TinFoil TinFoil = new TinFoil.TinFoil();

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
