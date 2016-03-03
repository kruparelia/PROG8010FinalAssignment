/*
    PROG8010 Group 2, Final Assignment: Dependency Manager
    Julia Aryal Sharma, 7375934
    Oscar Lucero, 7177884
    Kunal Ruparelia, 7128416    
    Charles Troster, 7388085
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dependencies
{
    class DependencyModel
    {
        // use \t as Tab to indent outputs
        const string MSG_INSTALLING = "\tInstalling {0}";
        const string MSG_NEEDED = "\t{0} is still needed.";
        const string MSG_REMOVING = "\tRemoving {0}";
        const string MSG_ALREADY_INSTALLED = "\t{0} is already installed.";
        const string MSG_NOT_INSTALLED = "\t{0} is not installed.";
        const string MSG_UNRECOGNIZED = "\tUnrecognized command {0}";
        
        Dictionary<string, Package> allPkg = new Dictionary<string, Package>();
        
        private ObservableCollection<string> outputs = new ObservableCollection<string>();
        public ObservableCollection<string> Outputs
        {
            get { return outputs; }
            set { outputs = value; }
        }
        
        // Accepts a string token and gets the matching Package from a Dictionary if it exists.
        // If the Package does not exist yet, it creates a new Package and adds it to the dictionary.
        Package getPkg (string token)
        {
            Package p;
            // Use uppercase for dictionary key and lookup so it's case-insensitive
            string upperToken = token.ToUpper();
            
            // Check dictionary for token (in uppercase), store in p if successful.
            // Otherwise create the package and index it in the Dictionary
            if (!allPkg.TryGetValue(upperToken, out p))
            {
                // Store original capitalization in Package name but index it by uppercase
                p = new Package(token);
                allPkg[upperToken] = p;
            }
            return p;
        }

        public void ParseFile(string filePath)
        {
            string[] allLines = File.ReadAllLines(filePath);
            // Interpret the lines one by one
            foreach (string line in allLines)
            {
                bool b = readCommand(line);
                // readCommand returns false if the command signals to stop reading the file
                if (!b)
                    break;
            }
        }

        bool readCommand (string line)
        {
            bool b = true;
            // Echo the input line
            Outputs.Add(line);
            // Array of strings split by whitespace
            string[] tokens = line.Split(' ');
          
            switch (tokens[0].ToUpper())
            {
                case "DEPEND":
                    // Send entire array to DEPEND command
                    depend(tokens);
                    break;
                case "INSTALL":
                    // Send the token after INSTALL and ignore the rest
                    if (tokens.Length > 1)
                        install(tokens[1]);
                    break;
                case "REMOVE":
                    // Send the token after REMOVE and ignore the rest
                    if (tokens.Length > 1)
                        remove(tokens[1]);
                    break;
                case "LIST":
                    listPkgs();
                    break;
                case "END":
                    // Send a "false" back to terminate reads after END command
                    b = false;
                    break;
                default:
                    Outputs.Add( String.Format( MSG_UNRECOGNIZED, tokens[0] ) );
                    break;
            }
            return b;
        }

        void depend(string[] tokens)
        {
            // Skip the DEPEND token, and get a list of packages matching remaining tokens
            List<Package> pkgList = tokens.Skip(1).Select(x => getPkg(x)).ToList();
            
            // Check there's at least one package and one dependency
            int pCount = pkgList.Count();
            if (pCount > 1)
            {
                // First package is the one that depends on others
                Package thisPkg = pkgList[0];

                // Iterate over remaining packages
                for (int i=1; i < pCount; i++)
                {
                    Package neededPkg = pkgList[i];
                    // Add the needed package to thisPkg's dependency list
                    thisPkg.Needs.Add(neededPkg);
                    // Add to the needed package's list of what depends on it
                    neededPkg.NeededBy.Add(thisPkg);
                }
            }
        }

        void install(string token)
        {
            Package thisPkg = getPkg(token);
            // If package already installed, send error and immediately return
            if (thisPkg.IsInstalled)
            {
                Outputs.Add( String.Format (MSG_ALREADY_INSTALLED, token) );
                return;
            }

            // From here on we know thisPkg is not installed yet
            // Mark package as explicitly installed by user
            thisPkg.Explicitly = true;
            
            // Create install queue and add thisPkg
            List<Package> toInstall = new List<Package>();
            toInstall.Add(thisPkg);

            do
            {
                // Missing dependencies will get added to this list
                // Any packages that can't be installed in this iteration will get deferred to this list
                List<Package> installNext = new List<Package>();

                foreach (Package p in toInstall)
                {
                    // Find missing dependencies of p
                    var missing = p.Needs.Where(x => !x.IsInstalled);

                    // Install package if nothing is missing
                    // Ignore if already installed
                    if (!missing.Any() && !p.IsInstalled)
                    {
                        p.IsInstalled = true;
                        Outputs.Add( String.Format(MSG_INSTALLING, p.Name) );
                    }

                    // Add missing packages to next install iteration and add p for next time
                    if (missing.Any())
                    {
                        // Add missing *first* so they are installed before the package that depends on them!
                        installNext.AddRange(missing);
                        // Add p to installNext so we can try again next time
                        installNext.Add(p);
                    }
                }
                // Send the installNext list into the next iteration of this loop
                toInstall = installNext;
            } 
            while ( toInstall.Any() );
            // When toInstall is empty, toinstall.Any() returns false and the loop ends.
        }

        void remove(string token)
        {
            Package thisPkg = getPkg(token);
            // If package not installed, send error and immediately return
            if (!thisPkg.IsInstalled)
            {
                Outputs.Add( String.Format(MSG_NOT_INSTALLED, token) );
                return;
            }
            // If any still need thisPkg, send error and immediately return
            if (thisPkg.StillNeeded())
            {
                Outputs.Add( String.Format(MSG_NEEDED, token) );                
                return;
            }

            // From here on we know thisPkg is already installed and safe to remove
            // Remove explicitly installed flag
            thisPkg.Explicitly = false;

            List<Package> toRemove = new List<Package>();
            toRemove.Add(thisPkg);

            do
            {
                // Removable dependencies will get added to this list for next iteration
                List<Package> removeNext = new List<Package>();

                foreach (Package p in toRemove)
                {
                    // If p already removed, "continue" skips the rest of the loop and goes to the next
                    // This may happen if multiple packages had the same dependency
                    if (!p.IsInstalled)
                        continue;

                    // Remove the package and write to the output
                    p.IsInstalled = false;
                    Outputs.Add( String.Format(MSG_REMOVING, p.Name) );
                  
                    // Find any dependencies of p that are 1) installed but 2) not explicitly
                    // AND 3) not needed by any installed packages
                    var queryNext = p.Needs.Where( x => x.IsInstalled && !x.Explicitly && !x.StillNeeded() );
                    removeNext.AddRange(queryNext);
                }
                toRemove = removeNext;
                // Send removeNext list into the next iteration of the do loop
            }
            while (toRemove.Any());
            // When toRemove is empty, .Any() returns false and ends the loop.
        }

        void listPkgs()
        {
            // For each Package in the Dictionary, find all that are installed and select their names
            var installed = allPkg.Where(x => x.Value.IsInstalled).Select(x => x.Value.Name);
          
            foreach (string s in installed)
            {
                Outputs.Add("\t" + s);
            }
        }

    }
}
