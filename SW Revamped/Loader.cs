using Oasys.Common.EventsProvider;
using Oasys.SDK;
using Oasys.SDK.Tools;
using System;
using System.Threading.Tasks;

namespace SWRevamped
{
    public static class Loader 
    {
        internal static int Tick = 0;

        [OasysModuleEntryPoint]
        public static void EntryPoint()
        {
            GameEvents.OnGameLoadComplete += Init;
            CoreEvents.OnCoreMainTick += TickF;
        }

        private static Task TickF()
        {
            Tick++;
            return Task.CompletedTask;
        }

        private static Task Init()
        {
            // Setup
            Logger.Log(Getter.Me().ModelName);
            if (Base.Champion.ChampWithNameSupported(Getter.Me().ModelName))
            {
                Base.ChampionModule champion = Base.ChampionModule.GetFromName(Getter.Me().ModelName);
                Logger.Log(champion.GetType() + " loaded!");
                champion.Init();
            }
            return Task.CompletedTask;
        }
    }
}