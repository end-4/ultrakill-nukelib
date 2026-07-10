namespace NukeLib.Scores;

/// <summary>
/// Stuff related to the leaderboard
/// </summary>
public static class LeaderboardHelper {
    /// <summary>
    /// Disables leaderboard score submissions for the current run
    /// </summary>
    public static void DisableLeaderboards() {
        // heeheha ass
        var assctl = MonoSingleton<AssistController>.Instance;
        if (assctl == null) return;
        assctl.cheatsEnabled = true;
    }
}
