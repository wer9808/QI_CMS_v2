using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace QI_CMS_v2.Models
{

    public class DisplayLayer
    {
        
        public int Id { get; set; }
        public string Name { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public int Z { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public Rect Bounds { get => new Rect(X, Y, Width, Height); }
        public List<Schedule> DefaultSchedules { get; set; } = [];
        public List<Schedule> RepeatSchedules { get; set; } = [];
        public List<Schedule> DisposableSchedules { get; set; } = [];

    }

    public class DisplayLayerManager
    {

        public List<DisplayLayer> Layers { get; set; } = [];
        public SortedDictionary<int, DisplayLayer> LayersByZ { get; set; } = new SortedDictionary<int, DisplayLayer>();
        private int _nextLayerId { get => Layers.Count == 0 ? 0 : Layers.Last().Id + 1; }


        private static DisplayLayerManager? _instance;
        public static DisplayLayerManager Instance { get => _instance ??= new DisplayLayerManager(); }

        public DisplayLayer Create()
        {
            var totalArea = DisplayManager.Instance.TotalDisplayArea;
            int newZ = LayersByZ.Count > 0 ? LayersByZ.Keys.Max() + 1 : 0;
            var displayLayer = new DisplayLayer()
            {
                Id = _nextLayerId,
                Name = $"Layer {_nextLayerId}",
                X = totalArea.X,
                Y = totalArea.Y,
                Z = newZ,
                Width = totalArea.Width,
                Height = totalArea.Height,
            };
            Layers.Add(displayLayer);
            LayersByZ[newZ] = displayLayer;

            return displayLayer;
        }

        internal void Remove(int id)
        {
            var removeDisplayLayer = Layers.Find(x => x.Id == id);
            if (removeDisplayLayer != null)
            {
                Layers.Remove(removeDisplayLayer);
                LayersByZ.Remove(removeDisplayLayer.Z);
            }
        }

        public void BringForward(int id)
        {
            var layer = Layers.FirstOrDefault(l => l.Id == id);
            if (layer == null) return;

            var zList = LayersByZ.Keys.ToList();
            int index = zList.IndexOf(layer.Z);
            if (index == zList.Count - 1) return; // 최상위면 이동할 필요 없음

            int nextZ = zList[index + 1];
            SwapZ(layer, LayersByZ[nextZ]);
        }

        public void SendBackward(int id)
        {
            var layer = Layers.FirstOrDefault(l => l.Id == id);
            if (layer == null) return;

            var zList = LayersByZ.Keys.ToList();
            int index = zList.IndexOf(layer.Z);
            if (index == 0) return; // 최하위면 이동할 필요 없음

            int prevZ = zList[index - 1];
            SwapZ(layer, LayersByZ[prevZ]);
        }

        private void SwapZ(DisplayLayer a, DisplayLayer b)
        {
            LayersByZ.Remove(a.Z);
            LayersByZ.Remove(b.Z);

            (a.Z, b.Z) = (b.Z, a.Z);

            LayersByZ[a.Z] = a;
            LayersByZ[b.Z] = b;
        }
    }

}
