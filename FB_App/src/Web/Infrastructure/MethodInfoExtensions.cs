using System.Reflection;

namespace FB_App.Web.Infrastructure;

public static class MethodInfoExtensions
{
    private static readonly char[] InvalidChars_ = ['<', '>'];
    public static bool IsAnonymous(this MethodInfo method)
    {
        return method.Name.Any(InvalidChars_.Contains);
    }

    public static void AnonymousMethod(this IGuardClause guardClause, Delegate input)
    {
        if (input.Method.IsAnonymous())
            throw new ArgumentException("The endpoint name must be specified when using anonymous handlers.");
    }
}
