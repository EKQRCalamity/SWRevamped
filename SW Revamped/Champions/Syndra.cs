using Oasys.Common;
using Oasys.Common.EventsProvider;
using Oasys.Common.GameObject;
using Oasys.Common.GameObject.Clients;
using Oasys.SDK.Rendering;
using Oasys.SDK.Tools;
using SharpDX;
using SWRevamped.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWRevamped.Champions
{
    internal class Ball
    {

    }

    internal static class BallManager
    {
        internal static List<AIBaseClient> Balls = new List<AIBaseClient>();
        internal static List<AIBaseClient> Balls2 = new List<AIBaseClient>();
        internal static int Size => Balls.Count();

        internal static Task CreateObj(List<AIBaseClient> callbackObjectList, AIBaseClient callbackObject, float callbackGameTime)
        {
            if (callbackObject.IsSyndraBallAlt())
            {
                Logger.Log($"Add: {callbackObject} | {callbackObject.Position}");
                Balls.Add(callbackObject);
            }
            return Task.CompletedTask;
        }

        internal static Task BallChecker()
        {
            List<AIBaseClient> balls = Balls.deepCopy();
            foreach (AIBaseClient b in balls)
            {
                if (!b.IsSyndraBall() || !b.IsNativeObjectPointerValid)
                {
                    Logger.Log($"Remove: {b}");
                    Balls.Remove(b);
                }
            }
            //Balls.RemoveAll(x => !x.IsNativeObjectPointerValid);
            return Task.CompletedTask;
        }

        internal static Task DeleteObj(List<AIBaseClient> callbackObjectList, AIBaseClient callbackObject, float callbackGameTime)
        {
            if (!callbackObject.IsSyndraBallAlt() && Balls.Contains(callbackObject))
            {
                Logger.Log($"Remove: {callbackObject} | {callbackObject.Position}");
                Balls.Remove(callbackObject);
            }
            return Task.CompletedTask;
        }

        internal static void Render()
        {
            foreach (GameObjectBase obj in Balls.deepCopy())
            {
                RenderFactory.DrawText("Ball", LeagueNativeRendererManager.WorldToScreen(obj.Position), Color.Black);
            }
        }

    }

    internal class Syndra : ChampionModule
    {
        internal override void Init()
        {
            GameEvents.OnCreateObject += BallManager.CreateObj;
            GameEvents.OnDeleteObject += BallManager.DeleteObj;
            CoreEvents.OnCoreRender += BallManager.Render;
            
        }
    }
}
