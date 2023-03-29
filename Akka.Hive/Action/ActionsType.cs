namespace Akka.Hive.Action
{
    /// <summary>
    /// Possible Actor action implementations
    /// </summary>
    public enum ActionsType
    {
        /// <summary>
        /// Custom action implementation
        /// </summary>
        Holon,
        /// <summary>
        /// Simulation action implementation
        /// </summary>
        Simulation,
        /// <summary>
        /// Sequencial simulation action implementation
        /// </summary>
        Sequencial
    }
}