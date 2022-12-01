using ImpinjOctane;
using ImpinjReader.Models;
using Reactive.Bindings;
using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ImpinjReader.ViewModels
{
    internal class MainWindowViewModel : INotifyPropertyChanged, IDisposable
    {

        /// <summary>
        /// デストラクタ
        /// </summary>
        public void Dispose()
        {
            Disposable.Dispose();
        }


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

            reader.OnConnectionLostEvent = OnConnectionLost;
            reader.OnKeepaliveReceivedEvent = OnKeepaliveReceived;
            reader.OnTagReportedEvent = OnTagReported;

            reader.OnStartCompleted = OnStartCompleted;
            reader.OnStopCompleted = OnStopCompleted;
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

        private void OnConnectionLost(string value)
        {
            Task.Run(() =>
            {
                Messages.Add(new Message()
                {
                    Title = value,
                });
            }).GetAwaiter().GetResult();
        }

        private void OnKeepaliveReceived(string value)
        {
            Task.Run(() =>
            {
                Messages.Add(new Message()
                {
                    Title = value,
                });
            }).GetAwaiter().GetResult();
        }

        private void OnTagReported(TagModel value)
        {
            Task.Run( () =>
            {
                MainModel.Result.Add(value);
            }).GetAwaiter().GetResult();
        }
    }
}
