using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DromeEd
{
    public class INIConfig
    {
        public Dictionary<string, INISection> Sections = new Dictionary<string, INISection>(StringComparer.InvariantCultureIgnoreCase);

        public INISection this[string sectionName]
        {
            get
            {
                if (!Sections.ContainsKey(sectionName))
                    Sections.Add(sectionName, new INISection(sectionName));
                return Sections[sectionName];
            }
            set
            {
                if (!Sections.ContainsKey(sectionName))
                {
                    Sections.Add(sectionName, value);
                }
                else
                {
                    Sections[sectionName] = value;
                }
            }
        }

        public INIConfig()
        {

        }

        public INIConfig(string filename)
        {
            Read(filename, false);
        }

        public string GetValueOrDefault(string section, string key, string defaultValue = null)
        {
            if (!Sections.ContainsKey(section))
                return defaultValue;

            INISection s = Sections[section];
            if (!s.Keys.ContainsKey(key))
                return defaultValue;
            else
                return s.Keys[key];
        }

        public void Read(string filename, bool clearExisting = true)
        {
            if (!System.IO.File.Exists(filename))
                return;

            if (clearExisting)
                Sections.Clear();

            using (System.IO.StreamReader reader = new System.IO.StreamReader(filename))
            {
                INISection currentSection = new INISection(""); // Default unnamed section

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine().Trim();

                    if (line.Length == 0 || line.StartsWith(";"))
                        continue;

                    if (line.StartsWith("["))
                    {
                        // Read section header
                        Sections.Add(currentSection.Name, currentSection);
                        currentSection = new INISection(line.TrimStart('[').TrimEnd(']'));
                    }
                    else
                    {
                        // Read key
                        string[] parts = line.Split('='); // Assume no equals sign in value
                        currentSection.Keys.Add(parts[0], parts[1]);
                    }
                }
                Sections.Add(currentSection.Name, currentSection);
            }
        }

        public void Write(string filename)
        {
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(filename, false))
            {
                foreach (INISection section in Sections.Values)
                {
                    if (!String.IsNullOrEmpty(section.Name))
                        writer.WriteLine("\n[" + section.Name + "]");
                    foreach (KeyValuePair<string, string> pair in section.Keys)
                    {
                        writer.WriteLine(pair.Key + "=" + pair.Value);
                    }
                }
            }
        }
    }

    public class INISection
    {
        public string Name { get; }
        public Dictionary<string, string> Keys = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        public string this[string name]
        {
            get
            {
                return Keys.ContainsKey(name) ? Keys[name] : "";
            }
            set
            {
                if (Keys.ContainsKey(name))
                    Keys[name] = value;
                else
                    Keys.Add(name, value);
            }
        }

        public INISection(string name)
        {
            Name = name;
        }
    }
}
