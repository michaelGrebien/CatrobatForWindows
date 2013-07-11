﻿using System.Xml.Linq;
using Catrobat.Core.Objects.Bricks;

namespace Catrobat.Core.Objects.Scripts
{
    public class StartScript : Script
    {
        public StartScript() {}

        public StartScript(Sprite parent) : base(parent) {}

        public StartScript(XElement xElement, Sprite parent) : base(xElement, parent) {}

        internal override void LoadFromXML(XElement xRoot) {}

        internal override XElement CreateXML()
        {
            var xRoot = new XElement("startScript");

            CreateCommonXML(xRoot);

            return xRoot;
        }

        public override DataObject Copy(Sprite parent)
        {
            var newStartScript = new StartScript(parent);
            if (_bricks != null)
            {
                newStartScript._bricks = _bricks.Copy(parent) as BrickList;
            }

            return newStartScript;
        }
    }
}