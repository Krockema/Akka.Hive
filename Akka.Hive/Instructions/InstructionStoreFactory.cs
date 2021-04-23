namespace Akka.Hive.Instructions
{
    /// <summary>
    /// Creates the required message store based on debugging requirements
    /// </summary>
    public static class InstructionStoreFactory
    {

        public static ICurrentInstructions CreateCurrent(bool debug)
        {
            return debug ? new InstructionStore() : new IntegerStore();
        }
        
        public static IFeatureInstructions CreateFeatureStore(bool debug)
        {
            return new FeatureStore();
        }
    }
}