﻿using System.Collections.Generic;

namespace Akka.Hive.Examples.Domain
{
    public class MaterialFactory
    {
        public static Material CreateTable()
        {
            return new Material
            {
                Id = 1
                , Name = "Table"
                , AssemblyDuration = 1200
                , Quantity = 1
                , Materials = new List<Material> { new Material { Id = 2
                                                                , Name = "Leg"
                                                                , AssemblyDuration = 300
                                                                , ParrentMaterialID = 1
                                                                , Quantity = 4
                                                                , IsReady = true } }
                                                        };
        }
    }
}