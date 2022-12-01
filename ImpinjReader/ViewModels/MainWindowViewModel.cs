using Common.Uhf;
using ImpinjOctane;
using ImpinjReader.Models;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ImpinjReader.ViewModels
{
    internal class MainWindowViewModel : INotifyPropertyChanged, IDisposable
    {

        /// <summary>
        /// PropertyChanged これが無いとメモリリークを起こす
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        // Disposeが必要なReactivePropertyやReactiveCommandを集約させるための仕掛け
        private CompositeDisposable Disposable { get; } = new CompositeDisposable();


        #region Data Model

        public MainWindowModel MainModel { get; set; } = new MainWindowModel();

        public ReactiveCollection<Message> Messages { get; set; } = new ReactiveCollection<Message>();

        #endregion

        public ReactiveCommand RfidInventoryStartCommand { get; } = new ReactiveCommand();
        public ReactiveCommand RfidInventoryStopCommand { get; } = new ReactiveCommand();
        public ReactiveCommand ClearCommand { get; } = new ReactiveCommand();

        

        #region 状態管理用（Model参照）


        #endregion

        private ImpinjReaderController reader = new ImpinjReaderController();


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindowViewModel()
        {
            // UIスレッド以外からコレクションを操作する為に必要
            BindingOperations.EnableCollectionSynchronization(Messages, new object());

            //実行ボタンの押下
            RfidInventoryStartCommand.Subscribe(_ => Start());
            RfidInventoryStopCommand.Subscribe(_ => Stop());
            ClearCommand.Subscribe(_ => Clear());

            reader.onReceiveMessage = onReceiveMessage;
            reader.OnConnectionLostEvent = OnConnectionLost;
            reader.OnKeepaliveReceivedEvent = OnKeepaliveReceived;
            reader.OnStartCompleted = OnStartCompleted;
            reader.OnStopCompleted = OnStopCompleted;

            //
            reader.ReceivedTags = OnReceivedTags;
        }


        /// <summary>
        /// デストラクタ
        /// </summary>
        public void Dispose()
        {
            Stop();
            Disposable.Dispose();
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        private void onReceiveMessage(string value)
        {
            Task.Run(() =>
            {
                Messages.Add(new Message()
                {
                    Title = value,
                });
            }).GetAwaiter().GetResult();
        }
        
        /// <summary>
        /// QRコード読み取り検出
        /// </summary>
        /// <param name="no"></param>
        private void Start()
        {
            string host = "192.168.0.101";

            _ = Task.Run(() =>
            {
                reader.Start(host);
                return Task.CompletedTask;
            });
        }

        private void Stop()
        {
            reader.Stop();
        }

        private void Clear()
        {
            Task.Run(() =>
            {
                MainModel.Tags.Clear();
            }).GetAwaiter().GetResult();
        }


        private void OnStartCompleted()
        {
            Task.Run(() =>
            {
                Messages.Add(new Message()
                {
                    Title = "Rader Start completed",
                });
            }).GetAwaiter().GetResult();
        }

        private void OnStopCompleted()
        {
            Task.Run(() =>
            {
                Messages.Add(new Message()
                {
                    Title = "Rader Stoped",
                });
            }).GetAwaiter().GetResult();
        }

        private void OnConnectionLost(ImpinjReaderDTO value)
        {
            var message = string.Format("Connection lost : {0} ({1})", value.Name, value.Address);
        }

        private void OnKeepaliveReceived(ImpinjReaderDTO value)
        {
            var message = string.Format("Keepalive received from {0} ({1})", value.Name, value.Address);
        }


        private void OnReceivedTags(IEnumerable<Common.Uhf.Tag> tags)
        {
            Task.Run(() =>
            {
                if (tags.Count() > 1)
                {
                    var a = 1;
                }
                foreach (var tag in tags)
                {
                    MainModel.Tags.Add(tag);
                }
            }).GetAwaiter().GetResult();
        }

    }
}
