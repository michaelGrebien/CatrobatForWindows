﻿using System.Linq;
using System.Xml.Linq;
using Catrobat.IDE.Core.Xml.XmlObjects.Scripts;

namespace Catrobat.IDE.Core.Xml.XmlObjects
{
    public partial class XmlSprite : XmlObject
    {
        public XmlCostumeList Costumes { get; set; }

        public string Name { get; set; }

        public XmlScriptList Scripts { get; set; }

        public XmlSoundList Sounds { get; set; }

        public XmlSprite()
        {
            Scripts = new XmlScriptList();
            Costumes = new XmlCostumeList();
            Sounds = new XmlSoundList();
        }

        public XmlSprite(XElement xElement)
        {
            LoadFromXml(xElement);
        }

        internal override void LoadFromXml(XElement xRoot)
        {
            if (xRoot.Element("lookList") != null)
            {
                Costumes = new XmlCostumeList(xRoot.Element("lookList"));
            }

            Name = xRoot.Element("name").Value;

            if (xRoot.Element("soundList") != null)
            {
                Sounds = new XmlSoundList(xRoot.Element("soundList"));
            }
            if (xRoot.Element("scriptList") != null)
            {
                Scripts = new XmlScriptList(xRoot.Element("scriptList"));
            }
        }

        internal override XElement CreateXml()
        {
            var xRoot = new XElement("object");

            if (Costumes != null)
            {
                xRoot.Add(Costumes.CreateXml());
            }

            xRoot.Add(new XElement("name")
            {
                Value = Name
            });

            if (Scripts != null)
            {
                xRoot.Add(Scripts.CreateXml());
            }

            if (Sounds != null)
            {
                xRoot.Add(Sounds.CreateXml());
            }

            return xRoot;
        }

        internal override void LoadReference()
        {
            foreach (var brick in Scripts.Scripts.SelectMany(script => script.Bricks.Bricks))
            {
                brick.LoadReference();
            }
        }
    }
}