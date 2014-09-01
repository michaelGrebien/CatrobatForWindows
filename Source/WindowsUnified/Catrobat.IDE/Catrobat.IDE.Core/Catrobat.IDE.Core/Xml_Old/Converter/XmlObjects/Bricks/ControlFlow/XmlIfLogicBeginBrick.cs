﻿using Catrobat.IDE.Core.Models.Bricks;
using Catrobat.IDE.Core.ExtensionMethods;
using Context = Catrobat.IDE.Core.Xml.Converter.XmlProgramConverter.ConvertContext;

// ReSharper disable once CheckNamespace
namespace Catrobat.IDE.Core.Xml.XmlObjects.Bricks.ControlFlow
{
    partial class XmlIfLogicBeginBrick
    {
        protected override Brick ToModel2(Context context)
        {
            var result = new IfBrick
            {
                Condition = IfCondition == null ? null : IfCondition.ToModel(context)
            };
            context.Bricks[this] = result;
            result.Else = IfLogicElseBrick == null ? null : (ElseBrick) IfLogicElseBrick.ToModel(context);
            result.End = IfLogicEndBrick == null ? null : (EndIfBrick) IfLogicEndBrick.ToModel(context);
            return result;
        }
    }
}