namespace Akka.Hive.Definitions
{
    /// <summary>
    /// Meta-data class. Nested/child actors can build path
    /// based on their parent(s) / position in hierarchy.
    /// </summary>
    public class ActorMetaData
    {
        public ActorMetaData(string name, ActorMetaData parent = null)
        {
            Name = name;
            Parent = parent;
            // if no parent, we assume a top-level actor
            var parentPath = parent != null ? parent.Path : "/user";
            Path = string.Format("{0}/{1}", parentPath, Name);
        }

        /// <summary>
        /// Name of the Actor
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Reference to the parent Actor
        /// </summary>
        public ActorMetaData Parent { get; set; }

        /// <summary>
        /// Actor path as string
        /// </summary>
        public string Path { get; private set; }
    }
}