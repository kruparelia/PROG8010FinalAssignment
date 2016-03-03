/*
    PROG8010 Group 2, Final Assignment: Dependency Manager
    Julia Aryal Sharma, 7375934
    Oscar Lucero, 7177884
    Kunal Ruparelia, 7128416    
    Charles Troster, 7388085
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dependencies
{
    class Package
    {
        public string Name { get; set; }
        public bool IsInstalled { get; set; }

        // Marks whether this package was explicitly added with the INSTALL command
        public bool Explicitly { get; set; }

        public List<Package> Needs { get; set; }
        public List<Package> NeededBy { get; set; }

        public Package(string name) {
            Name = name;
            Needs = new List<Package>();
            NeededBy = new List<Package>();
            IsInstalled = false;
            Explicitly = false;
        }

        public bool StillNeeded()
        {
            // Query for packages that need this one and are still installed
            var query = NeededBy.Where(x => x.IsInstalled);
            // Returns true if the query finds even one package, otherwise false
            return query.Any();
        }

    }
}
