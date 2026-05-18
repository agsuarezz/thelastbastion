public static class GameOverStatsData
{
    public static int roundsCompleted;
    public static int enemiesKilled;
    public static int goldEarned;
    public static int towersPlaced;
    public static float timePlayed;

    public static void ResetStats()
    {
        roundsCompleted = 0;
        enemiesKilled = 0;
        goldEarned = 0;
        towersPlaced = 0;
        timePlayed = 0f;
    }
}