using System.Diagnostics;

namespace ContinuousMonitory;

internal static class ActivityExtensions
{
    public static void EnrichWithName(this Activity activity, string name)
    {
        activity.SetTag("person.name", name);
    }
}
