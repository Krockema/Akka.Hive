using System;
using System.Collections.Generic;
using System.Text;

namespace SimTest.SimulationHelper
{
    /// <summary>
    /// Static helper class used to define paths to fixed-name actors
    /// (helps eliminate errors when using <see cref="ActorSelection"/>)
    /// </summary>
    public static class ActorPaths
    {
        public static readonly ActorMetaData SystemAgent = new ActorMetaData("SystemAgent");
        public static readonly ActorMetaData StorageDirectory = new ActorMetaData("StorageDirectory", SystemAgent);
        public static readonly ActorMetaData MediatorDirectory = new ActorMetaData("MediatorDirectory", SystemAgent);
        public static readonly ActorMetaData SimulationContext = new ActorMetaData("SimulationContext");

        // public static readonly ActorMetaData NestedValidatorActor = new ActorMetaData("childValidator", ValidatorActor);

    }
}
