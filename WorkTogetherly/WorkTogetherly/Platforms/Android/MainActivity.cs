using Android.App;
using Android.Content.PM;
using Android.OS;

namespace WorkTogetherly
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            // Passing null prevents Android from restoring fragment state that MAUI can't resolve (jumpToStart crash)
            base.OnCreate(null);
        }
    }
}
