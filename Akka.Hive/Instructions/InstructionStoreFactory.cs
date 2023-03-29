namespace Akka.Hive.Instructions
{
    /// <summary>
    /// Creates the required message store based on debugging requirements
    /// </summary>
    public static class InstructionStoreFactory
    {

        public static ICurrentInstructions CreateCurrent(bool debug)
        {
            if (debug) return new InstructionStore();
                       return new IntegerStore();
        }

        public static ICurrentInstructions CreateSequencialCurrent()
        {
            return new SequenceStore();
        }

        public static IFeatureInstructions CreateFeatureStore(bool debug)
        {
            return new FeatureStore();
        }
    }
}