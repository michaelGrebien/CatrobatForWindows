﻿using System.Threading.Tasks;
using Catrobat.IDE.Core.Models;

namespace Catrobat.IDE.Core.Services
{
    public interface IPlayerLauncherService
    {
        Task LaunchPlayer(Project project, bool isLaunchedFromTile = false);

        Task LaunchPlayer(string projectName, bool isLaunchedFromTile = false);
    }
}