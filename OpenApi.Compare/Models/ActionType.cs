namespace OpenApi.Compare.Models
{
    /// <summary>
    /// <see cref="ActionType"/> describes the action taken.
    /// </summary>
    public enum ActionType
    {
        /// <summary>
        /// The action resulted in a new element.
        /// </summary>
        Added = 0,

        /// <summary>
        /// The action modified an existing element.
        /// </summary>
        Modified,

        /// <summary>
        /// The action removed an existing element.
        /// </summary>
        Removed,
    }
}