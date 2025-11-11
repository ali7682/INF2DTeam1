using System.Collections.Concurrent;

public static class SessionManager
{
    private static readonly ConcurrentDictionary<string, UserModel> Sessions = new();

    public static void AddSession(string token, UserModel User)
    {
        Sessions.TryAdd(token, User);
    }

    public static bool RemoveSession(string token)
    {
        if (Sessions.TryRemove(token, out _))
            return true;

        return false;
    }

    public static UserModel? GetSession(string token)
    {
        if (Sessions.ContainsKey(token))
            return Sessions[token];

        return null;
    }

    // AuthController gebruikte 'DoesSessionExist', maar hij bestond niet (gaf een error)
    public static bool DoesSessionExist(string token)
    {
        bool? exists = Sessions.ContainsKey(token);

        return exists != null && exists == true;
    }
}
