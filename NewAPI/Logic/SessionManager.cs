static class SessionManager
{
    public static Dictionary<string, UserModel> Sessions = new();

    public static void AddSession(string token, UserModel User)
    {
        Sessions.Add(token, User);
    }

    public static bool RemoveSession(string token)
    {
        if (Sessions.Remove(token))
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
        return Sessions.ContainsKey(token);
    }
}