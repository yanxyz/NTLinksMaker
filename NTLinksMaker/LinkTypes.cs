using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace NTLinksMaker
{
    public class LinkType
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public string Switch { get; set; }
    }

    public class LinkTypes
    {
        public List<LinkType> List = new List<LinkType>
        {
            new LinkType { Name = "File Hard link", Value = 0, Switch = "/h" },
            new LinkType { Name = "Directory Junction", Value = 1, Switch = "/j" },
            new LinkType { Name = "File symbolic link", Value = 2, Switch = "" },
            new LinkType { Name = "Directory symbolic link", Value = 3, Switch = "/d" }
        };
    }
}
