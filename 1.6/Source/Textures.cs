using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AnimalOutposts
{
    [StaticConstructorOnStartup]
    public static class Textures
    {
        public static readonly Texture2D AddPair = ContentFinder<Texture2D>.Get("UI/Gizmo/AddPair");
        public static readonly Texture2D RemovePair = ContentFinder<Texture2D>.Get("UI/Gizmo/RemovePair");
        public static readonly Texture2D DeliverEarly = ContentFinder<Texture2D>.Get("UI/Gizmo/DeliverEarly");
        public static readonly Texture2D DeliverJuveniles = ContentFinder<Texture2D>.Get("UI/Gizmo/DeliverJuveniles");

    }
}
