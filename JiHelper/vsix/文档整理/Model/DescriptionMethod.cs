using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jisons
{
    public static class DescriptionMethod
    {
        public const string StartTitle = "/// <summary> ";
        public const string EndTitle = " </summary>";
        public const string SpaceTitle = "///           ";

        public static string Documentation(string description, int spacecolum = 0)
        {

            string startlinespace = " ";
            int starttext = description.IndexOf("///");
            starttext += spacecolum;
            for (int i = 0; i <= starttext; i++)
            {
                startlinespace += " ";
            }

            string newdescription = description;
            int starttextindex = description.IndexOf(">") + 1;
            int endtextindex = description.LastIndexOf("<");
            if (starttextindex < endtextindex)
            {
                var descripttext = new string(description.Skip(starttextindex).Take(endtextindex - starttextindex).ToArray());
                var lines = descripttext.Split(new string[] { "\r\n", "///" }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Count() <= 1)
                {
                    newdescription = new string(startlinespace.Skip(spacecolum).Take(starttext - spacecolum).ToArray()) + StartTitle + lines.FirstOrDefault().TrimStart().TrimEnd() + EndTitle;
                }
                else
                {
                    newdescription = new string(startlinespace.Skip(spacecolum).Take(starttext - spacecolum).ToArray()) + StartTitle + lines.FirstOrDefault().TrimStart().TrimEnd() + Environment.NewLine;
                    int startindex = lines.Count() - 1;
                    for (int i = 1; i <= startindex; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(lines[i]))
                        {
                            var t = lines[i].TrimStart().TrimEnd();
                            newdescription += new string(startlinespace.Take(startlinespace.Length - 3).ToArray()) + SpaceTitle + t + (i == startindex ? "" : Environment.NewLine);
                        }
                    }

                    if (newdescription.EndsWith("\r\n"))
                    {
                        newdescription = new string(newdescription.Take(newdescription.Length - 2).ToArray());
                    }
                    newdescription += EndTitle;
                }
            }

            return newdescription;
        }

    }
}
