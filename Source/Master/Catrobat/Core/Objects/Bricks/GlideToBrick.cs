﻿using System.ComponentModel;
using System.Xml.Linq;
using Catrobat.Core.Objects.Formulas;

namespace Catrobat.Core.Objects.Bricks
{
    public class GlideToBrick : Brick
    {
        protected Formula _durationInSeconds;
        public Formula DurationInSeconds
        {
            get { return _durationInSeconds; }
            set
            {
                if (_durationInSeconds == value)
                {
                    return;
                }

                _durationInSeconds = value;
                RaisePropertyChanged();
            }
        }

        protected Formula _xDestination;
        public Formula XDestination
        {
            get { return _xDestination; }
            set
            {
                if (_xDestination == value)
                {
                    return;
                }

                _xDestination = value;
                RaisePropertyChanged();
            }
        }

        protected Formula _yDestination;
        public Formula YDestination
        {
            get { return _yDestination; }
            set
            {
                if (_yDestination == value)
                {
                    return;
                }

                _yDestination = value;
                RaisePropertyChanged();
            }
        }


        public GlideToBrick() {}

        public GlideToBrick(Sprite parent) : base(parent) {}

        public GlideToBrick(XElement xElement, Sprite parent) : base(xElement, parent) {}

        internal override void LoadFromXML(XElement xRoot)
        {
            _durationInSeconds = new Formula(xRoot.Element("durationInSeconds"));
            _xDestination = new Formula(xRoot.Element("xDestination"));
            _yDestination = new Formula(xRoot.Element("yDestination"));
        }

        internal override XElement CreateXML()
        {
            var xRoot = new XElement("glideToBrick");

            var xVariable1 = new XElement("durationInSeconds");
            xVariable1.Add(_durationInSeconds.CreateXML());
            xRoot.Add(xVariable1);

            var xVariable2 = new XElement("xDestination");
            xVariable2.Add(_xDestination.CreateXML());
            xRoot.Add(xVariable2);

            var xVariable3 = new XElement("yDestination");
            xVariable3.Add(_yDestination.CreateXML());
            xRoot.Add(xVariable3);

            return xRoot;
        }

        public override DataObject Copy(Sprite parent)
        {
            var newBrick = new GlideToBrick(parent);
            newBrick._durationInSeconds = _durationInSeconds.Copy(parent) as Formula;
            newBrick._xDestination = _xDestination.Copy(parent) as Formula;
            newBrick._yDestination = _yDestination.Copy(parent) as Formula;

            return newBrick;
        }
    }
}