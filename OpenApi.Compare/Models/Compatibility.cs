namespace OpenApi.Compare.Models
{
    /// <summary>
    /// <see cref="Compatibility"/> indicates how clients would be impacted by this change.
    /// </summary>
    public enum Compatibility
    {
        /// <summary>
        /// The change would not impact client behavior.
        /// </summary>
        Backwards = 0,

        /// <summary>
        /// The change would impact client behavior.
        /// </summary>
        Breaking,
    }
}