using System;
using System.Collections.Generic;

namespace Akka.Hive.Examples.Domain
{
    public class MaterialRequest : IComparable<MaterialRequest>
    {
        private static int count = 1;
        public MaterialRequest(Material material, Dictionary<int, bool> childRequests,int parent, long due, bool isHead)
        {
            Id = GetCounter();
            Material = material;
            Parent = parent;
            ChildRequests = childRequests;
            Due = due;
            IsHead = isHead;
        }

        static int GetCounter()
        {
            return count++;
        }
        public bool IsHead { get; }
        public int Id { get;  }
        public Material Material { get;  }
        public int Parent { get; }
        public Dictionary<int, bool> ChildRequests { get; }
        public long Due { get; }

        public int CompareTo(MaterialRequest other)
        {
            if (this.Due < other.Due) return -1;
            else if (this.Due > other.Due) return 1;
            else return 0;
        }
    }
}
