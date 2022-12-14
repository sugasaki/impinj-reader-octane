using Common.Uhf;
using Impinj.OctaneSdk;
using ImpinjOctane;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ImpinjReader.Models
{
    internal class MainWindowModel : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// PropertyChanged これが無いとメモリリークを起こす
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;


        // Disposeが必要なReactivePropertyやReactiveCommandを集約させるための仕掛け
        private CompositeDisposable Disposable { get; } = new CompositeDisposable();


        public ReactiveCollection<Common.Uhf.Tag> Tags { get; set; } = new ReactiveCollection<Common.Uhf.Tag>();

        public ReactiveProperty<string> LastInventTag { get; set; } = new ReactiveProperty<string>();

        public ReactiveProperty<string> HostIpAddres { get; set; } = new ReactiveProperty<string>();


        /// <summary>
        /// デストラクタ
        /// </summary>
        public void Dispose()
        {
            Disposable.Dispose();
        }


        public MainWindowModel()
        {
            // UIスレッド以外からコレクションを操作する為に必要
            BindingOperations.EnableCollectionSynchronization(Tags, new object());

            LastInventTag.Value = "";

            HostIpAddres.Value = "192.168.0.101";
        }


        public void Add(IEnumerable<Common.Uhf.Tag> tags)
        {
            foreach (var tag in tags)
            {
                Tags.Add(tag);
            }
            LastInventTag.Value = GetName(tags.Last());
        }



        public string GetName(Common.Uhf.Tag tag)
        {
            switch (tag.Epc)
            {
                case "E28011700000020EE77280A1":
                    return "100";
                case "1C647F1F61A9DCA062EA03BD":
                    return "22098";
                case "94647F1F8BA9DCA04AEA03BD":
                    return "22098";
                case "E200001C4919008215603515":
                    return "1368";
                case "E200001C4919008212403595":
                    return "1368";
                case "E28011700000020EE7725481":
                    return "30547";
                case "E28011700000020EE7725491":
                    return "10114";
                case "E28011700000020EE77254F1":
                    return "101";
                default:
                    return tag.Epc;
            }
        }



    }
}
