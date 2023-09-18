namespace iSun.Workshop.Persistence.Extensions
{
    public static class ReverseTicksExtensions
    {
        /// <summary>
        /// Used to convert date time to reverse ticks (log tail pattern)
        /// </summary>
        public static string ToReverseTicks(this DateTime date) => (DateTime.MaxValue.Ticks - date.Ticks).ToString("d19");

        /// <summary>
        /// Used to convert reverse ticks (log tail pattern) back to DateTime
        /// </summary>
        public static DateTime FromReverseTicks(this string reverseTicks) => new DateTime(DateTime.MaxValue.Ticks - long.Parse(reverseTicks), DateTimeKind.Utc);
    }
}
